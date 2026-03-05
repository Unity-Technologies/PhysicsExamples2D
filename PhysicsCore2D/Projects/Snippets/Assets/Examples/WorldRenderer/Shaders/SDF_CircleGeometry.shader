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
// - Packed boolean flags (saves 8-9 bytes interpolators)
// - Half precision for colors and scalars
// - Early squared-distance rejection
// - Hardware reciprocal operations
// - clip() instead of discard
// - Constant buffer for globals
// - Optimized blend function
// - FIXED: Proper pixel size calculation for orthographic and perspective cameras

Shader "PhysicsCore2D/SDF_CircleGeometry"
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

            bool isInteriorFill(int fillOptions) { return fillOptions & 1; }
            bool isOutlineFill(int fillOptions) { return fillOptions & 2; }
            bool isOrientationFill(int fillOptions) { return fillOptions & 4; }

            struct vertexInput
            {
                float4 vertex   : POSITION;
            };

            // OPTIMIZED: Reduced interpolators by packing bools and using half precision
            struct fragInput
            {
                float4 vertex       : SV_POSITION;
                float4 position     : TEXCOORD0;
                half4 color         : COLOR;
                half4 fillColor     : FILLCOLOR;
                half thickness      : THICKNESS;
                uint flags          : FLAGS;  // drawOutline (bit 0), drawOrientation (bit 1), drawInterior (bit 2)
            }; 

            struct circleGeometryElement
            {
                float4 transform;
                float radius;
                float depth;
                int fillOptions;
                float4 color;
            };

            // OPTIMIZED: Use cbuffer for better constant packing
            cbuffer ShaderConstants
            {
                float thickness;
                float fillAlpha;
                int transform_plane;
            };

            StructuredBuffer<circleGeometryElement> element_buffer;
            float4x4 transform_plane_matrix;
            
            fragInput vert(const vertexInput input, const uint instance_id: SV_InstanceID)
            {
                fragInput output;

                const circleGeometryElement element = element_buffer[instance_id];
                
                // Frag position.
                const float4 local_mesh_vertex = input.vertex;
                output.position = local_mesh_vertex;

                // Fill flags.
                bool drawOutline = isOutlineFill(element.fillOptions);
                bool drawOrientation = isOrientationFill(element.fillOptions);
                bool drawInterior = isInteriorFill(element.fillOptions);
                
                // OPTIMIZED: Pack flags into single uint
                output.flags = (drawOutline ? 0x1 : 0) | (drawOrientation ? 0x2 : 0) | (drawInterior ? 0x4 : 0);
                
                // Color.
                output.color = half4(element.color);
                if (drawInterior)
                {
                    if (drawOutline)
                        output.fillColor = half4(element.color.rgb, element.color.a * fillAlpha);
                    else
                        output.fillColor = half4(element.color);
                }
                else
                {
                    output.fillColor = half4(0, 0, 0, 0);
                }
                
                const float4 xf = element.transform;
                const float c = xf.z;
                const float s = xf.w;
                
                const float radius = element.radius;
                const float2 p = local_mesh_vertex.xy * radius.xx;
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
                output.thickness = half(thickness * (pixel_size / radius) * pixel_scaling);

                // Transformed vertex.
                output.vertex = clipPos;
                
                return output;
            }

            // OPTIMIZED: Use hardware reciprocal
            float4 blend_colors(float4 front, float4 back)
            {
                const float3 c_src = front.rgb;
                const float alpha_src = front.a;
                const float3 c_dst = back.rgb;
                const float alpha_dst = back.a;

                float3 c_out = c_src * alpha_src + c_dst * alpha_dst * (1.0 - alpha_src);
                float alpha_out = alpha_src + alpha_dst * (1.0 - alpha_src);
                
                const float inv_alpha = rcp(alpha_out);
                c_out = c_out * inv_alpha;

                return float4(c_out, alpha_out);
            }           
            
            half4 frag(const fragInput input) : SV_Target
            {
                // OPTIMIZED: Unpack flags
                const bool drawOutline = (input.flags & 0x1) != 0;
                const bool drawOrientation = (input.flags & 0x2) != 0;
                const bool drawInterior = (input.flags & 0x4) != 0;
                
                // Radius in unit quad.
                const float radius = 1.0;

                // Distance to axis line segment.
                const float2 w = input.position;

                // OPTIMIZED: Early rejection with squared distance
                const float dw_sq = dot(w, w);
                const float thickness = input.thickness;
                const float max_dist = radius + thickness;
                clip(max_dist * max_dist - dw_sq);
                
                // Now compute actual distance
                const float dw = sqrt(dw_sq);
                
                // If filled, roll the fill alpha down at the border.
                half4 interior = half4(0, 0, 0, 0);
                if (drawInterior)
                    interior = half4(input.fillColor.rgb, input.fillColor.a * smoothstep(radius + thickness, radius, dw));

                // Roll the border alpha down from 1 to 0 across the border thickness.
                half4 outline = half4(0, 0, 0, 0);
                if (drawOutline)
                {
                    // Union of circle and axis.
                    const float2 e = float2(radius, 0.0);
                    const float we = dot(w, e);
                    
                    // OPTIMIZED: Use hardware reciprocal
                    const float ee = dot(e, e);
                    const float inv_ee = rcp(ee);
                    const float t = clamp(we * inv_ee, 0.0, 1.0);
                    const float2 b = w - e * t;
                    const float da = length(b);
                    const float dc = abs(dw - radius);

                    const float distance = drawOrientation ? min(da, dc) : dc;
                    
                    const half4 outline_color = input.color;
                    outline = half4(outline_color.rgb, outline_color.a * smoothstep(thickness, 0.0, distance));
                }

                return blend_colors(outline, interior);
            }
            
            ENDHLSL
        }
    }
}
