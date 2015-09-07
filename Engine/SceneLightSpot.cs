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
        /// Ligth direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Spot exponent used in the spotlight calculation to control the cone
        /// </summary>
        public float Angle = 0.0f;
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius = 1f;
        /// <summary>
        /// Gets the bounding sphere of the active light
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                return new BoundingSphere(this.Position, this.Radius);
            }
        }
    }
}
