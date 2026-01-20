using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Custom : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_CustomCount;
    private float m_CustomAngle;

    private void OnEnable()
    {
        // Fetch some important types.
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        // Fetch the camera manipulator and set-up the sample scene size and zoom.
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 12f;
        m_CameraManipulator.CameraPosition = Vector2.zero;

        // Set up the scene reset action.
        // NOTE: This is call when the "Reset" is selected in the Sandbox menu.
        // It should reset your sample entirely.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Reset the initial state.
        m_CustomCount = 200;
        m_CustomAngle = 199f;
        
        // Set-up the options menu to the sample.
        SetupOptions();

        // Set-up the initial scene.
        SetupScene();
    }

    // The sandbox provides controls to override various options.
    // You should restore them all to a canonical state afterwards if selected.
    private void OnDisable()
    {
    }
    
    // Set-up the options menu.
    // Basically grabbing the named UI-Element options and assigning initial values and change-events.
    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
            
            // Custom Count.
            var customCount = root.Q<SliderInt>("custom-count");
            customCount.value = m_CustomCount;
            customCount.RegisterValueChangedCallback(evt => { m_CustomCount = evt.newValue; });
            
            // Custom Angle.
            var customAngle = root.Q<Slider>("custom-angle");
            customAngle.value = m_CustomAngle;
            customAngle.RegisterValueChangedCallback(evt => { m_CustomAngle = evt.newValue; });
            
            // Fetch the scene description and assign it.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        // Here you can create the initial physics objects you might need...
    }

    private void Update()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Draw a custom pattern.
        const float radius = 10f;
        var lastPosition = Vector2.right * radius; 
        for (var n = 1; n < m_CustomCount; ++n)
        {
            var angle = PhysicsMath.ToRadians(n * m_CustomAngle);
            PhysicsMath.CosSin(angle, out var cosine, out var sine);
            var newPosition = new Vector2(cosine, sine) * radius;
            
            world.DrawLine(lastPosition, newPosition, Color.chartreuse);

            lastPosition = newPosition;
        }
    }
}