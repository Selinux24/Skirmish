using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area base class
    /// </summary>
    public abstract class GraphArea : IGraphArea
    {
        /// <summary>
        /// Id counter
        /// </summary>
        private static int ID = 1;
        /// <summary>
        /// Gets the next id
        /// </summary>
        /// <returns>Returns the next id</returns>
        private static int GetNextId()
        {
            return ID++;
        }

        /// <inheritdoc/>
        public int Id { get; private set; }
        /// <inheritdoc/>
        public GraphAreaTypes AreaType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected GraphArea()
        {
            Id = GetNextId();
        }

        /// <inheritdoc/>
        public abstract BoundingBox GetBounds();
    }
}
