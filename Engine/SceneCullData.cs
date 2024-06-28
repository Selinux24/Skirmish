
namespace Engine
{
    /// <summary>
    /// Cull data
    /// </summary>
    public struct SceneCullData
    {
        /// <summary>
        /// Empty cull data
        /// </summary>
        public static SceneCullData Empty
        {
            get
            {
                return new()
                {
                    Culled = false,
                    Distance = float.MaxValue,
                };
            }
        }

        /// <summary>
        /// Cull flag. If true, the item is culled
        /// </summary>
        public bool Culled { get; set; }
        /// <summary>
        /// Distance from point of view when the item is'nt culled
        /// </summary>
        public float Distance { get; set; }
    }
}
