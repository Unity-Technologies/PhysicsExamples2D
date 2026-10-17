// Ported from Yaazarai's GMShaders-Radiance-Cascades (Unlicense, public domain):
// Shd_RadianceCascades_BilinearFix.fsh, the direction-first layout with the bilinear and forking fixes.
// https://github.com/Yaazarai/GMShaders-Radiance-Cascades
//
// The technique itself is Alexander Sannikov's Radiance Cascades: https://github.com/Raikiri/RadianceCascadesPaper
//
// The layout, the merge and the forking fix are the original's. The march is not: it reads the scene directly, accumulating what
// each point gives off against what is still visible through everything nearer, and uses the distance field only to decide how
// far it may move between two of those reads.
Shader "Hidden/RC2DGI/Cascade"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Cascade"

            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            #define TAU 6.283185

            // What every point on screen looks like: the color is the light it gives off, already weighted by how much of the
            // point is covered, and the alpha is that coverage.
            TEXTURE2D(_ColorTex);
            SAMPLER(sampler_ColorTex);

            // How far the nearest surface is from every texel, which is how far a ray may move without passing through anything.
            TEXTURE2D(_DistanceTex);
            SAMPLER(sampler_DistanceTex);

            // The cascade one level coarser than this one, already finished.
            TEXTURE2D(_ParentTex);
            SAMPLER(sampler_ParentTex);

            // The size of the screen buffers and of the cascade buffers, both in pixels.
            float2 _RenderExtent;
            float2 _CascadeExtent;

            // Which level is being drawn and how many there are in total. Level zero is the finest.
            float _CascadeIndex;
            float _CascadeCount;

            // The probe spacing and the ray length of the finest level, both in pixels. Every level above doubles the first
            // and quadruples the second.
            float _CascadeLinear;
            float _CascadeInterval;

            // How far, in texels, a ray of the finest level moves between two reads of the scene. Every level above doubles it.
            float _StepStride;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);

                return output;
            }

            // The most reads one ray is allowed. A ray only spends one on each stride it takes through something, so this is
            // reached only where a ray runs the whole length of a surface, where what lies beyond is already dark anyway.
            #define MAX_STEPS 256

            // Walks a ray from one point to another and returns the light gathered along it, with the alpha holding how much of
            // whatever lies beyond the far end still shows through. Every value here is in texels.
            //
            // Through anything at all the ray moves one stride at a time, adding what that point gives off, weighted by how much
            // is still visible through everything before it, then absorbing by the point's coverage. Through empty space it jumps
            // straight to whatever is in front of it, because nothing between here and there can add or absorb anything, which is
            // what keeps the open parts of the screen from costing thousands of reads of nothing.
            //
            // There is no hit test anywhere: the nearest surface only ever sets the length of a step. A ray therefore never
            // registers a surface it should have missed or misses one it should have hit, however rough that distance is.
            // A ray that leaves the buffer travels on through empty space, and one fully absorbed stops.
            float4 Raymarch(float2 start, float2 end)
            {
                float2 span = end - start;
                float distance = length(span);
                float2 direction = span / max(distance, 0.00001);

                float stride = _StepStride * pow(2.0, _CascadeIndex);

                float3 radiance = 0.0;
                float transmittance = 1.0;
                float travelled = 0.0;

                for (int i = 0; i < MAX_STEPS; i++)
                {
                    if (travelled >= distance)
                        break;

                    float2 ray = (start + direction * travelled) / _RenderExtent;

                    if (any(floor(ray) != 0.0))
                        break;

                    // A texel of slack, so a step always stops short of the surface the distance was measured to rather than
                    // landing on it and reading a coverage the ray should have approached one stride at a time.
                    float clearance = SAMPLE_TEXTURE2D_LOD(_DistanceTex, sampler_DistanceTex, ray, 0).r - 1.0;

                    if (clearance > stride)
                    {
                        travelled += clearance;

                        continue;
                    }

                    float4 scene = SAMPLE_TEXTURE2D_LOD(_ColorTex, sampler_ColorTex, ray, 0);

                    radiance += scene.rgb * transmittance;
                    transmittance *= 1.0 - scene.a;

                    if (transmittance <= 0.001)
                        break;

                    travelled += stride;
                }

                return float4(radiance, transmittance);
            }

            // Lays the coarser level's radiance behind one of this level's rays, reading a single one of its probes rather than
            // blending four of them. The blending happens afterwards, between rays that were each traced to their own probe.
            // What the coarser level saw shows through by however much this ray let through, and the two visibilities compound.
            float4 MergeNearestProbe(float4 radiance, float index, float2 probe)
            {
                if (radiance.a <= 0.0 || _CascadeIndex >= _CascadeCount - 1.0)
                    return radiance;

                float angularN1 = pow(2.0, floor(_CascadeIndex + 1.0));
                float2 extentN1 = floor(_CascadeExtent / angularN1);
                float2 interpN1 = float2(fmod(index, angularN1), floor(index / angularN1)) * extentN1;

                interpN1 += clamp(probe + 0.5, 0.5, extentN1 - 0.5);

                float4 parent = SAMPLE_TEXTURE2D_LOD(_ParentTex, sampler_ParentTex, interpN1 / _CascadeExtent, 0);

                return float4(radiance.rgb + radiance.a * parent.rgb, radiance.a * parent.a);
            }

            // The four coarser probes this one sits between.
            void GetBilinearProbes(float2 probe, out float2 probes[4])
            {
                float2 probeN1 = floor((probe - 1.0) / 2.0);

                probes[0] = probeN1 + float2(0.0, 0.0);
                probes[1] = probeN1 + float2(1.0, 0.0);
                probes[2] = probeN1 + float2(0.0, 1.0);
                probes[3] = probeN1 + float2(1.0, 1.0);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 coord = floor(input.uv * _CascadeExtent);

                float sqrAngular = pow(2.0, floor(_CascadeIndex));
                float2 extent = floor(_CascadeExtent / sqrAngular);
                float4 probe = float4(fmod(coord, extent), floor(coord / extent));

                float interval = (_CascadeInterval * (1.0 - pow(4.0, _CascadeIndex))) / (1.0 - 4.0);
                float limit = _CascadeInterval * pow(4.0, _CascadeIndex);

                float2 linearSpacing = float2(_CascadeLinear * pow(2.0, _CascadeIndex), _CascadeLinear * pow(2.0, _CascadeIndex));
                float2 linearSpacingN1 = float2(_CascadeLinear * pow(2.0, _CascadeIndex + 1.0), _CascadeLinear * pow(2.0, _CascadeIndex + 1.0));

                float2 origin = (probe.xy + 0.5) * linearSpacing;
                float angular = sqrAngular * sqrAngular * 4.0;
                float index = (probe.z + probe.w * sqrAngular) * 4.0;

                float2 bilinearN1[4];
                GetBilinearProbes(probe.xy, bilinearN1);

                float2 texelIndexN1 = floor((probe.xy - 1.0) / 2.0);
                float2 texelIndexN1_N = floor(texelIndexN1 * 2.0 + 1.0);
                float2 weight = 0.25 + (probe.xy - texelIndexN1_N) * 0.5;

                float4 result = 0.0;

                for (int i = 0; i < 4; i++)
                {
                    float preaverage = index + float(i);

                    float theta = (preaverage + 0.5) * (TAU / angular);
                    float thetaNm1 = (floor(preaverage / 4.0) + 0.5) * (TAU / (angular / 4.0));

                    // The original negates the sine because GameMaker's vertical axis points down. Texture coordinates here point
                    // up, so negating it mirrors the whole light field against the scene it was gathered from.
                    float2 delta = float2(cos(theta), sin(theta));
                    float2 deltaNm1 = float2(cos(thetaNm1), sin(thetaNm1));

                    float2 rayStart = origin + deltaNm1 * interval;

                    float4 samples[4];

                    for (int j = 0; j < 4; j++)
                    {
                        float2 originN1 = (bilinearN1[j] + 0.5) * linearSpacingN1;
                        float2 rayEnd = originN1 + delta * (interval + limit);

                        samples[j] = Raymarch(rayStart, rayEnd);
                        samples[j] = MergeNearestProbe(samples[j], preaverage, bilinearN1[j]);
                    }

                    float4 top = lerp(samples[0], samples[1], weight.x);
                    float4 bottom = lerp(samples[2], samples[3], weight.x);

                    result += lerp(top, bottom, weight.y) * 0.25;
                }

                return result;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
