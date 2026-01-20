namespace Unity.U2D.Physics.Extras
{
    public static class ExecutionOrder
    {
        public const int SceneWorld = -10000;
        public const int SceneBody = SceneWorld + 1;
        public const int SceneShape = SceneBody + 1;
        public const int SceneJoint = SceneBody + 1;
    }
}