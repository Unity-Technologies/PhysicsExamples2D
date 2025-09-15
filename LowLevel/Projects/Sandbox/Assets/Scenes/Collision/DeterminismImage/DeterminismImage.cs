//#define DEFINE_TARGETSPRITE

using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class DeterminismImage : MonoBehaviour
{
    public Sprite TargetSprite;
    
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;
    
    private ControlsMenu.CustomButton m_SnapshotButton;
    
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;

    private static readonly PhysicsMask TexelBit = new(8);
    private NativeHashMap<int, Color> m_ShapeColorMap;
    
    private const int TotalSpawnCount = 7000;
    private const int SpawnBatchCount = 10;
    private const float SpawnRadius = 0.2f;

    private readonly PhysicsAABB m_ImageRegion = new()
    {
        lowerBound = new Vector2(-20f, -13.25f),
        upperBound = new Vector2(20f, 13.25f),
    };
    
    private int m_SpawnCount;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 16f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 0f);

#if DEFINE_TARGETSPRITE
        {
            m_SnapshotButton = m_SandboxManager.ControlsMenu[0];
            m_SnapshotButton.Set("Snapshot");
            m_SnapshotButton.button.clickable.clicked += SnapshotImage;
        }
#else        
        m_ShapeColorMap = this.CreateColorMap(); 
#endif
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Get the default world.        
        var world = PhysicsWorld.defaultWorld;
        
        // Turn on interior drawing only.
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);

        SetupOptions();

        SetupScene();

        // Register for spawning.
        PhysicsEvents.PreSimulate += SpawnTexels;
    }

    private void OnDisable()
    {
#if DEFINE_TARGETSPRITE
        // Unregister.
        m_SnapshotButton.button.clickable.clicked -= SnapshotImage;
#else
        if (m_ShapeColorMap.IsCreated)
            m_ShapeColorMap.Dispose();
#endif
        // Unregister.
        PhysicsEvents.PreSimulate -= SpawnTexels;
        
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Reset the draw fill options.
        world.drawFillOptions = m_OldDrawFillOptions;
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        m_SpawnCount = 0;
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Ground.
        {
            var groundBody = world.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                m_ImageRegion.lowerBound,
                new(m_ImageRegion.lowerBound.x, m_ImageRegion.upperBound.y),
                m_ImageRegion.upperBound,
                new(m_ImageRegion.upperBound.x, m_ImageRegion.lowerBound.y)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }
    }

    private void SpawnTexels(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld)
            return;
        
        if (m_SpawnCount >= TotalSpawnCount)
            return;

        if (m_SandboxManager.WorldPaused)
            return;
        
        ref var random = ref m_SandboxManager.Random;
        
        var bodyDef = new PhysicsBodyDefinition
        {
            bodyType = RigidbodyType2D.Dynamic,
            gravityScale = 0.5f,
            linearVelocity = Vector2.up * 40f,
            bodyConstraints = RigidbodyConstraints2D.FreezeRotation
        };

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = TexelBit,
                contacts = PhysicsMask.All
            }
        };
        var geometry = CircleGeometry.Create(SpawnRadius);

        var span = m_ImageRegion.extents * 0.25f;
        for (var n = 0; n < SpawnBatchCount; ++n)
        {
            bodyDef.position = new Vector2(random.NextFloat(-span.x, span.x), -13.25f + SpawnRadius);
      
#if !DEFINE_TARGETSPRITE
            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_ShapeColorMap[m_SpawnCount] };
#endif            
            var shape = world.CreateBody(bodyDef).CreateShape(geometry, shapeDef);
            
#if DEFINE_TARGETSPRITE
            shape.userData = new PhysicsUserData { intValue = m_SpawnCount };
#endif
            ++m_SpawnCount;
        }
    }

    private void SnapshotImage()
    {
        if (TargetSprite == null)
        {
            Debug.LogWarning("No Target Sprite Set!");
            return;
        }
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        var texture = TargetSprite.texture;
        if (!texture.isReadable)
        {
            Debug.LogWarning("Target Sprite is not Readable!");
            return;
        }
        
        var textureRect = TargetSprite.textureRect;
        var localSpaceOffset = m_ImageRegion.lowerBound;
        var localSpaceScale = textureRect.size / (m_ImageRegion.extents * 2f);

        using var hitResults = world.OverlapAABB(m_ImageRegion, new PhysicsQuery.QueryFilter { hitCategories = TexelBit });

        if (hitResults.Length != TotalSpawnCount)
        {
            Debug.LogWarning("All Spawn Items were not found!");
            return;
        }
        
        // Write the file.
        var writer = new StreamWriter("Assets/Scenes/Collision/DeterminismImage/ShapeColorMap.cs", false);
        writer.WriteLine("using UnityEngine;\nusing Unity.Collections;\npublic static class DeterminismImageExtensions\n{\npublic static NativeHashMap<int, Color> CreateColorMap(this DeterminismImage determinismImage)\n{\nvar map = new NativeHashMap<int, Color>(7000, Allocator.Persistent);");
        
        foreach (var result in hitResults)
        {
            var shape = result.shape;
            var position = shape.body.position;
            var texturePosition = (position - localSpaceOffset) * localSpaceScale;
            var texel = texture.GetPixel((int)texturePosition.x, (int)texturePosition.y);

            writer.WriteLine($"map.Add({shape.userData.intValue}, new Color({texel.r}f, {texel.g}f, {texel.b}f, {texel.a}f));");
            shape.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = texel };
        }

        writer.WriteLine("return map;\n}\n}");
        writer.Dispose();
    }
}