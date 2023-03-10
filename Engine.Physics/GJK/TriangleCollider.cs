using SharpDX;
using System;

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
        public Vector3[] Points { get; set; } = Array.Empty<Vector3>();
        public Vector3 Normal { get; set; } = Vector3.Zero;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public TriangleCollider()
        {

        }

        public Vector3 Support(Vector3 dir)
        {
            //Find which triangle vertex is furthest along dir
            float dot0 = Vector3.Dot(Points[0], dir);
            float dot1 = Vector3.Dot(Points[1], dir);
            float dot2 = Vector3.Dot(Points[2], dir);
            Vector3 furthest_point = Points[0];
            if (dot1 > dot0)
            {
                furthest_point = Points[1];
                if (dot2 > dot1)
                    furthest_point = Points[2];
            }
            else if (dot2 > dot0)
            {
                furthest_point = Points[2];
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
