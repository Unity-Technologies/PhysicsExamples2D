Shader "Hidden/RC2DGI/SceneCapture"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "SceneCapture"

            Cull Off
            ZWrite Off
            ZTest Always

            // Drawn over a transparent clear, so a point's color ends up weighted by how much of it the sprite covers and the alpha
            // says how much that is. That is exactly what the lighting reads: the color is the light given off, the alpha is the surface.
            // The alpha blends with a source factor of one, so it accumulates the coverage itself rather than the coverage squared.
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            // The full pipeline core, because the per-instance object-to-world matrix only exists once its input and transform
            // headers are in scope.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SpriteTex);
            SAMPLER(sampler_SpriteTex);

            // The camera's matrices are not bound for a pass drawing outside the camera's own render, so the combined one is supplied.
            float4x4 _CaptureViewProjection;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct Attributes
            {
                float3 positionOS : POSITION;

                // The sprite's own coordinates, which already cover its region of whatever texture it was packed into.
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS);

                output.positionCS = mul(_CaptureViewProjection, float4(positionWS, 1.0));
                output.uv = input.uv;

                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float4 sprite = SAMPLE_TEXTURE2D(_SpriteTex, sampler_SpriteTex, input.uv);

                return sprite * UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColor);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
