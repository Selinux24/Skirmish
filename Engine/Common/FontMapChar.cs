
namespace Engine.Common
{
    /// <summary>
    /// Font map character
    /// </summary>
    public struct FontMapChar
    {
        /// <summary>
        /// X map coordinate
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y map coordinate
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Character map width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Character map height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets text representation of character map
        /// </summary>
        /// <returns>Returns text representation of character map</returns>
        public override string ToString()
        {
            return string.Format("X: {0}; Y: {1}; Width: {2}; Height: {3}", this.X, this.Y, this.Width, this.Height);
        }
    }
}
