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
        /// Maximum path elements
        /// </summary>
        private readonly int maxSimplePath;

        /// <summary>
        /// Polygon reference list
        /// </summary>
        public int[] Path { get; private set; }
        /// <summary>
        /// Start node value
        /// </summary>
        public int Start
        {
            get
            {
                return Count > 0 ? Path[0] : 0;
            }
        }
        /// <summary>
        /// End node value
        /// </summary>
        public int End
        {
            get
            {
                return Count > 0 ? Path[Count - 1] : 0;
            }
        }
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

        /// <summary>
        /// Starts the path from the specified reference
        /// </summary>
        /// <param name="r">Poly reference</param>
        public void StartPath(int r)
        {
            Path[0] = r;
            Count = 1;
        }
        /// <summary>
        /// Adds a reference list to the current path
        /// </summary>
        /// <param name="rlist">Reference list</param>
        public void AddRange(IEnumerable<int> rlist)
        {
            AddRange(rlist, rlist.Count());
        }
        /// <summary>
        /// Adds a reference list to the current path
        /// </summary>
        /// <param name="rlist">Reference list</param>
        /// <param name="count">Number of elements of the reference list</param>
        public void AddRange(IEnumerable<int> rlist, int count)
        {
            int maxPath = Path.Length;
            int[] newPath = new int[maxPath];

            Array.Copy(rlist.ToArray(), 0, newPath, 0, count);

            Path = newPath;
            Count = count;
        }
        /// <summary>
        /// Merges the specified reference list
        /// </summary>
        /// <param name="rlist">Reference list</param>
        /// <param name="count">Number of elements of the reference list</param>
        public void Merge(IEnumerable<int> rlist, int count)
        {
            // Make space for the old path.
            if (count - 1 + Count > maxSimplePath)
            {
                Count = maxSimplePath - (count - 1);
            }

            // Copy old path in the beginning.
            List<int> tmp = new List<int>(Path);
            tmp.InsertRange(0, rlist);
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
        /// <summary>
        /// Cuts the path
        /// </summary>
        /// <param name="offset">Cut offset</param>
        public void Cut(int offset)
        {
            Count -= offset;
            for (int i = 1; i < Count; i++)
            {
                Path[i] = Path[i + offset];
            }
        }
        /// <summary>
        /// Prune path at position
        /// </summary>
        /// <param name="npos">Position index</param>
        public void Prune(int npos)
        {
            for (int i = npos; i < Count; ++i)
            {
                Path[i - npos] = Path[i];
            }

            Count -= npos;
        }
        /// <summary>
        /// Clears the path
        /// </summary>
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
