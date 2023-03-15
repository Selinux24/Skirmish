using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Cylinder obstacle
    /// </summary>
    public struct ObstacleCylinder : IObstacle
    {
        /// <summary>
        /// Center position
        /// </summary>
        public Vector3 Center { get; set; }
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cylinder">Cylinder</param>
        public ObstacleCylinder(BoundingCylinder cylinder)
        {
            Center = cylinder.Center;
            Radius = cylinder.Radius;
            Height = cylinder.Height;
        }

        /// <summary>
        /// Gets the obstacle bounds
        /// </summary>
        /// <returns>Returns a bounding box</returns>
        public BoundingBox GetBounds()
        {
            Vector3 bmin;
            Vector3 bmax;
            float hh = Height * 0.5f;

            bmin.X = Center.X - Radius;
            bmin.Y = Center.Y - hh;
            bmin.Z = Center.Z - Radius;
            bmax.X = Center.X + Radius;
            bmax.Y = Center.Y + hh;
            bmax.Z = Center.Z + Radius;

            return new BoundingBox(bmin, bmax);
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
            var bbox = GetBounds();

            float r2 = (float)Math.Pow(Radius / cs + 0.5f, 2.0f);

            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float px = (Center.X - orig.X) * ics;
            float pz = (Center.Z - orig.Z) * ics;

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
