using SharpDX;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// Box
    /// </summary>
    /// <remarks>
    /// Assume these are axis aligned!
    /// </remarks>
    public struct BoxCollider : ICollider
    {
        public Vector3 Min { get; set; } = Vector3.Zero;
        public Vector3 Max { get; set; } = Vector3.Zero;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public BoxCollider()
        {

        }
        public BoxCollider(Vector3 extents)
        {
            Min = -extents;
            Max = extents;
        }
        public BoxCollider(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result;
            result.X = (dir.X > 0) ? Max.X : Min.X;
            result.Y = (dir.Y > 0) ? Max.Y : Min.Y;
            result.Z = (dir.Z > 0) ? Max.Z : Min.Z;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}
