using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Oriented bounding box
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct OrientedBoundingBox : IEquatable<OrientedBoundingBox>, IFormattable
    {
        const string OrientedBoundingBoxStr = "Extents:{0} Transformation:{1}";

        /// <summary>
        /// Extents
        /// </summary>
        [JsonProperty]
        public Vector3 Extents;
        /// <summary>
        /// Transform matrix
        /// </summary>
        [JsonProperty]
        public Matrix4x4 Transformation;

        /// <summary>
        /// Box size
        /// </summary>
        public readonly Vector3 Size => Extents * 2;
        /// <summary>
        /// Box center
        /// </summary>
        public readonly Vector3 Center => Transformation.TranslationVector;

        /// <summary>
        /// Constructor
        /// </summary>
        public OrientedBoundingBox()
        {
            Extents = Vector3.Zero;
            Transformation = new(0);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bb">Bounding box</param>
        public OrientedBoundingBox(BoundingBox bb)
        {
            var center = bb.Minimum + (bb.Maximum - bb.Minimum) / 2f;
            Extents = bb.Maximum - center;
            Transformation = Matrix4x4.Translation(center);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minimum">Minimum point</param>
        /// <param name="maximum">Maximum point</param>
        public OrientedBoundingBox(Vector3 minimum, Vector3 maximum)
        {
            var center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - center;
            Transformation = Matrix4x4.Translation(center);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Point list</param>
        public OrientedBoundingBox(Vector3[] points)
        {
            ArgumentNullException.ThrowIfNull(points);

            Vector3 minimum = new(float.MaxValue);
            Vector3 maximum = new(float.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                minimum = Vector3.Min(minimum, points[i]);
                maximum = Vector3.Max(maximum, points[i]);
            }

            var center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - center;
            Transformation = Matrix4x4.Translation(center);
        }

        /// <summary>
        /// Gets the bounding box of the oriented bounding box after it is transformed
        /// </summary>
        public readonly BoundingBox GetBoundingBox()
        {
            return BoundingBox.FromPoints(GetCorners());
        }

        /// <summary>
        /// Transforms the box
        /// </summary>
        /// <param name="mat">Transform matrix</param>
        public void Transform(Matrix4x4 mat)
        {
            Transformation *= mat;
        }
        /// <summary>
        /// Scales the box
        /// </summary>
        /// <param name="scaling">Scaling vector</param>
        public void Scale(Vector3 scaling)
        {
            Extents *= scaling;
        }
        /// <summary>
        /// Translates the box
        /// </summary>
        /// <param name="translation">Translation vector</param>
        public void Translate(Vector3 translation)
        {
            Transformation.TranslationVector += translation;
        }

        /// <summary>
        /// Gets a oriented bounding box transformed by the given matrix
        /// </summary>
        /// <param name="box">Oriented bounding box</param>
        /// <param name="transform">Transform</param>
        public static OrientedBoundingBox Transform(OrientedBoundingBox box, Matrix4x4 transform)
        {
            if (transform.IsIdentity)
            {
                return box;
            }


            if (!transform.Decompose(out var scale, out var rotation, out var translation))
            {
                return box;
            }

            var trnObb = box;
            trnObb.Scale(scale);
            trnObb.Transformation = Matrix4x4.RotationQuaternion(rotation) * Matrix4x4.Translation(translation);
            return trnObb;
        }

        /// <summary>
        /// Gets the oriented bounding box edge list
        /// </summary>
        /// <returns>Returns the edge list of the current oriented bounding box</returns>
        public readonly Segment[] GetEdges()
        {
            var vertices = GetVertices();

            return BoundingBox.GetEdges(vertices);
        }
        /// <summary>
        /// Gets the oriented bounding box face planes list
        /// </summary>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane[] GetFaces()
        {
            var vertices = GetVertices();

            var edges = BoundingBox.GetEdges(vertices);

            var topNormal = Vector3.Cross(edges[0].Direction, edges[1].Direction);
            var bottomNormal = Vector3.Cross(edges[5].Direction, edges[4].Direction);
            var frontNormal = Vector3.Cross(edges[8].Direction, edges[3].Direction);
            var backNormal = Vector3.Cross(edges[9].Direction, edges[1].Direction);
            var leftNormal = Vector3.Cross(edges[10].Direction, edges[2].Direction);
            var rightNormal = Vector3.Cross(edges[8].Direction, edges[0].Direction);

            return
            [
                new Plane(BoundingBox.GetVertex(vertices, BoxVertices.FrontLeftTop), topNormal),
                new Plane(BoundingBox.GetVertex(vertices, BoxVertices.FrontLeftBottom), bottomNormal),
                new Plane(BoundingBox.GetVertex(vertices, BoxVertices.FrontLeftTop), frontNormal),
                new Plane(BoundingBox.GetVertex(vertices, BoxVertices.BackLeftTop), backNormal),
                new Plane(BoundingBox.GetVertex(vertices, BoxVertices.FrontLeftTop), leftNormal),
                new Plane(BoundingBox.GetVertex(vertices, BoxVertices.FrontRightBottom), rightNormal),
            ];
        }
        /// <summary>
        /// Gets the oriented bounding box face plane
        /// </summary>
        /// <param name="face">Face</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane GetFace(BoxFaces face)
        {
            return GetFace((int)face);
        }
        /// <summary>
        /// Gets the oriented bounding box face plane
        /// </summary>
        /// <param name="faceIndex">Face index</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane GetFace(int faceIndex)
        {
            var faces = GetFaces();

            return BoundingBox.GetFace(faces, faceIndex);
        }

        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertex">Box vertex</param>
        public readonly Vector3 GetVertex(BoxVertices vertex)
        {
            return GetVertex((int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="index">Vertex index</param>
        public readonly Vector3 GetVertex(int index)
        {
            var vertices = GetVertices();

            return BoundingBox.GetVertex(vertices, index);
        }

        /// <summary>
        /// Gets the oriented bounding box vertices
        /// </summary>
        public readonly Vector3[] GetVertices()
        {
            var extents = Extents;
            var trn = Transformation;

            var normal = new Vector3(extents.X, 0f, 0f);
            var normal2 = new Vector3(0f, extents.Y, 0f);
            var normal3 = new Vector3(0f, 0f, extents.Z);
            normal = Vector3.TransformNormal(normal, trn);
            normal2 = Vector3.TransformNormal(normal2, trn);
            normal3 = Vector3.TransformNormal(normal3, trn);
            Vector3 translationVector = trn.TranslationVector;

            return
            [
                translationVector + normal + normal2 + normal3,
                translationVector + normal + normal2 - normal3,
                translationVector - normal + normal2 - normal3,
                translationVector - normal + normal2 + normal3,
                translationVector + normal - normal2 + normal3,
                translationVector + normal - normal2 - normal3,
                translationVector - normal - normal2 - normal3,
                translationVector - normal - normal2 + normal3,
            ];
        }
        /// <summary>
        /// Gets the oriented bounding box corner points
        /// </summary>
        public readonly Vector3[] GetCorners()
        {
            var xv = new Vector3(Extents.X, 0, 0);
            var yv = new Vector3(0, Extents.Y, 0);
            var zv = new Vector3(0, 0, Extents.Z);
            xv = Vector3.TransformNormal(xv, Transformation);
            yv = Vector3.TransformNormal(yv, Transformation);
            zv = Vector3.TransformNormal(zv, Transformation);

            var center = Transformation.TranslationVector;

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
            return ProjectToVector(Extents, Transformation, vector);
        }
        /// <summary>
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="extents">Box extents</param>
        /// <param name="trn">Box transform</param>
        /// <param name="vector">Vector</param>
        public static float ProjectToVector(Vector3 extents, Matrix4x4 trn, Vector3 vector)
        {
            var xAxis = trn.Right;
            var yAxis = trn.Up;
            var zAxis = trn.Backward;

            return
                extents.X * MathF.Abs(Vector3.Dot(vector, xAxis)) +
                extents.Y * MathF.Abs(Vector3.Dot(vector, yAxis)) +
                extents.Z * MathF.Abs(Vector3.Dot(vector, zAxis));
        }

        /// <summary>
        /// Creates an oriented bounding box from point list
        /// </summary>
        /// <param name="points">Point list</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(Vector3[] points)
        {
            return new OrientedBoundingBox([.. points]);
        }
        /// <summary>
        /// Creates an oriented bounding box from a transformed point list and it's transform matrix
        /// </summary>
        /// <param name="points">Point list</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(Vector3[] points, Matrix4x4 transform)
        {
            ArgumentNullException.ThrowIfNull(points);

            if (transform.IsIdentity)
            {
                return new OrientedBoundingBox([.. points]);
            }

            //Next, remove any point transform and set points to origin
            Matrix4x4.Invert(ref transform, out var inv);
            Vector3[] trPoints = new Vector3[points.Length];
            Vector3.TransformCoordinate(points, ref inv, trPoints);
            var obb = new OrientedBoundingBox(trPoints);

            //Apply the original transform to OBB
            obb.Transformation *= transform;

            return obb;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                OrientedBoundingBoxStr,
                Extents.ToString(),
                Transformation.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                OrientedBoundingBoxStr,
                Extents.ToString(format, CultureInfo.CurrentCulture),
                Transformation.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                OrientedBoundingBoxStr,
                Extents.ToString(),
                Transformation.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                OrientedBoundingBoxStr,
                Extents.ToString(format, formatProvider),
                Transformation.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Extents, Transformation);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref OrientedBoundingBox value)
        {
            return
                Extents == value.Extents &&
                Transformation == value.Transformation;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(OrientedBoundingBox value)
        {
            return Equals(ref value);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object obj)
        {
            if (obj is not OrientedBoundingBox) return false;

            var strongValue = (OrientedBoundingBox)obj;
            return Equals(ref strongValue);
        }
    }
}