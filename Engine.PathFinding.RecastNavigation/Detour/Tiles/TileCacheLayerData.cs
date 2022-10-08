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

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Heights == null && this.Areas == null && this.Connections == null)
            {
                return "Empty;";
            }

            return string.Format("Heights {0}; Areas {1}; Connections {2};",
                this.Heights?.Count(i => i != 0xff),
                this.Areas?.Count(i => i != AreaTypes.RC_NULL_AREA),
                this.Connections?.Count(i => i != 0x00));
        }
    }
}
