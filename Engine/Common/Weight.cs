
namespace Engine.Common
{
    /// <summary>
    /// Weight info
    /// </summary>
    public struct Weight
    {
        /// <summary>
        /// Vertex index of this weight
        /// </summary>
        public int VertexIndex { get; set; }
        /// <summary>
        /// Joint name
        /// </summary>
        public string Joint { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        public float WeightValue { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"VertexIndex: {VertexIndex}; Joint: {Joint}; Weight: {WeightValue};";
        }
    }
}
