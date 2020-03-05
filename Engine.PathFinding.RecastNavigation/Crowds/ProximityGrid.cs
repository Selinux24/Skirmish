using System;

namespace Engine.PathFinding.RecastNavigation.Crowds
{
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

        struct Item
        {
            public int Id { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public int Next { get; set; }
        };

        private float m_cellSize;
        private float m_invCellSize;
        private Item[] m_pool;
        private int m_poolHead;
        private int m_poolSize;
        private int[] m_buckets;
        private int m_bucketsSize;
        private readonly int[] m_bounds = new int[4];


        public ProximityGrid()
        {

        }

        public bool Init(int poolSize, float cellSize)
        {
            m_cellSize = cellSize;
            m_invCellSize = 1.0f / m_cellSize;

            // Allocate hashs buckets
            m_bucketsSize = NextPow2(poolSize);
            m_buckets = new int[m_bucketsSize];

            // Allocate pool of items.
            m_poolSize = poolSize;
            m_poolHead = 0;
            m_pool = new Item[m_poolSize];

            Clear();

            return true;
        }
        public void Clear()
        {
            m_buckets = new int[m_bucketsSize];
            m_poolHead = 0;
            m_bounds[0] = 0xffff;
            m_bounds[1] = 0xffff;
            m_bounds[2] = -0xffff;
            m_bounds[3] = -0xffff;
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
                        m_buckets[h] = idx;
                    }
                }
            }
        }
        public int QueryItems(float minx, float miny, float maxx, float maxy, int maxIds, out int[] ids)
        {
            ids = new int[maxIds];

            int iminx = (int)Math.Floor(minx * m_invCellSize);
            int iminy = (int)Math.Floor(miny * m_invCellSize);
            int imaxx = (int)Math.Floor(maxx * m_invCellSize);
            int imaxy = (int)Math.Floor(maxy * m_invCellSize);

            int n = 0;

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    int h = HashPos2(x, y, m_bucketsSize);
                    int idx = m_buckets[h];
                    while (idx != 0xffff)
                    {
                        Item item = m_pool[idx];
                        if ((int)item.X == x && (int)item.Y == y)
                        {
                            // Check if the id exists already.
                            int end = Array.IndexOf(ids, n);
                            int i = 0;
                            while (i != end && i != item.Id)
                            {
                                ++i;
                            }
                            // Item not found, add it.
                            if (i == end)
                            {
                                if (n >= maxIds)
                                    return n;
                                ids[n++] = item.Id;
                            }
                        }
                        idx = item.Next;
                    }
                }
            }

            return n;
        }
        public int GetItemCountAt(int x, int y)
        {
            int n = 0;

            int h = HashPos2(x, y, m_bucketsSize);
            int idx = m_buckets[h];
            while (idx != 0xffff)
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
            return m_bounds;
        }
        public float GetCellSize()
        {
            return m_cellSize;
        }
    }
}
