// MIT License
// Copyright (c) 2022 Erin Catto
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// The above notice is included due to a significant portion of the following shader code
// coming from Box2D.
//
// OPTIMIZED VERSION - Key improvements:
// - Half precision for colors and scalars
// - Early squared-distance rejection
// - clip() instead of discard
// - Constant buffer for globals
// - Simplified fragment shader logic
// - Reduced redundant calculations
// - FIXED: Proper pixel size calculation for orthographic and perspective cameras

Shader "PhysicsCore2D/SDF_Point"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Off

        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            // Swizzle for Transform Plane:
            // 0 = XY plane with Z rotation.
            // 1 = XZ plane with Y rotation.
            // 2 = ZY plane with X rotation.
            // 3 = Custom Plane.
            float4 transformPlaneSwizzle(float4 input, int transform_plane)
            {
                if (transform_plane == 0)
                    return input.xyzw;

                if (transform_plane == 1)
                    return input.xzyw;

                return input.zyxw;
            }
 
            struct vertexInput
            {
                float4 vertex   : POSITION;
            };

            // OPTIMIZED: Use half precision for colors and scalars
            struct fragInput
            {
                float4 vertex       : SV_POSITION;
                float4 position     : TEXCOORD0;
                half4 color         : COLOR;
                half thickness      : THICKNESS;
            }; 

            struct pointElement
            {
                float2 position;
                float radius;
                float depth;
                float4 color;
            };

            // OPTIMIZED: Use cbuffer for better constant packing
            cbuffer ShaderConstants
            {
                int transform_plane;
            };

            StructuredBuffer<pointElement> element_buffer;
            float4x4 transform_plane_matrix;
            
            fragInput vert(const vertexInput input, const uint instance_id: SV_InstanceID)
            {
                fragInput output;

                const pointElement element = element_buffer[instance_id];

                // Frag position.
                const float4 local_mesh_vertex = input.vertex;
                output.position = local_mesh_vertex;
                
                // Color.
                output.color = half4(element.color);

                // For orthographic, pixel_size is view-independent so we can compute it up front.
                // For perspective we need the real clip-space depth, so we defer to after transform.
                float pixel_size;
                float pixel_scaling;
                if (unity_OrthoParams.w == 1.0f) // Orthographic
                {
                    pixel_size = unity_OrthoParams.x / (_ScreenParams.y * 0.5);
                    pixel_scaling = 1.0f / 1.2f;
                }
                else // Perspective - temporary value; corrected below once we have clipPos.
                {
                    // Use an approximate depth from element position for the initial scaling.
                    // This is only used to size the quad; pixel_size is corrected after projection.
                    const float4 approx = float4(element.position.xy, element.depth, 1.0);
                    const float4 approx_clip = UnityObjectToClipPos(approx);
                    pixel_size = abs(approx_clip.w / (_ScreenParams.y * UNITY_MATRIX_P[1][1] * 0.5));
                    pixel_scaling = 1.2f;
                }

                // Vertex.
                const float radius = element.radius;
                const float2 position = element.position;
                const float scaling = radius * pixel_size;
                const float2 p = (local_mesh_vertex.xy * scaling.xx) + position.xy;
                
                // Calculate transformed (plane) vertex.
                float4 transformed = float4(p.xy, element.depth, local_mesh_vertex.w);
                if (transform_plane == 3)
                    transformed = mul(transform_plane_matrix, transformed);
                else
                    transformed = transformPlaneSwizzle(transformed, transform_plane);

                // Get clip position from the actual final vertex.
                float4 clipPos = UnityObjectToClipPos(transformed);

                // For perspective, correct pixel_size using the real clip depth now that we have it.
                if (unity_OrthoParams.w != 1.0f)
                    pixel_size = abs(clipPos.w / (_ScreenParams.y * UNITY_MATRIX_P[1][1] * 0.5));

                // When transform_plane == 3 the matrix may encode a world-space scale that
                // changes the apparent size of the geometry. Compensate so thickness stays constant.
                const float matrix_scale = (transform_plane == 3)
                    ? 0.5 * (length(transform_plane_matrix[0].xyz) + length(transform_plane_matrix[1].xyz))
                    : 1.0;

                // Thickness.
                output.thickness = half((pixel_size / (scaling * matrix_scale)) * pixel_scaling);
                
                // Transformed vertex.
                output.vertex = clipPos;
                
                return output;
            }

            half4 frag(fragInput input) : SV_Target
            {
                const float radius = 0.9;
                
                // Distance to point circumference.
                const float2 w = input.position.xy;
                const float dw_sq = dot(w, w);
                
                const float thickness = input.thickness;
                const float max_dist = radius + thickness;
                
                // OPTIMIZED: Early rejection with squared distance
                clip(max_dist * max_dist - dw_sq);
                
                // Now compute actual distance
                const float dw = sqrt(dw_sq);
                
                half4 color = input.color;
                
                // OPTIMIZED: Simplified logic - calculate distance and alpha in one path
                const float dist = abs(dw - radius);
                
                // If inside the radius, alpha stays at 1.0
                // If outside, blend based on distance
                const float alpha_mult = (dw >= radius) ? smoothstep(thickness, 0.0, dist) : 1.0;
                color.a *= alpha_mult;
                
                return color;
            }
            
            ENDHLSL
        }
    }
}
