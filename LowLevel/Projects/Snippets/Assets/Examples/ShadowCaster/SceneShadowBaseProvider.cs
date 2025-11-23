using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Rendering.Universal;

public abstract class SceneShadowBaseProvider : ShadowShape2DProvider
{
    protected void AddShapeShadow(PhysicsShape shape, ref NativeList<float> radii, ref NativeList<Vector3> vertices, ref NativeList<int> indices)
    {
        // Finish if the shape is invalid.
        if (!shape.isValid)
            return;
        
        var bodyTransform = shape.body.transform;
        var vertexIndex = vertices.Length;
        
        switch (shape.shapeType)
        {
            case PhysicsShape.ShapeType.Circle:
            {
                var geometry = shape.circleGeometry.Transform(bodyTransform);

                radii.Add(geometry.radius);
                indices.Add(vertexIndex);
                indices.Add(vertexIndex);
                vertices.Add(geometry.center);

                return;
            }
            
            case PhysicsShape.ShapeType.Capsule:
            {
                var geometry = shape.capsuleGeometry.Transform(bodyTransform);

                radii.Add(geometry.radius);
                indices.Add(vertexIndex++);
                vertices.Add(geometry.center1);
                radii.Add(geometry.radius);
                indices.Add(vertexIndex);
                vertices.Add(geometry.center2);

                return;
            }
            
            case PhysicsShape.ShapeType.Polygon:
            {
                var geometry = shape.polygonGeometry.Transform(bodyTransform);
                var geometryVertices = geometry.vertices;

                var edgeIndex = vertexIndex;
                for (var n = 0; n < (geometry.count - 1); ++n)
                {
                    radii.Add(geometry.radius);
                    vertices.Add(geometryVertices[n]);
                    indices.Add(edgeIndex++);
                    indices.Add(edgeIndex);
                }

                radii.Add(geometry.radius);
                vertices.Add(geometryVertices[geometry.count - 1]);
                indices.Add(edgeIndex);
                indices.Add(vertexIndex);

                return;
            }
            
            case PhysicsShape.ShapeType.Segment:
            {
                var geometry = shape.segmentGeometry.Transform(bodyTransform);

                radii.Add(0.05f);
                radii.Add(0.05f);
                vertices.Add(geometry.point1);
                vertices.Add(geometry.point2);
                indices.Add(vertexIndex++);
                indices.Add(vertexIndex);

                return;
            }
            
            case PhysicsShape.ShapeType.ChainSegment:
            {
                var geometry = shape.chainSegmentGeometry.segment.Transform(bodyTransform);

                radii.Add(0.05f);
                radii.Add(0.05f);
                vertices.Add(geometry.point1);
                vertices.Add(geometry.point2);
                indices.Add(vertexIndex++);
                indices.Add(vertexIndex);

                return;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
