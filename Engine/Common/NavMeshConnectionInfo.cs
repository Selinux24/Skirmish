
namespace Engine.Common
{
    /// <summary>
    /// Polygon connection info
    /// </summary>
    public class NavMeshConnectionInfo
    {
        /// <summary>
        /// First node index
        /// </summary>
        public int First;
        /// <summary>
        /// Second node index
        /// </summary>
        public int Second;
        /// <summary>
        /// Connection segment
        /// </summary>
        public Line3 Segment;

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("First: {0}; Second: {1}; Segment: {2}", this.First, this.Second, this.Segment);
        }
    }
}
