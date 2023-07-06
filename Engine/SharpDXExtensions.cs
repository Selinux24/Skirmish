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
        /// Project the point in the vector
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="point">Point</param>
        public static Vector3 ProjectPoint(this Vector3 vector, Vector3 point)
        {
            // Normalize the projection vector
            if (!vector.IsNormalized)
            {
                vector = Vector3.Normalize(vector);
            }

            // Calculate the dot product between the point and the projection vector
            float dotProduct = Vector3.Dot(point, vector);

            // Calculate the projection of the point onto the projection vector
            return dotProduct * vector;
        }

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
        /// Constructs a BoundingSphere that fully contains the given points.
        /// </summary>
        /// <param name="points">Point list</param>
        public static BoundingSphere BoundingSphereFromPoints(IEnumerable<Vector3> points)
        {
            return BoundingSphere.FromPoints(points.ToArray());
        }
        /// <summary>
        /// Constructs a BoundingBox that fully contains the given points.
        /// </summary>
        /// <param name="points">Point list</param>
        public static BoundingBox BoundingBoxFromPoints(IEnumerable<Vector3> points)
        {
            var box = BoundingBox.FromPoints(points.ToArray());

            return box.Normalize();
        }

        /// <summary>
        /// Normalizes the minimum and maximum bounding box limits.
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <returns>Returns a bounding box which assures that the minimum vector contains the minimum values, and the maximum vector contains the maximum values</returns>
        public static BoundingBox Normalize(this BoundingBox box)
        {
            var newBox = box;

            var min = newBox.Minimum;
            var max = newBox.Maximum;
            newBox.Minimum = new Vector3(Math.Min(max.X, min.X), Math.Min(max.Y, min.Y), Math.Min(max.Z, min.Z));
            newBox.Maximum = new Vector3(Math.Max(max.X, min.X), Math.Max(max.Y, min.Y), Math.Max(max.Z, min.Z));

            return newBox;
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

            // Extract scaling
            if (!transform.Decompose(out var scale, out _, out _))
            {
                return sphere;
            }

            // Gets the new position
            var center = Vector3.TransformCoordinate(sphere.Center, transform);

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
            var trnBox = new BoundingBox(min, max);

            return trnBox.Normalize();
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
            //Top edges
            yield return new Segment(vertices.ElementAt((int)BoxVertices.FrontRightTop), vertices.ElementAt((int)BoxVertices.BackRightTop));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.BackRightTop), vertices.ElementAt((int)BoxVertices.BackLeftTop));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.BackLeftTop), vertices.ElementAt((int)BoxVertices.FrontLeftTop));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.FrontLeftTop), vertices.ElementAt((int)BoxVertices.FrontRightTop));

            //Bottom edges
            yield return new Segment(vertices.ElementAt((int)BoxVertices.FrontRightBottom), vertices.ElementAt((int)BoxVertices.BackRightBottom));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.BackRightBottom), vertices.ElementAt((int)BoxVertices.BackLeftBottom));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.BackLeftBottom), vertices.ElementAt((int)BoxVertices.FrontLeftBottom));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.FrontLeftBottom), vertices.ElementAt((int)BoxVertices.FrontRightBottom));

            //Vertical edges
            yield return new Segment(vertices.ElementAt((int)BoxVertices.FrontRightTop), vertices.ElementAt((int)BoxVertices.FrontRightBottom));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.BackRightTop), vertices.ElementAt((int)BoxVertices.BackRightBottom));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.BackLeftTop), vertices.ElementAt((int)BoxVertices.BackLeftBottom));
            yield return new Segment(vertices.ElementAt((int)BoxVertices.FrontLeftTop), vertices.ElementAt((int)BoxVertices.FrontLeftBottom));
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

            var topNormal = Vector3.Cross(edges.ElementAt(0).Direction, edges.ElementAt(1).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), topNormal);

            var bottomNormal = Vector3.Cross(edges.ElementAt(5).Direction, edges.ElementAt(4).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftBottom), bottomNormal);

            var frontNormal = Vector3.Cross(edges.ElementAt(8).Direction, edges.ElementAt(3).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), frontNormal);

            var backNormal = Vector3.Cross(edges.ElementAt(9).Direction, edges.ElementAt(1).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.BackLeftTop), backNormal);

            var leftNormal = Vector3.Cross(edges.ElementAt(10).Direction, edges.ElementAt(2).Direction);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), leftNormal);

            var rightNormal = Vector3.Cross(edges.ElementAt(8).Direction, edges.ElementAt(0).Direction);
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
            var min = box.Minimum;
            var max = box.Maximum;

            yield return new Vector3(max.X, max.Y, max.Z);
            yield return new Vector3(max.X, max.Y, min.Z);
            yield return new Vector3(min.X, max.Y, min.Z);
            yield return new Vector3(min.X, max.Y, max.Z);
            yield return new Vector3(max.X, min.Y, max.Z);
            yield return new Vector3(max.X, min.Y, min.Z);
            yield return new Vector3(min.X, min.Y, min.Z);
            yield return new Vector3(min.X, min.Y, max.Z);
        }
        /// <summary>
        /// Gets the oriented bounding box vertices
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        public static IEnumerable<Vector3> GetVertices(this OrientedBoundingBox obb)
        {
            var extents = obb.Extents;
            var trn = obb.Transformation;

            var normal = new Vector3(extents.X, 0f, 0f);
            var normal2 = new Vector3(0f, extents.Y, 0f);
            var normal3 = new Vector3(0f, 0f, extents.Z);
            Vector3.TransformNormal(ref normal, ref trn, out normal);
            Vector3.TransformNormal(ref normal2, ref trn, out normal2);
            Vector3.TransformNormal(ref normal3, ref trn, out normal3);
            Vector3 translationVector = trn.TranslationVector;

            yield return translationVector + normal + normal2 + normal3;
            yield return translationVector + normal + normal2 - normal3;
            yield return translationVector - normal + normal2 - normal3;
            yield return translationVector - normal + normal2 + normal3;
            yield return translationVector + normal - normal2 + normal3;
            yield return translationVector + normal - normal2 - normal3;
            yield return translationVector - normal - normal2 - normal3;
            yield return translationVector - normal - normal2 + normal3;
        }

        /// <summary>
        /// Creates an oriented bounding box from a transformed point list and it's transform matrix
        /// </summary>
        /// <param name="points">Point list</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(IEnumerable<Vector3> points)
        {
            return FromPoints(points, Matrix.Identity);
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
            var ptArray = points.ToArray();

            //Next, remove any point transform and set points to origin
            var inv = Matrix.Invert(transform);
            Vector3.TransformCoordinate(ptArray, ref inv, ptArray);

            //Create the OBB from origin points
            var obb = new OrientedBoundingBox(ptArray);

            //Apply the original transform to OBB
            obb.Transformation *= transform;

            return obb;
        }

        /// <summary>
        /// Generates a list of four bounding boxes which makes an QuadTree subdivision of the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns four boxes</returns>
        /// <remarks>
        /// By index:
        /// 0 - Top Left
        /// 1 - Top Right
        /// 2 - Bottom Left
        /// 3 - Bottom Right
        /// </remarks>
        public static IEnumerable<BoundingBox> QuadTree(this BoundingBox bbox)
        {
            var M = bbox.Maximum;
            var c = (bbox.Maximum + bbox.Minimum) * 0.5f;
            var m = bbox.Minimum;

            //-1-1-1   +0+1+0 - Top Left
            yield return new BoundingBox(new Vector3(m.X, m.Y, m.Z), new Vector3(c.X, M.Y, c.Z));
            //-1-1+0   +0+1+1 - Top Right
            yield return new BoundingBox(new Vector3(m.X, m.Y, c.Z), new Vector3(c.X, M.Y, M.Z));
            //+0-1-1   +1+1+0 - Bottom Left
            yield return new BoundingBox(new Vector3(c.X, m.Y, m.Z), new Vector3(M.X, M.Y, c.Z));
            //+0-1+0   +1+1+1 - Bottom Right
            yield return new BoundingBox(new Vector3(c.X, m.Y, c.Z), new Vector3(M.X, M.Y, M.Z));
        }
        /// <summary>
        /// Generates a list of eight bounding boxes which makes an OcTree subdivision of the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns eight boxes</returns>
        /// <remarks>
        /// By index:
        /// 0 - Top Left Front
        /// 1 - Top Left Back
        /// 2 - Top Right Front
        /// 3 - Top Right Back
        /// 4 - Bottom Left Front
        /// 5 - Bottom Left Back
        /// 6 - Bottom Right Front
        /// 7 - Bottom Right Back
        /// </remarks>
        public static IEnumerable<BoundingBox> Octree(this BoundingBox bbox)
        {
            var m = bbox.Minimum;
            var M = bbox.Maximum;
            var c = bbox.Center;

            //-1+0-1   +0+1+0 - Top Left Front
            yield return new BoundingBox(new Vector3(m.X, c.Y, m.Z), new Vector3(c.X, M.Y, c.Z));
            //-1+0+0   +0+1+1 - Top Left Back
            yield return new BoundingBox(new Vector3(m.X, c.Y, c.Z), new Vector3(c.X, M.Y, M.Z));

            //+0+0-1   +1+1+0 - Top Right Front
            yield return new BoundingBox(new Vector3(c.X, c.Y, m.Z), new Vector3(M.X, M.Y, c.Z));
            //+0+0+0   +1+1+1 - Top Right Back
            yield return new BoundingBox(new Vector3(c.X, c.Y, c.Z), new Vector3(M.X, M.Y, M.Z));

            //-1-1-1   +0+0+0 - Bottom Left Front
            yield return new BoundingBox(new Vector3(m.X, m.Y, m.Z), new Vector3(c.X, c.Y, c.Z));
            //-1-1+0   +0+0+1 - Bottom Left Back
            yield return new BoundingBox(new Vector3(m.X, m.Y, c.Z), new Vector3(c.X, c.Y, M.Z));

            //+0-1-1   +1+0+0 - Bottom Right Front
            yield return new BoundingBox(new Vector3(c.X, m.Y, m.Z), new Vector3(M.X, c.Y, c.Z));
            //+0-1+0   +1+0+1 - Bottom Right Back
            yield return new BoundingBox(new Vector3(c.X, m.Y, c.Z), new Vector3(M.X, c.Y, M.Z));
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
