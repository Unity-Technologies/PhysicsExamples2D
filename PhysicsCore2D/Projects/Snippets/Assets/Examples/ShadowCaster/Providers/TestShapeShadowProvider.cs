using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.U2D.Physics.Extras;

[Serializable]
internal class TestShapeShadowProvider : TestShadowProviderBase
{
    private ShadowShape2D m_PersistantShadowShape;

    public override bool IsShapeSource(Component sourceComponent) => sourceComponent is TestShape;

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
        // Fetch the test shadow region.
        var testShape = sourceComponent as TestShape;
        if (testShape != null && testShape.isActiveAndEnabled)
        {
            var shape = testShape.shape;
            
            // Create the shadow outputs.
            const int vertexCount = 8;
            const int indexCount = 8;
            var radii = new NativeList<float>(vertexCount, Allocator.Temp);
            var vertices = new NativeList<Vector3>(vertexCount, Allocator.Temp);
            var indices = new NativeList<int>(indexCount, Allocator.Temp);

            // Add the shape shadow.
            AddShapeShadow(shape, ref radii, ref vertices, ref indices);

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

        // Reset shapes.
        m_PersistantShadowShape.SetShape(new NativeArray<Vector3>(), new NativeArray<int>(), ShadowShape2D.OutlineTopology.Lines, ShadowShape2D.WindingOrder.CounterClockwise);
    }
}