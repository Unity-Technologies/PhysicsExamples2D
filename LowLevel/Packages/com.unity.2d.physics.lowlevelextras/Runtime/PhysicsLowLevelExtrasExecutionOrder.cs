namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    public static class PhysicsLowLevelExtrasExecutionOrder
    {
        public const int SceneWorld = -10000;
        public const int SceneBody = SceneWorld + 1;
        public const int SceneShape = SceneBody + 1;
        public const int SceneJoint = SceneBody + 1;
    }
}