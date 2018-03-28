using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        public static class Recast
        {
            /// <summary>
            /// Defines the number of bits allocated to rcSpan::smin and rcSpan::smax.
            /// </summary>
            public const int RC_SPAN_HEIGHT_BITS = 13;
            /// <summary>
            /// Defines the maximum value for rcSpan::smin and rcSpan::smax.
            /// </summary>
            public const int RC_SPAN_MAX_HEIGHT = (1 << RC_SPAN_HEIGHT_BITS) - 1;
            /// <summary>
            /// The number of spans allocated per span spool.
            /// </summary>
            public const int RC_SPANS_PER_POOL = 2048;
            /// <summary>
            /// Heighfield border flag.
            /// If a heightfield region ID has this bit set, then the region is a border 
            /// region and its spans are considered unwalkable.
            /// (Used during the region and contour build process.)
            /// </summary>
            public const int RC_BORDER_REG = 0x8000;
            /// <summary>
            /// Polygon touches multiple regions.
            /// If a polygon has this region ID it was merged with or created
            /// from polygons of different regions during the polymesh
            /// build step that removes redundant border vertices. 
            /// (Used during the polymesh and detail polymesh build processes)
            /// </summary>
            public const int RC_MULTIPLE_REGS = 0;
            /// <summary>
            /// Border vertex flag.
            /// If a region ID has this bit set, then the associated element lies on
            /// a tile border. If a contour vertex's region ID has this bit set, the 
            /// vertex will later be removed in order to match the segments and vertices 
            /// at tile boundaries.
            /// (Used during the build process.)
            /// </summary>
            public const int RC_BORDER_VERTEX = 0x10000;
            /// <summary>
            /// Area border flag.
            /// If a region ID has this bit set, then the associated element lies on
            /// the border of an area.
            /// (Used during the region and contour build process.)
            /// </summary>
            public const int RC_AREA_BORDER = 0x20000;
            /// <summary>
            /// Applied to the region id field of contour vertices in order to extract the region id.
            /// The region id field of a vertex may have several flags applied to it.  So the
            /// fields value can't be used directly.
            /// </summary>
            public const int RC_CONTOUR_REG_MASK = 0xffff;
            /// <summary>
            /// An value which indicates an invalid index within a mesh.
            /// </summary>
            public const int RC_MESH_NULL_IDX = 0xffff;
            /// <summary>
            /// The value returned by #rcGetCon if the specified direction is not connected
            /// to another span. (Has no neighbor.)
            /// </summary>
            public const int RC_NOT_CONNECTED = 0x3f;

            public const int RC_NULL_NEI = 0xffff;
            public const int RC_UNSET_HEIGHT = 0xffff;
        }

        public const int NULL_IDX = 0xffff;

        public const int MaxLayers = 32;
        public const int VertexBucketCount2 = (1 << 8);
        public const int MaxRemEdges = 48;
        public const int ExpectedLayersPerTile = 4;

        /// <summary>
        /// The maximum number of vertices per navigation polygon.
        /// </summary>
        public const int DT_VERTS_PER_POLYGON = 6;
        /// <summary>
        /// A magic number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int DT_NAVMESH_MAGIC = 'D' << 24 | 'N' << 16 | 'A' << 8 | 'V';
        /// <summary>
        /// A version number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int DT_NAVMESH_VERSION = 7;
        /// <summary>
        /// A magic number used to detect the compatibility of navigation tile states.
        /// </summary>
        public const int DT_NAVMESH_STATE_MAGIC = 'D' << 24 | 'N' << 16 | 'M' << 8 | 'S';
        /// <summary>
        /// A version number used to detect compatibility of navigation tile states.
        /// </summary>
        public const int DT_NAVMESH_STATE_VERSION = 1;
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
        /// <summary>
        /// The maximum number of user defined area ids.
        /// </summary>
        public const int DT_MAX_AREAS = 64;
        /// <summary>
        /// Limit raycasting during any angle pahfinding
        /// The limit is given as a multiple of the character radius
        /// </summary>
        public const float DT_RAY_CAST_LIMIT_PROPORTIONS = 50.0f;

        public const int DT_NODE_PARENT_BITS = 24;
        public const int DT_NODE_STATE_BITS = 2;
        public const int DT_MAX_STATES_PER_NODE = 1 << DT_NODE_STATE_BITS;  // number of extra states per node. See dtNode::state


        public const int VERTEX_BUCKET_COUNT = (1 << 12);
        public const int MAX_OFFMESH_CONNECTIONS = 256;
        public const int MAX_VOLUMES = 256;
        /// <summary>
        /// Search heuristic scale.
        /// </summary>
        public const float H_SCALE = 0.999f;
    }
}
