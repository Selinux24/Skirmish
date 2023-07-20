using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Oriented box obstacle
    /// </summary>
    public struct ObstacleOrientedBox : IObstacle
    {
        /// <summary>
        /// Gets the Y axis rotation from a transform matrix
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the Y axis angle, only if the rotation is in the Y axis</returns>
        private static float GetYRotation(Matrix transform)
        {
            if (transform.Decompose(out _, out var rotation, out _))
            {
                return GetYRotation(rotation);
            }
            else
            {
                throw new ArgumentException("Bad transform. Cannot decompose.", nameof(transform));
            }
        }
        /// <summary>
        /// Gets the Y axis rotation from a rotation quaternion
        /// </summary>
        /// <param name="rotation">Rotation Quaternion</param>
        /// <returns>Returns the Y axis angle, only if the rotation is in the Y axis</returns>
        private static float GetYRotation(Quaternion rotation)
        {
            var yRotation = 0f;

            // Validates the angle and axis
            if (rotation.Angle != 0)
            {
                Vector3 epsilon = Vector3.Up * 0.0001f;

                if (Vector3.NearEqual(rotation.Axis, Vector3.Up, epsilon))
                {
                    yRotation = rotation.Angle;
                }

                if (Vector3.NearEqual(rotation.Axis, Vector3.Down, epsilon))
                {
                    yRotation = -rotation.Angle;
                }
            }

            return yRotation;
        }

        /// <summary>
        /// Box center
        /// </summary>
        public Vector3 Center { get; set; }
        /// <summary>
        /// Half extents
        /// </summary>
        public Vector3 HalfExtents { get; set; }
        /// <summary>
        /// Y axis rotation in radians
        /// </summary>
        public float YRadians { get; set; }
        /// <summary>
        /// Auxiliary rotation vector
        /// </summary>
        /// <remarks>{ cos(0.5f*angle)*sin(-0.5f*angle); cos(0.5f*angle)*cos(0.5f*angle) - 0.5 }</remarks>
        public readonly Vector2 RotAux
        {
            get
            {
                float coshalf = (float)Math.Cos(0.5f * YRadians);
                float sinhalf = (float)Math.Sin(-0.5f * YRadians);
                return new Vector2(coshalf * sinhalf, coshalf * coshalf - 0.5f);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obbox">Oriented bounding box</param>
        public ObstacleOrientedBox(OrientedBoundingBox obbox)
        {
            var yRotation = GetYRotation(obbox.Transformation);

            Center = obbox.Center;
            HalfExtents = obbox.Extents;
            YRadians = yRotation;
        }

        /// <summary>
        /// Gets the obstacle bounds
        /// </summary>
        /// <returns>Returns a bounding box</returns>
        public readonly BoundingBox GetBounds()
        {
            float maxr = 1.41f * Math.Max(HalfExtents.X, HalfExtents.Z);

            Vector3 bmin;
            Vector3 bmax;

            bmin.X = Center.X - maxr;
            bmax.X = Center.X + maxr;
            bmin.Y = Center.Y - HalfExtents.Y;
            bmax.Y = Center.Y + HalfExtents.Y;
            bmin.Z = Center.Z - maxr;
            bmax.Z = Center.Z + maxr;

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
            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float cx = (Center.X - orig.X) * ics;
            float cz = (Center.Z - orig.Z) * ics;

            var bounds = ComputeBounds(orig, w, h, cx, cz, ics, ich);
            if (!bounds.HasValue)
            {
                return true;
            }

            var min = bounds.Value.Min;
            var max = bounds.Value.Max;

            float xhalf = HalfExtents.X * ics + 0.5f;
            float zhalf = HalfExtents.Z * ics + 0.5f;

            for (int z = min.Z; z <= max.Z; ++z)
            {
                for (int x = min.X; x <= max.X; ++x)
                {
                    float x2 = 2.0f * (x - cx);
                    float z2 = 2.0f * (z - cz);

                    if (FilterRotation(x2, z2, xhalf, zhalf))
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
        private readonly bool FilterRotation(float x2, float z2, float xhalf, float zhalf)
        {
            float xrot = RotAux.Y * x2 + RotAux.X * z2;
            if (xrot > xhalf || xrot < -xhalf)
            {
                return true;
            }

            float zrot = RotAux.Y * z2 - RotAux.X * x2;
            if (zrot > zhalf || zrot < -zhalf)
            {
                return true;
            }

            return false;
        }
        private BoundingBoxInt? ComputeBounds(Vector3 orig, int w, int h, float cx, float cz, float ics, float ich)
        {
            float maxr = 1.41f * Math.Max(HalfExtents.X, HalfExtents.Z);
            int minx = (int)Math.Floor(cx - maxr * ics);
            int maxx = (int)Math.Floor(cx + maxr * ics);
            int minz = (int)Math.Floor(cz - maxr * ics);
            int maxz = (int)Math.Floor(cz + maxr * ics);
            int miny = (int)Math.Floor((Center.Y - HalfExtents.Y - orig.Y) * ich);
            int maxy = (int)Math.Floor((Center.Y + HalfExtents.Y - orig.Y) * ich);

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
