using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        public const int MaxLayers = 32;
        public const int VertexBucketCount2 = (1 << 8);
        public const int MaxRemEdges = 48;

        public const int ExpectedLayersPerTile = 4;
        public const int NotConnected = 0x3f;
        /// <summary>
        /// A magic number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int Magic = 'D' << 24 | 'N' << 16 | 'A' << 8 | 'V';
        /// <summary>
        /// A version number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int Version = 7;
        public const int VertsPerPolygon = 6;
        public const int DT_VERTS_PER_POLYGON = 6;
        public const int NullIdx = 0xffff;

        /// <summary>
        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        /// </summary>
        public const int DT_EXT_LINK = 0x8000;
        /// <summary>
        /// A value that indicates the entity does not link to anything.
        /// </summary>
        public const int DT_NULL_LINK = int.MaxValue;
        /// <summary>
        /// A flag that indicates that an off-mesh connection can be traversed in both directions. (Is bidirectional.)
        /// </summary>
        public const int DT_OFFMESH_CON_BIDIR = 1;

        public const int RC_BORDER_REG = 0x8000;

        public const int RC_NULL_NEI = 0xffff;
        public const int RC_CONTOUR_REG_MASK = 0xffff;
        public const int RC_AREA_BORDER = 0x20000;
        public const int RC_BORDER_VERTEX = 0x10000;
        public const int RC_MESH_NULL_IDX = 0xffff;
        public const int VERTEX_BUCKET_COUNT = (1 << 12);
        public const int RC_MULTIPLE_REGS = 0;
        public const int RC_UNSET_HEIGHT = 0xffff;
        /// <summary>
        /// The number of spans allocated per span spool.
        /// </summary>
        public const int RC_SPANS_PER_POOL = 2048;
    }
}
