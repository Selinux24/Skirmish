using SharpDX;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Straight path
    /// </summary>
    public class StraightPath
    {
        /// <summary>
        /// Maximum nodes
        /// </summary>
        public const int MaxStraightPath = 256;

        /// <summary>
        /// Path position list
        /// </summary>
        public Vector3[] Path { get; set; }
        /// <summary>
        /// Path flags
        /// </summary>
        public StraightPathFlagTypes[] Flags { get; set; }
        /// <summary>
        /// Path references
        /// </summary>
        public int[] Refs { get; set; }
        /// <summary>
        /// Position count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StraightPath() : this(MaxStraightPath)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxStraightPath">Maximum path count</param>
        public StraightPath(int maxStraightPath)
        {
            this.Path = new Vector3[maxStraightPath];
            this.Flags = new StraightPathFlagTypes[maxStraightPath];
            this.Refs = new int[maxStraightPath];
            this.Count = 0;
        }

        /// <summary>
        /// Copies the instance
        /// </summary>
        /// <returns>Returns a new instance</returns>
        public StraightPath Copy()
        {
            return new StraightPath
            {
                Path = Path.ToArray(),
                Flags = Flags.ToArray(),
                Refs = Refs.ToArray(),
                Count = Count,
            };
        }
    }
}
