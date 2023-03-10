using SharpDX;

namespace Engine.Physics.GJK
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


        Vector3 Support(Vector3 dir);
    }
}
