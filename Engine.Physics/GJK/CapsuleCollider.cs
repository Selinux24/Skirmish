using SharpDX;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// Capsule: Height-aligned with y-axis
    /// </summary>
    public struct CapsuleCollider : ICollider
    {
        public float R { get; set; } = 0;
        public float YBase { get; set; } = 0;
        public float YCap { get; set; } = 0;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public CapsuleCollider()
        {

        }

        public Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result = Vector3.Normalize(dir) * R;
            result.Y += (dir.Y > 0) ? YCap : YBase;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}
