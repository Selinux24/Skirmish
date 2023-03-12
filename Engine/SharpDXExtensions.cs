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


            if (!transform.Decompose(out var scale, out var rotation, out var translation))
            {
                return obb;
            }

            var trnObb = obb;
            trnObb.Scale(scale);
            trnObb.Transformation = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);
            return trnObb;
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
            return GetEdges(bbox.GetVertices());
        }
        /// <summary>
        /// Gets the oriented bounding box edge list
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <returns>Returns the edge list of the current oriented bounding box</returns>
        public static IEnumerable<Segment> GetEdges(this OrientedBoundingBox obb)
        {
            return GetEdges(obb.GetVertices());
        }
        /// <summary>
        /// Gets the edge list
        /// </summary>
        /// <param name="vertices">Box vertices</param>
        /// <returns>Returns the edge list of the current box vertices</returns>
        public static IEnumerable<Segment> GetEdges(IEnumerable<Vector3> vertices)
        {
            return new[]
            {
                //Top edges
                new Segment(vertices.ElementAt((int)BoxVertices.FrontRightTop),    vertices.ElementAt((int)BoxVertices.BackRightTop)),
                new Segment(vertices.ElementAt((int)BoxVertices.BackRightTop),     vertices.ElementAt((int)BoxVertices.BackLeftTop)),
                new Segment(vertices.ElementAt((int)BoxVertices.BackLeftTop),      vertices.ElementAt((int)BoxVertices.FrontLeftTop)),
                new Segment(vertices.ElementAt((int)BoxVertices.FrontLeftTop),     vertices.ElementAt((int)BoxVertices.FrontRightTop)),

                //Bottom edges
                new Segment(vertices.ElementAt((int)BoxVertices.FrontRightBottom), vertices.ElementAt((int)BoxVertices.BackRightBottom)),
                new Segment(vertices.ElementAt((int)BoxVertices.BackRightBottom),  vertices.ElementAt((int)BoxVertices.BackLeftBottom)),
                new Segment(vertices.ElementAt((int)BoxVertices.BackLeftBottom),   vertices.ElementAt((int)BoxVertices.FrontLeftBottom)),
                new Segment(vertices.ElementAt((int)BoxVertices.FrontLeftBottom),  vertices.ElementAt((int)BoxVertices.FrontRightBottom)),

                //Vertical edges
                new Segment(vertices.ElementAt((int)BoxVertices.FrontRightTop),    vertices.ElementAt((int)BoxVertices.FrontRightBottom)),
                new Segment(vertices.ElementAt((int)BoxVertices.BackRightTop),     vertices.ElementAt((int)BoxVertices.BackRightBottom)),
                new Segment(vertices.ElementAt((int)BoxVertices.BackLeftTop),      vertices.ElementAt((int)BoxVertices.BackLeftBottom)),
                new Segment(vertices.ElementAt((int)BoxVertices.FrontLeftTop),     vertices.ElementAt((int)BoxVertices.FrontLeftBottom))
            };
        }

        /// <summary>
        /// Gets the bounding box face planes list
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public static IEnumerable<Plane> GetFaces(this BoundingBox bbox)
        {
            var vertices = bbox.GetVertices();

            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3.Up);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftBottom), Vector3.Down);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3.ForwardLH);
            yield return new Plane(GetVertex(vertices, BoxVertices.BackLeftTop), Vector3.BackwardLH);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3.Left);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontRightBottom), Vector3.Right);
        }
        /// <summary>
        /// Gets the oriented bounding box face planes list
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public static IEnumerable<Plane> GetFaces(this OrientedBoundingBox obb)
        {
            var vertices = obb.GetVertices();

            var edges = GetEdges(vertices);

            Vector3 topNormal = Vector3.Cross(edges.ElementAt(0).Direction, edges.ElementAt(1).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), topNormal);

            Vector3 bottomNormal = Vector3.Cross(edges.ElementAt(5).Direction, edges.ElementAt(4).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftBottom), bottomNormal);

            Vector3 frontNormal = Vector3.Cross(edges.ElementAt(8).Direction, edges.ElementAt(3).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), frontNormal);

            Vector3 backNormal = Vector3.Cross(edges.ElementAt(9).Direction, edges.ElementAt(1).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.BackLeftTop), backNormal);

            Vector3 leftNormal = Vector3.Cross(edges.ElementAt(10).Direction, edges.ElementAt(2).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), leftNormal);

            Vector3 rightNormal = Vector3.Cross(edges.ElementAt(8).Direction, edges.ElementAt(0).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontRightBottom), rightNormal);
        }

        /// <summary>
        /// Gets the bounding box face plane
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="face">Face</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public static Plane GetFace(this BoundingBox bbox, BoxFaces face)
        {
            return GetFace(bbox.GetFaces(), face);
        }
        /// <summary>
        /// Gets the bounding box face plane
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="faceIndex">Face index</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public static Plane GetFace(this BoundingBox bbox, int faceIndex)
        {
            return GetFace(bbox.GetFaces(), faceIndex);
        }
        /// <summary>
        /// Gets the oriented bounding box face plane
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="face">Face</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public static Plane GetFace(this OrientedBoundingBox obb, BoxFaces face)
        {
            return GetFace(obb.GetFaces(), face);
        }
        /// <summary>
        /// Gets the oriented bounding box face plane
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="faceIndex">Face index</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public static Plane GetFace(this OrientedBoundingBox obb, int faceIndex)
        {
            return GetFace(obb.GetFaces(), faceIndex);
        }
        /// <summary>
        /// Gets the specified box face
        /// </summary>
        /// <param name="faces">Box faces collection</param>
        /// <param name="face">Face</param>
        public static Plane GetFace(IEnumerable<Plane> faces, BoxFaces face)
        {
            return faces.ElementAtOrDefault((int)face);
        }
        /// <summary>
        /// Gets the specified box face
        /// </summary>
        /// <param name="faces">Box faces collection</param>
        /// <param name="faceIndex">Face index</param>
        public static Plane GetFace(IEnumerable<Plane> faces, int faceIndex)
        {
            return faces.ElementAtOrDefault(faceIndex);
        }

        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="vertex">Box vertex</param>
        public static Vector3 GetVertex(this BoundingBox box, BoxVertices vertex)
        {
            return GetVertex(box.GetVertices(), (int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="index">Vertex index</param>
        public static Vector3 GetVertex(this BoundingBox box, int index)
        {
            return GetVertex(box.GetVertices(), index);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="vertex">Box vertex</param>
        public static Vector3 GetVertex(this OrientedBoundingBox obb, BoxVertices vertex)
        {
            return GetVertex(obb.GetVertices(), (int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="index">Vertex index</param>
        public static Vector3 GetVertex(this OrientedBoundingBox obb, int index)
        {
            return GetVertex(obb.GetVertices(), index);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="vertex">Box vertex</param>
        public static Vector3 GetVertex(IEnumerable<Vector3> vertices, BoxVertices vertex)
        {
            return vertices.ElementAtOrDefault((int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="index">Vertex index</param>
        public static Vector3 GetVertex(IEnumerable<Vector3> vertices, int index)
        {
            return vertices.ElementAtOrDefault(index);
        }

        /// <summary>
        /// Gets the bounding box vertices
        /// </summary>
        /// <param name="box">Bounding box</param>
        public static IEnumerable<Vector3> GetVertices(this BoundingBox box)
        {
            var corners = box.GetCorners();

            // Hack sharpDX BoundingBox vertex order, to compatibilice with OrientedBoundingBox
            return new[]
            {
                corners[1],
                corners[5],
                corners[4],
                corners[0],
                corners[2],
                corners[6],
                corners[7],
                corners[3],
            };
        }
        /// <summary>
        /// Gets the oriented bounding box vertices
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        public static IEnumerable<Vector3> GetVertices(this OrientedBoundingBox obb)
        {
            return obb.GetCorners();
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
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="vector">Vector</param>
        public static float ProjectToVector(this BoundingBox box, Vector3 vector)
        {
            return ProjectToVector(box.GetExtents(), Matrix.Identity, vector);
        }
        /// <summary>
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="vector">Vector</param>
        public static float ProjectToVector(this OrientedBoundingBox box, Vector3 vector)
        {
            return ProjectToVector(box.Extents, box.Transformation, vector);
        }
        /// <summary>
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="extents">Box extents</param>
        /// <param name="trn">Box transform</param>
        /// <param name="vector">Vector</param>
        public static float ProjectToVector(Vector3 extents, Matrix trn, Vector3 vector)
        {
            var xAxis = trn.Right;
            var yAxis = trn.Up;
            var zAxis = trn.Backward;

            return
                extents.X * Math.Abs(Vector3.Dot(vector, xAxis)) +
                extents.Y * Math.Abs(Vector3.Dot(vector, yAxis)) +
                extents.Z * Math.Abs(Vector3.Dot(vector, zAxis));
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
