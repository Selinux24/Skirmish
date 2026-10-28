using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine.Mathematics
{
    /// <summary>
    /// Oriented bounding box
    /// </summary>
    public struct OrientedBoundingBox : IEquatable<OrientedBoundingBox>
    {
        public Vector3 Extents;
        public Matrix4x4 Transformation;

        public readonly Vector3 Size => Extents * 2;
        public readonly Vector3 Center => Transformation.Translation;

        /// <summary>
        /// Creates an oriented bounding box from a transformed point list and it's transform matrix
        /// </summary>
        /// <param name="points">Point list</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(IEnumerable<Vector3> points)
        {
            return FromPoints(points, Matrix4x4.Identity);
        }
        /// <summary>
        /// Creates an oriented bounding box from a transformed point list and it's transform matrix
        /// </summary>
        /// <param name="points">Point list</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(IEnumerable<Vector3> points, Matrix4x4 transform)
        {
            if (transform.IsIdentity)
            {
                return new OrientedBoundingBox([.. points]);
            }

            //First, get item points
            var ptArray = points.ToArray();

            //Next, remove any point transform and set points to origin
            Matrix4x4.Invert(transform, out var inv);
            Vector3Extensions.TransformCoordinate(ptArray, ref inv, ptArray);

            //Create the OBB from origin points
            var obb = new OrientedBoundingBox(ptArray);

            //Apply the original transform to OBB
            obb.Transformation *= transform;

            return obb;
        }

        public OrientedBoundingBox()
        {
            Extents = Vector3.Zero;
            Transformation = Matrix4x4.Create(0);
        }
        public OrientedBoundingBox(BoundingBox bb)
        {
            var Center = bb.Min + (bb.Max - bb.Min) / 2f;
            Extents = bb.Max - Center;
            Transformation = Matrix4x4.CreateTranslation(Center);
        }
        public OrientedBoundingBox(Vector3 minimum, Vector3 maximum)
        {
            var Center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - Center;
            Transformation = Matrix4x4.CreateTranslation(Center);
        }
        public OrientedBoundingBox(Vector3[] points)
        {
            if (points == null || points.Length == 0)
            {
                throw new ArgumentNullException(nameof(points));
            }

            Vector3 minimum = new(float.MaxValue);
            Vector3 maximum = new(float.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                minimum = Vector3.Min(minimum, points[i]);
                maximum = Vector3.Max(maximum, points[i]);
            }

            var Center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - Center;
            Transformation = Matrix4x4.CreateTranslation(Center);
        }

        public readonly BoundingBox GetBoundingBox()
        {
            return BoundingBox.CreateFromPoints(GetCorners());
        }

        public void Transform(Matrix4x4 mat)
        {
            Transformation *= mat;
        }
        public void Scale(Vector3 scaling)
        {
            Extents *= scaling;
        }
        public void Translate(Vector3 translation)
        {
            Transformation.Translation += translation;
        }

        /// <summary>
        /// Gets a oriented bounding box transformed by the given matrix
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="transform">Transform</param>
        public readonly OrientedBoundingBox SetTransform(Matrix4x4 transform)
        {
            if (transform.IsIdentity)
            {
                return this;
            }


            if (!Matrix4x4.Decompose(transform, out var scale, out var rotation, out var translation))
            {
                return this;
            }

            var trnObb = this;
            trnObb.Scale(scale);
            trnObb.Transformation = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
            return trnObb;
        }

        /// <summary>
        /// Gets the oriented bounding box edge list
        /// </summary>
        /// <returns>Returns the edge list of the current oriented bounding box</returns>
        public readonly IEnumerable<Segment> GetEdges()
        {
            return BoundingBoxExtensions.GetEdges(GetVertices());
        }
        /// <summary>
        /// Gets the oriented bounding box face planes list
        /// </summary>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly IEnumerable<Plane> GetFaces()
        {
            var vertices = GetVertices();

            var edges = BoundingBoxExtensions.GetEdges(vertices);

            var topNormal = Vector3.Cross(edges.ElementAt(0).Direction, edges.ElementAt(1).Direction);
            yield return PlaneExtensions.CreatePlane(BoundingBoxExtensions.GetVertex(vertices, BoxVertices.FrontLeftTop), topNormal);

            var bottomNormal = Vector3.Cross(edges.ElementAt(5).Direction, edges.ElementAt(4).Direction);
            yield return PlaneExtensions.CreatePlane(BoundingBoxExtensions.GetVertex(vertices, BoxVertices.FrontLeftBottom), bottomNormal);

            var frontNormal = Vector3.Cross(edges.ElementAt(8).Direction, edges.ElementAt(3).Direction);
            yield return PlaneExtensions.CreatePlane(BoundingBoxExtensions.GetVertex(vertices, BoxVertices.FrontLeftTop), frontNormal);

            var backNormal = Vector3.Cross(edges.ElementAt(9).Direction, edges.ElementAt(1).Direction);
            yield return PlaneExtensions.CreatePlane(BoundingBoxExtensions.GetVertex(vertices, BoxVertices.BackLeftTop), backNormal);

            var leftNormal = Vector3.Cross(edges.ElementAt(10).Direction, edges.ElementAt(2).Direction);
            yield return PlaneExtensions.CreatePlane(BoundingBoxExtensions.GetVertex(vertices, BoxVertices.FrontLeftTop), leftNormal);

            var rightNormal = Vector3.Cross(edges.ElementAt(8).Direction, edges.ElementAt(0).Direction);
            yield return PlaneExtensions.CreatePlane(BoundingBoxExtensions.GetVertex(vertices, BoxVertices.FrontRightBottom), rightNormal);
        }
        /// <summary>
        /// Gets the oriented bounding box face plane
        /// </summary>
        /// <param name="face">Face</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane GetFace(BoxFaces face)
        {
            return BoundingBoxExtensions.GetFace(GetFaces(), face);
        }
        /// <summary>
        /// Gets the oriented bounding box face plane
        /// </summary>
        /// <param name="faceIndex">Face index</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane GetFace(int faceIndex)
        {
            return BoundingBoxExtensions.GetFace(GetFaces(), faceIndex);
        }

        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertex">Box vertex</param>
        public readonly Vector3 GetVertex(BoxVertices vertex)
        {
            return BoundingBoxExtensions.GetVertex(GetVertices(), (int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="index">Vertex index</param>
        public readonly Vector3 GetVertex(int index)
        {
            return BoundingBoxExtensions.GetVertex(GetVertices(), index);
        }

        /// <summary>
        /// Gets the oriented bounding box vertices
        /// </summary>
        public readonly IEnumerable<Vector3> GetVertices()
        {
            var extents = Extents;
            var trn = Transformation;

            var normal = new Vector3(extents.X, 0f, 0f);
            var normal2 = new Vector3(0f, extents.Y, 0f);
            var normal3 = new Vector3(0f, 0f, extents.Z);
            normal = Vector3.TransformNormal(normal, trn);
            normal2 = Vector3.TransformNormal(normal2, trn);
            normal3 = Vector3.TransformNormal(normal3, trn);
            Vector3 translationVector = trn.Translation;

            yield return translationVector + normal + normal2 + normal3;
            yield return translationVector + normal + normal2 - normal3;
            yield return translationVector - normal + normal2 - normal3;
            yield return translationVector - normal + normal2 + normal3;
            yield return translationVector + normal - normal2 + normal3;
            yield return translationVector + normal - normal2 - normal3;
            yield return translationVector - normal - normal2 - normal3;
            yield return translationVector - normal - normal2 + normal3;
        }

        public readonly Vector3[] GetCorners()
        {
            var xv = new Vector3(Extents.X, 0, 0);
            var yv = new Vector3(0, Extents.Y, 0);
            var zv = new Vector3(0, 0, Extents.Z);
            xv = Vector3.TransformNormal(xv, Transformation);
            yv = Vector3.TransformNormal(yv, Transformation);
            zv = Vector3.TransformNormal(zv, Transformation);

            var center = Transformation.Translation;

            var corners = new Vector3[8];
            corners[0] = center + xv + yv + zv;
            corners[1] = center + xv + yv - zv;
            corners[2] = center - xv + yv - zv;
            corners[3] = center - xv + yv + zv;
            corners[4] = center + xv - yv + zv;
            corners[5] = center + xv - yv - zv;
            corners[6] = center - xv - yv - zv;
            corners[7] = center - xv - yv + zv;

            return corners;
        }

        /// <summary>
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="vector">Vector</param>
        public readonly float ProjectToVector(Vector3 vector)
        {
            return BoundingBoxExtensions.ProjectToVector(Extents, Transformation, vector);
        }

        public readonly bool Equals(OrientedBoundingBox other)
        {
            return Extents.Equals(other.Extents)
                && Transformation.Equals(other.Transformation);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is OrientedBoundingBox box && Equals(box);
        }
        public override readonly int GetHashCode() => HashCode.Combine(Extents, Transformation);
        public override readonly string ToString() => base.ToString();

        public static bool operator ==(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return (left.Extents == right.Extents)
                && (left.Transformation == right.Transformation);
        }
        public static bool operator !=(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return (left.Extents != right.Extents)
                || (left.Transformation != right.Transformation);
        }
    }
}