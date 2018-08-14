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
        /// <returns>Returns xy components from Vector4</returns>
        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
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
        /// <returns>Returns xz components from Vector4</returns>
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
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
        /// Gets the x maganitude of the current bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the x maganitude of the current bounding box</returns>
        public static float GetX(this BoundingBox bbox)
        {
            return bbox.Maximum.X - bbox.Minimum.X;
        }
        /// <summary>
        /// Gets the y maganitude of the current bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the y maganitude of the current bounding box</returns>
        public static float GetY(this BoundingBox bbox)
        {
            return bbox.Maximum.Y - bbox.Minimum.Y;
        }
        /// <summary>
        /// Gets the z maganitude of the current bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the z maganitude of the current bounding box</returns>
        public static float GetZ(this BoundingBox bbox)
        {
            return bbox.Maximum.Z - bbox.Minimum.Z;
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector3 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector4 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z) || float.IsNaN(vector.W);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Color4 color)
        {
            return float.IsNaN(color.Red) || float.IsNaN(color.Green) || float.IsNaN(color.Blue) || float.IsNaN(color.Alpha);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector3 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector4 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z) || float.IsInfinity(vector.W);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Color4 color)
        {
            return float.IsInfinity(color.Red) || float.IsInfinity(color.Green) || float.IsInfinity(color.Blue) || float.IsInfinity(color.Alpha);
        }
    }
}
