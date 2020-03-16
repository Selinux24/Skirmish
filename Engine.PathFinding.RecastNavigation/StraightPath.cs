using SharpDX;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using System;

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

        public void RemoveFirst()
        {
            Count--;
            if (Count > 0)
            {
                Array.ConstrainedCopy(Flags, 1, Flags, 0, Count);
                Array.ConstrainedCopy(Refs, 1, Refs, 0, Count);
                Array.ConstrainedCopy(Path, 1, Path, 0, Count);
            }

            Flags[Count] = 0;
            Refs[Count] = 0;
            Path[Count] = Vector3.Zero;
        }
    }
}
