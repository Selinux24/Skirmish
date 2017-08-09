namespace Engine.Collections
{
    /// <summary>
    /// An "item" is simply a coordinate on the proximity grid
    /// </summary>
    class ProximityGridItem<T>
    {
        /// <summary>
        /// Item value
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// X index value
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y index value
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Next index
        /// </summary>
        public int Next { get; set; }
    }
}
