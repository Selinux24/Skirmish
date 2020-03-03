using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Simple path
    /// </summary>
    public class SimplePath
    {
        /// <summary>
        /// Maximum nodes
        /// </summary>
        public const int MaxSimplePath = 256;

        /// <summary>
        /// Polygon reference list
        /// </summary>
        public int[] Path { get; set; }
        /// <summary>
        /// Polygon count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimplePath() : this(MaxSimplePath)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSimplePath">Maximum path count</param>
        public SimplePath(int maxSimplePath)
        {
            this.Path = new int[maxSimplePath];
            this.Count = 0;
        }

        /// <summary>
        /// Copies the instance
        /// </summary>
        /// <returns>Returns a new instance</returns>
        public SimplePath Copy()
        {
            return new SimplePath
            {
                Path = Path.ToArray(),
                Count = Count,
            };
        }
    }
}
