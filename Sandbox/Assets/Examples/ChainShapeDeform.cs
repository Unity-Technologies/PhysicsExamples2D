using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

[ExampleScene("Shapes", "Demonstrates how a Chain Shape can be created and its vertices updated in realtime.")]
public sealed class ChainShapeDeform : SandboxExampleBehaviour
{
    private int m_VertexCount = 64;
    private float m_BaseRadius = 8f;
    private float m_ModulationRadius = 1f;
    private int m_ModulationFrequency = 8;
    private float m_Speed = 1f;
    private float m_Phase;

    private NativeArray<Vector2> m_Vertices;
    private PhysicsChain m_Chain;

    protected override float CameraSize => 14f;
    protected override Vector2 CameraPosition => Vector2.zero;

    protected override void OnExampleEnable()
    {
        m_Phase = 0f;
    }

    protected override void OnExampleDisable()
    {
        if (m_Vertices.IsCreated)
            m_Vertices.Dispose();
    }

    protected override void SetupOptions()
    {
        AddSliderInt("Vertex Count", m_VertexCount, 8, 1024, v => m_VertexCount = v, rebuild: true);
        AddSlider("Base Radius", m_BaseRadius, 1f, 20f, v => m_BaseRadius = v);
        AddSlider("Modulation Radius", m_ModulationRadius, 0f, 5f, v => m_ModulationRadius = v);
        AddSliderInt("Modulation Frequency", m_ModulationFrequency, 1, 32, v => m_ModulationFrequency = v);
        AddSlider("Speed", m_Speed, -50f, 50f, v => m_Speed = v);
    }

    protected override void SetupScene()
    {
        if (m_Vertices.IsCreated)
            m_Vertices.Dispose();

        m_Vertices = new NativeArray<Vector2>(m_VertexCount, Allocator.Persistent);
        FillVertices(m_Phase);

        var world = World;
        var body = world.CreateBody();

        var chainDef = PhysicsChainDefinition.defaultDefinition;
        chainDef.isLoop = true;
        var surfaceMaterial = chainDef.surfaceMaterial;
        surfaceMaterial.customColor = ShapeColor;
        chainDef.surfaceMaterial = surfaceMaterial;

        m_Chain = PhysicsChain.Create(body, m_Vertices.AsReadOnlySpan(), chainDef);
    }

    private void Update()
    {
        if (!m_Chain.isValid)
            return;

        m_Phase += Time.deltaTime * m_Speed;
        FillVertices(m_Phase);
        m_Chain.UpdateVertices(m_Vertices.AsReadOnlySpan(), true);
    }

    private void FillVertices(float phase)
    {
        for (var i = 0; i < m_VertexCount; i++)
        {
            var theta = 2f * math.PI * i / m_VertexCount;
            var r = m_BaseRadius + m_ModulationRadius * math.sin(m_ModulationFrequency * theta + phase);
            m_Vertices[i] = new Vector2(math.cos(theta) * r, math.sin(theta) * r);
        }
    }
}
