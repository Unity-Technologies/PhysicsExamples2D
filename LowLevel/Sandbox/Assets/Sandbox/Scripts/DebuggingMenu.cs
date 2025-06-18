using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class DebuggingMenu : MonoBehaviour
{
    private CameraManipulator m_CameraManipulator;
    private UIDocument m_UIDocument;
    
    private int m_SampledCount;
    private PhysicsWorld.WorldCounters m_LastCounters;
    private PhysicsWorld.WorldCounters m_TotalCounters;
    private PhysicsWorld.WorldCounters m_MaxCounters;
    private PhysicsWorld.WorldProfile m_LastProfile;
    private PhysicsWorld.WorldProfile m_TotalProfile;
    private PhysicsWorld.WorldProfile m_MaxProfile;
	
    // Profile.
    private Label m_SimulationStepElement;
    private Label m_ContactPairsElement;
    private Label m_ContactUpdatesElement;
    private Label m_SolvingElement;
    private Label m_MergeIslandsElement;
    private Label m_PrepareStagesElement;
    private Label m_PrepareConstraintsElement;
    private Label m_SolveConstraintsElement;
    private Label m_IntegrateVelocitiesElement;
    private Label m_WarmStartingElement;
    private Label m_SolveImpulsesElement;
    private Label m_IntegrateTransformsElement;
    private Label m_RelaxImpulsesElement;
    private Label m_ApplyBouncinessElement;
    private Label m_StoreImpulsesElement;
    private Label m_SplitIslandsElement;
    private Label m_BodyTransformsElement;
    private Label m_JointEventsElement;
    private Label m_HitEventsElement;
    private Label m_BroadphaseUpdatesElement;
    private Label m_SolveContinuousElement;
    private Label m_SleepIslandsElement;
    private Label m_UpdateTriggersElement;
    private Label m_WriteTransformsElement;
    
    // Counters.
    private Label m_BodyCountElement;
    private Label m_ShapeCountElement;
    private Label m_JointCountElement;
    private Label m_ContactCountElement;
    private Label m_IslandCountElement;
    private Label m_StaticTreeHeightElement;
    private Label m_MoveableTreeHeightElement;
    private Label m_StackBytesUsedElement;
    private Label m_TotalBytesUsedElement;
    private Label m_TaskCountElement;

    public void ResetStats()
    {
        m_SampledCount = 0;
        m_TotalCounters = m_MaxCounters = default;
        m_TotalProfile = m_MaxProfile = default;
    }
    
    private void OnEnable()
    {
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // Reset the stats.
        ResetStats();
        
        // Update states when a world has finished simulating. 
        PhysicsEvents.PostSimulate += UpdateStats;

        // Menu Region.
        {
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );
        }
        
        // Profile.
        {
            m_SimulationStepElement = root.Q<Label>("simulation-step");
            m_ContactPairsElement = root.Q<Label>("contact-pairs");
            m_ContactUpdatesElement = root.Q<Label>("contact-updates");
            m_SolvingElement = root.Q<Label>("solving");
            m_MergeIslandsElement = root.Q<Label>("merge-islands");
            m_SolveConstraintsElement = root.Q<Label>("solve-constraints");
            m_PrepareStagesElement = root.Q<Label>("prepare-stages");
            m_PrepareConstraintsElement = root.Q<Label>("prepare-constraints");
            m_SolveConstraintsElement = root.Q<Label>("solve-constraints");
            m_IntegrateVelocitiesElement = root.Q<Label>("integrate-velocities");
            m_WarmStartingElement = root.Q<Label>("warm-starting");
            m_SolveImpulsesElement = root.Q<Label>("solve-impulses");
            m_IntegrateTransformsElement = root.Q<Label>("integrate-transforms");
            m_RelaxImpulsesElement = root.Q<Label>("relax-impulses");
            m_ApplyBouncinessElement = root.Q<Label>("apply-bounciness");
            m_StoreImpulsesElement = root.Q<Label>("store-impulses");
            m_SplitIslandsElement = root.Q<Label>("split-islands");
            m_BodyTransformsElement = root.Q<Label>("body-transforms");
            m_JointEventsElement = root.Q<Label>("joint-events");
            m_HitEventsElement = root.Q<Label>("hit-events");
            m_BroadphaseUpdatesElement = root.Q<Label>("broadphase-updates");
            m_SolveContinuousElement = root.Q<Label>("solve-continuous");
            m_SleepIslandsElement = root.Q<Label>("sleep-islands");
            m_UpdateTriggersElement = root.Q<Label>("update-Triggers");
            m_WriteTransformsElement = root.Q<Label>("write-transforms");
        }
        
        // Counters.
        {
            m_BodyCountElement = root.Q<Label>("body-count");
            m_ShapeCountElement = root.Q<Label>("shape-count");
            m_JointCountElement = root.Q<Label>("joint-count");
            m_ContactCountElement = root.Q<Label>("contact-count");
            m_IslandCountElement = root.Q<Label>("island-count");
            m_StaticTreeHeightElement = root.Q<Label>("static-tree-height");
            m_MoveableTreeHeightElement = root.Q<Label>("moveable-tree-height");
            m_StackBytesUsedElement = root.Q<Label>("stack-bytes-used");
            m_TotalBytesUsedElement = root.Q<Label>("total-bytes-used");
            m_TaskCountElement = root.Q<Label>("task-count");
        }
        
        // Reset Stats.
        {
            var resetStats = root.Q<Button>("reset-stats");
            resetStats.clicked += ResetStats;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to the post-simulate event. 
        PhysicsEvents.PostSimulate -= UpdateStats;
    }

    private void Update()
    {
        var sampleScale = 1f / m_SampledCount;
        const float memoryScale = 1f / 1048576f;
        
        const string color = "<color=#7FFFD4>";
        const string endColor = "</color>";
        
        // Profile.
        m_SimulationStepElement.text = $"<b>Simulation Step</b>: {color}{m_LastProfile.simulationStep:F2}{endColor} ~[{color}{m_TotalProfile.simulationStep * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.simulationStep:F2}{endColor}]";
        m_ContactPairsElement.text = $"<b>Contact Pairs</b>: {color}{m_LastProfile.contactPairs:F2}{endColor} ~[{color}{m_TotalProfile.contactPairs * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.contactPairs:F2}{endColor}]";
        m_ContactUpdatesElement.text = $"<b>Contact Updates</b>: {color}{m_LastProfile.contactUpdates:F2}{endColor} ~[{color}{m_TotalProfile.contactUpdates * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.contactUpdates:F2}{endColor}]";
        m_SolvingElement.text = $"<b>Solving</b>: {color}{m_LastProfile.solving:F2}{endColor} ~[{color}{m_TotalProfile.solving * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.solving:F2}{endColor}]";
        m_MergeIslandsElement.text = $"<b>Merge Islands</b>: {color}{m_LastProfile.mergeIslands:F2}{endColor} ~[{color}{m_TotalProfile.mergeIslands * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.mergeIslands:F2}{endColor}]";
        m_PrepareStagesElement.text = $"<b>Prepare Stages</b>: {color}{m_LastProfile.prepareStages:F2}{endColor} ~[{color}{m_TotalProfile.prepareStages * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.prepareStages:F2}{endColor}]";
        m_PrepareConstraintsElement.text = $"<b>Prepare Constraints</b>: {color}{m_LastProfile.prepareConstraints:F2}{endColor} ~[{color}{m_TotalProfile.prepareConstraints * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.prepareConstraints:F2}{endColor}]";
        m_SolveConstraintsElement.text = $"<b>Solve Constraints</b>: {color}{m_LastProfile.solveConstraints:F2}{endColor} ~[{color}{m_TotalProfile.solveConstraints * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.solveConstraints:F2}{endColor}]";
        m_IntegrateVelocitiesElement.text = $"<b>Integrate Velocities</b>: {color}{m_LastProfile.integrateVelocities:F2}{endColor} ~[{color}{m_TotalProfile.integrateVelocities * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.integrateVelocities:F2}{endColor}]";
        m_WarmStartingElement.text = $"<b>Warm Starting</b>: {color}{m_LastProfile.warmStarting:F2}{endColor} ~[{color}{m_TotalProfile.warmStarting * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.warmStarting:F2}{endColor}]";
        m_SolveImpulsesElement.text = $"<b>Solve Impulses</b>: {color}{m_LastProfile.solveImpulses:F2}{endColor} ~[{color}{m_TotalProfile.solveImpulses * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.solveImpulses:F2}{endColor}]";
        m_IntegrateTransformsElement.text = $"<b>Integrate Transforms</b>: {color}{m_LastProfile.integrateTransforms:F2}{endColor} ~[{color}{m_TotalProfile.integrateTransforms * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.integrateTransforms:F2}{endColor}]";
        m_RelaxImpulsesElement.text = $"<b>Relax Impulses</b>: {color}{m_LastProfile.relaxImpulses:F2}{endColor} ~[{color}{m_TotalProfile.relaxImpulses * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.relaxImpulses:F2}{endColor}]";
        m_ApplyBouncinessElement.text = $"<b>Apply Bounciness</b>: {color}{m_LastProfile.applyBounciness:F2}{endColor} ~[{color}{m_TotalProfile.applyBounciness * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.applyBounciness:F2}{endColor}]";
        m_StoreImpulsesElement.text = $"<b>Store Impulses</b>: {color}{m_LastProfile.storeImpulses:F2}{endColor} ~[{color}{m_TotalProfile.storeImpulses * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.storeImpulses:F2}{endColor}]";
        m_SplitIslandsElement.text = $"<b>Split Islands</b>: {color}{m_LastProfile.splitIslands:F2}{endColor} ~[{color}{m_TotalProfile.splitIslands * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.splitIslands:F2}{endColor}]";
        m_BodyTransformsElement.text = $"<b>PhysicsBody Transforms</b>: {color}{m_LastProfile.bodyTransforms:F2}{endColor} ~[{color}{m_TotalProfile.bodyTransforms * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.bodyTransforms:F2}{endColor}]";
        m_JointEventsElement.text = $"<b>Joint Events</b>: {color}{m_LastProfile.jointEvents:F2}{endColor} ~[{color}{m_TotalProfile.jointEvents * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.jointEvents:F2}{endColor}]";
        m_HitEventsElement.text = $"<b>Hit Events</b>: {color}{m_LastProfile.hitEvents:F2}{endColor} ~[{color}{m_TotalProfile.hitEvents * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.hitEvents:F2}{endColor}]";
        m_BroadphaseUpdatesElement.text = $"<b>Broadphase Updates</b>: {color}{m_LastProfile.broadphaseUpdates:F2}{endColor} ~[{color}{m_TotalProfile.broadphaseUpdates * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.broadphaseUpdates:F2}{endColor}]";
        m_SolveContinuousElement.text = $"<b>Solve Continuous</b>: {color}{m_LastProfile.solveContinuous:F2}{endColor} ~[{color}{m_TotalProfile.solveContinuous * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.solveContinuous:F2}{endColor}]";
        m_SleepIslandsElement.text = $"<b>Sleep Islands</b>: {color}{m_LastProfile.sleepIslands:F2}{endColor} ~[{color}{m_TotalProfile.sleepIslands * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.sleepIslands:F2}{endColor}]";
        m_UpdateTriggersElement.text = $"<b>Update Triggers</b>: {color}{m_LastProfile.updateTriggers:F2}{endColor} ~[{color}{m_TotalProfile.updateTriggers * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.updateTriggers:F2}{endColor}]";
        m_WriteTransformsElement.text = $"<b>Write Transforms</b>: {color}{m_LastProfile.writeTransforms:F2}{endColor} ~[{color}{m_TotalProfile.writeTransforms * sampleScale:F2}{endColor}] >[{color}{m_MaxProfile.writeTransforms:F2}{endColor}]";

        
        // Counters.
        m_BodyCountElement.text = $"<b>Bodies</b>: {color}{m_LastCounters.bodyCount}{endColor} >[{color}{m_MaxCounters.bodyCount}{endColor}]";
        m_ShapeCountElement.text = $"<b>Shapes</b>: {color}{m_LastCounters.shapeCount}{endColor} >[{color}{m_MaxCounters.shapeCount}{endColor}]";
        m_ContactCountElement.text = $"<b>Contacts</b>: {color}{m_LastCounters.contactCount}{endColor} >[{color}{m_MaxCounters.contactCount}{endColor}]";
        m_JointCountElement.text = $"<b>Joints</b>: {color}{m_LastCounters.jointCount}{endColor} >[{color}{m_MaxCounters.jointCount}{endColor}]";
        m_IslandCountElement.text = $"<b>Island</b>: {color}{m_LastCounters.islandCount}{endColor} >[{color}{m_MaxCounters.islandCount}{endColor}]";
        m_StaticTreeHeightElement.text = $"<b>Static Tree</b>: {color}{m_LastCounters.staticTreeHeight}{endColor} >[{color}{m_MaxCounters.staticTreeHeight}{endColor}] <color=#696969>Height</color>";
        m_MoveableTreeHeightElement.text = $"<b>Moveable Tree</b>: {color}{m_LastCounters.treeHeight}{endColor} >[{color}{m_MaxCounters.treeHeight}{endColor}] <color=#696969>Height</color>"; 
        m_StackBytesUsedElement.text = $"<b>Stack Memory</b>: {color}{m_LastCounters.stackUsed * memoryScale:F2}{endColor} >[{color}{m_MaxCounters.stackUsed * memoryScale:F2}{endColor}] <color=#696969>MB</color>";
        m_TotalBytesUsedElement.text = $"<b>Total Memory</b>: {color}{m_LastCounters.memoryUsed * memoryScale:F2}{endColor} >[{color}{m_MaxCounters.memoryUsed * memoryScale:F2}{endColor}] <color=#696969>MB</color>";
        m_TaskCountElement.text = $"<b>Tasks</b>: {color}{m_LastCounters.taskCount}{endColor} >[{color}{m_MaxCounters.taskCount}{endColor}]";
    }
    
    private unsafe void UpdateStats(PhysicsWorld world, float deltaTime)
    {
        if (!world.isDefaultWorld)
            return;

        // Bump the sample count.
        ++m_SampledCount;
        
        // Profile.
        {
            m_LastProfile = PhysicsWorld.globalProfile;
            
            var profileFloatCount = sizeof(PhysicsWorld.WorldProfile) / sizeof(float);
            fixed (float* pTotalProfile = &m_TotalProfile.simulationStep)
            {
                fixed (float* pMaxProfile = &m_MaxProfile.simulationStep)
                {
                    fixed (float* pWorldProfile = &m_LastProfile.simulationStep)
                    {
                        for (var n = 0; n < profileFloatCount; ++n)
                        {
                            var profileValue = pWorldProfile[n];
                            pTotalProfile[n] += profileValue;
                            pMaxProfile[n] = math.max(profileValue, pMaxProfile[n]);
                        }
                    }
                }
            }
        }

        // Counters.
        {
            m_LastCounters = PhysicsWorld.globalCounters;
            
            var countersIntCount = sizeof(PhysicsWorld.WorldCounters) / sizeof(int);
            fixed (int* pTotalCounters = &m_TotalCounters.bodyCount)
            {
                fixed (int* pMaxCounters = &m_MaxCounters.bodyCount)
                {
                    fixed (int* pWorldCounters = &m_LastCounters.bodyCount)
                    {
                        for (var n = 0; n < countersIntCount; ++n)
                        {
                            var countersValue = pWorldCounters[n];
                            pTotalCounters[n] += countersValue;
                            pMaxCounters[n] = math.max(countersValue, pMaxCounters[n]);
                        }
                    }
                }
            }
        }
    }
}
