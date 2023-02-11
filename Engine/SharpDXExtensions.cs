using SharpDX;

namespace Engine
{
    /// <summary>
    /// SharpDX classes extension
    /// </summary>
    public static class SharpDXExtensions
    {
        /// <summary>
        /// Limits the vector length to specified magnitude
        /// </summary>
        /// <param name="vector">Vector to limit</param>
        /// <param name="magnitude">Magnitude</param>
        /// <returns></returns>
        public static Vector3 Limit(this Vector3 vector, float magnitude)
        {
            if (vector.Length() > magnitude)
            {
                return Vector3.Normalize(vector) * magnitude;
            }

            return vector;
        }

        /// <summary>
        /// Returns xyz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xyz components from Vector4</returns>
        public static Vector3 XYZ(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Returns xxx components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xxx components from Vector4</returns>
        public static Vector3 XXX(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.X, vector.X);
        }
        /// <summary>
        /// Returns xxx components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns xxx components from Vector3</returns>
        public static Vector3 XXX(this Vector3 vector)
        {
            return new Vector3(vector.X, vector.X, vector.X);
        }

        /// <summary>
        /// Returns yyy components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns yyy components from Vector4</returns>
        public static Vector3 YYY(this Vector4 vector)
        {
            return new Vector3(vector.Y, vector.Y, vector.Y);
        }
        /// <summary>
        /// Returns yyy components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yyy components from Vector3</returns>
        public static Vector3 YYY(this Vector3 vector)
        {
            return new Vector3(vector.Y, vector.Y, vector.Y);
        }
        /// <summary>
        /// Returns zzz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns zzz components from Vector4</returns>
        public static Vector3 ZZZ(this Vector4 vector)
        {
            return new Vector3(vector.Z, vector.Z, vector.Z);
        }
        /// <summary>
        /// Returns zzz components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zzz components from Vector3</returns>
        public static Vector3 ZZZ(this Vector3 vector)
        {
            return new Vector3(vector.Z, vector.Z, vector.Z);
        }

        /// <summary>
        /// Returns xy components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xy components from Vector4</returns>
        public static Vector2 XY(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        /// <summary>
        /// Returns xy components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns xy components from Vector3</returns>
        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        /// <summary>
        /// Returns yx components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns yx components from Vector4</returns>
        public static Vector2 YX(this Vector4 vector)
        {
            return new Vector2(vector.Y, vector.X);
        }
        /// <summary>
        /// Returns yx components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yx components from Vector3</returns>
        public static Vector2 YX(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.X);
        }

        /// <summary>
        /// Returns xz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xz components from Vector4</returns>
        public static Vector2 XZ(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        /// <summary>
        /// Returns xz components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns xz components from Vector3</returns>
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        /// <summary>
        /// Returns zx components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns zx components from Vector4</returns>
        public static Vector2 ZX(this Vector4 vector)
        {
            return new Vector2(vector.Z, vector.X);
        }
        /// <summary>
        /// Returns zx components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zx components from Vector3</returns>
        public static Vector2 ZX(this Vector3 vector)
        {
            return new Vector2(vector.Z, vector.X);
        }

        /// <summary>
        /// Returns yz components from Vector4
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yz components from Vector4</returns>
        public static Vector2 YZ(this Vector4 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns yz components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yz components from Vector3</returns>
        public static Vector2 YZ(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns zy components from Vector4
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zy components from Vector4</returns>
        public static Vector2 ZY(this Vector4 vector)
        {
            return new Vector2(vector.Z, vector.Y);
        }
        /// <summary>
        /// Returns zy components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zy components from Vector3</returns>
        public static Vector2 ZY(this Vector3 vector)
        {
            return new Vector2(vector.Z, vector.Y);
        }

        /// <summary>
        /// Returns rgb components from Color4
        /// </summary>
        /// <param name="color">Color4</param>
        /// <returns>Returns rgb components from Color4</returns>
        public static Color3 RGB(this Color4 color)
        {
            return new Color3(color.Red, color.Green, color.Blue);
        }
        /// <summary>
        /// Returns rgb components from Color
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns rgb components from Color</returns>
        public static Color3 RGB(this Color color)
        {
            return color.ToColor4().RGB();
        }

        /// <summary>
        /// Gets the bounding box center
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the center of the current bounding box</returns>
        public static Vector3 GetCenter(this BoundingBox bbox)
        {
            return (bbox.Minimum + bbox.Maximum) * 0.5f;
        }
        /// <summary>
        /// Gets the bounding box extents
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the bounding box extents</returns>
        public static Vector3 GetExtents(this BoundingBox bbox)
        {
            var center = bbox.GetCenter();

            return bbox.Maximum - center;
        }
        /// <summary>
        /// Gets the XY rectangle of the box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the XY rectangle of the box</returns>
        public static RectangleF GetRectangleXY(this BoundingBox bbox)
        {
            return new RectangleF
            {
                Left = bbox.Minimum.X,
                Top = bbox.Minimum.Y,
                Right = bbox.Maximum.X,
                Bottom = bbox.Maximum.Y,
            };
        }
        /// <summary>
        /// Gets the XZ rectangle of the box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the XZ rectangle of the box</returns>
        public static RectangleF GetRectangleXZ(this BoundingBox bbox)
        {
            return new RectangleF
            {
                Left = bbox.Minimum.X,
                Top = bbox.Minimum.Z,
                Right = bbox.Maximum.X,
                Bottom = bbox.Maximum.Z,
            };
        }

        /// <summary>
        /// Gets whether almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector3 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector4 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z) || float.IsNaN(vector.W);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Color4 color)
        {
            return float.IsNaN(color.Red) || float.IsNaN(color.Green) || float.IsNaN(color.Blue) || float.IsNaN(color.Alpha);
        }

        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector3 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector2 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector4 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z) || float.IsInfinity(vector.W);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Color4 color)
        {
            return float.IsInfinity(color.Red) || float.IsInfinity(color.Green) || float.IsInfinity(color.Blue) || float.IsInfinity(color.Alpha);
        }
    }
}
