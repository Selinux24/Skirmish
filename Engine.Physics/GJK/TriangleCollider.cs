using SharpDX;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// Triangle
    /// </summary>
    /// <remarks>
    /// Triangle: Kind of a hack 
    ///  "All physics code is an awful hack" - Will, #HandmadeDev
    /// Need to fake a prism for GJK to converge
    /// NB: Currently using world-space points, ignore matRS and pos from base class
    /// Don't use EPA with this! Might resolve collision along any one of prism's faces
    /// Only resolve around triangle normal
    /// </remarks>
    public struct TriangleCollider : ICollider
    {
        public Vector3 Point1 { get; set; } = Vector3.Zero;
        public Vector3 Point2 { get; set; } = Vector3.Zero;
        public Vector3 Point3 { get; set; } = Vector3.Zero;
        public Vector3 Normal { get; set; } = Vector3.Zero;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public TriangleCollider()
        {

        }

        public TriangleCollider(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 normal)
        {
            Point1 = point1;
            Point1 = point2;
            Point1 = point3;
            Normal = normal;
        }

        public TriangleCollider(Triangle tri)
        {
            Point1 = tri.Point1;
            Point1 = tri.Point2;
            Point1 = tri.Point3;
            Normal = tri.Normal;
        }

        public Vector3 Support(Vector3 dir)
        {
            //Find which triangle vertex is furthest along dir
            float dot0 = Vector3.Dot(Point1, dir);
            float dot1 = Vector3.Dot(Point2, dir);
            float dot2 = Vector3.Dot(Point3, dir);
            Vector3 furthest_point = Point1;
            if (dot1 > dot0)
            {
                furthest_point = Point2;
                if (dot2 > dot1)
                    furthest_point = Point3;
            }
            else if (dot2 > dot0)
            {
                furthest_point = Point3;
            }

            //fake some depth behind triangle so we have volume
            if (Vector3.Dot(dir, Normal) < 0)
            {
                furthest_point -= Normal;
            }

            return furthest_point;
        }
    }
}
