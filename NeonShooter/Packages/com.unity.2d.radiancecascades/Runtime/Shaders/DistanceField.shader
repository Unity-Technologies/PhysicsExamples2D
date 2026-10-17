// How far the nearest surface is from every texel, built by jump flooding the scene's coverage.
//
// The cascade uses this only to decide how far a ray may move between two reads of the scene, never to decide whether a ray hit
// anything. A texel whose recorded surface is slightly wrong therefore only changes where a step lands, which the march's own
// accumulation absorbs, rather than turning a ray's answer on or off.
Shader "Hidden/RC2DGI/DistanceField"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        // Marks a texel that has not found a surface yet. Any real coordinate is inside the zero to one range, so this can never
        // be mistaken for one.
        #define NO_SURFACE -1.0

        // What every point on screen looks like. Only the alpha is read here: anything with any coverage at all is a surface.
        TEXTURE2D(_ColorTex);
        SAMPLER(sampler_ColorTex);

        // The nearest surface found so far, as the previous round of the flood left it.
        TEXTURE2D(_SourceTex);
        SAMPLER(sampler_SourceTex);

        // The size of the buffers, in texels.
        float2 _RenderExtent;

        // How far this round of the flood looks for a closer surface, in texels.
        float _JumpDistance;

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

        // Measures between two texture coordinates in texels, so the result is a true distance rather than one stretched by the
        // buffer's shape.
        float SurfaceDistance(float2 from, float2 to)
        {
            return length((to - from) * _RenderExtent);
        }
        ENDHLSL

        Pass
        {
            Name "DistanceFieldSeed"

            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Starts the search off: a texel the sprites cover at all records itself as the nearest surface, and every other texel
            // records nothing. Any coverage counts, because a ray has to slow down at the faint edge of a shape as well as its middle.
            float2 Frag(Varyings input) : SV_Target
            {
                float coverage = SAMPLE_TEXTURE2D_LOD(_ColorTex, sampler_ColorTex, input.uv, 0).a;

                return coverage > 0.0 ? input.uv : float2(NO_SURFACE, NO_SURFACE);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DistanceFieldFlood"

            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Looks at the eight neighbors a fixed jump away and keeps whichever of their surfaces is closest to this texel.
            // Running this repeatedly with the jump halving each time spreads every surface's position across the whole image in a
            // handful of rounds rather than one round per texel of reach.
            float2 Frag(Varyings input) : SV_Target
            {
                float2 best = SAMPLE_TEXTURE2D_LOD(_SourceTex, sampler_SourceTex, input.uv, 0).rg;
                float bestDistance = best.x < 0.0 ? 1e9 : SurfaceDistance(input.uv, best);

                float2 jump = _JumpDistance / _RenderExtent;

                for (int y = -1; y <= 1; ++y)
                {
                    for (int x = -1; x <= 1; ++x)
                    {
                        float2 neighborUv = input.uv + float2(x, y) * jump;

                        if (any(neighborUv < 0.0) || any(neighborUv > 1.0))
                            continue;

                        float2 candidate = SAMPLE_TEXTURE2D_LOD(_SourceTex, sampler_SourceTex, neighborUv, 0).rg;

                        if (candidate.x < 0.0)
                            continue;

                        float candidateDistance = SurfaceDistance(input.uv, candidate);

                        if (candidateDistance < bestDistance)
                        {
                            bestDistance = candidateDistance;
                            best = candidate;
                        }
                    }
                }

                return best;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DistanceFieldMeasure"

            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Turns the surface each texel found into how far away it is, in texels. A texel that found nothing reports the whole
            // buffer, since a ray there has nothing in front of it to slow down for.
            float Frag(Varyings input) : SV_Target
            {
                float2 surface = SAMPLE_TEXTURE2D_LOD(_SourceTex, sampler_SourceTex, input.uv, 0).rg;

                if (surface.x < 0.0)
                    return length(_RenderExtent);

                return SurfaceDistance(input.uv, surface);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
