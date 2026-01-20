using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.U2D.Physics.Extras;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class SceneOutlineShapeTool
    {
        /// <summary>
        /// Abstract Geometry Tool.
        /// </summary>
        private abstract class SceneOutlineShapeGeometryEditorTool : SceneGeometryTool
        {
            protected SceneOutlineShapeGeometryEditorTool(SceneOutlineShape sceneOutlineShape, IGeometryToolSettings geometryToolSettings) : base(geometryToolSettings)
            {
                Target = sceneOutlineShape;

                UpdateTool();
            }

            protected readonly SceneOutlineShape Target;
            protected PhysicsBody Body;
            protected PhysicsWorld World;
            protected PhysicsWorld.TransformPlane TransformPlane;
            protected bool TargetShapeChanged;

            public sealed override bool UpdateTool()
            {
                Body = Target.SceneBody.Body;
                World = Target.SceneBody.SceneWorld.World;
                TransformPlane = World.transformPlane;
                return true;
            }

            /// <summary>
            /// Check the conditions of the target to ensure it's valid to edit or not.
            /// </summary>
            /// <returns>If the target is valid to edit or not.</returns>
            public override bool isValid => Target != null && Target.isActiveAndEnabled && !Mathf.Approximately(Vector3.Scale(PhysicsMath.GetTranslationAxes(World.transformPlane), Target.transform.lossyScale).sqrMagnitude, 0.0f);
        }
    }
}