using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets a sphere transformed by the given matrix
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="transform">Transform</param>
        public static BoundingSphere SetTransform(this BoundingSphere sphere, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return sphere;
            }

            // Gets the new position
            var center = Vector3.TransformCoordinate(sphere.Center, transform);

            // Calculates the scale vector
            var scale = new Vector3(
                transform.Column1.Length(),
                transform.Column2.Length(),
                transform.Column3.Length());

            // Gets the new sphere radius, based on the maximum scale axis value
            float radius = sphere.Radius * Math.Max(Math.Max(scale.X, scale.Y), scale.Z);

            return new BoundingSphere(center, radius);
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
        /// Gets the bounding box edge list
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the edge list of the current bounding box</returns>
        public static IEnumerable<Segment> GetEdges(this BoundingBox bbox)
        {
            List<Segment> segments = new List<Segment>(12);

            var corners = bbox.GetCorners();

            //Top edges
            segments.Add(new Segment(corners[0], corners[1]));
            segments.Add(new Segment(corners[1], corners[2]));
            segments.Add(new Segment(corners[2], corners[3]));
            segments.Add(new Segment(corners[3], corners[0]));

            //Bottom edges
            segments.Add(new Segment(corners[4], corners[5]));
            segments.Add(new Segment(corners[5], corners[6]));
            segments.Add(new Segment(corners[6], corners[7]));
            segments.Add(new Segment(corners[7], corners[4]));

            //Vertical edges
            segments.Add(new Segment(corners[0], corners[4]));
            segments.Add(new Segment(corners[1], corners[5]));
            segments.Add(new Segment(corners[2], corners[6]));
            segments.Add(new Segment(corners[3], corners[7]));

            return segments;
        }
        /// <summary>
        /// Gets the specified corner
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="corner">Box corner</param>
        public static Vector3 GetCorner(this BoundingBox box, BoxCorners corner)
        {
            return GetCorner(box, (int)corner);
        }
        /// <summary>
        /// Gets the specified corner
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="index">Corner index</param>
        public static Vector3 GetCorner(this BoundingBox box, int index)
        {
            return box.GetCorners().ElementAtOrDefault(index);
        }
        /// <summary>
        /// Gets a box transformed by the given matrix
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="transform">Transform</param>
        public static BoundingBox SetTransform(this BoundingBox box, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return box;
            }

            // Gets the new position
            var min = Vector3.TransformCoordinate(box.Minimum, transform);
            var max = Vector3.TransformCoordinate(box.Maximum, transform);

            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Gets the specified corner
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="corner">Box corner</param>
        public static Vector3 GetCorner(this OrientedBoundingBox obb, BoxCorners corner)
        {
            return GetCorner(obb, (int)corner);
        }
        /// <summary>
        /// Gets the specified corner
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="index">Corner index</param>
        public static Vector3 GetCorner(this OrientedBoundingBox obb, int index)
        {
            return obb.GetCorners().ElementAtOrDefault(index);
        }
        /// <summary>
        /// Creates an oriented bounding box from a transformed point list and it's transform matrix
        /// </summary>
        /// <param name="points">Point list</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(IEnumerable<Vector3> points, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return new OrientedBoundingBox(points.ToArray());
            }

            //First, get item points
            Vector3[] ptArray = points.ToArray();

            //Next, remove any point transform and set points to origin
            Matrix inv = Matrix.Invert(transform);
            Vector3.TransformCoordinate(ptArray, ref inv, ptArray);

            //Create the obb from origin points
            var obb = new OrientedBoundingBox(ptArray);

            //Apply the original transform to obb
            obb.Transformation *= transform;

            return obb;
        }
        /// <summary>
        /// Gets a oriented bounding box transformed by the given matrix
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="transform">Transform</param>
        public static OrientedBoundingBox SetTransform(this OrientedBoundingBox obb, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return obb;
            }

            var trnObb = obb;
            trnObb.Transformation = transform;
            return trnObb;
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
