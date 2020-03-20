using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using SharpDX;

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
        /// Fix ups corridor
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="npath">Current path size</param>
        /// <param name="maxPath">Maximum path size</param>
        /// <param name="visited">Visted references</param>
        /// <param name="nvisited">Number of visited references</param>
        /// <returns>Returns the new size of the path</returns>
        public static void FixupCorridor(SimplePath path, int maxPath, SimplePath visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = path.Count - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path.Path[i] == visited.Path[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, path.Count);
            int size = Math.Max(0, path.Count - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size != 0)
            {
                Array.Copy(path.Path, orig, path.Path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path.Path[i] = visited.Path[visited.Count - 1 - i];
            }

            path.Count = req + size;
        }
        /// <summary>
        /// This function checks if the path has a small U-turn, that is,
        /// a polygon further in the path is adjacent to the first polygon
        /// in the path. If that happens, a shortcut is taken.
        /// This can happen if the target (T) location is at tile boundary,
        /// and we're (S) approaching it parallel to the tile edge.
        /// The choice at the vertex can be arbitrary, 
        ///  +---+---+
        ///  |:::|:::|
        ///  +-S-+-T-+
        ///  |:::|   | -- the step can end up in here, resulting U-turn path.
        ///  +---+---+
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="npath">Current path size</param>
        /// <param name="navQuery">Navigation query</param>
        /// <returns>Returns the new size of the path</returns>
        public static void FixupShortcuts(SimplePath path, NavMeshQuery navQuery)
        {
            if (path.Count < 3)
            {
                return;
            }

            // Get connected polygons
            int maxNeis = 16;
            List<int> neis = new List<int>();

            if (navQuery.GetAttachedNavMesh().GetTileAndPolyByRef(path.Start, out MeshTile tile, out Poly poly))
            {
                return;
            }

            for (int k = poly.FirstLink; k != DetourUtils.DT_NULL_LINK; k = tile.Links[k].Next)
            {
                var link = tile.Links[k];
                if (link.NRef != 0 && neis.Count < maxNeis)
                {
                    neis.Add(link.NRef);
                }
            }

            // If any of the neighbour polygons is within the next few polygons
            // in the path, short cut to that polygon directly.
            int maxLookAhead = 6;
            int cut = 0;
            for (int i = Math.Min(maxLookAhead, path.Count) - 1; i > 1 && cut == 0; i--)
            {
                for (int j = 0; j < neis.Count; j++)
                {
                    if (path.Path[i] == neis[j])
                    {
                        cut = i;
                        break;
                    }
                }
            }
            if (cut > 1)
            {
                int offset = cut - 1;
                path.Cut(offset);
            }
        }

        public static void MergeCorridorStartMoved(SimplePath path, int maxPath, SimplePath visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = path.Count - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path.Path[i] == visited.Path[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, path.Count);
            int size = Math.Max(0, path.Count - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size > 0)
            {
                Array.ConstrainedCopy(path.Path, orig, path.Path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path.Path[i] = visited.Path[visited.Count - 1 - i];
            }

            path.Count = req + size;
        }

        public static void MergeCorridorEndMoved(SimplePath path, int maxPath, SimplePath visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = 0; i < path.Count; ++i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path.Path[i] == visited.Path[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return;
            }

            // Concatenate paths.
            int ppos = furthestPath + 1;
            int vpos = furthestVisited + 1;
            int count = Math.Min(visited.Count - vpos, maxPath - ppos);
            if (count > 0)
            {
                Array.ConstrainedCopy(path.Path, ppos, visited.Path, vpos, count);
            }

            path.Count = ppos + count;
        }

        public static void MergeCorridorStartShortcut(SimplePath path, int maxPath, SimplePath visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = path.Count - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path.Path[i] == visited.Path[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = furthestVisited;
            if (req <= 0)
            {
                return;
            }

            int orig = furthestPath;
            int size = Math.Max(0, path.Count - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size > 0)
            {
                Array.ConstrainedCopy(path.Path, orig, path.Path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path.Path[i] = visited.Path[i];
            }

            path.Count = req + size;
        }

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
        public int Count { get; private set; }

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
        public void StartPath(IEnumerable<int> rlist)
        {
            StartPath(rlist, rlist.Count());
        }
        /// <summary>
        /// Adds a reference list to the current path
        /// </summary>
        /// <param name="rlist">Reference list</param>
        /// <param name="count">Number of elements of the reference list</param>
        public void StartPath(IEnumerable<int> rlist, int count)
        {
            int maxPath = Path.Length;
            int[] newPath = new int[maxPath];

            Array.Copy(rlist.ToArray(), 0, newPath, 0, count);

            Path = newPath;
            Count = count;
        }
        /// <summary>
        /// Adds a reference to the list
        /// </summary>
        /// <param name="r">Reference to add</param>
        public bool Add(int r)
        {
            if (Count >= maxSimplePath)
            {
                return false;
            }

            Path[Count++] = r;

            return true;
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
        /// Fix path start
        /// </summary>
        /// <param name="safeRef">Safe reference</param>
        public void FixStart(int safeRef)
        {
            if (Count < 3 && Count > 0)
            {
                Path[0] = safeRef;
                Path[1] = 0;
                Path[2] = End;
                Count = 3;
            }
            else
            {
                Path[0] = safeRef;
                Path[1] = 0;
            }
        }
        /// <summary>
        /// Sets the path length
        /// </summary>
        /// <param name="length">New length</param>
        public void SetLength(int length)
        {
            Count = length;
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
