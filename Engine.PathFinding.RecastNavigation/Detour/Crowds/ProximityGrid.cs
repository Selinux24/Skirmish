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
        struct Item
        {
            /// <summary>
            /// Item identifier
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// X position
            /// </summary>
            public float X { get; set; }
            /// <summary>
            /// Y position
            /// </summary>
            public float Y { get; set; }
            /// <summary>
            /// Next item in the pool
            /// </summary>
            public int Next { get; set; }
        };

        private readonly float m_cellSize;
        private readonly float m_invCellSize;
        private readonly Item[] m_pool;
        private int m_poolHead;
        private readonly int m_poolSize;
        private readonly int[] m_buckets;
        private readonly int m_bucketsSize;
        private readonly int[] m_bounds;

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

            m_bounds = new[]
            {
                int.MaxValue,
                int.MaxValue,
                int.MinValue,
                int.MinValue
            };
        }

        public void Clear()
        {
            for (int i = 0; i < m_bucketsSize; i++)
            {
                m_buckets[i] = int.MaxValue;
            }

            m_poolHead = 0;

            m_bounds[0] = int.MaxValue;
            m_bounds[1] = int.MaxValue;
            m_bounds[2] = int.MinValue;
            m_bounds[3] = int.MinValue;
        }
        public void AddItem(int id, float minx, float miny, float maxx, float maxy)
        {
            int iminx = (int)Math.Floor(minx * m_invCellSize);
            int iminy = (int)Math.Floor(miny * m_invCellSize);
            int imaxx = (int)Math.Floor(maxx * m_invCellSize);
            int imaxy = (int)Math.Floor(maxy * m_invCellSize);

            m_bounds[0] = Math.Min(m_bounds[0], iminx);
            m_bounds[1] = Math.Min(m_bounds[1], iminy);
            m_bounds[2] = Math.Max(m_bounds[2], imaxx);
            m_bounds[3] = Math.Max(m_bounds[3], imaxy);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    if (m_poolHead < m_poolSize)
                    {
                        int h = HashPos2(x, y, m_bucketsSize);
                        int idx = m_poolHead;
                        m_poolHead++;
                        Item item = m_pool[idx];
                        item.X = (short)x;
                        item.Y = (short)y;
                        item.Id = id;
                        item.Next = m_buckets[h];
                        m_pool[idx] = item;
                        m_buckets[h] = idx;
                    }
                }
            }
        }
        public int[] QueryItems(float minx, float miny, float maxx, float maxy)
        {
            List<int> ids = new List<int>();

            int iminx = (int)Math.Floor(minx * m_invCellSize);
            int iminy = (int)Math.Floor(miny * m_invCellSize);
            int imaxx = (int)Math.Floor(maxx * m_invCellSize);
            int imaxy = (int)Math.Floor(maxy * m_invCellSize);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    int h = HashPos2(x, y, m_bucketsSize);
                    int idx = m_buckets[h];

                    while (idx != int.MaxValue)
                    {
                        var item = m_pool[idx];

                        if ((int)item.X == x && (int)item.Y == y && !ids.Contains(item.Id))
                        {
                            ids.Add(item.Id);
                        }

                        idx = item.Next;
                    }
                }
            }

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
                if ((int)item.X == x && (int)item.Y == y)
                {
                    n++;
                }

                idx = item.Next;
            }

            return n;
        }
        public int[] GetBounds()
        {
            return m_bounds.ToArray();
        }
        public float GetCellSize()
        {
            return m_cellSize;
        }
    }
}
