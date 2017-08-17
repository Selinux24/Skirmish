using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Navigation mesh Path
    /// </summary>
    public class PolygonPath
    {
        /// <summary>
        /// Polygons in the path
        /// </summary>
        private List<PolyId> polygons = new List<PolyId>();

        /// <summary>
        /// Gets or sets polygons in the path by index
        /// </summary>
        /// <param name="i">Polygon index</param>
        /// <returns>Returns the polygon if exists, or Null if not</returns>
        public PolyId this[int i]
        {
            get
            {
                return i < polygons.Count ? polygons[i] : PolyId.Null;
            }
            set
            {
                if (i < polygons.Count)
                {
                    polygons[i] = value;
                }
                else
                {
                    polygons.Insert(i, value);
                }
            }
        }
        /// <summary>
        /// Gets the polygon count in the path
        /// </summary>
        public int Count
        {
            get
            {
                return this.polygons.Count;
            }
        }
        /// <summary>
        /// Total path cost
        /// </summary>
        public float TotalCost { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PolygonPath()
        {
            this.TotalCost = 0;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="otherPath">Other path</param>
        public PolygonPath(PolygonPath otherPath)
            : this()
        {
            this.TotalCost = otherPath.TotalCost;

            this.polygons.AddRange(otherPath.polygons);
        }

        /// <summary>
        /// Add a new polygon to the path
        /// </summary>
        /// <param name="polygon">New polygon</param>
        public void Add(PolyId polygon)
        {
            polygons.Add(polygon);
        }
        /// <summary>
        /// Adds a polygon range to the path
        /// </summary>
        /// <param name="polygons">Polygon list</param>
        public void AddRange(IEnumerable<PolyId> polygons)
        {
            this.polygons.AddRange(polygons);
        }
        /// <summary>
        /// Remove polygon at index
        /// </summary>
        /// <param name="index">Index to remove</param>
        public void RemoveAt(int index)
        {
            this.polygons.RemoveAt(index);
        }
        /// <summary>
        /// Remove polygon range at index
        /// </summary>
        /// <param name="index">Start index</param>
        /// <param name="count">Polygon count to remove</param>
        public void RemoveRange(int index, int count)
        {
            polygons.RemoveRange(index, count);
        }
        /// <summary>
        /// Reverse the path
        /// </summary>
        public void Reverse()
        {
            this.polygons.Reverse();
        }
        /// <summary>
        /// Clears the path
        /// </summary>
        public void Clear()
        {
            this.TotalCost = 0;

            this.polygons.Clear();
        }

        /// <summary>
        /// Appends another path to current
        /// </summary>
        /// <param name="other">Other path</param>
        public void AppendPath(PolygonPath other)
        {
            this.polygons.AddRange(other.polygons);
        }
        /// <summary>
        /// Add cost to current
        /// </summary>
        /// <param name="cost">Cost value to add</param>
        public void AppendCost(float cost)
        {
            this.TotalCost += cost;
        }
        /// <summary>
        /// Remove current polygon list trackbacks
        /// </summary>
        public void RemoveTrackbacks()
        {
            for (int i = 0; i < this.polygons.Count; i++)
            {
                if (i - 1 >= 0 && i + 1 < this.polygons.Count)
                {
                    if (this.polygons[i - 1] == this.polygons[i + 1])
                    {
                        this.polygons.RemoveRange(i - 1, 2);
                        i -= 2;
                    }
                }
            }
        }
    }
}
