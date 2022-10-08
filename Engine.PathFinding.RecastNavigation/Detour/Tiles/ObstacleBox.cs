using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Axis aligned box obstacle
    /// </summary>
    public struct ObstacleBox : IObstacle
    {
        /// <summary>
        /// Box
        /// </summary>
        public BoundingBox Box { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        public ObstacleBox(BoundingBox bbox)
        {
            Box = bbox;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Box minimum</param>
        /// <param name="max">Box maximum</param>
        public ObstacleBox(Vector3 min, Vector3 max)
        {
            Box = new BoundingBox(min, max);
        }

        /// <summary>
        /// Gets the obstacle bounds
        /// </summary>
        /// <returns>Returns a bounding box</returns>
        public BoundingBox GetBounds()
        {
            return Box;
        }
        /// <summary>
        /// Marks the build context area with the specified area type
        /// </summary>
        /// <param name="tc">Build context</param>
        /// <param name="orig">Origin</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        /// <param name="area">Area type</param>
        /// <returns>Returns true if all layer areas were marked</returns>
        public bool MarkArea(NavMeshTileBuildContext tc, Vector3 orig, float cs, float ch, AreaTypes area)
        {
            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            var bounds = ComputeBounds(orig, w, h, ics, ich);
            if (!bounds.HasValue)
            {
                return true;
            }

            var min = bounds.Value.Min;
            var max = bounds.Value.Max;

            for (int z = min.Z; z <= max.Z; ++z)
            {
                for (int x = min.X; x <= max.X; ++x)
                {
                    int y = tc.Layer.Heights[x + z * w];
                    if (y < min.Y || y > max.Y)
                    {
                        continue;
                    }

                    tc.Layer.Areas[x + z * w] = area;
                }
            }

            return true;
        }
        private BoundingBoxInt? ComputeBounds(Vector3 orig, int w, int h, float ics, float ich)
        {
            int minx = (int)Math.Floor((Box.Minimum.X - orig.X) * ics);
            int miny = (int)Math.Floor((Box.Minimum.Y - orig.Y) * ich);
            int minz = (int)Math.Floor((Box.Minimum.Z - orig.Z) * ics);
            int maxx = (int)Math.Floor((Box.Maximum.X - orig.X) * ics);
            int maxy = (int)Math.Floor((Box.Maximum.Y - orig.Y) * ich);
            int maxz = (int)Math.Floor((Box.Maximum.Z - orig.Z) * ics);

            if (maxx < 0) return null;
            if (minx >= w) return null;
            if (maxz < 0) return null;
            if (minz >= h) return null;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            return new BoundingBoxInt
            {
                Min = new Vector3Int(minx, miny, minz),
                Max = new Vector3Int(maxx, maxy, maxz),
            };
        }
    }
}
