using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Proximity grid
    /// </summary>
    public class ProximityGrid
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

        /// <summary>
        /// Grid pool item
        /// </summary>
        public class Item
        {
            /// <summary>
            /// Item identifier
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// X position
            /// </summary>
            public int X { get; set; }
            /// <summary>
            /// Y position
            /// </summary>
            public int Y { get; set; }
            /// <summary>
            /// Next item in the pool
            /// </summary>
            public int Next { get; set; }
            /// <summary>
            /// Real item position
            /// </summary>
            public Vector3 RealPosition { get; set; }
            /// <summary>
            /// Item radius
            /// </summary>
            public float Radius { get; set; }
        };

        private readonly float m_cellSize;
        private readonly float m_invCellSize;
        private int m_poolHead;
        private Item[] m_pool;
        private readonly int m_poolSize;
        private readonly int[] m_buckets;
        private readonly int m_bucketsSize;
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
            m_poolHead = 0;
            m_pool = new Item[m_poolSize];

            m_bounds = new RectangleF()
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue
            };
        }

        public void Clear()
        {
            for (int i = 0; i < m_bucketsSize; i++)
            {
                m_buckets[i] = int.MaxValue;
            }

            m_poolHead = 0;
            m_pool = new Item[m_poolSize];

            m_bounds = new RectangleF()
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue
            };
        }
        public void AddItem(int id, Vector3 position, float radius)
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

                        m_pool[idx] = new Item()
                        {
                            Id = id,
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
        public IEnumerable<int> QueryItems(Vector3 position, float range)
        {
            return QueryItems(position, range, out _);
        }
        public IEnumerable<int> QueryItems(Vector3 position, float range, out IEnumerable<Item> items)
        {
            Vector2 min = new Vector2(position.X - range, position.Z - range);
            Vector2 max = new Vector2(position.X + range, position.Z + range);

            List<int> ids = new List<int>();
            List<Item> itemList = new List<Item>();

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

                        if (item.X == x && item.Y == y && !ids.Contains(item.Id))
                        {
                            ids.Add(item.Id);
                            itemList.Add(item);
                        }

                        idx = item.Next;
                    }
                }
            }

            items = itemList.ToArray();

            return ids.ToArray();
        }
        public int GetItemCountAt(int x, int y)
        {
            int n = 0;

            int h = HashPos2(x, y, m_bucketsSize);
            int idx = m_buckets[h];

            while (idx != int.MaxValue)
            {
                Item item = m_pool[idx];

                if (item.X == x && item.Y == y)
                {
                    n++;
                }

                idx = item.Next;
            }

            return n;
        }
        public RectangleF GetBounds()
        {
            return m_bounds;
        }
        public float GetCellSize()
        {
            return m_cellSize;
        }
    }
}
