using SharpDX;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// Sphere
    /// </summary>
    public struct SphereCollider : ICollider
    {
        public float R { get; set; } = 0;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public SphereCollider()
        {

        }

        public Vector3 Support(Vector3 dir)
        {
            return Vector3.Normalize(dir) * R + Position;
        }
    }
}
