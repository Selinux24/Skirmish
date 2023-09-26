using Engine.PathFinding.RecastNavigation.Recast;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Layer data
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCacheLayerData
    {
        /// <summary>
        /// Empty layer data
        /// </summary>
        public static TileCacheLayerData Empty
        {
            get
            {
                return new TileCacheLayerData()
                {
                    Heights = null,
                    Areas = null,
                    Connections = null,
                };
            }
        }

        /// <summary>
        /// Height list
        /// </summary>
        public int[] Heights;
        /// <summary>
        /// Area types list
        /// </summary>
        public AreaTypes[] Areas;
        /// <summary>
        /// Connection list
        /// </summary>
        public int[] Connections;

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            if (Heights == null && Areas == null && Connections == null)
            {
                return "Empty;";
            }

            return $"Heights {Heights?.Count(i => i != 0xff)}; Areas {Areas?.Count(i => i != AreaTypes.RC_NULL_AREA)}; Connections {Connections?.Count(i => i != 0x00)};";
        }
    }
}
