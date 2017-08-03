using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
	/// A <see cref="ProximityGrid{T}"/> is a uniform 2d grid that can efficiently retrieve items near a specified grid cell.
	/// </summary>
	/// <typeparam name="T">An equatable type.</typeparam>
    public class ProximityGrid<T>
    {
        private const int Invalid = -1;

        /// <summary>
        /// An "item" is simply a coordinate on the proximity grid
        /// </summary>
        class Item
        {
            public T Value { get; set; }

            public int X { get; set; }

            public int Y { get; set; }

            public int Next { get; set; }
        }

        private float cellSize;
        private float invCellSize;
        private Item[] pool;
        private int poolHead;
        private int[] buckets;
        private BoundingRectanglei bounds;

        /// <summary>
        /// Hash function
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        /// <param name="n">Total size of hash table</param>
        /// <returns>A hash value</returns>
        public static int HashPos2(int x, int y, int n)
        {
            return ((x * 73856093) ^ (y * 19349663)) & (n - 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximityGrid{T}"/> class.
        /// </summary>
        /// <param name="poolSize">The size of the item array</param>
        /// <param name="cellSize">The size of each cell</param>
        public ProximityGrid(int poolSize, float cellSize)
        {
            this.cellSize = cellSize;
            this.invCellSize = 1.0f / cellSize;

            //allocate hash buckets
            this.buckets = new int[Helper.NextPowerOfTwo(poolSize)];

            //allocate pool of items
            this.pool = new Item[poolSize];
            for (int i = 0; i < this.pool.Length; i++)
            {
                this.pool[i] = new Item();
            }

            this.Clear();
        }

        /// <summary>
        /// Reset all the data
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < this.buckets.Length; i++)
            {
                this.buckets[i] = Invalid;
            }

            this.poolHead = 0;

            this.bounds = new BoundingRectanglei(Vector2i.Max, Vector2i.Min);
        }
        /// <summary>
        /// Take all the coordinates within a certain range and add them all to an array
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minX">Minimum x-coordinate</param>
        /// <param name="minY">Minimum y-coordinate</param>
        /// <param name="maxX">Maximum x-coordinate</param>
        /// <param name="maxY">Maximum y-coordinate</param>
        public void AddItem(T value, float minX, float minY, float maxX, float maxY)
        {
            int invMinX = (int)Math.Floor(minX * invCellSize);
            int invMinY = (int)Math.Floor(minY * invCellSize);
            int invMaxX = (int)Math.Floor(maxX * invCellSize);
            int invMaxY = (int)Math.Floor(maxY * invCellSize);

            this.bounds.Min.X = Math.Min(this.bounds.Min.X, invMinX);
            this.bounds.Min.Y = Math.Min(this.bounds.Min.Y, invMinY);
            this.bounds.Max.X = Math.Max(this.bounds.Max.X, invMaxX);
            this.bounds.Max.Y = Math.Max(this.bounds.Max.Y, invMaxY);

            for (int y = invMinY; y <= invMaxY; y++)
            {
                for (int x = invMinX; x <= invMaxX; x++)
                {
                    if (this.poolHead < this.pool.Length)
                    {
                        int h = HashPos2(x, y, this.buckets.Length);
                        int idx = this.poolHead;
                        this.poolHead++;
                        this.pool[idx].X = x;
                        this.pool[idx].Y = y;
                        this.pool[idx].Value = value;
                        this.pool[idx].Next = this.buckets[h];
                        this.buckets[h] = idx;
                    }
                }
            }
        }
        /// <summary>
        /// Adds the value to the array
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="r">Radius</param>
        /// <param name="p">Position</param>
        public void AddItem(T value, float r, Vector2 p)
        {
            this.AddItem(value, p.X - r, p.Y - r, p.X + r, p.Y + r);
        }
        /// <summary>
        /// Adds the value to the array
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="r">Radius</param>
        /// <param name="p">Position</param>
        public void AddItem(T value, float r, Vector3 p)
        {
            this.AddItem(value, p.X - r, p.Z - r, p.X + r, p.Z + r);
        }
        /// <summary>
        /// Take all the items within a certain range and add their ids to an array.
        /// </summary>
        /// <param name="minX">The minimum x-coordinate</param>
        /// <param name="minY">The minimum y-coordinate</param>
        /// <param name="maxX">The maximum x-coordinate</param>
        /// <param name="maxY">The maximum y-coordinate</param>
        /// <param name="values">The array of values</param>
        /// <param name="maxVals">The maximum number of values that can be stored</param>
        /// <returns>The number of unique values</returns>
        public int QueryItems(float minX, float minY, float maxX, float maxY, T[] values, int maxVals)
        {
            int invMinX = (int)Math.Floor(minX * invCellSize);
            int invMinY = (int)Math.Floor(minY * invCellSize);
            int invMaxX = (int)Math.Floor(maxX * invCellSize);
            int invMaxY = (int)Math.Floor(maxY * invCellSize);

            int n = 0;

            for (int y = invMinY; y <= invMaxY; y++)
            {
                for (int x = invMinX; x <= invMaxX; x++)
                {
                    int h = HashPos2(x, y, this.buckets.Length);
                    int idx = this.buckets[h];

                    while (idx != Invalid)
                    {
                        if (this.pool[idx].X == x && this.pool[idx].Y == y)
                        {
                            //check if the id exists already
                            int i = 0;
                            while (i != n && !values[i].Equals(this.pool[idx].Value))
                            {
                                i++;
                            }

                            //item not found, add it
                            if (i == n)
                            {
                                if (n >= maxVals)
                                {
                                    return n;
                                }

                                values[n++] = this.pool[idx].Value;
                            }
                        }

                        idx = this.pool[idx].Next;
                    }
                }
            }

            return n;
        }

        public int QueryItems(Vector2 pos, float range, T[] values, int maxVals)
        {
            return this.QueryItems(pos.X - range, pos.Y - range, pos.X + range, pos.Y + range, values, maxVals);
        }

        public int QueryItems(Vector3 pos, float range, T[] values, int maxVals)
        {
            return this.QueryItems(pos.X - range, pos.Z - range, pos.X + range, pos.Z + range, values, maxVals);
        }
        /// <summary>
        /// Gets the number of items at a specific location.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The number of items at the specified coordinates.</returns>
        public int GetItemCountAtLocation(int x, int y)
        {
            int n = 0;
            int h = HashPos2(x, y, this.buckets.Length);
            int idx = buckets[h];

            while (idx != Invalid)
            {
                var item = this.pool[idx];
                if (item.X == x && item.Y == y)
                {
                    n++;
                }
                idx = item.Next;
            }

            return n;
        }
    }
}
