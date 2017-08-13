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

        /// <summary>
        /// Gets the text representation of the item
        /// </summary>
        /// <returns>Returns the text representation of the item</returns>
        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }
    }
}
