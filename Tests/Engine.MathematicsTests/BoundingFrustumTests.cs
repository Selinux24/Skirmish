using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class BoundingFrustumTests
    {
        private static readonly float Epsilon = 1e-5f;

        private static BoundingFrustum CreatePerspective(float fov = MathUtils.PiOver2, float aspect = 2f, float near = 1f, float far = 5f)
        {
            var projection = Matrix4x4.PerspectiveFovLH(fov, aspect, near, far);
            return new BoundingFrustum(projection);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual, float tolerance = 1e-5f)
        {
            Assert.AreEqual(expected.X, actual.X, tolerance, "X component");
            Assert.AreEqual(expected.Y, actual.Y, tolerance, "Y component");
            Assert.AreEqual(expected.Z, actual.Z, tolerance, "Z component");
        }

        private static void AssertPlane(Vector3 expectedNormal, float expectedD, Plane actual)
        {
            AssertVector(expectedNormal, actual.Normal);
            Assert.AreEqual(expectedD, actual.D, Epsilon);
            Assert.AreEqual(1f, actual.Normal.Length(), Epsilon);
        }

        [TestMethod]
        public void ConstructorAndPlaneProperties_ExtractPerspectivePlanesAndExposeMatrix()
        {
            var matrix = Matrix4x4.PerspectiveFovLH(MathUtils.PiOver2, 2f, 1f, 5f);
            var frustum = new BoundingFrustum(matrix);

            Assert.AreEqual(matrix, frustum.Matrix);
            AssertPlane(Vector3.Normalize(new Vector3(0.5f, 0f, 1f)), 0f, frustum.Left);
            AssertPlane(Vector3.Normalize(new Vector3(-0.5f, 0f, 1f)), 0f, frustum.Right);
            AssertPlane(Vector3.Normalize(new Vector3(0f, -1f, 1f)), 0f, frustum.Top);
            AssertPlane(Vector3.Normalize(new Vector3(0f, 1f, 1f)), 0f, frustum.Bottom);
            AssertPlane(Vector3.UnitZ, -1f, frustum.Near);
            AssertPlane(-Vector3.UnitZ, 5f, frustum.Far);
            Assert.IsFalse(frustum.IsOrthographic);
        }

        [TestMethod]
        public void MatrixSetter_RecomputesPlanesAndOrthographicDetectionUsesSideNormals()
        {
            var frustum = CreatePerspective();
            var orthographicMatrix = Matrix4x4.LookAtLH(Vector3.Zero, Vector3.ForwardLH, Vector3.Up) *
                Matrix4x4.OrthoLH(8f, 4f, 1f, 5f);
            frustum.Matrix = orthographicMatrix;

            Assert.AreEqual(orthographicMatrix, frustum.Matrix);
            AssertPlane(Vector3.UnitX, 4f, frustum.Left);
            AssertPlane(-Vector3.UnitX, 4f, frustum.Right);
            AssertPlane(-Vector3.UnitY, 2f, frustum.Top);
            AssertPlane(Vector3.UnitY, 2f, frustum.Bottom);
            Assert.IsTrue(frustum.IsOrthographic);
        }

        [TestMethod]
        public void GetPlane_ReturnsNamedPlanesInDocumentedOrderAndDefaultForInvalidIndex()
        {
            var frustum = CreatePerspective();
            Plane[] expected = [frustum.Left, frustum.Right, frustum.Top, frustum.Bottom, frustum.Near, frustum.Far];

            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], frustum.GetPlane(i), $"plane index {i}");
            Assert.AreEqual(default, frustum.GetPlane(-1));
            Assert.AreEqual(default, frustum.GetPlane(6));
        }

        [TestMethod]
        public void FromCamera_ScalarAndParameterOverloadsCreateEquivalentPerspectiveFrustums()
        {
            var camera = new FrustumCameraParams
            {
                Position = new Vector3(2f, -1f, 3f),
                LookAtDir = Vector3.ForwardLH,
                UpDir = Vector3.Up,
                FOV = MathUtils.PiOver2,
                ZNear = 1f,
                ZFar = 7f,
                AspectRatio = 1.5f
            };
            var byParams = BoundingFrustum.FromCamera(camera);
            var byScalars = BoundingFrustum.FromCamera(camera.Position, camera.LookAtDir, camera.UpDir, camera.FOV, camera.ZNear, camera.ZFar, camera.AspectRatio);

            Assert.IsTrue(byParams == byScalars);
            Assert.IsFalse(byParams.IsOrthographic);
            Assert.HasCount(8, byParams.GetCorners());
            AssertVector(new Vector3(3.5f, -2f, 4f), byParams.GetCorners()[0]);
        }

        [TestMethod]
        public void GetCorners_ReturnsStablePerspectiveCornerOrderAndFillsExactSizedBuffer()
        {
            var frustum = CreatePerspective();
            Vector3[] expected =
            [
                new(2f, -1f, 1f), new(2f, 1f, 1f), new(-2f, 1f, 1f), new(-2f, -1f, 1f),
                new(10f, -5f, 5f), new(10f, 5f, 5f), new(-10f, 5f, 5f), new(-10f, -5f, 5f)
            ];
            var allocated = frustum.GetCorners();
            var buffer = new Vector3[8];
            frustum.GetCorners(buffer);

            Assert.HasCount(8, allocated);
            for (int i = 0; i < expected.Length; i++)
            {
                AssertVector(expected[i], allocated[i]);
                AssertVector(expected[i], buffer[i]);
            }
            Assert.ThrowsExactly<ArgumentNullException>(() => frustum.GetCorners(null));
            Assert.ThrowsExactly<ArgumentException>(() => frustum.GetCorners(new Vector3[7]));
            Assert.ThrowsExactly<ArgumentException>(() => frustum.GetCorners(new Vector3[9]));
        }

        [TestMethod]
        public void GetCorners_OrthographicMatrixProducesConstantSizeNearAndFarFaces()
        {
            var view = Matrix4x4.LookAtLH(Vector3.Zero, Vector3.ForwardLH, Vector3.Up);
            var frustum = new BoundingFrustum(view * Matrix4x4.OrthoLH(8f, 4f, 1f, 5f));
            var corners = frustum.GetCorners();

            AssertVector(new Vector3(4f, -2f, 1f), corners[0]);
            AssertVector(new Vector3(4f, 2f, 1f), corners[1]);
            AssertVector(new Vector3(-4f, 2f, 1f), corners[2]);
            AssertVector(new Vector3(-4f, -2f, 1f), corners[3]);
            AssertVector(new Vector3(4f, -2f, 5f), corners[4]);
            AssertVector(new Vector3(-4f, 2f, 5f), corners[6]);
            Assert.IsTrue(frustum.IsOrthographic);
        }

        [TestMethod]
        public void GetCameraParams_PerspectiveFrustumRecoversCameraAndProjectionValues()
        {
            var source = new FrustumCameraParams
            {
                Position = Vector3.Zero,
                LookAtDir = Vector3.ForwardLH,
                UpDir = Vector3.Up,
                FOV = MathUtils.PiOver2,
                ZNear = 1f,
                ZFar = 5f,
                AspectRatio = 2f
            };
            var recovered = BoundingFrustum.FromCamera(source).GetCameraParams();

            AssertVector(source.Position, recovered.Position);
            AssertVector(source.LookAtDir, recovered.LookAtDir);
            AssertVector(source.UpDir, recovered.UpDir);
            Assert.AreEqual(source.FOV, recovered.FOV, Epsilon);
            Assert.AreEqual(source.ZNear, recovered.ZNear, Epsilon);
            Assert.AreEqual(source.ZFar, recovered.ZFar, Epsilon);
            Assert.AreEqual(source.AspectRatio, recovered.AspectRatio, Epsilon);
        }

        [TestMethod]
        public void GetWidthAndHeightAtDepth_ScaleWithPerspectiveDepthAndUseHorizontalAspect()
        {
            var frustum = CreatePerspective(MathUtils.PiOver2, 2f, 1f, 5f);

            Assert.AreEqual(4f, frustum.GetWidthAtDepth(1f), Epsilon);
            Assert.AreEqual(2f, frustum.GetHeightAtDepth(1f), Epsilon);
            Assert.AreEqual(12f, frustum.GetWidthAtDepth(3f), Epsilon);
            Assert.AreEqual(6f, frustum.GetHeightAtDepth(3f), Epsilon);
            Assert.AreEqual(0f, frustum.GetWidthAtDepth(0f), Epsilon);
            Assert.AreEqual(-2f, frustum.GetHeightAtDepth(-1f), Epsilon);
        }

        [TestMethod]
        public void ZoomToExtents_BoxAndPointOverloadsAgreeAndVectorFollowsNearDirection()
        {
            var frustum = CreatePerspective();
            var box = new BoundingBox(new Vector3(-0.5f, -0.5f, 2f), new Vector3(0.5f, 0.5f, 3f));
            var points = box.GetCorners();
            var distanceByRef = frustum.GetZoomToExtentsShiftDistance(ref box);
            var distanceByPoints = frustum.GetZoomToExtentsShiftDistance(points);
            var vectorByRef = frustum.GetZoomToExtentsShiftVector(ref box);
            var vectorByPoints = frustum.GetZoomToExtentsShiftVector(points);

            Assert.AreEqual(distanceByPoints, distanceByRef, Epsilon);
            Assert.IsGreaterThan(0f, distanceByPoints);
            AssertVector(distanceByPoints * frustum.Near.Normal, vectorByPoints);
            AssertVector(vectorByPoints, vectorByRef);
            Assert.IsGreaterThan(0f, MathF.Abs(vectorByPoints.Z));
        }

        [TestMethod]
        public void ContainsPoint_RefAndValueOverloadsDistinguishInteriorBoundaryAndExterior()
        {
            var frustum = CreatePerspective();
            var inside = new Vector3(0f, 0f, 3f);
            var boundary = new Vector3(2f, 0f, 1f);
            var outside = new Vector3(20f, 0f, 3f);
            Assert.AreEqual(ContainmentType.Contains, frustum.Contains(inside));
            Assert.AreEqual(frustum.Contains(inside), frustum.Contains(ref inside));
            Assert.AreEqual(ContainmentType.Intersects, frustum.Contains(boundary));
            Assert.AreEqual(ContainmentType.Disjoint, frustum.Contains(outside));
        }

        [TestMethod]
        public void ContainsBox_RefValueAndOutOverloadsReportInteriorPartialAndDisjoint()
        {
            var frustum = CreatePerspective();
            var inside = new BoundingBox(new Vector3(-20f, -20f, -1f), new Vector3(20f, 20f, 6f));
            var partial = new BoundingBox(new Vector3(1.5f, -0.25f, 2f), new Vector3(2.5f, 0.25f, 3f));
            var outside = new BoundingBox(new Vector3(20f, 20f, 2f), new Vector3(21f, 21f, 3f));
            frustum.Contains(ref inside, out var outResult);

            Assert.AreEqual(ContainmentType.Contains, frustum.Contains(inside));
            Assert.AreEqual(frustum.Contains(ref inside), outResult);
            Assert.AreEqual(ContainmentType.Intersects, frustum.Contains(partial));
            Assert.AreEqual(ContainmentType.Disjoint, frustum.Contains(outside));
        }

        [TestMethod]
        public void ContainsSphere_RefValueAndOutOverloadsReportInteriorPartialAndDisjoint()
        {
            var frustum = CreatePerspective();
            var inside = new BoundingSphere(new Vector3(0f, 0f, 2.5f), 20f);
            var partial = new BoundingSphere(new Vector3(2.1f, 0f, 2f), 3f);
            var outside = new BoundingSphere(new Vector3(20f, 0f, 3f), 0.5f);
            frustum.Contains(ref inside, out var outResult);

            Assert.AreEqual(ContainmentType.Contains, frustum.Contains(inside));
            Assert.AreEqual(frustum.Contains(ref inside), outResult);
            Assert.AreEqual(ContainmentType.Intersects, frustum.Contains(partial));
            Assert.AreEqual(ContainmentType.Disjoint, frustum.Contains(outside));
        }

        [TestMethod]
        public void IntersectsSphereAndBox_RefAndOutOverloadsMatchContainment()
        {
            var frustum = CreatePerspective();
            var sphere = new BoundingSphere(new Vector3(0f, 0f, 3f), 0.25f);
            var distantSphere = new BoundingSphere(new Vector3(20f, 0f, 3f), 0.25f);
            var box = new BoundingBox(new Vector3(-0.25f, -0.25f, 2f), new Vector3(0.25f, 0.25f, 3f));
            var distantBox = new BoundingBox(new Vector3(20f, 20f, 2f), new Vector3(21f, 21f, 3f));
            frustum.Intersects(ref sphere, out var sphereOut);
            frustum.Intersects(ref box, out var boxOut);

            Assert.IsTrue(frustum.Intersects(ref sphere));
            Assert.IsTrue(sphereOut);
            Assert.IsFalse(frustum.Intersects(ref distantSphere));
            Assert.IsTrue(frustum.Intersects(ref box));
            Assert.IsTrue(boxOut);
            Assert.IsFalse(frustum.Intersects(ref distantBox));
        }

        [TestMethod]
        public void IntersectsPlane_ReportsCrossingAndSeparatingPlanesThroughRefAndOut()
        {
            var frustum = CreatePerspective();
            var crossing = new Plane(Vector3.UnitX, 0f);
            var separated = new Plane(Vector3.UnitZ, -0.5f);
            frustum.Intersects(ref crossing, out var crossingOut);
            frustum.Intersects(ref separated, out var separatedOut);

            Assert.AreEqual(PlaneIntersectionType.Intersecting, frustum.Intersects(ref crossing));
            Assert.AreEqual(frustum.Intersects(ref crossing), crossingOut);
            Assert.AreEqual(PlaneIntersectionType.Front, frustum.Intersects(ref separated));
            Assert.AreEqual(frustum.Intersects(ref separated), separatedOut);
        }

        [TestMethod]
        public void IntersectsRay_ProvidesEntryExitDistancesAndRejectsOffAxisRay()
        {
            var frustum = CreatePerspective();
            var through = new Ray(new Vector3(0f, 0f, 0f), Vector3.ForwardLH);
            var miss = new Ray(new Vector3(20f, 0f, 0f), Vector3.ForwardLH);
            var hit = frustum.Intersects(ref through, out float? entry, out float? exit);
            var missed = frustum.Intersects(ref miss, out float? missEntry, out float? missExit);

            Assert.IsTrue(hit);
            Assert.AreEqual(0f, entry!.Value, Epsilon);
            Assert.AreEqual(5f, exit!.Value, Epsilon);
            Assert.IsTrue(frustum.Intersects(ref through));
            Assert.IsFalse(missed);
            Assert.IsNull(missEntry);
            Assert.IsNull(missExit);
        }

        [TestMethod]
        public void EqualityNearEqualAndHashCode_CompareUnderlyingMatrixWithTolerance()
        {
            var first = CreatePerspective();
            var same = CreatePerspective();
            var withinToleranceMatrix = first.Matrix;
            withinToleranceMatrix.M11 += 0.0001f;
            var withinTolerance = new BoundingFrustum(withinToleranceMatrix);
            var different = CreatePerspective(MathUtils.PiOver2, 2f, 1f, 6f);

            Assert.IsTrue(first == same);
            Assert.IsFalse(first != same);
            Assert.IsTrue(first.Equals(same));
            Assert.IsTrue(first.Equals((object)same));
            Assert.IsTrue(first.Equals(ref same));
            Assert.IsFalse(first.Equals((object)null));
            Assert.IsFalse(first.Equals("not a frustum"));
            Assert.AreEqual(first.GetHashCode(), same.GetHashCode());
            Assert.IsTrue(BoundingFrustum.NearEqual(first, withinTolerance, 0.001f));
            Assert.IsFalse(BoundingFrustum.NearEqual(first, withinTolerance, 0.00001f));
            Assert.IsFalse(first.Equals(different));
            Assert.IsTrue(first != different);
        }

        [TestMethod]
        public void Formatting_UsesCameraSummaryAndHonorsFormatAndCultureOverloads()
        {
            var frustum = CreatePerspective();
            var invariant = CultureInfo.InvariantCulture;
            var english = CultureInfo.GetCultureInfo("en-US");
            var french = CultureInfo.GetCultureInfo("fr-FR");
            var plain = frustum.ToString();

            Assert.IsTrue(plain.StartsWith("Camera:", StringComparison.Ordinal));
            Assert.AreEqual(plain, frustum.ToString(invariant));
            Assert.AreEqual(plain, frustum.ToString((string)null));
            var formatted = frustum.ToString("F2");
            Assert.AreNotEqual(plain, formatted);
            Assert.IsTrue(formatted.Contains("FoV:", StringComparison.Ordinal));
            Assert.IsTrue(frustum.ToString("F2", invariant).Contains("Far:5.00", StringComparison.Ordinal));
            Assert.AreNotEqual(frustum.ToString("F2", english), frustum.ToString("F2", french));
            Assert.AreEqual(frustum.ToString(invariant), frustum.ToString(null, invariant));
            Assert.IsTrue(frustum.ToString((string)null, french).Contains("Camera:", StringComparison.Ordinal));
        }
    }
}




