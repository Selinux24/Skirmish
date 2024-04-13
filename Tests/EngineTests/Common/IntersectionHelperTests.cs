using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Engine.Common.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class IntersectionHelperTests
    {
        static TestContext _testContext;

        static BoundingSphere bsph1;
        static BoundingSphere bsph2;
        static BoundingSphere bsph3;
        static BoundingSphere bsph4;
        static BoundingSphere bsph5;

        static IntersectionVolumeSphere sph1;
        static IntersectionVolumeSphere sph1_1;
        static IntersectionVolumeSphere sph2;
        static IntersectionVolumeSphere sph3;
        static IntersectionVolumeSphere sph4;
        static IntersectionVolumeSphere sph5;

        static BoundingBox bbox1;
        static BoundingBox bbox2;
        static BoundingBox bbox3;
        static BoundingBox bbox4;
        static BoundingBox bbox5;

        static IntersectionVolumeAxisAlignedBox box1;
        static IntersectionVolumeAxisAlignedBox box2;
        static IntersectionVolumeAxisAlignedBox box3;
        static IntersectionVolumeAxisAlignedBox box4;
        static IntersectionVolumeAxisAlignedBox box5;

        static Triangle[] tmesh1;
        static Triangle[] tmesh2;
        static Triangle[] tmesh3;
        static Triangle[] tmesh4;
        static Triangle[] tmesh5;

        static IntersectionVolumeMesh mesh1;
        static IntersectionVolumeMesh mesh2;
        static IntersectionVolumeMesh mesh3;
        static IntersectionVolumeMesh mesh4;
        static IntersectionVolumeMesh mesh5;

        static Mock<IIntersectable> i1;
        static Mock<IIntersectable> i2;
        static Mock<IIntersectable> i3;
        static Mock<IIntersectable> i4;
        static Mock<IIntersectable> i5;

        static BoundingFrustum bfrustum1;
        static BoundingFrustum bfrustum2;
        static BoundingFrustum bfrustum3;

        static IntersectionVolumeFrustum frustum1;
        static IntersectionVolumeFrustum frustum2;
        static IntersectionVolumeFrustum frustum3;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            bsph1 = new BoundingSphere(new Vector3(-1.0f, 0f, 0f), 1f);
            bsph2 = new BoundingSphere(new Vector3(+1.0f, 0f, 0f), 1f);
            bsph3 = new BoundingSphere(new Vector3(+3.0f, 0f, 0f), 1f);
            bsph4 = new BoundingSphere(new Vector3(-1.0f, 0f, 0f), 0.5f);
            bsph5 = new BoundingSphere(new Vector3(-1.0f, 0f, 0f), 2f);

            sph1 = new IntersectionVolumeSphere(bsph1.Center, bsph1.Radius);
            sph2 = new IntersectionVolumeSphere(bsph2.Center, bsph2.Radius);
            sph3 = new IntersectionVolumeSphere(bsph3.Center, bsph3.Radius);
            sph4 = new IntersectionVolumeSphere(bsph4.Center, bsph4.Radius);
            sph5 = new IntersectionVolumeSphere(bsph5.Center, bsph5.Radius);


            bbox1 = new BoundingBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
            bbox2 = new BoundingBox((Vector3.One * -0.5f) + Vector3.Left, (Vector3.One * 0.5f) + Vector3.Left);
            bbox3 = new BoundingBox(Vector3.One * -0.5f + (Vector3.Left * 3), Vector3.One * 0.5f + (Vector3.Left * 3));
            bbox4 = new BoundingBox(Vector3.One * -0.25f, Vector3.One * 0.25f);
            bbox5 = new BoundingBox(Vector3.One * -1f, Vector3.One * 1f);

            box1 = new IntersectionVolumeAxisAlignedBox(bbox1.Minimum, bbox1.Maximum);
            box2 = new IntersectionVolumeAxisAlignedBox(bbox2.Minimum, bbox2.Maximum);
            box3 = new IntersectionVolumeAxisAlignedBox(bbox3.Minimum, bbox3.Maximum);
            box4 = new IntersectionVolumeAxisAlignedBox(bbox4.Minimum, bbox4.Maximum);
            box5 = new IntersectionVolumeAxisAlignedBox(bbox5.Minimum, bbox5.Maximum);


            tmesh1 = Triangle.ComputeTriangleList(box1).ToArray();
            tmesh2 = Triangle.ComputeTriangleList(box2).ToArray();
            tmesh3 = Triangle.ComputeTriangleList(box3).ToArray();
            tmesh4 = Triangle.ComputeTriangleList(box4).ToArray();
            tmesh5 = Triangle.ComputeTriangleList(box5).ToArray();

            mesh1 = new IntersectionVolumeMesh(tmesh1);
            mesh2 = new IntersectionVolumeMesh(tmesh2);
            mesh3 = new IntersectionVolumeMesh(tmesh3);
            mesh4 = new IntersectionVolumeMesh(tmesh4);
            mesh5 = new IntersectionVolumeMesh(tmesh5);

            var bsph1_1 = BoundingSphere.FromBox(bbox1);
            bsph1_1.Radius *= 0.5f;
            sph1_1 = new IntersectionVolumeSphere(bsph1_1);

            i1 = new Mock<IIntersectable>();
            i1.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Sphere)).Returns(sph1);
            i1.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Box)).Returns(box1);
            i1.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Mesh)).Returns(mesh1);

            i2 = new Mock<IIntersectable>();
            i2.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Sphere)).Returns(sph2);
            i2.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Box)).Returns(box2);
            i2.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Mesh)).Returns(mesh2);

            i3 = new Mock<IIntersectable>();
            i3.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Sphere)).Returns(sph3);
            i3.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Box)).Returns(box3);
            i3.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Mesh)).Returns(mesh3);

            i4 = new Mock<IIntersectable>();
            i4.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Sphere)).Returns(sph4);
            i4.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Box)).Returns(box4);
            i4.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Mesh)).Returns(mesh4);

            i5 = new Mock<IIntersectable>();
            i5.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Sphere)).Returns(sph5);
            i5.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Box)).Returns(box5);
            i5.Setup(i => i.GetIntersectionVolume(IntersectDetectionMode.Mesh)).Returns(mesh5);

            var proj = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1f, 0.1f, 1f);
            var view1 = Matrix.LookAtLH(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.Up);
            var view2 = Matrix.LookAtLH(new Vector3(+1, 0, 0), Vector3.Zero, Vector3.Up);
            var view3 = Matrix.LookAtLH(new Vector3(-2, 0, 0), new Vector3(-3, 0, 0), Vector3.Up);

            bfrustum1 = new BoundingFrustum(view1 * proj);
            bfrustum2 = new BoundingFrustum(view2 * proj);
            bfrustum3 = new BoundingFrustum(view3 * proj);

            frustum1 = new IntersectionVolumeFrustum(bfrustum1.Matrix);
            frustum2 = new IntersectionVolumeFrustum(bfrustum2.Matrix);
            frustum3 = new IntersectionVolumeFrustum(bfrustum3.Matrix);
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void IntersectsIntersectableTest()
        {
            Assert.IsFalse(IntersectionHelper.Intersects(null, null));

            Assert.IsFalse(IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, null));
            Assert.IsFalse(IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, null, IntersectDetectionMode.Sphere));

            Assert.IsFalse(IntersectionHelper.Intersects(null, IntersectDetectionMode.Sphere, i1.Object, IntersectDetectionMode.Sphere));
        }

        [TestMethod()]
        public void IntersectsIntersectableSphereTest()
        {
            bool res1 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, i2.Object, IntersectDetectionMode.Sphere);
            bool res2 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, i3.Object, IntersectDetectionMode.Sphere);
            bool res3 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, i4.Object, IntersectDetectionMode.Sphere);
            bool res4 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, i5.Object, IntersectDetectionMode.Sphere);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableBoxTest()
        {
            bool res1 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, i2.Object, IntersectDetectionMode.Box);
            bool res2 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, i3.Object, IntersectDetectionMode.Box);
            bool res3 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, i4.Object, IntersectDetectionMode.Box);
            bool res4 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, i5.Object, IntersectDetectionMode.Box);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableMeshTest()
        {
            bool res1 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, i2.Object, IntersectDetectionMode.Mesh);
            bool res2 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, i3.Object, IntersectDetectionMode.Mesh);
            bool res3 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, i4.Object, IntersectDetectionMode.Mesh);
            bool res4 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, i5.Object, IntersectDetectionMode.Mesh);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }

        [TestMethod()]
        public void IntersectsIntersectableVolumeSphereTest()
        {
            bool res1 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph2);
            bool res2 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph3);
            bool res3 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph4);
            bool res4 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableVolumeBoxTest()
        {
            bool res1 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, box2);
            bool res2 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, box3);
            bool res3 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, box4);
            bool res4 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Box, box5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableVolumeMeshTest()
        {
            bool res1 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh2);
            bool res2 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh3);
            bool res3 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh4);
            bool res4 = IntersectionHelper.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }

        [TestMethod()]
        public void IntersectsVolumeSphereTest()
        {
            bool res1 = IntersectionHelper.Intersects(sph1, sph2);
            bool res2 = IntersectionHelper.Intersects(sph1, sph3);
            bool res3 = IntersectionHelper.Intersects(sph1, sph4);
            bool res4 = IntersectionHelper.Intersects(sph1, sph5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsVolumeBoxTest()
        {
            bool res1 = IntersectionHelper.Intersects(box1, box2);
            bool res2 = IntersectionHelper.Intersects(box1, box3);
            bool res3 = IntersectionHelper.Intersects(box1, box4);
            bool res4 = IntersectionHelper.Intersects(box1, box5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsVolumeMeshTest()
        {
            bool res1 = IntersectionHelper.Intersects(mesh1, mesh2);
            bool res2 = IntersectionHelper.Intersects(mesh1, mesh3);
            bool res3 = IntersectionHelper.Intersects(mesh1, mesh4);
            bool res4 = IntersectionHelper.Intersects(mesh1, mesh5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsVolumeFrustumTest()
        {
            bool res1 = IntersectionHelper.Intersects(frustum1, frustum2);
            bool res2 = IntersectionHelper.Intersects(frustum1, frustum3);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
        }

        [TestMethod()]
        public void IntersectableSphereComparerTest()
        {
            bool res1 = bsph1 == sph1;
            bool res2 = sph1 == bsph1;
            bool res3 = bsph1 == sph2;
            bool res4 = sph1 == bsph2;

            Assert.IsTrue(res1);
            Assert.IsTrue(res2);
            Assert.IsFalse(res3);
            Assert.IsFalse(res4);

            Assert.AreEqual((IntersectionVolumeSphere)bsph1, sph1);
            Assert.AreEqual(sph1, (IntersectionVolumeSphere)bsph1);
            Assert.AreNotEqual((IntersectionVolumeSphere)bsph1, sph2);
            Assert.AreNotEqual(sph1, (IntersectionVolumeSphere)bsph2);
        }
        [TestMethod()]
        public void IntersectableBoxComparerTest()
        {
            bool res1 = bbox1 == box1;
            bool res2 = box1 == bbox1;
            bool res3 = bbox1 == box2;
            bool res4 = box1 == bbox2;

            Assert.IsTrue(res1);
            Assert.IsTrue(res2);
            Assert.IsFalse(res3);
            Assert.IsFalse(res4);

            Assert.AreEqual((IntersectionVolumeAxisAlignedBox)bbox1, box1);
            Assert.AreEqual(box1, (IntersectionVolumeAxisAlignedBox)bbox1);
            Assert.AreNotEqual((IntersectionVolumeAxisAlignedBox)bbox1, box2);
            Assert.AreNotEqual(box1, (IntersectionVolumeAxisAlignedBox)bbox2);
        }
        [TestMethod()]
        public void IntersectableMeshComparerTest()
        {
            CollectionAssert.AreEqual(tmesh1, (Triangle[])mesh1);
            CollectionAssert.AreEqual((Triangle[])mesh1, tmesh1);
            CollectionAssert.AreNotEqual(tmesh1, (Triangle[])mesh2);
            CollectionAssert.AreNotEqual((Triangle[])mesh1, tmesh2);

            Assert.AreEqual((IntersectionVolumeMesh)tmesh1, mesh1);
            Assert.AreEqual(mesh1, (IntersectionVolumeMesh)tmesh1);
            Assert.AreNotEqual((IntersectionVolumeMesh)tmesh1, mesh2);
            Assert.AreNotEqual(mesh1, (IntersectionVolumeMesh)tmesh2);
        }
        [TestMethod()]
        public void IntersectableFrustumComparerTest()
        {
            bool res1 = frustum1 == bfrustum1;
            bool res2 = frustum1 == bfrustum1;
            bool res3 = bfrustum1 == frustum2;
            bool res4 = frustum1 == bfrustum2;

            Assert.IsTrue(res1);
            Assert.IsTrue(res2);
            Assert.IsFalse(res3);
            Assert.IsFalse(res4);

            Assert.AreEqual((IntersectionVolumeFrustum)bfrustum1, frustum1);
            Assert.AreEqual(frustum1, (IntersectionVolumeFrustum)bfrustum1);
            Assert.AreNotEqual((IntersectionVolumeFrustum)bfrustum1, frustum2);
            Assert.AreNotEqual(frustum1, (IntersectionVolumeFrustum)bfrustum2);
        }

        [TestMethod()]
        public void IntersectableSphereConstructorTest()
        {
            var sph = new IntersectionVolumeSphere(bsph1);

            Assert.AreEqual(bsph1, (BoundingSphere)sph);
        }
        [TestMethod()]
        public void IntersectableBoxConstructorTest()
        {
            var box = new IntersectionVolumeAxisAlignedBox(bbox1);

            Assert.AreEqual(bbox1, (BoundingBox)box);
        }
        [TestMethod()]
        public void IntersectableMeshConstructorTest()
        {
            var mesh = new IntersectionVolumeMesh(tmesh1);

            CollectionAssert.AreEqual(tmesh1, (Triangle[])mesh);
        }
        [TestMethod()]
        public void IntersectableMeshBadConstructorTest()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var mesh = new IntersectionVolumeMesh([]); });
        }

        [TestMethod()]
        public void IntersectsSphereBoxTest1()
        {
            var res = sph1.Contains(bbox1);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsSphereBoxTest2()
        {
            var res = sph1.Contains(bbox2);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void IntersectsSphereBoxTest3()
        {
            var res = sph1.Contains(bbox3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsSphereSphereTest1()
        {
            var res = sph1.Contains(sph1);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void IntersectsSphereSphereTest2()
        {
            var res = sph1.Contains(sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsSphereSphereTest3()
        {
            var res = sph1.Contains(sph3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsSphereMeshTest1()
        {
            var res = sph1.Contains(tmesh1);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsSphereMeshTest2()
        {
            var res = sph1.Contains(tmesh2);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void IntersectsSphereMeshTest3()
        {
            var res = sph1.Contains(tmesh3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsSphereFrustumTest1()
        {
            var res = sph1.Contains(frustum1);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsSphereFrustumTest2()
        {
            var res = sph1.Contains(frustum3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsBoxBoxTest1()
        {
            var res = box1.Contains(bbox1);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void IntersectsBoxBoxTest2()
        {
            var res = box1.Contains(bbox2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsBoxBoxTest3()
        {
            var res = box1.Contains(bbox3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsBoxSphereTest1()
        {
            var res = box1.Contains(sph1_1);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void IntersectsBoxSphereTest2()
        {
            var res = box1.Contains(sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsBoxSphereTest3()
        {
            var res = box1.Contains(sph3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsBoxMeshTest1()
        {
            var res = box1.Contains(tmesh1);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void IntersectsBoxMeshTest2()
        {
            var res = box1.Contains(tmesh2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsBoxMeshTest3()
        {
            var res = box1.Contains(tmesh3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void IntersectsBoxFrustumTest1()
        {
            var res = box1.Contains(frustum1);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void IntersectsBoxFrustumTest2()
        {
            var res = box1.Contains(frustum3);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
    }
}
