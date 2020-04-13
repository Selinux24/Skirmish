
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Compact span
    /// </summary>
    public struct CompactSpan
    {
        /// <summary>
        /// The value returned by #rcGetCon if the specified direction is not connected
        /// to another span. (Has no neighbor.)
        /// </summary>
        public const int RC_NOT_CONNECTED = 0x3f;

        /// <summary>
        /// Default compact span
        /// </summary>
        public CompactSpan Default
        {
            get
            {
                return new CompactSpan()
                {
                    connection = 24,
                    H = 8,
                };
            }
        }

        /// <summary>
        /// Packed neighbor connection data.
        /// </summary>
        private int connection;

        /// <summary>
        /// The lower extent of the span. (Measured from the heightfield's base.)
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// The id of the region the span belongs to. (Or zero if not in a region.)
        /// </summary>
        public int Reg { get; set; }
        /// <summary>
        /// The height of the span.  (Measured from #y.)
        /// </summary>
        public int H { get; set; }

        /// <summary>
        /// Gets the connection data in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <returns>Returns the connection data</returns>
        public int GetCon(int dir)
        {
            int shift = dir * 6;
            int value = (connection >> shift) & RC_NOT_CONNECTED;
            return value;
        }
        /// <summary>
        /// Sets the connection data in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="value">Connection data</param>
        public void SetCon(int dir, int value)
        {
            int shift = dir * 6;
            int con = connection;
            connection = (con & ~(RC_NOT_CONNECTED << shift)) | ((value & RC_NOT_CONNECTED) << shift);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Lower Extent {0}; Region {1}; Connection {2}; Height {3};", this.Y, this.Reg, this.connection, this.H);
        }
    }
}
