using UnityEngine;

namespace Unity.U2D.Physics.Editor.Extras
{
    // Single ScriptableObject that backs every PropertyField the browser binds. One field per
    // bindable struct type; PropertyField only renders the bound property path so unused fields
    // are dormant.
    //
    // Lives in its own file matching the class name so Unity can locate the script asset and
    // suppresses the "No script asset for <type>. Check that the definition is in a file of the
    // same name." warning that fires when SerializedObject can't resolve m_Script.
    internal sealed class PhysicsInspectorHolder : ScriptableObject
    {
        public PhysicsWorldDefinition world;
        public PhysicsBodyDefinition body;
        public PhysicsShapeDefinition shape;
        public CircleGeometry circle;
        public CapsuleGeometry capsule;
        public PolygonGeometry polygon;
        public SegmentGeometry segment;
        public ChainSegmentGeometry chainSegment;
    }
}
