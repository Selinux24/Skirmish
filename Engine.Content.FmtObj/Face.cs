
namespace Engine.Content.FmtObj
{
    /// <summary>
    /// Face
    /// </summary>
    struct Face
    {
        /// <summary>
        /// Position
        /// </summary>
        public uint Position { get; set; }
        /// <summary>
        /// Texture UV map
        /// </summary>
        public uint UV { get; set; }
        /// <summary>
        /// Normal
        /// </summary>
        public uint Normal { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Item list</param>
        public Face(uint[] items)
        {
            Position = items.Length > 0 ? items[0] : 0;
            UV = items.Length > 1 ? items[1] : 0;
            Normal = items.Length > 2 ? items[2] : 0;
        }

        /// <summary>
        /// Gets the position index
        /// </summary>
        /// <param name="offset">Index offset</param>
        /// <returns>Returns the based 0 index</returns>
        public int GetPositionIndex(int offset)
        {
            return (int)Position - 1 - offset;
        }
        /// <summary>
        /// Gets the Uv index
        /// </summary>
        /// <param name="offset">Index offset</param>
        /// <returns>Returns the based 0 index</returns>
        public int? GetUVIndex(int offset)
        {
            return UV != 0 ? (int)UV - 1 - offset : (int?)null;
        }
        /// <summary>
        /// Gets the normal index
        /// </summary>
        /// <param name="offset">Index offset</param>
        /// <returns>Returns the based 0 index</returns>
        public int? GetNormalIndex(int offset)
        {
            return Normal != 0 ? (int)Normal - 1 - offset : (int?)null;
        }

        /// <summary>
        /// Gets the text representation of the face
        /// </summary>
        public override string ToString()
        {
            return $"{Position}/{UV}/{Normal}";
        }
    }
}
