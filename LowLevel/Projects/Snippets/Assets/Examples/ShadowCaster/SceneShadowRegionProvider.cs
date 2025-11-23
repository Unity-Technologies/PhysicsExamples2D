using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Rendering.Universal;

[Serializable]
public class SceneShadowRegionProvider : ShadowShape2DProvider
{
    private ShadowShape2D m_PersistantShadowShape;

    public override bool IsShapeSource(Component sourceComponent) => sourceComponent is SceneShadowRegion;

    public override void OnPersistantDataCreated(Component sourceComponent, ShadowShape2D persistantShadowShape)
    {
        m_PersistantShadowShape = persistantShadowShape;
    }

    public override void Enabled(Component sourceComponent, ShadowShape2D persistantShadowShape)
    {
        m_PersistantShadowShape = persistantShadowShape;
    }

    public override void Disabled(Component sourceComponent, ShadowShape2D persistantShadowShape)
    {
        m_PersistantShadowShape = null;
    }

    public override void OnBeforeRender(Component sourceComponent, Bounds worldCullingBounds, ShadowShape2D persistantShadowShape)
    {
        // Fetch the scene shadow region.
        var sceneShadowRegion = sourceComponent as SceneShadowRegion;
        if (sceneShadowRegion != null)
        {
            // Does it overlap the culling bounds?
            if (sceneShadowRegion.Overlap(worldCullingBounds))
            {
                // Yes, so fetch shapes that overlap the region.
                using var regionOverlaps = sceneShadowRegion.GetOverlaps();

                if (regionOverlaps.Length > 0)
                {
                    // Yes, so create the shadow outputs.
                    const int vertexCount = 100;
                    const int indexCount = 100;

                    var radii = new NativeList<float>(vertexCount, Allocator.Temp);
                    var vertices = new NativeList<Vector3>(vertexCount, Allocator.Temp);
                    var indices = new NativeList<int>(indexCount, Allocator.Temp);

                    foreach (var overlap in regionOverlaps)
                    {
                        if (!overlap.isValid)
                            continue;

                        var vertexIndex = vertices.Length;

                        var shape = overlap.shape;
                        var bodyTransform = shape.body.transform;

                        switch (shape.shapeType)
                        {
                            case PhysicsShape.ShapeType.Circle:
                            {
                                var geometry = shape.circleGeometry.Transform(bodyTransform);

                                radii.Add(geometry.radius);
                                indices.Add(vertexIndex);
                                indices.Add(vertexIndex);
                                vertices.Add(geometry.center);

                                break;
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

                                break;
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

                                break;
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

                                break;
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

                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    // Calculate transformation required to move the Collider geometry into shadow-space.
                    var toShadowSpace = sourceComponent.transform.worldToLocalMatrix;

                    // Set the shadow shape.
                    var createInteriorGeometry = !sourceComponent.TryGetComponent<Renderer>(out var renderer);
                    m_PersistantShadowShape.SetShape(vertices.AsArray(), indices.AsArray(), radii.AsArray(), toShadowSpace, ShadowShape2D.WindingOrder.CounterClockwise, true, createInteriorGeometry);

                    // Clean up.
                    indices.Dispose();
                    vertices.Dispose();
                    radii.Dispose();

                    return;
                }
            }
        }

        // Reset shapes.
        // NOTE: This is a poor way to implement a "clear" as it's implicitly doing something.
        m_PersistantShadowShape.SetShape(new NativeArray<Vector3>(), new NativeArray<int>(), ShadowShape2D.OutlineTopology.Lines, ShadowShape2D.WindingOrder.CounterClockwise);
    }
}