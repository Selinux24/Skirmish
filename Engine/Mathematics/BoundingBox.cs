// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Represents an axis-aligned bounding box in three dimensional space.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
    /// </remarks>
    /// <param name="minimum">The minimum vertex of the bounding box.</param>
    /// <param name="maximum">The maximum vertex of the bounding box.</param>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoundingBox(Vector3 minimum, Vector3 maximum) : IEquatable<BoundingBox>, IFormattable
    {
        const string BoundingBoxStr = "Minimum:{0} Maximum:{1}";

        /// <summary>
        /// The minimum point of the box.
        /// </summary>
        public Vector3 Minimum = minimum;
        /// <summary>
        /// The maximum point of the box.
        /// </summary>
        public Vector3 Maximum = maximum;
        /// <summary>
        /// Returns the width of the bounding box
        /// </summary>
        public readonly float Width => Maximum.X - Minimum.X;
        /// <summary>
        /// Returns the height of the bounding box
        /// </summary>
        public readonly float Height => Maximum.Y - Minimum.Y;
        /// <summary>
        /// Returns the height of the bounding box
        /// </summary>
        public readonly float Depth => Maximum.Z - Minimum.Z;
        /// <summary>
        /// Returns the size of the bounding box
        /// </summary>
        public readonly Vector3 Size => Maximum - Minimum;
        /// <summary>
        /// Returns the size of the bounding box
        /// </summary>
        public readonly Vector3 Center => (Maximum + Minimum) * 0.5f;
        /// <summary>
        /// Gets the bounding box extents
        /// </summary>
        public readonly Vector3 Extents => Maximum - Center;

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public readonly Vector3[] GetCorners()
        {
            Vector3[] results = new Vector3[8];
            GetCorners(results);
            return results;
        }
        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public readonly void GetCorners(Vector3[] corners)
        {
            corners[0] = new Vector3(Minimum.X, Maximum.Y, Maximum.Z);
            corners[1] = new Vector3(Maximum.X, Maximum.Y, Maximum.Z);
            corners[2] = new Vector3(Maximum.X, Minimum.Y, Maximum.Z);
            corners[3] = new Vector3(Minimum.X, Minimum.Y, Maximum.Z);
            corners[4] = new Vector3(Minimum.X, Maximum.Y, Minimum.Z);
            corners[5] = new Vector3(Maximum.X, Maximum.Y, Minimum.Z);
            corners[6] = new Vector3(Maximum.X, Minimum.Y, Minimum.Z);
            corners[7] = new Vector3(Minimum.X, Minimum.Y, Minimum.Z);
        }
        /// <summary>
        /// Gets the bounding box face planes list
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly IEnumerable<Plane> GetFaces()
        {
            var vertices = GetVertices();

            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3Extensions.Up);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftBottom), Vector3Extensions.Down);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3Extensions.ForwardLH);
            yield return new Plane(GetVertex(vertices, BoxVertices.BackLeftTop), Vector3Extensions.BackwardLH);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontLeftTop), Vector3Extensions.Left);
            yield return new Plane(GetVertex(vertices, BoxVertices.FrontRightBottom), Vector3Extensions.Right);
        }
        /// <summary>
        /// Gets the bounding box face plane
        /// </summary>
        /// <param name="face">Face</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane GetFace(BoxFaces face)
        {
            return GetFace(GetFaces(), face);
        }
        /// <summary>
        /// Gets the bounding box face plane
        /// </summary>
        /// <param name="faceIndex">Face index</param>
        /// <returns>Returns the face list of the current bounding box</returns>
        public readonly Plane GetFace(int faceIndex)
        {
            return GetFace(GetFaces(), faceIndex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertex">Box vertex</param>
        public readonly Vector3 GetVertex(BoxVertices vertex)
        {
            return GetVertex(GetVertices(), (int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="index">Vertex index</param>
        public readonly Vector3 GetVertex(int index)
        {
            return GetVertex(GetVertices(), index);
        }
        /// <summary>
        /// Gets the bounding box edge list
        /// </summary>
        /// <returns>Returns the edge list of the current bounding box</returns>
        public readonly IEnumerable<Segment> GetEdges()
        {
            return GetEdges(GetVertices());
        }
        /// <summary>
        /// Gets the bounding box vertices
        /// </summary>
        public readonly IEnumerable<Vector3> GetVertices()
        {
            yield return new Vector3(Maximum.X, Maximum.Y, Maximum.Z);
            yield return new Vector3(Maximum.X, Maximum.Y, Minimum.Z);
            yield return new Vector3(Minimum.X, Maximum.Y, Minimum.Z);
            yield return new Vector3(Minimum.X, Maximum.Y, Maximum.Z);
            yield return new Vector3(Maximum.X, Minimum.Y, Maximum.Z);
            yield return new Vector3(Maximum.X, Minimum.Y, Minimum.Z);
            yield return new Vector3(Minimum.X, Minimum.Y, Minimum.Z);
            yield return new Vector3(Minimum.X, Minimum.Y, Maximum.Z);
        }
        /// <summary>
        /// Gets the XY rectangle of the box
        /// </summary>
        /// <returns>Returns the XY rectangle of the box</returns>
        public Vortice.Mathematics.Rect GetRectangleXY()
        {
            return new()
            {
                Left = Minimum.X,
                Top = Minimum.Y,
                Right = Maximum.X,
                Bottom = Maximum.Y,
            };
        }
        /// <summary>
        /// Gets the XZ rectangle of the box
        /// </summary>
        /// <returns>Returns the XZ rectangle of the box</returns>
        public Vortice.Mathematics.Rect GetRectangleXZ()
        {
            return new()
            {
                Left = Minimum.X,
                Top = Minimum.Z,
                Right = Maximum.X,
                Bottom = Maximum.Z,
            };
        }

        /// <summary>
        /// Normalizes the minimum and maximum bounding box limits.
        /// </summary>
        /// <returns>Returns a bounding box which assures that the minimum vector contains the minimum values, and the maximum vector contains the maximum values</returns>
        public readonly BoundingBox Normalize()
        {
            var newBox = this;

            var min = newBox.Minimum;
            var max = newBox.Maximum;
            newBox.Minimum = new(MathF.Min(max.X, min.X), MathF.Min(max.Y, min.Y), MathF.Min(max.Z, min.Z));
            newBox.Maximum = new(MathF.Max(max.X, min.X), MathF.Max(max.Y, min.Y), MathF.Max(max.Z, min.Z));

            return newBox;
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray)
        {
            return Collision.RayIntersectsBox(ref ray, ref this, out float _);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out float distance)
        {
            return Collision.RayIntersectsBox(ref ray, ref this, out distance);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out Vector3 point)
        {
            return Collision.RayIntersectsBox(ref ray, ref this, out point);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref Plane plane)
        {
            return Collision.PlaneIntersectsBox(ref plane, ref this);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingBox box)
        {
            return Collision.BoxIntersectsBox(ref this, ref box);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(BoundingBox box)
        {
            return Intersects(ref box);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingSphere sphere)
        {
            return Collision.BoxIntersectsSphere(ref this, ref sphere);
        }
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(BoundingSphere sphere)
        {
            return Intersects(ref sphere);
        }

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3 point)
        {
            return Collision.BoxContainsPoint(ref this, ref point);
        }
        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }
        /// <summary>
        /// Determines whether the current objects contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref BoundingBox box)
        {
            return Collision.BoxContainsBox(ref this, ref box);
        }
        /// <summary>
        /// Determines whether the current objects contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(BoundingBox box)
        {
            return Contains(ref box);
        }
        /// <summary>
        /// Determines whether the current objects contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            return Collision.BoxContainsSphere(ref this, ref sphere);
        }
        /// <summary>
        /// Determines whether the current objects contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }

        /// <summary>
        /// Constructs a <see cref="BoundingBox"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
        public static void FromPoints(IEnumerable<Vector3> points, out BoundingBox result)
        {
            ArgumentNullException.ThrowIfNull(points);

            Vector3 min = new(float.MaxValue);
            Vector3 max = new(float.MinValue);

            foreach (var p in points)
            {
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            result = new BoundingBox(min, max);
        }
        /// <summary>
        /// Constructs a <see cref="BoundingBox"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <returns>The newly constructed bounding box.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
        public static BoundingBox FromPoints(IEnumerable<Vector3> points)
        {
            FromPoints(points, out var bbox);
            return bbox;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingBox"/> from a given sphere.
        /// </summary>
        /// <param name="sphere">The sphere that will designate the extents of the box.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        public static void FromSphere(ref BoundingSphere sphere, out BoundingBox result)
        {
            result.Minimum = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
            result.Maximum = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
        }
        /// <summary>
        /// Constructs a <see cref="BoundingBox"/> from a given sphere.
        /// </summary>
        /// <param name="sphere">The sphere that will designate the extents of the box.</param>
        /// <returns>The newly constructed bounding box.</returns>
        public static BoundingBox FromSphere(BoundingSphere sphere)
        {
            BoundingBox box;
            box.Minimum = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
            box.Maximum = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
            return box;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the two specified boxes.
        /// </summary>
        /// <param name="value1">The first box to merge.</param>
        /// <param name="value2">The second box to merge.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        public static void Merge(ref BoundingBox value1, ref BoundingBox value2, out BoundingBox result)
        {
            result.Minimum = Vector3.Min(value1.Minimum, value2.Minimum);
            result.Maximum = Vector3.Max(value1.Maximum, value2.Maximum);
        }
        /// <summary>
        /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the two specified boxes.
        /// </summary>
        /// <param name="value1">The first box to merge.</param>
        /// <param name="value2">The second box to merge.</param>
        /// <returns>The newly constructed bounding box.</returns>
        public static BoundingBox Merge(BoundingBox value1, BoundingBox value2)
        {
            BoundingBox box;
            box.Minimum = Vector3.Min(value1.Minimum, value2.Minimum);
            box.Maximum = Vector3.Max(value1.Maximum, value2.Maximum);
            return box;
        }
        /// <summary>
        /// Merges the array of bounding boxes into one bounding box
        /// </summary>
        /// <param name="boxes">Array of boxes</param>
        /// <returns>Returns a bounding box containing all the boxes in the array</returns>
        public static BoundingBox Merge(IEnumerable<BoundingBox> boxes)
        {
            var fbbox = new BoundingBox();

            if (boxes?.Any() != true)
            {
                return fbbox;
            }

            var defaultBox = new BoundingBox();

            foreach (var bbox in boxes.Where(b => b != defaultBox))
            {
                fbbox = Merge(fbbox, bbox);
            }

            return fbbox;
        }

        /// <summary>
        /// Gets a box transformed by the given matrix
        /// </summary>
        /// <param name="bbox">Box</param>
        /// <param name="transform">Transform</param>
        public static BoundingBox Transform(BoundingBox bbox, Matrix4x4 transform)
        {
            if (transform.IsIdentity)
            {
                return bbox;
            }

            if (!transform.Decompose(out var scale, out _, out var translation))
            {
                return bbox;
            }

            var extents = bbox.Extents;
            var center = bbox.Center;

            extents *= scale;
            center = Vector3.Transform(center, Matrix4x4.Translation(translation));

            // Gets the new position
            var trnBox = new BoundingBox(-extents + center, +extents + center);

            return trnBox.Normalize();
        }

        /// <summary>
        /// Projects the specified box to the given vector
        /// </summary>
        /// <param name="vector">Vector</param>
        public readonly float ProjectToVector(Vector3 vector)
        {
            return ProjectToVector(Extents, Matrix4x4.Identity, vector);
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
        /// Generates a list of four bounding boxes which makes an QuadTree subdivision of the specified bounding box
        /// </summary>
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
        public readonly IEnumerable<BoundingBox> SubdivideQuadtree(bool separateBothAxis = false, float separation = float.Epsilon)
        {
            var M = Maximum;
            var m = Minimum;
            var c = Center;
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
        public readonly IEnumerable<BoundingBox> SubdivideOctree(float separation = float.Epsilon)
        {
            var m = Minimum;
            var M = Maximum;
            var c = Center;

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

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingBoxStr,
                Minimum.ToString(),
                Maximum.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingBoxStr,
                Minimum.ToString(format, CultureInfo.CurrentCulture),
                Maximum.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                BoundingBoxStr,
                Minimum.ToString(),
                Maximum.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                BoundingBoxStr,
                Minimum.ToString(format, formatProvider),
                Maximum.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Minimum, Maximum);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref BoundingBox value)
        {
            return
                Minimum == value.Minimum &&
                Maximum == value.Maximum;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingBox value)
        {
            return Equals(ref value);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object value)
        {
            if (value is not BoundingBox) return false;

            var strongValue = (BoundingBox)value;
            return Equals(ref strongValue);
        }
    }
}
