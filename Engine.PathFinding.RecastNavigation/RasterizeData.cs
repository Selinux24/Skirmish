
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Rasterize data
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="smin">Span minimum</param>
    /// <param name="smax">Span maximum</param>
    /// <param name="area">Area</param>
    /// <param name="flagMergeThr">Flag merge threshold</param>
    public struct RasterizeData(int x, int y, int smin, int smax, AreaTypes area, int flagMergeThr)
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public int X { get; set; } = x;
        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y { get; set; } = y;
        /// <summary>
        /// Span minimum
        /// </summary>
        public int SMin { get; set; } = smin;
        /// <summary>
        /// Span maximum
        /// </summary>
        public int SMax { get; set; } = smax;
        /// <summary>
        /// Area
        /// </summary>
        public AreaTypes Area { get; set; } = area;
        /// <summary>
        /// Flag merge threshold
        /// </summary>
        public int FlagMergeThr { get; set; } = flagMergeThr;
    }
}
