using SharpDX;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Cylinder: Height-aligned with y-axis (rotate using matRS)
    /// </summary>
    public struct CylinderCollider
    {
        public float R { get; set; } = 0;
        public float YBase { get; set; } = 0;
        public float YCap { get; set; } = 0;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public CylinderCollider()
        {

        }

        public Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 dir_xz = new Vector3(dir.X, 0, dir.Z);
            Vector3 result = Vector3.Normalize(dir_xz) * R;
            result.Y = dir.Y > 0 ? YCap : YBase;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}
