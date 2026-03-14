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
// - Hardware reciprocal operations
// - Precomputed constant values
// - clip() instead of discard
// - Constant buffer for globals
// - Simplified distance calculation
// - FIXED: Proper pixel size calculation for orthographic and perspective cameras

Shader "PhysicsCore2D/SDF_Line"
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
                half ratio          : RATIO;
            }; 

            struct lineElement
            {
                float4 transform;
                float length;
                float depth;
                float4 color;
            };

            // OPTIMIZED: Use cbuffer for better constant packing
            cbuffer ShaderConstants
            {
                float thickness;
                int transform_plane;
            };

            StructuredBuffer<lineElement> element_buffer;
            float4x4 transform_plane_matrix;
            
            fragInput vert(const vertexInput input, const uint instance_id: SV_InstanceID)
            {
                fragInput output;

                const lineElement element = element_buffer[instance_id];

                // Frag position.
                const float4 local_mesh_vertex = input.vertex;
                output.position = local_mesh_vertex;
                
                // Color.
                output.color = half4(element.color);

                // Ratio.
                const float ratio = 0.2;
                output.ratio = half(rcp(ratio));
                
                // Scale quad large enough to hold line/radius.
                const float half_length = 0.5 * element.length;
                const float2 scale = float2(half_length, half_length * ratio);

                const float4 xf = element.transform;
                const float c = xf.z;
                const float s = xf.w;

                const float2 p = local_mesh_vertex.xy * scale.xy;
                const float2 p_rot = float2(c * p.x - s * p.y, s * p.x + c * p.y) + xf.xy;

                // Calculate transformed (plane) vertex.
                float4 transformed = float4(p_rot.xy, element.depth, local_mesh_vertex.w);
                if (transform_plane == 3)
                    transformed = mul(transform_plane_matrix, transformed);
                else
                    transformed = transformPlaneSwizzle(transformed, transform_plane);  
                
                // Get clip position first
                float4 clipPos = UnityObjectToClipPos(transformed);
                
                float pixel_size;
                float pixel_scaling;
                if (unity_OrthoParams.w == 1.0f) // Orthographic
                {
                    // For orthographic projection, pixel size is constant
                    // unity_OrthoParams.x is the camera's orthographic size (half-height)
                    pixel_size = unity_OrthoParams.x / (_ScreenParams.y * 0.5);

                    // No scaling.
                    pixel_scaling = 1.0f / 1.2f;
                }
                else // Perspective
                {
                    // For perspective, pixel size increases with distance from camera
                    // clipPos.w is the view-space depth (distance from camera)
                    // UNITY_MATRIX_P[1][1] is 1/tan(fov/2) for vertical FOV
                    pixel_size = abs(clipPos.w / (_ScreenParams.y * UNITY_MATRIX_P[1][1] * 0.5));

                    // Mesh extents scaling.
                    pixel_scaling = 1.2f;
                }

                // Thickness.
                // When transform_plane == 3 the matrix may encode a world-space scale that
                // changes the apparent size of the geometry.  Compensate by dividing it back
                // out so the normalised-quad thickness stays constant.
                const float matrix_scale = (transform_plane == 3)
                    ? 0.5 * (length(transform_plane_matrix[0].xyz) + length(transform_plane_matrix[1].xyz))
                    : 1.0;
                output.thickness = half(thickness * (pixel_size / (scale.y * matrix_scale)) * pixel_scaling);

                // Transformed vertex.
                output.vertex = clipPos;
                
                return output;
            }
            
            half4 frag(const fragInput input) : SV_Target
            {
                // OPTIMIZED: Precompute constant values (v2 - v1 = float2(2, 0))
                const float2 v1 = float2(-1, 0);
                
                // Distance to line segment.
                const float2 e = float2(2, 0);  // v2 - v1
                const float2 w = input.position - v1;
                const float we = dot(w, e);
                
                // OPTIMIZED: Use hardware reciprocal and precompute ee
                const float ee = 4.0;  // dot(float2(2, 0), float2(2, 0)) = 4
                const float inv_ee = 0.25;  // rcp(4.0) = 0.25
                const float t = clamp(we * inv_ee, 0.0, 1.0);
                const float2 b = w - e * t;
                
                // OPTIMIZED: Calculate with scale factors
                const float2 scaled_b = b * float2(1.0 + input.ratio, 1.0);
                const float d_sq = dot(scaled_b, scaled_b);
                
                const float thickness = input.thickness;
                const float thickness_sq = thickness * thickness;
                
                // OPTIMIZED: Early rejection with squared distance
                clip(thickness_sq - d_sq);
                
                // Now compute actual distance
                const float d = sqrt(d_sq);
                
                const half4 color = input.color;
                return half4(color.rgb, color.a * smoothstep(thickness, 0.0, d));
            }

            ENDHLSL
        }
    }
}
