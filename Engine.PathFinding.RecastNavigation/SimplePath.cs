using System;
using System.Collections.Generic;
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
        public int[] Path { get; private set; }
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


        public void AddRange(IEnumerable<int> path)
        {
            AddRange(path, path.Count());
        }

        public void AddRange(IEnumerable<int> path, int count)
        {
            int maxPath = Path.Length;
            int[] newPath = new int[maxPath];

            Array.Copy(path.ToArray(), 0, newPath, 0, count);

            Path = newPath;
            Count = count;
        }

        public void Insert(int index, IEnumerable<int> path, int count)
        {
            List<int> tmp = new List<int>(Path);
            tmp.InsertRange(index, path);
            Path = tmp.ToArray();
            Count += count - 1;
        }

        /// <summary>
        /// Copies the instance
        /// </summary>
        /// <returns>Returns a new instance</returns>
        public SimplePath Copy(int maxPath = 0)
        {
            if (maxPath == 0)
            {
                return new SimplePath(Path.Length)
                {
                    Path = Path.ToArray(),
                    Count = Count,
                };
            }

            if (Path.Length > maxPath)
            {
                return new SimplePath(maxPath)
                {
                    Path = Path.Take(maxPath).ToArray(),
                    Count = Count,
                };
            }
            else
            {
                int[] tmp = new int[maxPath];
                Array.Copy(Path, 0, tmp, 0, maxPath);

                return new SimplePath(maxPath)
                {
                    Path = tmp,
                    Count = Count,
                };
            }
        }
    }
}
