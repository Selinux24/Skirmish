using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Cylinder obstacle
    /// </summary>
    public readonly struct ObstacleCylinder : IObstacle
    {
        /// <summary>
        /// Center position
        /// </summary>
        private readonly Vector3 center;
        /// <summary>
        /// Radius
        /// </summary>
        private readonly float radius;
        /// <summary>
        /// Height
        /// </summary>
        private readonly float height;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cylinder">Cylinder</param>
        public ObstacleCylinder(BoundingCylinder cylinder)
        {
            center = cylinder.Center;
            radius = cylinder.Radius;
            height = cylinder.Height;
        }

        /// <inheritdoc/>
        public readonly BoundingBox GetBounds()
        {
            Vector3 bmin;
            Vector3 bmax;
            float hh = height * 0.5f;

            bmin.X = center.X - radius;
            bmin.Y = center.Y - hh;
            bmin.Z = center.Z - radius;
            bmax.X = center.X + radius;
            bmax.Y = center.Y + hh;
            bmax.Z = center.Z + radius;

            return new BoundingBox(bmin, bmax);
        }
        /// <inheritdoc/>
        public readonly bool MarkArea(NavMeshTileBuildContext tc, Vector3 orig, float cs, float ch, AreaTypes area)
        {
            var bbox = GetBounds();

            float r2 = (float)Math.Pow(radius / cs + 0.5f, 2.0f);

            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float px = (center.X - orig.X) * ics;
            float pz = (center.Z - orig.Z) * ics;

            var bounds = ComputeBounds(bbox, orig, w, h, ics, ich);
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
                    float dx = (x + 0.5f) - px;
                    float dz = (z + 0.5f) - pz;
                    if (dx * dx + dz * dz > r2)
                    {
                        continue;
                    }

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
        /// <summary>
        /// Computes obstacle bounds
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="orig">Origin</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="ics">Cell size</param>
        /// <param name="ich">Cell height</param>
        private static BoundingBoxInt? ComputeBounds(BoundingBox bbox, Vector3 orig, int w, int h, float ics, float ich)
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
