using UnityEngine;
using Unity.U2D.Physics;
using Unity.U2D.Physics.Extras;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class SceneChainTool
    {
        /// <summary>
        /// Abstract Geometry Tool.
        /// </summary>
        private abstract class SceneChainGeometryEditorTool : SceneGeometryTool
        {
            protected SceneChainGeometryEditorTool(SceneChain sceneChain, IGeometryToolSettings geometryToolSettings) : base(geometryToolSettings)
            {
                Target = sceneChain;

                UpdateTool();
            }

            protected readonly SceneChain Target;
            protected PhysicsChain Chain;
            protected PhysicsBody Body;
            protected PhysicsWorld World;
            protected PhysicsWorld.TransformPlane TransformPlane;
            protected bool TargetShapeChanged;

            public sealed override bool UpdateTool()
            {
                if (Chain.isValid)
                    return true;

                Chain = Target.ChainShape;
                if (!Chain.isValid)
                    return true;

                Body = Chain.body;
                World = Chain.world;
                TransformPlane = World.transformPlane;
                return true;
            }

            /// <summary>
            /// Check the conditions of the target to ensure it's valid to edit or not.
            /// </summary>
            /// <returns>If the target is valid to edit or not.</returns>
            public override bool isValid => Target != null && Chain.isValid && Target.isActiveAndEnabled && !Mathf.Approximately(Vector3.Scale(PhysicsMath.GetTranslationAxes(World.transformPlane), Target.transform.lossyScale).sqrMagnitude, 0.0f);
        }
    }
}