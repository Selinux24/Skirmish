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

        private readonly int maxSimplePath;

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
        /// <param name="max">Maximum path count</param>
        public SimplePath(int max)
        {
            maxSimplePath = max;

            this.Path = new int[maxSimplePath];
            this.Count = 0;
        }


        public void StartPath(int r)
        {
            Path[0] = r;
            Count = 1;
        }

        public void Concatenate(SimplePath visited, int furthestPath, int furthestVisited)
        {
            // Adjust beginning of the buffer to include the visited.
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, Count);
            int size = Math.Max(0, Count - orig);
            if (req + size > maxSimplePath)
            {
                size = maxSimplePath - req;
            }
            if (size > 0)
            {
                Array.Copy(Path, orig, Path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                Path[i] = visited.Path[visited.Count - 1 - i];
            }

            Count = req + size;
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

        public void Merge(IEnumerable<int> path, int count)
        {
            // Make space for the old path.
            if (count - 1 + Count > maxSimplePath)
            {
                Count = maxSimplePath - (count - 1);
            }

            // Copy old path in the beginning.
            List<int> tmp = new List<int>(Path);
            tmp.InsertRange(0, path);
            Path = tmp.ToArray();
            Count += count - 1;

            // Remove trackbacks
            for (int j = 0; j < Count; ++j)
            {
                if (j - 1 >= 0 && j + 1 < Count)
                {
                    bool samePoly = Path[j - 1] == Path[j + 1];
                    if (samePoly)
                    {
                        Array.ConstrainedCopy(Path, j + 1, Path, j - 1, Count - (j + 1));
                        Count -= 2;
                        j -= 2;
                    }
                }
            }
        }

        public void Prune(int npos)
        {
            for (int i = npos; i < Count; ++i)
            {
                Path[i - npos] = Path[i];
            }

            Count -= npos;
        }

        public void Clear()
        {
            Count = 0;
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
