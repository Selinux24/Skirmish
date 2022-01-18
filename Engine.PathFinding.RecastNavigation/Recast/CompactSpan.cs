
namespace Engine.PathFinding.RecastNavigation.Recast
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
                    Con = 24,
                    H = 8,
                };
            }
        }

        /// <summary>
        /// The lower extent of the span. (Measured from the heightfield's base.)
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// The id of the region the span belongs to. (Or zero if not in a region.)
        /// </summary>
        public int Reg { get; set; }
        /// <summary>
        /// Packed neighbor connection data.
        /// </summary>
        public int Con { get; set; }
        /// <summary>
        /// The height of the span.  (Measured from #y.)
        /// </summary>
        public int H { get; set; }

        /// <summary>
        /// Sets a connection in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="i">Connection index</param>
        public void SetCon(int dir, int i)
        {
            int shift = dir * 6;
            int con = Con;
            Con = (con & ~(0x3f << shift)) | ((i & 0x3f) << shift);
        }
        /// <summary>
        /// Gets the connection index in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <returns>Returns the connection index</returns>
        public int GetCon(int dir)
        {
            int shift = dir * 6;
            return (Con >> shift) & 0x3f;
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return $"Lower Extent {Y}; Region {Reg}; Connection {Con}; Height {H};";
        }
    }
}
