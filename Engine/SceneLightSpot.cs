using SharpDX;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot : SceneLight
    {
        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position = Vector3.Zero;
        /// <summary>
        /// Light range
        /// </summary>
        public float Range = 0.0f;
        /// <summary>
        /// Ligth direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Spot exponent used in the spotlight calculation to control the cone
        /// </summary>
        public float Spot = 0.0f;
        /// <summary>
        /// Stores the three attenuation constants in the format (a0, a1, a2) that control how light intensity falls off with distance
        /// </summary>
        /// <remarks>
        /// Constant weaken (1,0,0)
        /// Inverse distance weaken (0,1,0)
        /// Inverse square law (0,0,1)
        /// </remarks>
        public Vector3 Attenuation = Vector3.Zero;
    }
}
