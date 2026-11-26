using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class IntersectionVolumeMeshTests
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
        public void Constructor_NullMesh_ThrowsArgumentNullException()
        {
            var ex = Assert.ThrowsExactly<ArgumentNullException>(() => new IntersectionVolumeMesh(null));
            Assert.AreEqual("mesh", ex.ParamName);
        }

        [TestMethod]
        public void Constructor_EmptyMesh_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new IntersectionVolumeMesh([]));
        }

        [TestMethod]
        public void Constructor_SingleTriangle_MeshFieldSetCorrectly()
        {
            var triangle = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0));
            var volume = new IntersectionVolumeMesh([triangle]);

            Assert.HasCount(1, volume.Mesh);
            Assert.AreEqual(triangle, volume.Mesh[0]);
        }

        [TestMethod]
        public void Constructor_MultipleTriangles_MeshFieldSetCorrectly()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0)),
                new Triangle(new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            Assert.HasCount(2, volume.Mesh);
            Assert.AreEqual(triangles[0], volume.Mesh[0]);
            Assert.AreEqual(triangles[1], volume.Mesh[1]);
        }

        #endregion

        #region Position

        [TestMethod]
        public void Position_FromSingleTriangle_ReturnsTriangleCenter()
        {
            var triangle = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(0, 2, 0));
            var expectedCenter = new Vector3(2f / 3f, 2f / 3f, 0);
            var volume = new IntersectionVolumeMesh([triangle]);

            Assert.IsLessThanOrEqualTo(ZeroTolerance, Math.Abs(volume.Position.X - expectedCenter.X));
            Assert.IsLessThanOrEqualTo(ZeroTolerance, Math.Abs(volume.Position.Y - expectedCenter.Y));
            Assert.IsLessThanOrEqualTo(ZeroTolerance, Math.Abs(volume.Position.Z - expectedCenter.Z));
        }

        [TestMethod]
        public void Position_FromMultipleTriangles_ReturnsCentroid()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(0, 2, 0)),
                new Triangle(new Vector3(2, 0, 0), new Vector3(2, 2, 0), new Vector3(0, 2, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            Assert.AreNotEqual(volume.Position, Vector3.Zero);
        }

        [TestMethod]
        public void Position_FromTrianglesInPositiveOctant_ReturnsPositiveValues()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(10, 10, 10), new Vector3(20, 10, 10), new Vector3(10, 20, 10))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            Assert.IsGreaterThan(0, volume.Position.X);
            Assert.IsGreaterThan(0, volume.Position.Y);
            Assert.IsGreaterThan(0, volume.Position.Z);
        }

        #endregion

        #region Mesh

        [TestMethod]
        public void Mesh_ReturnsSameArrayAsConstructor()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            Assert.AreSame(triangles, volume.Mesh);
        }

        #endregion

        #region Contains(BoundingBox)

        [TestMethod]
        public void Contains_BoundingBoxInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var box = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 0, -1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(box));
        }
        [TestMethod]
        public void Contains_BoundingBoxInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var box = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 0, -1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref box));
        }

        [TestMethod]
        public void Contains_BoundingBoxOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var box = new BoundingBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(box));
        }
        [TestMethod]
        public void Contains_BoundingBoxOutsideRef_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var box = new BoundingBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref box));
        }

        #endregion

        #region Contains(BoundingSphere)

        [TestMethod]
        public void Contains_BoundingSphereInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var sphere = new BoundingSphere(new Vector3(0, -2.5f, 0), 2f);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(sphere));
        }
        [TestMethod]
        public void Contains_BoundingSphereInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var sphere = new BoundingSphere(new Vector3(0, -2.5f, 0), 2f);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref sphere));
        }

        [TestMethod]
        public void Contains_BoundingSphereOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var sphere = new BoundingSphere(new Vector3(10, 10, 10), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(sphere));
        }
        [TestMethod]
        public void Contains_BoundingSphereOutsideRef_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var sphere = new BoundingSphere(new Vector3(10, 10, 10), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref sphere));
        }

        #endregion

        #region Contains(BoundingFrustum)

        [TestMethod]
        public void Contains_BoundingFrustumInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, -2.5f, 4), new Vector3(0, -2.5f, 1), Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 3f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(frustum));
        }
        [TestMethod]
        public void Contains_BoundingFrustumInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, -2.5f, 4), new Vector3(0, -2.5f, 1), Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 3f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref frustum));
        }

        [TestMethod]
        public void Contains_BoundingFrustumOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, -100), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 10f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(frustum));
        }
        [TestMethod]
        public void Contains_BoundingFrustumOutsideRef_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, -100), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 10f);
            var viewProj = view * proj;
            var frustum = new BoundingFrustum(viewProj);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(ref frustum));
        }

        #endregion

        #region Contains(Triangle[])

        [TestMethod]
        public void Contains_SubsetMesh_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var mesh = new Triangle[]
            {
                new(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(0, 0, 1))
            };

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(mesh));
        }

        [TestMethod]
        public void Contains_OuterMesh_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var mesh = new Triangle[]
            {
                new(new Vector3(10, 10, 10), new Vector3(20, 10, 10), new Vector3(15, 20, 10))
            };

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(mesh));
        }

        #endregion

        #region Contains(IntersectionVolumeAxisAlignedBox)

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 0, -1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(1, 0, -1));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeAxisAlignedBoxOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var other = new IntersectionVolumeAxisAlignedBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(other));
        }

        #endregion

        #region Contains(IntersectionVolumeSphere)

        [TestMethod]
        public void Contains_IntersectionVolumeSphereInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var other = new IntersectionVolumeSphere(new Vector3(0, -2, 0), 2);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeSphereInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var other = new IntersectionVolumeSphere(new Vector3(0, -2, 0), 2);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref other));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeSphereOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var other = new IntersectionVolumeSphere(new Vector3(10, 10, 10), 1);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(other));
        }

        #endregion

        #region Contains(IntersectionVolumeFrustum)

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, -2, 4), new Vector3(0, -2, 1), Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 3f);
            var viewProj = view * proj;
            var innerFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(viewProj));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(innerFrustum));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, -2, 4), new Vector3(0, -2, 1), Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 8, 1f, 1f, 3f);
            var viewProj = view * proj;
            var innerFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(viewProj));

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref innerFrustum));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeFrustumOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);

            var view = Matrix4x4.LookAtLH(new Vector3(0, 0, -100), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4x4.PerspectiveFovLH(MathF.PI / 4, 1f, 1f, 10f);
            var viewProj = view * proj;
            var farFrustum = new IntersectionVolumeFrustum(new BoundingFrustum(viewProj));

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(farFrustum));
        }

        #endregion

        #region Contains(IntersectionVolumeMesh)

        [TestMethod]
        public void Contains_IntersectionVolumeMeshInside_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(0, 0, 1))
            ]);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(mesh));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeMeshInsideRef_ReturnsContains()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(-5, -5, -5), new Vector3(5, -5, -5), new Vector3(0, 0, 5))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(0, 0, 1))
            ]);

            Assert.AreEqual(ContainmentType.Contains, volume.Contains(ref mesh));
        }

        [TestMethod]
        public void Contains_IntersectionVolumeMeshOutside_ReturnsDisjoint()
        {
            var triangles = new[]
            {
                new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0))
            };
            var volume = new IntersectionVolumeMesh(triangles);
            var mesh = new IntersectionVolumeMesh(
            [
                new Triangle(new Vector3(10, 10, 10), new Vector3(20, 10, 10), new Vector3(15, 20, 10))
            ]);

            Assert.AreEqual(ContainmentType.Disjoint, volume.Contains(mesh));
        }

        #endregion
    }
}
