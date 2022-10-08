using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Proximity grid
    /// </summary>
    public class ProximityGrid<T> where T : class
    {
        private static int NextPow2(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }
        private static int HashPos2(int x, int y, int n)
        {
            return ((x * 73856093) ^ (y * 19349663)) & (n - 1);
        }

        private readonly float m_cellSize;
        private readonly float m_invCellSize;
        private readonly int m_bucketsSize;
        private readonly int[] m_buckets;
        private readonly int m_poolSize;
        private readonly ProximityGridItem<T>[] m_pool;
        private int m_poolHead;
        private RectangleF m_bounds;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="poolSize">Node pool size</param>
        /// <param name="cellSize">Cell size</param>
        public ProximityGrid(int poolSize, float cellSize)
        {
            m_cellSize = cellSize;
            m_invCellSize = 1.0f / m_cellSize;

            // Allocate hashs buckets
            m_bucketsSize = NextPow2(poolSize);
            m_buckets = Helper.CreateArray(m_bucketsSize, int.MaxValue);

            // Allocate pool of items.
            m_poolSize = poolSize;
            m_pool = new ProximityGridItem<T>[m_poolSize];
            m_poolHead = 0;

            m_bounds = new RectangleF()
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue
            };
        }

        /// <summary>
        /// Adds an item to the grid
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        public void AddItem(T item, Vector3 position, float radius)
        {
            Vector2 min = new Vector2(position.X - radius, position.Z - radius);
            Vector2 max = new Vector2(position.X + radius, position.Z + radius);

            m_bounds = new RectangleF()
            {
                Left = Math.Min(m_bounds.Left, min.X),
                Top = Math.Min(m_bounds.Top, min.Y),
                Right = Math.Max(m_bounds.Right, max.X),
                Bottom = Math.Max(m_bounds.Bottom, max.Y)
            };

            int iminx = (int)Math.Floor(min.X * m_invCellSize);
            int iminy = (int)Math.Floor(min.Y * m_invCellSize);
            int imaxx = (int)Math.Floor(max.X * m_invCellSize);
            int imaxy = (int)Math.Floor(max.Y * m_invCellSize);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    if (m_poolHead < m_poolSize)
                    {
                        int h = HashPos2(x, y, m_bucketsSize);
                        int idx = m_poolHead++;

                        m_pool[idx] = new ProximityGridItem<T>()
                        {
                            Item = item,
                            X = x,
                            Y = y,
                            RealPosition = position,
                            Radius = radius,
                            Next = m_buckets[h],
                        };

                        m_buckets[h] = idx;
                    }
                }
            }
        }
        /// <summary>
        /// Clears the grid
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < m_bucketsSize; i++)
            {
                m_buckets[i] = int.MaxValue;
            }

            m_poolHead = 0;
            for (int i = 0; i < m_poolSize; i++)
            {
                m_pool[i] = null;
            }

            m_bounds = new RectangleF()
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue
            };
        }
        /// <summary>
        /// Query the items at position by range
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="range">Range</param>
        /// <returns>Returns the in range item list</returns>
        public IEnumerable<T> QueryItems(Vector3 position, float range)
        {
            return QueryItems(position, range, out _);
        }
        /// <summary>
        /// Query the items at position by range
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="range">Range</param>
        /// <param name="items">Resulting grid items</param>
        /// <returns>Returns the in range item list</returns>
        public IEnumerable<T> QueryItems(Vector3 position, float range, out IEnumerable<ProximityGridItem<T>> items)
        {
            Vector2 min = new Vector2(position.X - range, position.Z - range);
            Vector2 max = new Vector2(position.X + range, position.Z + range);

            List<T> itemList = new List<T>();
            List<ProximityGridItem<T>> pItemList = new List<ProximityGridItem<T>>();

            int iminx = (int)Math.Floor(min.X * m_invCellSize);
            int iminy = (int)Math.Floor(min.Y * m_invCellSize);
            int imaxx = (int)Math.Floor(max.X * m_invCellSize);
            int imaxy = (int)Math.Floor(max.Y * m_invCellSize);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    int h = HashPos2(x, y, m_bucketsSize);
                    int idx = m_buckets[h];

                    while (idx != int.MaxValue)
                    {
                        var item = m_pool[idx];

                        if (item.X == x && item.Y == y && !itemList.Contains(item.Item))
                        {
                            itemList.Add(item.Item);
                            pItemList.Add(item);
                        }

                        idx = item.Next;
                    }
                }
            }

            items = pItemList.ToArray();

            return itemList.ToArray();
        }
        /// <summary>
        /// Gets the item count at proximity grid coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Returns the number of items</returns>
        public int GetItemCountAt(int x, int y)
        {
            int n = 0;

            int h = HashPos2(x, y, m_bucketsSize);
            int idx = m_buckets[h];

            while (idx != int.MaxValue)
            {
                var item = m_pool[idx];

                if (item.X == x && item.Y == y)
                {
                    n++;
                }

                idx = item.Next;
            }

            return n;
        }
        /// <summary>
        /// Gets the proximity grid bounds
        /// </summary>
        /// <returns>Returns a rectangle</returns>
        public RectangleF GetBounds()
        {
            return m_bounds;
        }
        /// <summary>
        /// Gets the proximity grid size
        /// </summary>
        /// <returns>Returns the size</returns>
        public float GetCellSize()
        {
            return m_cellSize;
        }
    }
}
