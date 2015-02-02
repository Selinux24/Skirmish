
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
        public int VertexIndex;
        /// <summary>
        /// Joint name
        /// </summary>
        public string Joint;
        /// <summary>
        /// Value
        /// </summary>
        public float WeightValue;

        /// <summary>
        /// Gets text representation of this weight
        /// </summary>
        /// <returns>Returns text representation of this weight</returns>
        public override string ToString()
        {
            string text = null;

            text += string.Format("VertexIndex: {0}; ", this.VertexIndex);
            text += string.Format("Joint: {0}; ", this.Joint);
            text += string.Format("Weight: {0}; ", this.WeightValue);

            return text;
        }
    }
}
