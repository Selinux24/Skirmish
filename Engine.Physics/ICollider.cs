using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Base interface for all collision shapes
    /// </summary>
    public interface ICollider
    {
        /// <summary>
        /// Origin in world space
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Rotation/scale component of model matrix
        /// </summary>
        Matrix RotationScale { get; set; }
        /// <summary>
        /// Inverse rotation/scale component of model matrix
        /// </summary>
        Matrix RotationScaleInverse { get; }

        /// <summary>
        /// Gets the support vector of the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        Vector3 Support(Vector3 dir);
    }
}
