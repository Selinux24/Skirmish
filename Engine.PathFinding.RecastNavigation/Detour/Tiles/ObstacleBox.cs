using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Axis aligned box obstacle
    /// </summary>
    public readonly struct ObstacleBox : IObstacle
    {
        /// <summary>
        /// Box
        /// </summary>
        private readonly BoundingBox bbox;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        public ObstacleBox(BoundingBox bbox)
        {
            this.bbox = bbox;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Box minimum</param>
        /// <param name="max">Box maximum</param>
        public ObstacleBox(Vector3 min, Vector3 max)
        {
            bbox = new BoundingBox(min, max);
        }

        /// <inheritdoc/>
        public readonly BoundingBox GetBounds()
        {
            return bbox;
        }
        /// <inheritdoc/>
        public bool MarkArea(TileCacheLayer layer, Vector3 orig, float cs, float ch, AreaTypes area)
        {
            int w = layer.Header.Width;
            int h = layer.Header.Height;
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
                    int y = layer.Heights[x + z * w];
                    if (y < min.Y || y > max.Y)
                    {
                        continue;
                    }

                    layer.Areas[x + z * w] = area;
                }
            }

            return true;
        }
        /// <summary>
        /// Computes the obstacle bounds
        /// </summary>
        /// <param name="orig">Origin</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="ics">Cell size</param>
        /// <param name="ich">Cell height</param>
        private BoundingBoxInt? ComputeBounds(Vector3 orig, int w, int h, float ics, float ich)
        {
            int minx = (int)Math.Floor((bbox.Minimum.X - orig.X) * ics);
            int miny = (int)Math.Floor((bbox.Minimum.Y - orig.Y) * ich);
            int minz = (int)Math.Floor((bbox.Minimum.Z - orig.Z) * ics);
            int maxx = (int)Math.Floor((bbox.Maximum.X - orig.X) * ics);
            int maxy = (int)Math.Floor((bbox.Maximum.Y - orig.Y) * ich);
            int maxz = (int)Math.Floor((bbox.Maximum.Z - orig.Z) * ics);

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
