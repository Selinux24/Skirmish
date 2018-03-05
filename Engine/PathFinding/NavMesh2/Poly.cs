
using System;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Defines a polygon within a dtMeshTile object.
    /// </summary>
    public class Poly
    {
        /// <summary>
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        /// </summary>
        public int firstLink;
        /// <summary>
        /// The indices of the polygon's vertices. The actual vertices are located in dtMeshTile::verts.
        /// </summary>
        public Polygoni verts = new Polygoni(Constants.VertsPerPolygon);
        /// <summary>
        /// Packed data representing neighbor polygons references and flags for each edge.
        /// </summary>
        public int[] neis = new int[Constants.VertsPerPolygon];
        /// <summary>
        /// The user defined polygon flags.
        /// </summary>
        public SamplePolyFlags flags;
        /// <summary>
        /// The number of vertices in the polygon.
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The bit packed area id and polygon type.
        /// </summary>
        private uint areaAndtype;

        public SamplePolyAreas Area
        {
            get
            {
                return (SamplePolyAreas)(areaAndtype & 0x3f);
            }
            set
            {
                areaAndtype = (areaAndtype & 0xc0) | ((uint)value & 0x3f);
            }
        }

        public PolyTypes Type
        {
            get
            {
                return (PolyTypes)(areaAndtype >> 6);
            }
            set
            {
                areaAndtype = (areaAndtype & 0x3f) | ((uint)value << 6);
            }
        }

        public override string ToString()
        {
            return string.Format("FirstLink {0}; Flags {1}; Area: {2}; Type: {3}; Verts {4}; VertCount: {5}; Neis: {6}",
                firstLink, flags, Area, Type, verts, vertCount, neis?.Join(","));
        }
    }
}
