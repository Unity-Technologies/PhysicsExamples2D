using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Arch : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private float m_Friction;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 8f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(true);
        
        m_Friction = 1f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();
    }
    
    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Friction.
            var friction = root.Q<Slider>("friction");
            friction.value = m_Friction;
            friction.RegisterValueChangedCallback(evt =>
            {
                m_Friction = evt.newValue;
                SetupScene();
            });

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        var world = PhysicsWorld.defaultWorld;

        var ps1 = new Vector2[]
        {
            new(16.0f, 0.0f),
            new(14.93803712795643f, 5.133601056842984f),
            new(13.79871746027416f, 10.24928069555078f),
            new(12.56252963284711f, 15.34107019122473f),
            new(11.20040987372525f, 20.39856541571217f),
            new(9.66521217819836f, 25.40369899225096f),
            new(7.87179930638133f, 30.3179337000085f),
            new(5.635199558196225f, 35.03820717801641f),
            new(2.405937953536585f, 39.09554102558315f)
        };

        var ps2 = new Vector2[]
        {
            new(24.0f, 0.0f),
            new(22.33619528222415f, 6.02299846205841f),
            new(20.54936888969905f, 12.00964361211476f),
            new(18.60854610798073f, 17.9470321677465f),
            new(16.46769273811807f, 23.81367936585418f),
            new(14.05325025774858f, 29.57079353071012f),
            new(11.23551045834022f, 35.13775818285372f),
            new(7.752568160730571f, 40.30450679009583f),
            new(3.016931552701656f, 44.28891593799322f)
        };

        const float scale = 0.25f;
        for (var i = 0; i < 9; ++i)
        {
            ps1[i] *= scale;
            ps2[i] *= scale;
        }

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_Friction } };

        // Ground.
        {
            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            body.CreateShape(new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) }, shapeDef);
        }

        // Arch.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };

            for (var i = 0; i < 8; ++i)
            {
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                var ps = new[] { ps1[i], ps2[i], ps2[i + 1], ps1[i + 1] };
                body.CreateShape(PolygonGeometry.Create(ps.AsSpan(), 0f, PhysicsTransform.identity), shapeDef);
            }

            for (var i = 0; i < 8; ++i)
            {
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                var ps = new[]
                {
                    new Vector2(-ps2[i].x, ps2[i].y),
                    new Vector2(-ps1[i].x, ps1[i].y),
                    new Vector2(-ps1[i + 1].x, ps1[i + 1].y),
                    new Vector2(-ps2[i + 1].x, ps2[i + 1].y)
                };
                body.CreateShape(PolygonGeometry.Create(ps.AsSpan(), 0f, PhysicsTransform.identity), shapeDef);
            }

            {
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                var ps = new[]
                {
                    ps1[8],
                    ps2[8],
                    new Vector2(-ps2[8].x, ps2[8].y),
                    new Vector2(-ps1[8].x, ps1[8].y)
                };
                body.CreateShape(PolygonGeometry.Create(ps.AsSpan(), 0f, PhysicsTransform.identity), shapeDef);
            }

            for (var i = 0; i < 4; ++i)
            {
                bodyDef.position = new Vector2(0.0f, 0.5f + ps2[8].y + 1.0f * i);
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(4f, 1f)), shapeDef);
            }
        }
    }
}