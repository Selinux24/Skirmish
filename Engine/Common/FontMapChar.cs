
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
        public float X { get; set; }
        /// <summary>
        /// Y map coordinate
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// Character map width
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        /// Character map height
        /// </summary>
        public float Height { get; set; }

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
