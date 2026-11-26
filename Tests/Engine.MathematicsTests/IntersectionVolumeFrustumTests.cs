using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class IntersectionVolumeFrustumTests
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
        public void Constructor_FromBoundingFrustum_FrustumFieldSetCorrectly()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);
            var volume = new IntersectionVolumeFrustum(frustum);

            Assert.AreEqual(frustum, volume.Frustum);
        }

        [TestMethod]
        public void Constructor_FromViewProjMatrix_CreatesFrustumCorrectly()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            Assert.AreNotEqual(volume.Frustum, new BoundingFrustum());
        }

        [TestMethod]
        public void Constructor_FromViewProjMatrix_EquivalentToBoundingFrustum()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;

            var volume = new IntersectionVolumeFrustum(viewProj);
            var frustum = new BoundingFrustum(viewProj);

            Assert.AreEqual(frustum, volume.Frustum);
        }

        #endregion

        #region Position

        [TestMethod]
        public void Position_ReturnsCameraPosition()
        {
            var cameraPos = new Vector3(5, 10, 15);
            var view = Matrix4x4.LookAtLH(cameraPos, Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            Assert.AreEqual(cameraPos.X, volume.Position.X, ZeroTolerance);
            Assert.AreEqual(cameraPos.Y, volume.Position.Y, ZeroTolerance);
            Assert.AreEqual(cameraPos.Z, volume.Position.Z, ZeroTolerance);
        }

        [TestMethod]
        public void Position_FromBoundingFrustumConstructor_ReturnsCameraPosition()
        {
            var cameraPos = new Vector3(5, 10, 15);
            var view = Matrix4x4.LookAtLH(cameraPos, Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);
            var volume = new IntersectionVolumeFrustum(frustum);

            Assert.AreEqual(cameraPos.X, volume.Position.X, ZeroTolerance);
            Assert.AreEqual(cameraPos.Y, volume.Position.Y, ZeroTolerance);
            Assert.AreEqual(cameraPos.Z, volume.Position.Z, ZeroTolerance);
        }

        #endregion

        #region Radius

        [TestMethod]
        public void Radius_ReturnsZFar()
        {
            const float zFar = 100f;
            const float Tolerance = 0.001f;
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, zFar);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            Assert.AreEqual(zFar, volume.Radius, Tolerance);
        }

        [TestMethod]
        public void Radius_FromBoundingFrustumConstructor_ReturnsZFar()
        {
            const float zFar = 200f;
            const float Tolerance = 0.001f;
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, zFar);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);
            var volume = new IntersectionVolumeFrustum(frustum);

            Assert.AreEqual(zFar, volume.Radius, Tolerance);
        }

        #endregion

        #region Implicit Operator

        [TestMethod]
        public void ImplicitOperator_FromBoundingFrustum_CreatesEquivalentVolume()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);
            IntersectionVolumeFrustum volume = frustum;

            Assert.AreEqual(frustum, volume.Frustum);
        }

        #endregion

        #region Contains(BoundingSphere)

        [TestMethod]
        public void Contains_SphereInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var sphere = new BoundingSphere(new Vector3(0, 0, 5), 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(sphere));
        }

        [TestMethod]
        public void Contains_SphereInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var sphere = new BoundingSphere(new Vector3(0, 0, 5), 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref sphere));
        }

        [TestMethod]
        public void Contains_SphereOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var sphere = new BoundingSphere(new Vector3(0, 0, -100), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(sphere));
        }

        [TestMethod]
        public void Contains_SphereOutsideRef_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var sphere = new BoundingSphere(new Vector3(0, 0, -100), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref sphere));
        }

        #endregion

        #region Contains(BoundingBox)

        [TestMethod]
        public void Contains_BoxInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var box = new BoundingBox(new Vector3(-1, -1, 4), new Vector3(1, 1, 6));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(box));
        }

        [TestMethod]
        public void Contains_BoxInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var box = new BoundingBox(new Vector3(-1, -1, 4), new Vector3(1, 1, 6));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref box));
        }

        [TestMethod]
        public void Contains_BoxOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var box = new BoundingBox(new Vector3(0, 0, 15), new Vector3(10, 10, 25));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(box));
        }

        [TestMethod]
        public void Contains_BoxOutsideRef_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var box = new BoundingBox(new Vector3(0, 0, 15), new Vector3(10, 10, 25));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref box));
        }

        #endregion

        #region Contains(BoundingFrustum)

        [TestMethod]
        public void Contains_SmallFrustumInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var nearView = Matrix4x4.LookAtLH(new Vector3(0, 0, 7), Vector3.Zero, Vector3.UnitY);
            var nearProj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 50f);
            var nearViewProj = nearView * nearProj;
            var smallFrustum = new BoundingFrustum(nearViewProj);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(smallFrustum));
        }

        [TestMethod]
        public void Contains_SmallFrustumInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var nearView = Matrix4x4.LookAtLH(new Vector3(0, 0, 7), Vector3.Zero, Vector3.UnitY);
            var nearProj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 50f);
            var nearViewProj = nearView * nearProj;
            var smallFrustum = new BoundingFrustum(nearViewProj);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref smallFrustum));
        }

        [TestMethod]
        public void Contains_FrustumOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var farView = Matrix4x4.LookAtLH(new Vector3(0, 0, 50), Vector3.Zero, Vector3.UnitY);
            var farProj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 5f);
            var farViewProj = farView * farProj;
            var farFrustum = new BoundingFrustum(farViewProj);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(farFrustum));
        }

        #endregion

        #region Contains(Triangle[])

        [TestMethod]
        public void Contains_MeshInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var mesh = new Triangle[]
            {
                new(new Vector3(-1, -1, 5), new Vector3(1, -1, 5), new Vector3(0, 1, 5))
            };

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(mesh));
        }

        [TestMethod]
        public void Contains_MeshOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var mesh = new Triangle[]
            {
                new(new Vector3(0, 0, 15), new Vector3(10, 0, 15), new Vector3(5, 10, 15))
            };

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(mesh));
        }

        #endregion

        #region Contains(IntersectionVolumeSphere)

        [TestMethod]
        public void Contains_IntersectionVolumeSphereInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var other = new IntersectionVolumeSphere(new Vector3(0, 0, 5), 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeSphereInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var other = new IntersectionVolumeSphere(new Vector3(0, 0, 5), 1);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeSphereOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var other = new IntersectionVolumeSphere(new Vector3(0, 0, 15), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(other));
        }

        #endregion

        #region Contains(IntersectionVolumeAxisAlignedBox)

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, 4), new Vector3(1, 1, 6));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, 4), new Vector3(1, 1, 6));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(0, 0, 15), new Vector3(10, 10, 25));

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
            var volume = new IntersectionVolumeFrustum(viewProj);

            var nearView = Matrix4x4.LookAtLH(new Vector3(0, 0, 7), Vector3.Zero, Vector3.UnitY);
            var nearProj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 50f);
            var nearViewProj = nearView * nearProj;
            var innerFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(nearViewProj));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(innerFrustum));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var nearView = Matrix4x4.LookAtLH(new Vector3(0, 0, 7), Vector3.Zero, Vector3.UnitY);
            var nearProj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 50f);
            var nearViewProj = nearView * nearProj;
            var innerFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(nearViewProj));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref innerFrustum));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var farView = Matrix4x4.LookAtLH(new Vector3(0, 0, 50), Vector3.Zero, Vector3.UnitY);
            var farProj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 5f);
            var farViewProj = farView * farProj;
            var farFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(farViewProj));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(farFrustum));
        }

        #endregion

        #region Contains(IntersectionVolumeMesh)

        [TestMethod]
        public void Contains_IntersectionVolumeMeshInside_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(-1, -1, 5), new Vector3(1, -1, 5), new Vector3(0, 1, 5))
            ]);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(mesh));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeMeshInsideRef_ReturnsContains()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(-1, -1, 5), new Vector3(1, -1, 5), new Vector3(0, 1, 5))
            ]);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref mesh));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeMeshOutside_ReturnsDisjoint()
        {
            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 100f);
            var viewProj = view * proj;
            var volume = new IntersectionVolumeFrustum(viewProj);

            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(0, 0, 15), new Vector3(10, 0, 15), new Vector3(5, 10, 15))
            ]);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(mesh));
        }

        #endregion
    }
}
