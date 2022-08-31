using SharpDX;

namespace Engine.BuiltIn
{
    /// <summary>
    /// Drawer mesh state
    /// </summary>
    public struct BuiltInDrawerMeshState
    {
        /// <summary>
        /// Default state
        /// </summary>
        public static BuiltInDrawerMeshState Default()
        {
            return new BuiltInDrawerMeshState
            {
                Local = Matrix.Identity,
                AnimationOffset1 = 0,
                AnimationOffset2 = 0,
                AnimationInterpolationAmount = 0f,
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        public Matrix Local { get; set; }
        /// <summary>
        /// First offset in the animation palette
        /// </summary>
        public uint AnimationOffset1 { get; set; }
        /// <summary>
        /// Second offset in the animation palette
        /// </summary>
        public uint AnimationOffset2 { get; set; }
        /// <summary>
        /// Interpolation amount between the offsets
        /// </summary>
        public float AnimationInterpolationAmount { get; set; }
    }
}
