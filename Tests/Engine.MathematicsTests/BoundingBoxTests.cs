using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class BoundingBoxTests
    {
        public TestContext TestContext { get; set; }

        BoundingBox aabb1;
        Vector3[] corners;
        Plane[] faces;

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");

            aabb1 = new(-Vector3.One, Vector3.One);

            corners =
            [
                new(){ X= 1, Y= 1, Z= 1 },
                new(){ X= 1, Y= 1, Z=-1 },
                new(){ X=-1, Y= 1, Z=-1 },
                new(){ X=-1, Y= 1, Z= 1 },
                new(){ X= 1, Y=-1, Z= 1 },
                new(){ X= 1, Y=-1, Z=-1 },
                new(){ X=-1, Y=-1, Z=-1 },
                new(){ X=-1, Y=-1, Z= 1 },
            ];

            faces =
            [
                new(){ Normal=new(){ X= 0, Y= 1, Z= 0 }, D=-1 },
                new(){ Normal=new(){ X= 0, Y=-1, Z= 0 }, D=-1 },
                new(){ Normal=new(){ X= 0, Y= 0, Z= 1 }, D=-1 },
                new(){ Normal=new(){ X= 0, Y= 0, Z=-1 }, D=-1 },
                new(){ Normal=new(){ X=-1, Y= 0, Z= 0 }, D=-1 },
                new(){ Normal=new(){ X= 1, Y= 0, Z= 0 }, D=-1 },
            ];
        }

        [TestMethod()]
        public void VerticesTest()
        {
            var aabbCorners = aabb1.GetVertices();
            Assert.AreSequenceEqual(aabbCorners, corners);
        }
        [TestMethod()]
        public void EdgesTest()
        {
            var aabbFaces = aabb1.GetFaces();
            Assert.AreSequenceEqual(aabbFaces, faces);
        }
        [TestMethod()]
        public void SubdivideOctreeSpanTest()
        {
            BoundingBox box = new(new Vector3(-1), new Vector3(1));
            Span<BoundingBox> subdivisions = stackalloc BoundingBox[8];

            box.SubdivideOctree(subdivisions, 0.1f);

            Assert.AreSequenceEqual(box.SubdivideOctree(0.1f), subdivisions.ToArray());
        }
        private static void AssertVector(Vector3 expected, Vector3 actual) { Assert.AreEqual(expected.X, actual.X, 1e-6f); Assert.AreEqual(expected.Y, actual.Y, 1e-6f); Assert.AreEqual(expected.Z, actual.Z, 1e-6f); }
        [TestMethod] public void ConstructorAndStateTest() { var b = new BoundingBox(new Vector3(-3, -2, -1), new Vector3(4, 5, 6)); AssertVector(new Vector3(-3, -2, -1), b.Minimum); AssertVector(new Vector3(4, 5, 6), b.Maximum); b.Minimum = new Vector3(-7, -8, -9); AssertVector(new Vector3(-7, -8, -9), b.Minimum); BoundingBox d = default; AssertVector(Vector3.Zero, d.Minimum); AssertVector(Vector3.Zero, d.Maximum); }
        [TestMethod] public void DimensionsAndCenterTest() { Assert.AreEqual(2f, aabb1.Width, 1e-6f); Assert.AreEqual(2f, aabb1.Height, 1e-6f); Assert.AreEqual(2f, aabb1.Depth, 1e-6f); AssertVector(new Vector3(2, 2, 2), aabb1.Size); AssertVector(Vector3.Zero, aabb1.Center); AssertVector(Vector3.One, aabb1.Extents); var b = new BoundingBox(new Vector3(-3, 2, 1), new Vector3(5, 8, 10)); Assert.AreEqual(8f, b.Width, 1e-6f); Assert.AreEqual(6f, b.Height, 1e-6f); Assert.AreEqual(9f, b.Depth, 1e-6f); }
        [TestMethod] public void CornersTest() { Assert.AreSequenceEqual([new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, -1, -1), new Vector3(-1, -1, -1)], aabb1.GetCorners()); var buffer = new Vector3[8]; aabb1.GetCorners(buffer); Assert.AreSequenceEqual([new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, -1, -1), new Vector3(-1, -1, -1)], buffer); Assert.ThrowsExactly<ArgumentNullException>(() => aabb1.GetCorners(null)); Assert.ThrowsExactly<ArgumentException>(() => aabb1.GetCorners(new Vector3[7])); }
        [TestMethod] public void FacesTest() { Assert.AreSequenceEqual(faces, aabb1.GetFaces()); for (int i = 0; i < faces.Length; i++) Assert.AreEqual(faces[i], aabb1.GetFace(i)); Assert.AreEqual(aabb1.GetFace(BoxFaces.Right), aabb1.GetFace(5)); Assert.ThrowsExactly<ArgumentException>(() => aabb1.GetFace(6)); }
        [TestMethod] public void VertexOverloadsTest() { foreach (BoxVertices v in Enum.GetValues<BoxVertices>()) Assert.AreEqual(aabb1.GetVertex((int)v), aabb1.GetVertex(v)); Assert.ThrowsExactly<ArgumentException>(() => aabb1.GetVertex(8)); Assert.ThrowsExactly<ArgumentNullException>(() => BoundingBox.GetVertex(null, 0)); }
        [TestMethod] public void InstanceEdgesTest() { Assert.HasCount(12, aabb1.GetEdges()); Assert.HasCount(4, aabb1.GetEdgesTop()); Assert.HasCount(4, aabb1.GetEdgesBottom()); Assert.HasCount(4, aabb1.GetEdgesVertical()); Assert.AreSequenceEqual(aabb1.GetEdges(), BoundingBox.GetEdges(aabb1.GetVertices())); }
        [TestMethod] public void RectanglesTest() { var xy = aabb1.GetRectangleXY(); Assert.AreEqual(-1f, xy.Left); Assert.AreEqual(-1f, xy.Top); Assert.AreEqual(1f, xy.Right); Assert.AreEqual(1f, xy.Bottom); var xz = aabb1.GetRectangleXZ(); Assert.AreEqual(-1f, xz.Left); Assert.AreEqual(-1f, xz.Top); Assert.AreEqual(1f, xz.Right); Assert.AreEqual(1f, xz.Bottom); }
        [TestMethod] public void NormalizeTest() { var r = new BoundingBox(new Vector3(5, -2, 7), new Vector3(-4, 9, -3)); var n = r.Normalize(); AssertVector(new Vector3(-4, -2, -3), n.Minimum); AssertVector(new Vector3(5, 9, 7), n.Maximum); AssertVector(new Vector3(5, -2, 7), r.Minimum); }
        [TestMethod] public void RayIntersectsTest() { Ray hit = new(new Vector3(-3, 0, 0), Vector3.UnitX); Assert.IsTrue(aabb1.Intersects(ref hit)); Assert.IsTrue(aabb1.Intersects(ref hit, out float d)); Assert.AreEqual(2f, d, 1e-5f); Assert.IsTrue(aabb1.Intersects(ref hit, out Vector3 p)); AssertVector(new Vector3(-1, 0, 0), p); Ray miss = new(new Vector3(0, 3, 0), Vector3.UnitX); Assert.IsFalse(aabb1.Intersects(ref miss)); Assert.IsFalse(aabb1.Intersects(ref miss, out d)); Assert.AreEqual(float.MaxValue, d); Assert.IsFalse(aabb1.Intersects(ref miss, out p)); AssertVector(Vector3.Zero, p); Ray inside = new(Vector3.Zero, Vector3.UnitX); Assert.IsTrue(aabb1.Intersects(ref inside)); }
        [TestMethod] public void PlaneAndShapeIntersectsTest() { var plane = new Plane(Vector3.UnitX, 0f); Assert.AreEqual(PlaneIntersectionType.Intersecting, aabb1.Intersects(ref plane)); var touch = new BoundingBox(new Vector3(1, -1, -1), new Vector3(2, 1, 1)); Assert.IsTrue(aabb1.Intersects(touch)); Assert.IsTrue(aabb1.Intersects(ref touch)); var far = new BoundingBox(new Vector3(3, 3, 3), new Vector3(4, 4, 4)); Assert.IsFalse(aabb1.Intersects(far)); var sphere = new BoundingSphere(Vector3.Zero, .5f); Assert.IsTrue(aabb1.Intersects(sphere)); Assert.IsTrue(aabb1.Intersects(ref sphere)); Assert.IsFalse(aabb1.Intersects(new BoundingSphere(new Vector3(3, 0, 0), .5f))); }
        [TestMethod] public void ContainsVectorTest() { foreach (var point in new[] { Vector3.Zero, new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(.5f, 0, -.5f) }) { var copy = point; Assert.AreEqual(aabb1.Contains(copy), aabb1.Contains(ref copy)); } var outside = new Vector3(2, 0, 0); Assert.AreEqual(ContainmentType.Disjoint, aabb1.Contains(outside)); }
        [TestMethod] public void ContainsBoxTest() { var inside = new BoundingBox(new Vector3(-.5f), new Vector3(.5f)); Assert.AreEqual(ContainmentType.Contains, aabb1.Contains(inside)); Assert.AreEqual(aabb1.Contains(ref inside), aabb1.Contains(inside)); var equal = aabb1; Assert.AreEqual(ContainmentType.Contains, aabb1.Contains(equal)); var outside = new BoundingBox(new Vector3(2, 2, 2), new Vector3(3, 3, 3)); Assert.AreEqual(ContainmentType.Disjoint, aabb1.Contains(outside)); var partial = new BoundingBox(new Vector3(.5f, -2, -2), new Vector3(2, 2, 2)); Assert.AreEqual(ContainmentType.Intersects, aabb1.Contains(partial)); }
        [TestMethod] public void ContainsSphereTest() { var inside = new BoundingSphere(Vector3.Zero, .5f); Assert.AreEqual(ContainmentType.Contains, aabb1.Contains(inside)); Assert.AreEqual(aabb1.Contains(ref inside), aabb1.Contains(inside)); var outside = new BoundingSphere(new Vector3(3, 0, 0), .5f); Assert.AreEqual(ContainmentType.Disjoint, aabb1.Contains(outside)); var partial = new BoundingSphere(new Vector3(1, 0, 0), 1f); Assert.AreEqual(ContainmentType.Intersects, aabb1.Contains(partial)); }
        [TestMethod] public void InstanceAndStaticProjectionTest() { Assert.AreEqual(1f, aabb1.ProjectToVector(Vector3.UnitX), 1e-6f); Assert.AreEqual(0f, BoundingBox.ProjectToVector(Vector3.Zero, Vector3.UnitX), 1e-6f); Assert.AreEqual(BoundingBox.ProjectToVector(Vector3.One, Vector3.UnitX), aabb1.ProjectToVector(Vector3.UnitX), 1e-6f); }
        [TestMethod] public void SubdivideQuadtreeTest() { Assert.HasCount(4, aabb1.SubdivideQuadtree()); Assert.HasCount(4, aabb1.SubdivideQuadtree(true, .1f)); foreach (var cell in aabb1.SubdivideQuadtree(false, .25f)) Assert.AreNotEqual(ContainmentType.Disjoint, aabb1.Contains(cell)); }
        [TestMethod] public void SubdivideOctreeSpanValidationTest() { var boxes = aabb1.SubdivideOctree(.1f); Assert.HasCount(8, boxes); Span<BoundingBox> span = stackalloc BoundingBox[8]; aabb1.SubdivideOctree(span, .1f); Assert.AreSequenceEqual(boxes, span.ToArray()); Assert.ThrowsExactly<ArgumentException>(() => aabb1.SubdivideOctree(stackalloc BoundingBox[7], .1f)); }
        [TestMethod] public void FromPointsTest() { var p = new[] { new Vector3(-5, 2, 0), new Vector3(7, -3, 4), new Vector3(1, 9, -8) }; BoundingBox.FromPoints(p, out var ob); var rb = BoundingBox.FromPoints(p); Assert.AreEqual(ob, rb); AssertVector(new Vector3(-5, -3, -8), rb.Minimum); AssertVector(new Vector3(7, 9, 4), rb.Maximum); Assert.ThrowsExactly<ArgumentNullException>(() => BoundingBox.FromPoints(null)); Assert.ThrowsExactly<ArgumentNullException>(() => { BoundingBox.FromPoints(null, out _); }); }
        [TestMethod] public void FromSphereTest() { var s = new BoundingSphere(new Vector3(2, -3, 4), 5f); BoundingBox.FromSphere(ref s, out var ob); var b = BoundingBox.FromSphere(s); Assert.AreEqual(ob, b); AssertVector(new Vector3(-3, -8, -1), b.Minimum); AssertVector(new Vector3(7, 2, 9), b.Maximum); }
        [TestMethod] public void MergeRefAndValueTest() { var a = new BoundingBox(new Vector3(-5, 0, 0), new Vector3(-1, 2, 2)); var b = new BoundingBox(new Vector3(1, -2, -2), new Vector3(4, 3, 5)); BoundingBox.Merge(ref a, ref b, out var r); var v = BoundingBox.Merge(a, b); Assert.AreEqual(r, v); AssertVector(new Vector3(-5, -2, -2), v.Minimum); AssertVector(new Vector3(4, 3, 5), v.Maximum); }
        [TestMethod] public void MergeArrayTest() { var boxes = new[] { new BoundingBox(new Vector3(-4, 1, 0), new Vector3(-2, 3, 2)), new BoundingBox(new Vector3(1, -5, -1), new Vector3(6, 2, 8)) }; var m = BoundingBox.Merge(boxes); AssertVector(new Vector3(-4, -5, -1), m.Minimum); AssertVector(new Vector3(6, 3, 8), m.Maximum); AssertVector(Vector3.Zero, BoundingBox.Merge([]).Minimum); Assert.ThrowsExactly<ArgumentNullException>(() => BoundingBox.Merge(null)); }
        [TestMethod] public void TransformTest() { Assert.AreEqual(aabb1, BoundingBox.Transform(aabb1, Matrix4x4.Identity)); var t = BoundingBox.Transform(aabb1, Matrix4x4.Translation(10, -2, 3)); AssertVector(new Vector3(9, -3, 2), t.Minimum); AssertVector(new Vector3(11, -1, 4), t.Maximum); var s = BoundingBox.Transform(aabb1, Matrix4x4.Scaling(2, 3, 4)); AssertVector(new Vector3(-2, -3, -4), s.Minimum); AssertVector(new Vector3(2, 3, 4), s.Maximum); }
        [TestMethod] public void StaticEdgesTest() { var v = aabb1.GetVertices(); Assert.HasCount(12, BoundingBox.GetEdges(v)); Assert.HasCount(4, BoundingBox.GetEdgesTop(v)); Assert.HasCount(4, BoundingBox.GetEdgesBottom(v)); Assert.HasCount(4, BoundingBox.GetEdgesVertical(v)); Assert.ThrowsExactly<ArgumentNullException>(() => BoundingBox.GetEdges(null)); Assert.ThrowsExactly<ArgumentException>(() => BoundingBox.GetEdges(new Vector3[7])); }
        [TestMethod] public void StaticVertexAndFaceTest() { var v = aabb1.GetVertices(); var f = aabb1.GetFaces(); AssertVector(v[3], BoundingBox.GetVertex(v, BoxVertices.FrontLeftTop)); AssertVector(v[3], BoundingBox.GetVertex(v, 3)); Assert.AreEqual(f[5], BoundingBox.GetFace(f, BoxFaces.Right)); Assert.AreEqual(f[5], BoundingBox.GetFace(f, 5)); Assert.ThrowsExactly<ArgumentException>(() => BoundingBox.GetVertex(v, 8)); Assert.ThrowsExactly<ArgumentException>(() => BoundingBox.GetFace(f, 6)); }
        [TestMethod] public void EqualityAndNearEqualTest() { var same = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1)); var shifted = new BoundingBox(new Vector3(-1.01f, -1, -1), new Vector3(1, 1, 1)); Assert.IsTrue(aabb1 == same); Assert.IsFalse(aabb1 != same); Assert.IsTrue(aabb1.Equals(same)); Assert.IsTrue(aabb1.Equals((object)same)); Assert.IsFalse(aabb1.Equals((object)null)); Assert.AreEqual(aabb1.GetHashCode(), same.GetHashCode()); Assert.IsTrue(BoundingBox.NearEqual(aabb1, shifted, .02f)); Assert.IsFalse(BoundingBox.NearEqual(aabb1, shifted, .005f)); Assert.IsTrue(aabb1.Equals(ref same)); }
        [TestMethod] public void FormattingTest() { var text = aabb1.ToString(); Assert.Contains("Minimum:", text); Assert.Contains("Maximum:", text); Assert.Contains("Minimum:", aabb1.ToString("F1")); var en = CultureInfo.GetCultureInfo("en-US"); var fr = CultureInfo.GetCultureInfo("fr-FR"); Assert.AreNotEqual(aabb1.ToString("F1", en), aabb1.ToString("F1", fr)); Assert.IsNotNull(aabb1.ToString(en)); Assert.IsNotNull(aabb1.ToString("F2", en)); }
        [TestMethod] public void DefaultValueTest() { BoundingBox b = default; Assert.AreEqual(0f, b.Width); Assert.AreEqual(0f, b.Height); Assert.AreEqual(0f, b.Depth); AssertVector(Vector3.Zero, b.Size); AssertVector(Vector3.Zero, b.Center); AssertVector(Vector3.Zero, b.Extents); Vector3 p = Vector3.Zero; Assert.AreEqual(ContainmentType.Contains, b.Contains(p)); }
    }
}
