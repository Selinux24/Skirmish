using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;

namespace Engine.Common.Tests
{
    [TestClass()]
    public class IntersectionHelperTests
    {
        static TestContext _testContext;

        static IntersectionVolumeSphere sph1;
        static IntersectionVolumeSphere sph2;
        static IntersectionVolumeSphere sph3;
        static IntersectionVolumeSphere sph4;
        static IntersectionVolumeSphere sph5;

        static IntersectionVolumeAxisAlignedBox box1;
        static IntersectionVolumeAxisAlignedBox box2;
        static IntersectionVolumeAxisAlignedBox box3;
        static IntersectionVolumeAxisAlignedBox box4;
        static IntersectionVolumeAxisAlignedBox box5;

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

        static IntersectionVolumeFrustum frustum1;
        static IntersectionVolumeFrustum frustum2;
        static IntersectionVolumeFrustum frustum3;
        static IntersectionVolumeFrustum frustum4;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            sph1 = new IntersectionVolumeSphere(new Vector3(-1.0f, 0f, 0f), 1f);
            sph2 = new IntersectionVolumeSphere(new Vector3(+1.0f, 0f, 0f), 1f);
            sph3 = new IntersectionVolumeSphere(new Vector3(+2.0f, 0f, 0f), 1f);
            sph4 = new IntersectionVolumeSphere(new Vector3(-1.0f, 0f, 0f), 0.5f);
            sph5 = new IntersectionVolumeSphere(new Vector3(-1.0f, 0f, 0f), 2f);

            box1 = new IntersectionVolumeAxisAlignedBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
            box2 = new IntersectionVolumeAxisAlignedBox((Vector3.One * -0.5f) + Vector3.Left, (Vector3.One * 0.5f) + Vector3.Left);
            box3 = new IntersectionVolumeAxisAlignedBox(Vector3.One * -0.5f + (Vector3.Left * 2), Vector3.One * 0.5f + (Vector3.Left * 2));
            box4 = new IntersectionVolumeAxisAlignedBox(Vector3.One * -0.25f, Vector3.One * 0.25f);
            box5 = new IntersectionVolumeAxisAlignedBox(Vector3.One * -1f, Vector3.One * 1f);

            mesh1 = new IntersectionVolumeMesh(Triangle.ComputeTriangleList(Topology.TriangleList, box1));
            mesh2 = new IntersectionVolumeMesh(Triangle.ComputeTriangleList(Topology.TriangleList, box2));
            mesh3 = new IntersectionVolumeMesh(Triangle.ComputeTriangleList(Topology.TriangleList, box3));
            mesh4 = new IntersectionVolumeMesh(Triangle.ComputeTriangleList(Topology.TriangleList, box4));
            mesh5 = new IntersectionVolumeMesh(Triangle.ComputeTriangleList(Topology.TriangleList, box5));

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

            var proj = Matrix.PerspectiveLH(1024, 1024, 0.1f, 1f);
            var view1 = Matrix.LookAtLH(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.Up);
            var view2 = Matrix.LookAtLH(new Vector3(+1, 0, 0), Vector3.Zero, Vector3.Up);
            var view3 = Matrix.LookAtLH(new Vector3(-2, 0, 0), new Vector3(-3, 0, 0), Vector3.Up);
            var view4 = Matrix.LookAtLH(new Vector3(-2, 0, 0), new Vector3(-1, 0, 0), Vector3.Up);

            frustum1 = new IntersectionVolumeFrustum(view1 * proj);
            frustum2 = new IntersectionVolumeFrustum(view2 * proj);
            frustum3 = new IntersectionVolumeFrustum(view3 * proj);
            frustum4 = new IntersectionVolumeFrustum(view4 * proj);
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
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
            bool res3 = IntersectionHelper.Intersects(frustum1, frustum4);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }
    }
}
