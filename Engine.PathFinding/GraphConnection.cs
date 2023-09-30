using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph connection
    /// </summary>
    public class GraphConnection : IGraphConnection
    {
        /// <summary>
        /// Id Counter
        /// </summary>
        private static int ID = 1000;
        /// <summary>
        /// Gets the next id
        /// </summary>
        /// <returns>Returns the next id</returns>
        private static int GetNextId()
        {
            return ID++;
        }

        /// <summary>
        /// Connection Id
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Start point
        /// </summary>
        public Vector3 Start { get; set; }
        /// <summary>
        /// End point
        /// </summary>
        public Vector3 End { get; set; }
        /// <summary>
        /// Points radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Connection direction
        /// </summary>
        public int Direction { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        public GraphConnectionAreaTypes AreaType { get; set; }
        /// <summary>
        /// Area flags
        /// </summary>
        public GraphConnectionFlagTypes FlagTypes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphConnection()
        {
            Id = GetNextId();
        }
    }
}
