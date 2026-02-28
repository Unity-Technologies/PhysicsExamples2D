namespace Unity.U2D.Physics.Extras
{
    public static class ExecutionOrder
    {
        public const int TestWorld = -10000;
        public const int TestBody = TestWorld + 1;
        public const int TestShape = TestBody + 1;
        public const int TestJoint = TestBody + 1;
    }
}