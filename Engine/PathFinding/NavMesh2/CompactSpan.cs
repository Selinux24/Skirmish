
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Compact span
    /// </summary>
    public struct CompactSpan
    {
        /// <summary>
        /// Default compact span
        /// </summary>
        public CompactSpan Default
        {
            get
            {
                return new CompactSpan()
                {
                    con = 24,
                    h = 8,
                };
            }
        }

        /// <summary>
        /// The lower extent of the span. (Measured from the heightfield's base.)
        /// </summary>
        public ushort y;
        /// <summary>
        /// The id of the region the span belongs to. (Or zero if not in a region.)
        /// </summary>
        public ushort reg;
        /// <summary>
        /// Packed neighbor connection data.
        /// </summary>
        public uint con;
        /// <summary>
        /// The height of the span.  (Measured from #y.)
        /// </summary>
        public uint h;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Lower Extent {0}; Region {1}; Connection {2}; Height {3};", this.y, this.reg, this.con, this.h);
        }
    }
}
