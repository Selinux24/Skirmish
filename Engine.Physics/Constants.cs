using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Physics constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Gravity
        /// </summary>
        public static Vector3 GravityForce { get; set; } = new Vector3(0f, -20f, 0f);
        /// <summary>
        /// Gravity for fast ballistics
        /// </summary>
        public static Vector3 FastProyectileGravityForce
        {
            get
            {
                return GravityForce * 0.05f;
            }
        }
        /// <summary>
        /// Gravity for zero mass bodies
        /// </summary>
        public static Vector3 ZeroMassGravityForce
        {
            get
            {
                return Vector3.Zero;
            }
        }
        /// <summary>
        /// Sleep epsilon
        /// </summary>
        public static float SleepEpsilon { get; set; } = 0.5f;
        /// <summary>
        /// Contact orientation factor
        /// </summary>
        public static float OrientationContactFactor { get; set; } = 0.0f;
        /// <summary>
        /// Linear damping
        /// </summary>
        public static float LinearDamping { get; set; } = 0.99f;
        /// <summary>
        /// Angular damping
        /// </summary>
        public static float AngularDamping { get; set; } = 0.8f;
        /// <summary>
        /// Zero tolerance value
        /// </summary>
        public static float ZeroTolerance { get; set; } = MathUtil.ZeroTolerance;
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static Vector3 ZeroToleranceVector { get; set; } = new Vector3(MathUtil.ZeroTolerance);
    }
}
