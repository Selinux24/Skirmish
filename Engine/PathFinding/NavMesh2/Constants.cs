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
    }
}
