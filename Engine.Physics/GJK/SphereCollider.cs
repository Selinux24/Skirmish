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
        public SphereCollider(float r)
        {
            R = r;
        }

        public Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result = Vector3.Normalize(dir) * R;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}
