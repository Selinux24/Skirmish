
namespace Engine.UI
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
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}; Width: {Width}; Height: {Height}";
        }
    }
}
