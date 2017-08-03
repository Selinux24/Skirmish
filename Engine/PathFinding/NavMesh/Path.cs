using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    public class Path
    {
        private List<PolyId> polys;
        private float cost;

        public PolyId this[int i]
        {
            get
            {
                return polys[i];
            }
            set
            {
                polys[i] = value;
            }
        }
        public int Count
        {
            get
            {
                return polys.Count;
            }
        }
        public float Cost
        {
            get
            {
                return cost;
            }
        }

        public Path()
        {
            polys = new List<PolyId>();
            cost = 0;
        }
        public Path(Path otherPath)
            : this()
        {
            polys.AddRange(otherPath.polys);
            cost = otherPath.Cost;
        }

        public void Clear()
        {
            polys.Clear();
            cost = 0;
        }
        public void Add(PolyId poly)
        {
            polys.Add(poly);
        }
        public void AddRange(IEnumerable<PolyId> polys)
        {
            this.polys.AddRange(polys);
        }
        public void AppendPath(Path other)
        {
            polys.AddRange(other.polys);
        }
        public void AddCost(float cost)
        {
            this.cost += cost;
        }
        public void Reverse()
        {
            polys.Reverse();
        }
        public void RemoveTrackbacks()
        {
            for (int i = 0; i < polys.Count; i++)
            {
                if (i - 1 >= 0 && i + 1 < polys.Count)
                {
                    if (polys[i - 1] == polys[i + 1])
                    {
                        polys.RemoveRange(i - 1, 2);
                        i -= 2;
                    }
                }
            }
        }
        public void FixupCorridor(List<PolyId> visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            //find furhtest common polygon
            bool found = false;
            for (int i = polys.Count - 1; i >= 0; i--)
            {
                for (int j = visited.Count - 1; j >= 0; j--)
                {
                    if (polys[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;
            }

            //no intersection in visited path
            if (furthestPath == -1 || furthestVisited == -1)
                return;

            //concatenate paths
            //adjust beginning of the buffer to include the visited
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, polys.Count);
            int size = Math.Max(0, polys.Count - orig);

            //remove everything before visited
            polys.RemoveRange(0, orig);

            //for (int i = 0; i < size; i++)
            //polys[req + i] = polys[orig + i];

            //store visited
            for (int i = 0; i < req; i++)
                //polys[i] = visited[(visited.Count - 1) - i];
                polys.Insert(i, visited[(visited.Count - 1) - i]);

            //return req + size;
            return;
        }
        public void RemoveAt(int index)
        {
            polys.RemoveAt(index);
        }
        public void RemoveRange(int index, int count)
        {
            polys.RemoveRange(index, count);
        }
    }
}
