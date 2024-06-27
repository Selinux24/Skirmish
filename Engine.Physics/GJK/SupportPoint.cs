using SharpDX;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// GJK simplex support point
    /// </summary>
    public struct SupportPoint
    {
        /// <summary>
        /// First collider support point
        /// </summary>
        public Vector3 Support1 { get; set; }
        /// <summary>
        /// Second collider support point
        /// </summary>
        public Vector3 Support2 { get; set; }
        /// <summary>
        /// Simplex point
        /// </summary>
        public Vector3 Point { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SupportPoint(ICollider coll1, ICollider coll2, Vector3 searchDir)
        {
            Support1 = coll1.Support(-searchDir);
            Support2 = coll2.Support(searchDir);
            Point = Support2 - Support1;
        }
    }
}
