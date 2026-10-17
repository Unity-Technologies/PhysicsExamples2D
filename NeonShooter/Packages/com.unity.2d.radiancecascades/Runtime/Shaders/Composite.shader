// The picture is the light gathered by the cascades landing on the background surface, scaled by an exposure, rolled off so the
// brightest parts do not clip, then encoded for the display.
Shader "Hidden/RC2DGI/Composite"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Composite"

            Cull Off
            ZWrite Off
            ZTest Always

            // Opaque, because this is laid down before the sprite batch and the sprites are drawn over it. Blending against what
            // the target already holds would tie the background to a channel the camera's own target then has to carry.
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            // The light arriving at each point, gathered from the emitters.
            TEXTURE2D(_IrradianceTex);
            SAMPLER(sampler_IrradianceTex);

            // The one brightness control, in stops: the gathered light is scaled by two to this power before the tonemap.
            float _Exposure;

            // How much of the light landing on the background it sends back, between the grid lines and on one. A surface
            // reflecting everything is a surface that disappears into the light: with emitters spread across the screen every
            // point then reads as bright as the lights themselves, which is a fog rather than a lit room. A dark surface is what
            // keeps the lights reading as lights.
            float4 _BaseColor;
            float4 _LineColor;

            // How dense the grid is, how thick a line is as a fraction of a cell, and where the pattern sits.
            float _CellsAcross;
            float _LineThickness;
            float2 _Offset;

            // Corrects the grid for the viewport's shape, so cells stay square rather than stretching with the window.
            float2 _AspectScale;

            // Which way the grid's plane is leaning, and how near the camera sits to it.
            float4x4 _GridRotation;
            float _GridPerspective;

            // How much of the lighting's buffers the screen covers. Those buffers are rounded up so every cascade level divides
            // them into whole blocks, which leaves a margin the screen does not reach, and stretching over it instead of scaling
            // into it resamples every texel and roughens the result.
            float2 _BufferScale;

            // What the lighting was built from, shown instead of the result when a debug view asks for it.
            TEXTURE2D(_ColorTex);
            SAMPLER(sampler_ColorTex);

            // Which of the lighting's working buffers to show in place of the lit surface. Zero draws the surface normally.
            int _DebugView;

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

            // Rolls the brightest light off toward white instead of clipping it, so a light's core stays distinct from the surface
            // around it however bright it is. Into the ACES working space, the reference rendering transform and output transform
            // as one rational curve, then back to sRGB primaries.
            //
            // The matrices and the curve are from Baking Lab, by MJP and David Neubelt, http://mynameismjp.wordpress.com/, all
            // code licensed under the MIT license, copyright (c) 2016 MJP. The licence is reproduced in full in ATTRIBUTION.md at
            // the root of this package. That file's own header records that the fit was originally written by Stephen Hill
            // (@self_shadow), who deserves all credit for coming up with it and implementing it.
            // https://github.com/TheRealMJP/BakingLab/blob/master/BakingLab/ACES.hlsl
            float3 TonemapACES(float3 color)
            {
                const float3x3 acesInput = float3x3(
                    0.59719, 0.35458, 0.04823,
                    0.07600, 0.90834, 0.01566,
                    0.02840, 0.13383, 0.83777);

                const float3x3 acesOutput = float3x3(
                    1.60475, -0.53108, -0.07367,
                    -0.10208, 1.10813, -0.00605,
                    -0.00327, -0.07276, 1.07602);

                color = mul(acesInput, color);

                float3 a = color * (color + 0.0245786) - 0.000090537;
                float3 b = color * (0.983729 * color + 0.4329510) + 0.238081;

                color = mul(acesOutput, a / b);

                return saturate(color);
            }

            // Encodes linear light for the display with the sRGB curve itself: linear near black, a power of one over 2.4 above it.
            float3 GammaSRGB(float3 linearColor)
            {
                float3 low = 12.92 * linearColor;
                float3 high = 1.055 * pow(linearColor, 1.0 / 2.4) - 0.055;

                return lerp(low, high, step(0.0031308, linearColor));
            }

            // Shows one of the buffers the lighting was built from, so each stage can be judged on its own rather than only by the
            // final image, where a fault anywhere upstream looks the same.
            float3 DebugColor(float2 uv)
            {
                uv *= _BufferScale;

                if (_DebugView == 1)
                    return SAMPLE_TEXTURE2D(_ColorTex, sampler_ColorTex, uv).aaa;

                return SAMPLE_TEXTURE2D(_IrradianceTex, sampler_IrradianceTex, uv).rgb;
            }

            // Where a point on screen lands on the grid's plane, measured in cells.
            //
            // The point is treated as a direction out of the camera, that direction is leaned by however far the plane is leaning,
            // and dividing by its depth is what lands it on the plane. That division is the whole effect: cells near the top of a
            // plane leaning away crowd together toward a vanishing point, while cells near the bottom spread apart, which is what
            // reads as looking across a surface rather than at it.
            float2 GridCell(float2 uv)
            {
                float3 direction = float3((uv - 0.5) * _AspectScale, _GridPerspective);

                direction = mul((float3x3)_GridRotation, direction);

                // Anything at or behind the camera's own depth is past the horizon, where the plane is edge on and the grid has no
                // meaningful position at all, so the depth is held just in front of it.
                return direction.xy / max(direction.z, 0.001) * _CellsAcross + _Offset;
            }

            // How much of the light landing at a point the surface sends back, which is the grid: the base color everywhere, the
            // line color on a cell boundary.
            float3 SurfaceColor(float2 uv)
            {
                // A line thickness of zero leaves the surface flat, so nothing competes with the lighting for attention.
                if (_LineThickness <= 0.0)
                    return _BaseColor.rgb;

                float2 cell = GridCell(uv);

                // Distance to the nearest cell edge along each axis, so the lines land on the cell boundaries.
                float2 toEdge = 0.5 - abs(frac(cell) - 0.5);

                // Derivatives keep a line the same visual width however dense the grid is, rather than aliasing away when cells get small.
                float2 width = fwidth(cell);
                float2 lineFalloff = 1.0 - smoothstep(0.0, max(width, _LineThickness), toEdge);

                return lerp(_BaseColor.rgb, _LineColor.rgb, saturate(max(lineFalloff.x, lineFalloff.y)));
            }

            float4 Frag(Varyings input) : SV_Target
            {
                if (_DebugView != 0)
                    return float4(DebugColor(input.uv), 1.0);

                // The light arriving at this point, and what the background does with it. The emitters' own texels are drawn over
                // the top by the sprite batch, so what is left here is only ever the surface between them.
                float3 radiance = SAMPLE_TEXTURE2D(_IrradianceTex, sampler_IrradianceTex, input.uv * _BufferScale).rgb;

                float3 display = GammaSRGB(TonemapACES(radiance * SurfaceColor(input.uv) * exp2(_Exposure)));

                // The alpha is left at one so the surface reads as opaque wherever no sprite covers it.
                return float4(display, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
