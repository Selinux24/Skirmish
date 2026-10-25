using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Vortice.Mathematics;

namespace Engine
{
    /// <summary>
    /// Provides extension methods for the BoundingBoxExtensions structure.
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public static class BoundingBoxExtensions
    {
        /// <summary>
        /// Constructs a BoundingBox that fully contains the given points.
        /// </summary>
        /// <param name="points">Point list</param>
        public static BoundingBox BoundingBoxFromPoints(IEnumerable<Vector3> points)
        {
            var box = BoundingBox.CreateFromPoints(points.ToArray());

            return box.Normalize();
        }

        /// <summary>
        /// Merges the array of bounding boxes into one bounding box
        /// </summary>
        /// <param name="boxes">Array of boxes</param>
        /// <returns>Returns a bounding box containing all the boxes in the array</returns>
        public static BoundingBox MergeBoundingBox(IEnumerable<BoundingBox> boxes)
        {
            var fbbox = new BoundingBox();

            if (boxes?.Any() != true)
            {
                return fbbox;
            }

            var defaultBox = new BoundingBox();

            foreach (var bbox in boxes.Where(b => b != defaultBox))
            {
                fbbox = MergeBoundingBox(fbbox, bbox);
            }

            return fbbox;
        }
        /// <summary>
        /// Merges the boxes
        /// </summary>
        /// <param name="sourceBox">Source box</param>
        /// <param name="newBox">New box</param>
        /// <returns>If source box is null or default, returns new box</returns>
        private static BoundingBox MergeBoundingBox(BoundingBox? sourceBox, BoundingBox newBox)
        {
            if (!sourceBox.HasValue || sourceBox == new BoundingBox())
            {
                return newBox;
            }

            return BoundingBox.CreateMerged(sourceBox.Value, newBox);
        }

        /// <summary>
        /// Normalizes the minimum and maximum bounding box limits.
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <returns>Returns a bounding box which assures that the minimum vector contains the minimum values, and the maximum vector contains the maximum values</returns>
        public static BoundingBox Normalize(this BoundingBox box)
        {
            var newBox = box;

            var min = newBox.Min;
            var max = newBox.Max;
            newBox.Min = new(MathF.Min(max.X, min.X), MathF.Min(max.Y, min.Y), MathF.Min(max.Z, min.Z));
            newBox.Max = new(MathF.Max(max.X, min.X), MathF.Max(max.Y, min.Y), MathF.Max(max.Z, min.Z));

            return newBox;
        }

        /// <summary>
        /// Gets a box transformed by the given matrix
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="transform">Transform</param>
        public static BoundingBox SetTransform(this BoundingBox box, Matrix4x4 transform)
        {
            if (transform.IsIdentity)
            {
                return box;
            }

            if (!Matrix4x4.Decompose(transform, out var scale, out _, out var translation))
            {
                return box;
            }

            var extents = box.GetExtents();
            var center = box.Center;

            extents *= scale;
            center = Vector3.Transform(center, Matrix4x4.CreateTranslation(translation));

            // Gets the new position
            var trnBox = new BoundingBox(-extents + center, +extents + center);

            return trnBox.Normalize();
        }

        /// <summary>
        /// Gets the bounding box center
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the center of the current bounding box</returns>
        public static Vector3 GetCenter(this BoundingBox bbox)
        {
            return (bbox.Min + bbox.Max) * 0.5f;
        }
        /// <summary>
        /// Gets the bounding box extents
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the bounding box extents</returns>
        public static Vector3 GetExtents(this BoundingBox bbox)
        {
            var center = bbox.GetCenter();

            return bbox.Max - center;
        }

        /// <summary>
        /// Gets the XY rectangle of the box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the XY rectangle of the box</returns>
        public static Rect GetRectangleXY(this BoundingBox bbox)
        {
            return new Rect
            {
                Left = bbox.Min.X,
                Top = bbox.Min.Y,
                Right = bbox.Max.X,
                Bottom = bbox.Max.Y,
            };
        }
        /// <summary>
        /// Gets the XZ rectangle of the box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the XZ rectangle of the box</returns>
        public static Rect GetRectangleXZ(this BoundingBox bbox)
        {
            return new Rect
            {
                Left = bbox.Min.X,
                Top = bbox.Min.Z,
                Right = bbox.Max.X,
                Bottom = bbox.Max.Z,
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

            yield return PlaneExtensions.CreatePlane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3Extensions.Up);
            yield return PlaneExtensions.CreatePlane(GetVertex(vertices, BoxVertices.FrontLeftBottom), Vector3Extensions.Down);
            yield return PlaneExtensions.CreatePlane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3Extensions.ForwardLH);
            yield return PlaneExtensions.CreatePlane(GetVertex(vertices, BoxVertices.BackLeftTop), Vector3Extensions.BackwardLH);
            yield return PlaneExtensions.CreatePlane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3Extensions.Left);
            yield return PlaneExtensions.CreatePlane(GetVertex(vertices, BoxVertices.FrontRightBottom), Vector3Extensions.Right);
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
        /// Gets the bounding box edge list
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the edge list of the current bounding box</returns>
        public static IEnumerable<Segment> GetEdges(this BoundingBox bbox)
        {
            return GetEdges(bbox.GetVertices());
        }
        /// <summary>
        /// Gets the bounding box vertices
        /// </summary>
        /// <param name="box">Bounding box</param>
        public static IEnumerable<Vector3> GetVertices(this BoundingBox box)
        {
            var min = box.Min;
            var max = box.Max;

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
        /// Generates a list of four bounding boxes which makes an QuadTree subdivision of the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="separateBothAxis">Separate in X and Z axis</param>
        /// <param name="separation">Boxes separation amount</param>
        /// <returns>Returns four boxes</returns>
        /// <remarks>
        /// By index:
        /// 0 - Top Left
        /// 1 - Top Right
        /// 2 - Bottom Left
        /// 3 - Bottom Right
        /// </remarks>
        public static IEnumerable<BoundingBox> SubdivideQuadtree(this BoundingBox bbox, bool separateBothAxis = false, float separation = float.Epsilon)
        {
            var M = bbox.Max;
            var m = bbox.Min;
            var c = bbox.Center;
            float addAxis = separation;
            float subAxis = separateBothAxis ? separation : 0;

            //-1-1   +0+0 - Top Left
            yield return new BoundingBox(new Vector3(m.X, m.Y, m.Z), new Vector3(c.X - subAxis, M.Y, c.Z - subAxis));
            //-1+0   +0+1 - Top Right
            yield return new BoundingBox(new Vector3(m.X, m.Y, c.Z + addAxis), new Vector3(c.X - subAxis, M.Y, M.Z));

            //+0-1   +1+0 - Bottom Left
            yield return new BoundingBox(new Vector3(c.X + addAxis, m.Y, m.Z), new Vector3(M.X, M.Y, c.Z - subAxis));
            //+0+0   +1+1 - Bottom Right
            yield return new BoundingBox(new Vector3(c.X + addAxis, m.Y, c.Z + addAxis), new Vector3(M.X, M.Y, M.Z));
        }
        /// <summary>
        /// Generates a list of eight bounding boxes which makes an OcTree subdivision of the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="separation">Boxes separation</param>
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
        public static IEnumerable<BoundingBox> SubdivideOctree(this BoundingBox bbox, float separation = float.Epsilon)
        {
            var m = bbox.Min;
            var M = bbox.Max;
            var c = bbox.Center;

            //-1+0-1   +0+1+0 - Top Left Front
            yield return new BoundingBox(new Vector3(m.X, c.Y, m.Z), new Vector3(c.X, M.Y, c.Z));
            //-1+0+0   +0+1+1 - Top Left Back
            yield return new BoundingBox(new Vector3(m.X, c.Y, c.Z + separation), new Vector3(c.X, M.Y, M.Z));

            //+0+0-1   +1+1+0 - Top Right Front
            yield return new BoundingBox(new Vector3(c.X + separation, c.Y, m.Z), new Vector3(M.X, M.Y, c.Z));
            //+0+0+0   +1+1+1 - Top Right Back
            yield return new BoundingBox(new Vector3(c.X + separation, c.Y, c.Z + separation), new Vector3(M.X, M.Y, M.Z));

            //-1-1-1   +0+0+0 - Bottom Left Front
            yield return new BoundingBox(new Vector3(m.X, m.Y + separation, m.Z), new Vector3(c.X, c.Y + separation, c.Z));
            //-1-1+0   +0+0+1 - Bottom Left Back
            yield return new BoundingBox(new Vector3(m.X, m.Y + separation, c.Z + separation), new Vector3(c.X, c.Y + separation, M.Z));

            //+0-1-1   +1+0+0 - Bottom Right Front
            yield return new BoundingBox(new Vector3(c.X + separation, m.Y + separation, m.Z), new Vector3(M.X, c.Y + separation, c.Z));
            //+0-1+0   +1+0+1 - Bottom Right Back
            yield return new BoundingBox(new Vector3(c.X + separation, m.Y + separation, c.Z + separation), new Vector3(M.X, c.Y + separation, M.Z));
        }

        /// <summary>
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="vector">Vector</param>
        public static float ProjectToVector(this BoundingBox box, Vector3 vector)
        {
            return ProjectToVector(box.GetExtents(), Matrix4x4.Identity, vector);
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
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="extents">Box extents</param>
        /// <param name="trn">Box transform</param>
        /// <param name="vector">Vector</param>
        public static float ProjectToVector(Vector3 extents, Matrix4x4 trn, Vector3 vector)
        {
            var xAxis = trn.GetRight();
            var yAxis = trn.GetUp();
            var zAxis = trn.GetBackward();

            return
                extents.X * MathF.Abs(Vector3.Dot(vector, xAxis)) +
                extents.Y * MathF.Abs(Vector3.Dot(vector, yAxis)) +
                extents.Z * MathF.Abs(Vector3.Dot(vector, zAxis));
        }
    }
}
