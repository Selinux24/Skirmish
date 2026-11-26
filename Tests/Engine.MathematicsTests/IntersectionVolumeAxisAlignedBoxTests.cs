using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class IntersectionVolumeAxisAlignedBoxTests
    {
        const float ZeroTolerance = 0.0001f;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        #region Constructors

        [TestMethod]
        public void Constructor_FromBoundingBox_BBoxFieldSetCorrectly()
        {
            var bbox = new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
            var volume = new IntersectionVolumeAxisAlignedBox(bbox);

            Assert.AreEqual(bbox, volume.BBox);
        }

        [TestMethod]
        public void Constructor_FromMinMax_BBoxFieldSetCorrectly()
        {
            var min = new Vector3(1, 2, 3);
            var max = new Vector3(4, 5, 6);
            var volume = new IntersectionVolumeAxisAlignedBox(min, max);

            Assert.AreEqual(min, volume.BBox.Minimum);
            Assert.AreEqual(max, volume.BBox.Maximum);
        }

        [TestMethod]
        public void Constructor_ZeroSizeBox_Valid()
        {
            var point = Vector3.Zero;
            var volume = new IntersectionVolumeAxisAlignedBox(point, point);

            Assert.AreEqual(point, volume.BBox.Minimum);
            Assert.AreEqual(point, volume.BBox.Maximum);
        }

        #endregion

        #region Position

        [TestMethod]
        public void Position_ReturnsBBoxCenter()
        {
            var min = new Vector3(0, 0, 0);
            var max = new Vector3(10, 10, 10);
            var expectedCenter = new Vector3(5, 5, 5);
            var volume = new IntersectionVolumeAxisAlignedBox(min, max);

            Assert.AreEqual(expectedCenter, volume.Position);
        }

        [TestMethod]
        public void Position_FromBoundingBoxConstructor_ReturnsCenter()
        {
            var bbox = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5));
            var volume = new IntersectionVolumeAxisAlignedBox(bbox);

            Assert.AreEqual(Vector3.Zero, volume.Position);
        }

        #endregion

        #region Implicit Operator

        [TestMethod]
        public void ImplicitOperator_FromBoundingBox_CreatesEquivalentVolume()
        {
            var bbox = new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
            IntersectionVolumeAxisAlignedBox volume = bbox;

            Assert.AreEqual(bbox.Minimum, volume.BBox.Minimum);
            Assert.AreEqual(bbox.Maximum, volume.BBox.Maximum);
        }

        #endregion

        #region Contains(BoundingSphere)

        [TestMethod]
        public void Contains_SphereInside_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var sphere = new BoundingSphere(Vector3.Zero, 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(sphere));
        }

        [TestMethod]
        public void Contains_SphereInsideRef_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var sphere = new BoundingSphere(Vector3.Zero, 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref sphere));
        }

        [TestMethod]
        public void Contains_SphereOutside_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var sphere = new BoundingSphere(new Vector3(10, 10, 10), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(sphere));
        }

        [TestMethod]
        public void Contains_SphereOutsideRef_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var sphere = new BoundingSphere(new Vector3(10, 10, 10), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref sphere));
        }

        #endregion

        #region Contains(BoundingBox)

        [TestMethod]
        public void Contains_BoxInside_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var box = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(box));
        }

        [TestMethod]
        public void Contains_BoxInsideRef_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var box = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref box));
        }

        [TestMethod]
        public void Contains_BoxOutside_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var box = new BoundingBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(box));
        }

        [TestMethod]
        public void Contains_BoxOutsideRef_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var box = new BoundingBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref box));
        }

        [TestMethod]
        public void Contains_SameBox_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5));
            var box = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(box));
        }

        #endregion

        #region Contains(BoundingFrustum)

        [TestMethod]
        public void Contains_SmallFrustumInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var smallFrustum = new BoundingFrustum(viewProj);

            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-500, -500, -500), new Vector3(500, 500, 500));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(smallFrustum));
        }

        [TestMethod]
        public void Contains_SmallFrustumInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var smallFrustum = new BoundingFrustum(viewProj);

            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-500, -500, -500), new Vector3(500, 500, 500));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref smallFrustum));
        }

        [TestMethod]
        public void Contains_LargeFrustumOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(100, 100, 100), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var farFrustum = new BoundingFrustum(viewProj);

            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(farFrustum));
        }

        #endregion

        #region Contains(Triangle[])

        [TestMethod]
        public void Contains_MeshInside_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var mesh = new Triangle[]
            {
                new(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(0, 0, 1))
            };

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(mesh));
        }

        [TestMethod]
        public void Contains_MeshOutside_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var mesh = new Triangle[]
            {
                new(new Vector3(10, 10, 10), new Vector3(20, 10, 10), new Vector3(15, 15, 20))
            };

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(mesh));
        }

        #endregion

        #region Contains(IntersectionVolumeSphere)

        [TestMethod]
        public void Contains_IntersectionVolumeSphereInside_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var other = new IntersectionVolumeSphere(Vector3.Zero, 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeSphereInsideRef_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var other = new IntersectionVolumeSphere(Vector3.Zero, 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeSphereOutside_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var other = new IntersectionVolumeSphere(new Vector3(10, 10, 10), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(other));
        }

        #endregion

        #region Contains(IntersectionVolumeAxisAlignedBox)

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxInside_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxInsideRef_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxOutside_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(other));
        }

        #endregion

        #region Contains(IntersectionVolumeFrustum)

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var innerFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(viewProj));

            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-500, -500, -500), new Vector3(500, 500, 500));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(innerFrustum));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var innerFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(viewProj));

            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-500, -500, -500), new Vector3(500, 500, 500));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref innerFrustum));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(100, 100, 100), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var farFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(viewProj));

            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(farFrustum));
        }

        #endregion

        #region Contains(IntersectionVolumeMesh)

        [TestMethod]
        public void Contains_IntersectionVolumeMeshInside_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(0, 0, 1))
            ]);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(mesh));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeMeshInsideRef_ReturnsContains()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(0, 0, 1))
            ]);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref mesh));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeMeshOutside_ReturnsDisjoint()
        {
            var volume = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(10, 10, 10), new Vector3(20, 10, 10), new Vector3(15, 15, 20))
            ]);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(mesh));
        }

        #endregion
    }
}
