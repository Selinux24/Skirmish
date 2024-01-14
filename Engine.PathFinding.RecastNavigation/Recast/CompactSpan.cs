using System.Collections.Generic;

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
        const int RC_NOT_CONNECTED = 0x3f;
        /// <summary>
        /// Maximum connectio layers
        /// </summary>
        public const int MaxLayers = RC_NOT_CONNECTED - 1;

        /// <summary>
        /// Default compact span
        /// </summary>
        public static CompactSpan Default
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
        /// Iterates over the specified span connections
        /// </summary>
        /// <param name="cs">Compact span</param>
        public readonly IEnumerable<(int dir, int con)> IterateSpanConnections()
        {
            for (int dir = 0; dir < 4; dir++)
            {
                if (!GetCon(dir, out int con))
                {
                    continue;
                }

                yield return (dir, con);
            }
        }

        /// <summary>
        /// Gets the connection index in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="con">Returns the connection index</param>
        /// <returns>Returns true if connected</returns>
        public readonly bool GetCon(int dir, out int con)
        {
            int shift = dir * 6;
            con = (Con >> shift) & RC_NOT_CONNECTED;
            return con != RC_NOT_CONNECTED;
        }
        /// <summary>
        /// Sets a connection in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="i">Connection index</param>
        public void SetCon(int dir, int i)
        {
            int shift = dir * 6;
            Con = (Con & ~(RC_NOT_CONNECTED << shift)) | ((i & RC_NOT_CONNECTED) << shift);
        }
        /// <summary>
        /// Disconnects the span in the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        public void Disconnect(int dir)
        {
            SetCon(dir, RC_NOT_CONNECTED);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Lower Extent {Y}; Region {Reg}; Connection {Con}; Height {H};";
        }
    }
}
