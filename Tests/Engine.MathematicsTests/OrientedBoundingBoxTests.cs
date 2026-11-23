using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class OrientedBoundingBoxTests
    {
        public TestContext TestContext { get; set; }

        OrientedBoundingBox obb1;
        Vector3[] corners;
        Plane[] faces;

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");

            obb1 = new(-Vector3.One, Vector3.One);

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
            var aabbCorners = obb1.GetVertices();
            Assert.AreSequenceEqual(aabbCorners, corners);
        }
        [TestMethod()]
        public void EdgesTest()
        {
            var aabbFaces = obb1.GetFaces();
            Assert.AreSequenceEqual(aabbFaces, faces);
        }
        const float Tolerance = 0.0001f;

        static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.X, actual.X, Tolerance);
            Assert.AreEqual(expected.Y, actual.Y, Tolerance);
            Assert.AreEqual(expected.Z, actual.Z, Tolerance);
        }

        [TestMethod]
        public void Constructors_ExposeExpectedCenterExtentsAndBounds()
        {
            var empty = new OrientedBoundingBox();
            Assert.AreEqual(Vector3.Zero, empty.Extents);
            Assert.AreEqual(Matrix4x4.Zero, empty.Transformation);
            Assert.AreEqual(default, default(OrientedBoundingBox));

            var source = new BoundingBox(new Vector3(-3, 2, -5), new Vector3(5, 8, 1));
            var fromBounds = new OrientedBoundingBox(source);
            AssertVector(new Vector3(4, 3, 3), fromBounds.Extents);
            AssertVector(new Vector3(1, 5, -2), fromBounds.Center);
            AssertVector(new Vector3(8, 6, 6), fromBounds.Size);
            Assert.AreEqual(source.Minimum, fromBounds.GetBoundingBox().Minimum);
            Assert.AreEqual(source.Maximum, fromBounds.GetBoundingBox().Maximum);

            var asymmetric = new OrientedBoundingBox(new Vector3(-4, 1, 2), new Vector3(2, 7, 10));
            AssertVector(new Vector3(3, 3, 4), asymmetric.Extents);
            AssertVector(new Vector3(-1, 4, 6), asymmetric.Center);
            asymmetric.Extents = new Vector3(2, 1, 3);
            asymmetric.Transformation = Matrix4x4.Translation(7, 8, 9);
            AssertVector(new Vector3(2, 1, 3), asymmetric.Extents);
            AssertVector(new Vector3(7, 8, 9), asymmetric.Center);
        }

        [TestMethod]
        public void PointConstructionAndFactories_EncloseExpectedCloudAndRejectNull()
        {
            Vector3[] points = [new(-3, 2, 1), new(5, -4, 7), new(1, 6, -5), new(5, 0, 3)];
            var constructed = new OrientedBoundingBox(points);
            var factory = OrientedBoundingBox.FromPoints(points);
            AssertVector(new Vector3(4, 5, 6), constructed.Extents);
            AssertVector(new Vector3(1, 1, 1), constructed.Center);
            Assert.AreEqual(factory, constructed);
            foreach (var point in points)
            {
                var bounds = constructed.GetBoundingBox();
                Assert.IsTrue(point.X >= bounds.Minimum.X && point.X <= bounds.Maximum.X);
                Assert.IsTrue(point.Y >= bounds.Minimum.Y && point.Y <= bounds.Maximum.Y);
                Assert.IsTrue(point.Z >= bounds.Minimum.Z && point.Z <= bounds.Maximum.Z);
            }
            var single = OrientedBoundingBox.FromPoints([new Vector3(2, -1, 4)]);
            Assert.AreEqual(Vector3.Zero, single.Extents);
            AssertVector(new Vector3(2, -1, 4), single.Center);
            Assert.ThrowsExactly<ArgumentNullException>(() => new OrientedBoundingBox((Vector3[])null));
            Assert.ThrowsExactly<ArgumentNullException>(() => OrientedBoundingBox.FromPoints(null));
            var empty = OrientedBoundingBox.FromPoints([]);
            Assert.IsTrue(float.IsInfinity(empty.Extents.X));
            var emptyConstructor = new OrientedBoundingBox([]);
            Assert.IsTrue(float.IsInfinity(emptyConstructor.Extents.X));
        }

        [TestMethod]
        public void TransformScaleAndTranslate_UpdateCornersAndBounds()
        {
            var box = new OrientedBoundingBox(new Vector3(-1, -2, -3), new Vector3(1, 2, 3));
            box.Scale(new Vector3(2, 0.5f, 3));
            box.Translate(new Vector3(5, -2, 4));
            AssertVector(new Vector3(4, 2, 18), box.Size);
            AssertVector(new Vector3(5, -2, 4), box.Center);
            var bounds = box.GetBoundingBox();
            AssertVector(new Vector3(3, -3, -5), bounds.Minimum);
            AssertVector(new Vector3(7, -1, 13), bounds.Maximum);

            var original = new OrientedBoundingBox(new Vector3(-1), new Vector3(1));
            var unchanged = original;
            var matrix = Matrix4x4.RotationY(MathF.PI / 2) * Matrix4x4.Translation(3, 4, 5);
            var expected = OrientedBoundingBox.Transform(original, matrix);
            var instance = original;
            instance.Transform(matrix);
            Assert.AreEqual(expected.Center, instance.Center);
            AssertVector(expected.Extents, instance.Extents);
            Assert.AreEqual(new Vector3(3, 4, 5), expected.Center);
            var identityResult = OrientedBoundingBox.Transform(original, Matrix4x4.Identity);
            Assert.AreEqual(original, identityResult);
            Assert.AreEqual(original, unchanged);
            Assert.HasCount(8, expected.GetBoundingBox().GetCorners());
        }

        [TestMethod]
        public void VerticesCornersEdgesAndFaces_ExposeConsistentGeometry()
        {
            var vertices = obb1.GetVertices();
            var cornersResult = obb1.GetCorners();
            Assert.AreSequenceEqual(vertices, cornersResult);
            Assert.HasCount(8, vertices);
            for (int i = 0; i < vertices.Length; i++)
            {
                Assert.AreEqual(vertices[i], obb1.GetVertex(i));
                Assert.AreEqual(vertices[i], obb1.GetVertex((BoxVertices)i));
            }
            var edges = obb1.GetEdges();
            Assert.HasCount(12, edges);
            foreach (var edge in edges)
            {
                Assert.IsTrue(Array.Exists(vertices, v => v == edge.Point1));
                Assert.IsTrue(Array.Exists(vertices, v => v == edge.Point2));
                Assert.IsGreaterThan(0, edge.Length);
            }
            var allFaces = obb1.GetFaces();
            Assert.HasCount(6, allFaces);
            for (int i = 0; i < allFaces.Length; i++)
            {
                Assert.AreEqual(allFaces[i].Normal, obb1.GetFace(i).Normal);
                Assert.AreEqual(allFaces[i].D, obb1.GetFace((BoxFaces)i).D, Tolerance);
            }
            Assert.ThrowsExactly<ArgumentException>(() => obb1.GetFace(6));
            Assert.ThrowsExactly<IndexOutOfRangeException>(() => obb1.GetFace(-1));
            Assert.ThrowsExactly<ArgumentException>(() => obb1.GetVertex(8));
            Assert.ThrowsExactly<IndexOutOfRangeException>(() => obb1.GetVertex(-1));
            Assert.ThrowsExactly<ArgumentException>(() => obb1.GetFace((BoxFaces)6));
            Assert.ThrowsExactly<ArgumentException>(() => obb1.GetVertex((BoxVertices)8));
        }

        [TestMethod]
        public void Projection_StaticAndInstanceFormsMatchKnownSupportValues()
        {
            var box = new OrientedBoundingBox(new Vector3(-2, -3, -4), new Vector3(2, 3, 4));
            Assert.AreEqual(2, box.ProjectToVector(Vector3.Right), Tolerance);
            Assert.AreEqual(3, box.ProjectToVector(Vector3.Up), Tolerance);
            Assert.AreEqual(4, box.ProjectToVector(Vector3.BackwardLH), Tolerance);
            Assert.AreEqual(9, box.ProjectToVector(Vector3.One), Tolerance);
            Assert.AreEqual(0, box.ProjectToVector(Vector3.Zero), Tolerance);
            Assert.AreEqual(box.ProjectToVector(Vector3.One), OrientedBoundingBox.ProjectToVector(box.Extents, box.Transformation, Vector3.One), Tolerance);
            box.Transform(Matrix4x4.RotationY(MathF.PI / 2));
            Assert.AreEqual(4, box.ProjectToVector(Vector3.Right), Tolerance);
            Assert.AreEqual(2, box.ProjectToVector(Vector3.BackwardLH), Tolerance);
        }

        [TestMethod]
        public void FromPoints_WithTransform_ProducesTransformedEnclosure()
        {
            Vector3[] points = [new(4, -6, -1), new(6, -2, 5)];
            var transform = Matrix4x4.Translation(5, -4, 2);
            var result = OrientedBoundingBox.FromPoints(points, transform);
            AssertVector(new Vector3(5, -4, 2), result.Center);
            AssertVector(new Vector3(1, 2, 3), result.Extents);
            foreach (var point in points)
            {
                var transformed = point;
                var bounds = result.GetBoundingBox();
                Assert.IsTrue(transformed.X >= bounds.Minimum.X && transformed.X <= bounds.Maximum.X);
                Assert.IsTrue(transformed.Y >= bounds.Minimum.Y && transformed.Y <= bounds.Maximum.Y);
                Assert.IsTrue(transformed.Z >= bounds.Minimum.Z && transformed.Z <= bounds.Maximum.Z);
            }
            Assert.ThrowsExactly<ArgumentNullException>(() => OrientedBoundingBox.FromPoints(null, transform));
        }

        [TestMethod]
        public void EqualityHashAndFormatting_CoverAllPublicOverloads()
        {
            var a = new OrientedBoundingBox(new Vector3(-1, -2, -3), new Vector3(3, 2, 1));
            var b = new OrientedBoundingBox(new Vector3(-1, -2, -3), new Vector3(3, 2, 1));
            var different = new OrientedBoundingBox(new Vector3(-2, -2, -3), new Vector3(3, 2, 1));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
            Assert.IsTrue(a.Equals(ref b));
            Assert.IsFalse(a.Equals(different));
            Assert.IsFalse(a.Equals((object)null));
            Assert.IsFalse(a.Equals("not an OBB"));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.IsTrue(a != different);

            var invariant = System.Globalization.CultureInfo.InvariantCulture;
            var defaultText = a.ToString(invariant);
            Assert.AreEqual(defaultText, a.ToString());
            Assert.IsTrue(a.ToString("F1").Contains("Extents:", StringComparison.Ordinal));
            Assert.IsTrue(a.ToString("F1", invariant).Contains("2.0", StringComparison.Ordinal));
            Assert.IsTrue(a.ToString("F1", invariant).Contains("Extents:", StringComparison.Ordinal));
        }
    }
}
