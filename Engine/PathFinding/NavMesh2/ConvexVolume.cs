using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    public struct ConvexVolume
    {
        public const int MaxConvexVolPoints = 12;

        public Vector3[] verts;
        public float hmin;
        public float hmax;
        public int nverts;
        public TileCacheAreas area;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Area {0}; Min {1} Max {2} Verts {3} -> {4}",
                area, hmin, hmax, nverts,
                verts != null ? string.Join(" ", verts) : "");
        }
    }
}
