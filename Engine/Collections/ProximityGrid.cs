using SharpDX;
using System;

namespace Engine.Collections
{
    /// <summary>
	/// A <see cref="ProximityGrid{T}"/> is a uniform 2d grid that can efficiently retrieve items near a specified grid cell.
	/// </summary>
	/// <typeparam name="T">An equatable type.</typeparam>
    public class ProximityGrid<T>
    {
        /// <summary>
        /// Hash function for Vector2
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        /// <param name="n">Total size of hash table</param>
        /// <returns>A hash value</returns>
        static int HashVector2(int x, int y, int n)
        {
            return ((x * 73856093) ^ (y * 19349663)) & (n - 1);
        }

        private readonly float cellSize;
        private readonly float inverseCellSize;
        private readonly ProximityGridItem<T>[] pool;
        private int poolHead;
        private readonly int[] buckets;
        private BoundingRectanglei bounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximityGrid{T}"/> class.
        /// </summary>
        /// <param name="poolSize">The size of the item array</param>
        /// <param name="cellSize">The size of each cell</param>
        public ProximityGrid(int poolSize, float cellSize)
        {
            this.cellSize = cellSize;
            this.inverseCellSize = 1.0f / cellSize;

            //allocate hash buckets
            this.buckets = new int[Helper.NextPowerOfTwo(poolSize)];

            //allocate pool of items
            this.pool = new ProximityGridItem<T>[poolSize];
            for (int i = 0; i < this.pool.Length; i++)
            {
                this.pool[i] = new ProximityGridItem<T>();
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
                this.buckets[i] = -1;
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
            int invMinX = (int)Math.Floor(minX * inverseCellSize);
            int invMinY = (int)Math.Floor(minY * inverseCellSize);
            int invMaxX = (int)Math.Floor(maxX * inverseCellSize);
            int invMaxY = (int)Math.Floor(maxY * inverseCellSize);

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
                        int h = HashVector2(x, y, this.buckets.Length);
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
        public int QueryItems(float minX, float minY, float maxX, float maxY, int maxVals, out T[] values)
        {
            values = new T[maxVals];

            int invMinX = (int)Math.Floor(minX * this.inverseCellSize);
            int invMinY = (int)Math.Floor(minY * this.inverseCellSize);
            int invMaxX = (int)Math.Floor(maxX * this.inverseCellSize);
            int invMaxY = (int)Math.Floor(maxY * this.inverseCellSize);

            int n = 0;

            for (int y = invMinY; y <= invMaxY; y++)
            {
                for (int x = invMinX; x <= invMaxX; x++)
                {
                    int hash = HashVector2(x, y, this.buckets.Length);
                    int idx = this.buckets[hash];

                    while (idx >= 0)
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
        /// <summary>
        /// Take all the items within a certain range and add their ids to an array.
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="range">Range around x & y values</param>
        /// <param name="values">The array of values</param>
        /// <param name="maxVals">The maximum number of values that can be stored</param>
        /// <returns>The number of unique values</returns>
        public int QueryItems(Vector2 pos, float range, int maxVals, out T[] values)
        {
            return this.QueryItems(pos.X - range, pos.Y - range, pos.X + range, pos.Y + range, maxVals, out values);
        }
        /// <summary>
        /// Take all the items within a certain range and add their ids to an array.
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="range">Range around x & z values</param>
        /// <param name="values">The array of values</param>
        /// <param name="maxVals">The maximum number of values that can be stored</param>
        /// <returns>The number of unique values</returns>
        public int QueryItems(Vector3 pos, float range, int maxVals, out T[] values)
        {
            return this.QueryItems(pos.X - range, pos.Z - range, pos.X + range, pos.Z + range, maxVals, out values);
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
            int h = HashVector2(x, y, this.buckets.Length);
            int idx = buckets[h];

            while (idx >= 0)
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
