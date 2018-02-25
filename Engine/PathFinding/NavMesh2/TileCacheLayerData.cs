using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    public struct TileCacheLayerData
    {
        public static TileCacheLayerData Empty
        {
            get
            {
                return new TileCacheLayerData()
                {
                    heights = null,
                    areas = null,
                    cons = null,
                };
            }
        }

        public byte[] heights;
        public TileCacheAreas[] areas;
        public byte[] cons;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.heights == null && this.areas == null && this.cons == null)
            {
                return "Empty;";
            }

            return string.Format("Heights {0}; Areas {1}; Connections {2};",
                this.heights?.Count(i => i != 0xff),
                this.areas?.Count(i => i != TileCacheAreas.NullArea),
                this.cons?.Count(i => i != 0x00));
        }
    }
}
