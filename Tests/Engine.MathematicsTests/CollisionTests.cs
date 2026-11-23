using Engine.Common;
using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class CollisionTests
    {
        public TestContext TestContext { get; set; }

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


            tmesh1 = [.. GeometryUtil.ComputeTriangleList(box1)];
            tmesh2 = [.. GeometryUtil.ComputeTriangleList(box2)];
            tmesh3 = [.. GeometryUtil.ComputeTriangleList(box3)];
            tmesh4 = [.. GeometryUtil.ComputeTriangleList(box4)];
            tmesh5 = [.. GeometryUtil.ComputeTriangleList(box5)];

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

            var proj = Matrix4x4.PerspectiveFovLH(MathUtils.PiOver2, 1f, 0.1f, 1f);
            var view1 = Matrix4x4.LookAtLH(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.Up);
            var view2 = Matrix4x4.LookAtLH(new Vector3(+1, 0, 0), Vector3.Zero, Vector3.Up);
            var view3 = Matrix4x4.LookAtLH(new Vector3(-2, 0, 0), new Vector3(-3, 0, 0), Vector3.Up);

            bfrustum1 = new BoundingFrustum(view1 * proj);
            bfrustum2 = new BoundingFrustum(view2 * proj);
            bfrustum3 = new BoundingFrustum(view3 * proj);

            frustum1 = new IntersectionVolumeFrustum(view1 * proj);
            frustum2 = new IntersectionVolumeFrustum(view2 * proj);
            frustum3 = new IntersectionVolumeFrustum(view3 * proj);
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void IntersectsIntersectableTest()
        {
            Assert.IsFalse(Collision.Intersects(null, null));

            Assert.IsFalse(Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, null));
            Assert.IsFalse(Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, null, IntersectDetectionMode.Sphere));

            Assert.IsFalse(Collision.Intersects(null, IntersectDetectionMode.Sphere, i1.Object, IntersectDetectionMode.Sphere));
        }

        [TestMethod()]
        public void IntersectsIntersectableSphereTest()
        {
            bool res1 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, i2.Object, IntersectDetectionMode.Sphere);
            bool res2 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, i3.Object, IntersectDetectionMode.Sphere);
            bool res3 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, i4.Object, IntersectDetectionMode.Sphere);
            bool res4 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, i5.Object, IntersectDetectionMode.Sphere);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableBoxTest()
        {
            bool res1 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, i2.Object, IntersectDetectionMode.Box);
            bool res2 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, i3.Object, IntersectDetectionMode.Box);
            bool res3 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, i4.Object, IntersectDetectionMode.Box);
            bool res4 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, i5.Object, IntersectDetectionMode.Box);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableMeshTest()
        {
            bool res1 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, i2.Object, IntersectDetectionMode.Mesh);
            bool res2 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, i3.Object, IntersectDetectionMode.Mesh);
            bool res3 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, i4.Object, IntersectDetectionMode.Mesh);
            bool res4 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, i5.Object, IntersectDetectionMode.Mesh);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }

        [TestMethod()]
        public void IntersectsIntersectableVolumeSphereTest()
        {
            bool res1 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph2);
            bool res2 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph3);
            bool res3 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph4);
            bool res4 = Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableVolumeBoxTest()
        {
            bool res1 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, box2);
            bool res2 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, box3);
            bool res3 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, box4);
            bool res4 = Collision.Intersects(i1.Object, IntersectDetectionMode.Box, box5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsIntersectableVolumeMeshTest()
        {
            bool res1 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh2);
            bool res2 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh3);
            bool res3 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh4);
            bool res4 = Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }

        [TestMethod()]
        public void IntersectsVolumeSphereTest()
        {
            bool res1 = Collision.Intersects(sph1, sph2);
            bool res2 = Collision.Intersects(sph1, sph3);
            bool res3 = Collision.Intersects(sph1, sph4);
            bool res4 = Collision.Intersects(sph1, sph5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsVolumeBoxTest()
        {
            bool res1 = Collision.Intersects(box1, box2);
            bool res2 = Collision.Intersects(box1, box3);
            bool res3 = Collision.Intersects(box1, box4);
            bool res4 = Collision.Intersects(box1, box5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsVolumeMeshTest()
        {
            bool res1 = Collision.Intersects(mesh1, mesh2);
            bool res2 = Collision.Intersects(mesh1, mesh3);
            bool res3 = Collision.Intersects(mesh1, mesh4);
            bool res4 = Collision.Intersects(mesh1, mesh5);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
            Assert.IsTrue(res4);
        }
        [TestMethod()]
        public void IntersectsVolumeFrustumTest()
        {
            bool res1 = Collision.Intersects(frustum1, frustum2);
            bool res2 = Collision.Intersects(frustum1, frustum3);

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
            Assert.AreSequenceEqual(tmesh1, mesh1.GetTriangles());
            Assert.AreSequenceEqual(mesh1.GetTriangles(), tmesh1);
            Assert.AreNotSequenceEqual(tmesh1, mesh2.GetTriangles());
            Assert.AreNotSequenceEqual(mesh1.GetTriangles(), tmesh2);

            Assert.AreEqual(new IntersectionVolumeMesh(tmesh1), mesh1);
            Assert.AreEqual(mesh1, new IntersectionVolumeMesh(tmesh1));
            Assert.AreNotEqual(new IntersectionVolumeMesh(tmesh1), mesh2);
            Assert.AreNotEqual(mesh1, new IntersectionVolumeMesh(tmesh2));
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

            Assert.AreSequenceEqual(tmesh1, mesh.GetTriangles());
        }
        [TestMethod()]
        public void IntersectableMeshBadConstructorTest()
        {
            Assert.Throws<ArgumentException>(() => { var mesh = new IntersectionVolumeMesh([]); });
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
        }        [TestMethod]
        public void CollisionClosestPointFamilies_CoverPointPlaneBoxSphereAndSegment()
        {
            var plane=new Plane(Vector3.UnitY,0f); var point=new Vector3(2f,3f,-1f);
            Collision.ClosestPointPlanePoint(ref plane,ref point,out var planePoint); Assert.AreEqual(new Vector3(2f,0f,-1f),planePoint);

            var box=new BoundingBox(new Vector3(-1f),new Vector3(1f)); var outside=new Vector3(3f,-2f,4f);
            Collision.ClosestPointBoxPoint(ref box,ref outside,out var boxPoint); Assert.AreEqual(new Vector3(1f,-1f,1f),boxPoint);

            var sphere=new BoundingSphere(Vector3.Zero,2f);
            var sphere2=new BoundingSphere(new Vector3(6f,0f,0f),2f); Collision.ClosestPointSpherePoint(ref sphere,ref outside,out var spherePoint);
            Assert.AreEqual(2f,spherePoint.Length(),0.0001f);

            var p1=Vector3.Zero; var p2=new Vector3(2f,0f,0f); var segmentPoint=new Vector3(3f,4f,0f);
            Collision.ClosestPointInSegment(ref p1,ref p2,ref segmentPoint,out float segmentDistance,out var segmentPointResult);
            Assert.AreEqual(6f,segmentDistance,0.0001f); Assert.AreEqual(p2,segmentPointResult);

            var ray=new Ray(Vector3.Zero,Vector3.UnitX); var rayPoint=new Vector3(3f,4f,0f);
            Collision.ClosestPointInRay(ref ray,ref rayPoint,out float rayDistance,out var rayPointResult);
            Assert.AreEqual(MathF.Sqrt(20f),rayDistance,0.0001f); Assert.AreEqual(Vector3.UnitX,rayPointResult);
        }

        [TestMethod]
        public void CollisionDistanceFamilies_PointAndShapeDistances()
        {
            var plane=new Plane(Vector3.UnitY,0f); var point=new Vector3(0f,5f,0f); Assert.AreEqual(5f,Collision.DistancePlanePoint(ref plane,ref point));
            var box=new BoundingBox(new Vector3(-1f),new Vector3(1f)); var outside=new Vector3(3f,0f,0f);
            var boxPoint = Vector3.Zero;
            Assert.AreEqual(0f,Collision.DistanceBoxPoint(ref box,ref boxPoint)); Assert.AreEqual(2f,Collision.DistanceBoxPoint(ref box,ref outside)); Assert.AreEqual(0f,Collision.DistanceBoxBox(ref box,ref box));
            var sphere=new BoundingSphere(Vector3.Zero,2f);
            var sphere2=new BoundingSphere(new Vector3(6f,0f,0f),2f); var spherePoint = Vector3.Zero;
            Assert.AreEqual(0f,Collision.DistanceSpherePoint(ref sphere,ref spherePoint)); Assert.AreEqual(1f,Collision.DistanceSpherePoint(ref sphere,ref outside));
            Assert.AreEqual(2f,Collision.DistanceSphereSphere(ref sphere,ref sphere2));
        }

        [TestMethod]
        public void CollisionRayFamilies_PointSegmentPlaneAndSphere()
        {
            var ray=new Ray(Vector3.Zero,Vector3.UnitX); var point=new Vector3(2f,0f,0f); Assert.IsTrue(Collision.RayIntersectsPoint(ref ray,ref point));
            var segment=new Segment(new Vector3(2f,-1f,0f),new Vector3(2f,1f,0f)); Assert.IsTrue(Collision.RayIntersectsSegment(ref ray,ref segment,out float sd,out var sp)); Assert.AreEqual(2f,sd,0.0001f); Assert.AreEqual(new Vector3(2f,0f,0f),sp);
            var plane=new Plane(Vector3.UnitX,-2f); Assert.IsTrue(Collision.RayIntersectsPlane(ref ray,ref plane,out float pd,out var pp)); Assert.AreEqual(2f,pd,0.0001f); Assert.AreEqual(new Vector3(2f,0f,0f),pp);
            var sphere=new BoundingSphere(new Vector3(4f,0f,0f),1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref ray,ref sphere,out float sphd,out var sphp)); Assert.AreEqual(3f,sphd,0.0001f); Assert.AreEqual(new Vector3(3f,0f,0f),sphp);
        }

        [TestMethod]
        public void CollisionRayFamilies_TriangleBoxAndCircle()
        {
            var tri=new Triangle(new Vector3(-1f,-1f,0f),new Vector3(1f,-1f,0f),new Vector3(0f,1f,0f)); var ray=new Ray(new Vector3(0f,0f,-1f),Vector3.UnitZ);
            Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray,ref tri,out float td,out var tp)); Assert.AreEqual(1f,td,0.0001f); Assert.AreEqual(Vector3.Zero,tp);
            var box=new BoundingBox(new Vector3(-1f),new Vector3(1f)); Assert.IsTrue(Collision.RayIntersectsBox(ref ray,ref box,out float bd,out var bp)); Assert.AreEqual(0f,bd,0.0001f); Assert.AreEqual(new Vector3(0f,0f,-1f),bp);
            var circleCenter=Vector3.Zero; var circleNormal=Vector3.UnitZ;
            Assert.IsTrue(Collision.RayIntersectsCircle3D(ref ray,ref circleCenter,ref circleNormal,1f,out float cd,out var cp)); Assert.AreEqual(1f,cd,0.0001f); Assert.AreEqual(Vector3.Zero,cp);
        }

        [TestMethod]
        public void CollisionSegmentAndPointContainmentFamilies()
        {
            var box=new BoundingBox(new Vector3(-1f),new Vector3(1f)); var boxPoint=Vector3.Zero; var outsidePoint=new Vector3(2f,0f,0f); var segment=new Segment(new Vector3(-2f,0f,0f),new Vector3(2f,0f,0f));
            Assert.IsTrue(Collision.SegmentIntersectsBox(ref segment,ref box,out float bd,out var bp)); Assert.AreEqual(1f,bd,0.0001f); Assert.AreEqual(new Vector3(-1f,0f,0f),bp);
            Assert.AreEqual(ContainmentType.Contains,Collision.BoxContainsPoint(ref box,ref boxPoint)); Assert.AreEqual(ContainmentType.Disjoint,Collision.BoxContainsPoint(ref box,ref outsidePoint));
            var sphere=new BoundingSphere(Vector3.Zero,1f); var spherePoint=Vector3.Zero; Assert.AreEqual(ContainmentType.Contains,Collision.SphereContainsPoint(ref sphere,ref spherePoint)); Assert.AreEqual(ContainmentType.Disjoint,Collision.SphereContainsPoint(ref sphere,ref outsidePoint));
        }

        [TestMethod] public void Collision_Overload_004_ClosestPointPointTriangle() { var p = new Vector3(0.5f, 0.5f, 2f); var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Collision.ClosestPointPointTriangle(ref p, ref a, ref b, ref c, out var q); AssertVector(new Vector3(0.5f, 0.5f, 0f), q); }
        [TestMethod] public void Collision_Overload_005_ClosestPointPlanePoint() { var plane = new Plane(Vector3.UnitY, 0f); var p = new Vector3(2f, 3f, -1f); Collision.ClosestPointPlanePoint(ref plane, ref p, out var q); AssertVector(new Vector3(2f, 0f, -1f), q); }
        [TestMethod] public void Collision_Overload_006_ClosestPointBoxPoint() { var box = CoverageBox(); var p = new Vector3(2f, 0f, 0f); Collision.ClosestPointBoxPoint(ref box, ref p, out var q); AssertVector(Vector3.UnitX, q); }
        [TestMethod] public void Collision_Overload_007_ClosestPointSpherePoint() { var sphere = new BoundingSphere(Vector3.Zero, 1f); var p = new Vector3(3f, 0f, 0f); Collision.ClosestPointSpherePoint(ref sphere, ref p, out var q); AssertVector(Vector3.UnitX, q); }
        [TestMethod] public void Collision_Overload_008_ClosestPointSphereSphere() { var a = new BoundingSphere(Vector3.Zero, 1f); var b = new BoundingSphere(new Vector3(4f, 0f, 0f), 2f); Collision.ClosestPointSphereSphere(ref a, ref b, out var q); AssertVector(Vector3.UnitX, q); }
        [TestMethod] public void Collision_Overload_009_ClosestPointInRay() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var p = new Vector3(0.5f, 2f, 0f); Collision.ClosestPointInRay(ref ray, ref p, out var q); AssertVector(new Vector3(0.5f, 0f, 0f), q); }
        [TestMethod] public void Collision_Overload_010_ClosestPointInRay() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var p = new Vector3(0.5f, 2f, 0f); Collision.ClosestPointInRay(ref ray, ref p, out float d, out var q); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0f, 0f), q); }
        [TestMethod] public void Collision_Overload_011_ClosestPointInSegment() { var a = new Vector3(-1f, 0f, 0f); var b = Vector3.UnitX; var p = new Vector3(0f, 2f, 0f); Collision.ClosestPointInSegment(ref a, ref b, ref p, out var q); AssertVector(Vector3.Zero, q); }
        [TestMethod] public void Collision_Overload_012_ClosestPointInSegment() { var a = new Vector3(-1f, 0f, 0f); var b = Vector3.UnitX; var p = new Vector3(0f, 2f, 0f); Collision.ClosestPointInSegment(ref a, ref b, ref p, out float t, out var q); Assert.AreEqual(0.5f, t, 1e-5f); AssertVector(Vector3.Zero, q); }
        [TestMethod] public void Collision_Overload_013_ClosestPointInSegment() { var segment = new Segment(new Vector3(-1f, 0f, 0f), Vector3.UnitX); var p = new Vector3(0f, 2f, 0f); Collision.ClosestPointInSegment(ref segment, ref p, out var q); AssertVector(Vector3.Zero, q); }
        [TestMethod] public void Collision_Overload_014_ClosestPointInSegment() { var segment = new Segment(new Vector3(-1f, 0f, 0f), Vector3.UnitX); var p = new Vector3(0f, 2f, 0f); Collision.ClosestPointInSegment(ref segment, ref p, out float t, out var q); Assert.AreEqual(0.5f, t, 1e-5f); AssertVector(Vector3.Zero, q); }
        [TestMethod] public void Collision_Overload_015_ClosestPointInBox() { var box = CoverageBox(); var p = new Vector3(2f, 0f, 0f); Collision.ClosestPointInBox(ref p, ref box, out var q); AssertVector(Vector3.UnitX, q); }
        [TestMethod] public void Collision_Overload_016_ClosestPointInBox() { var obb = new OrientedBoundingBox(new Vector3(-1f), Vector3.One); var p = new Vector3(2f, 0f, 0f); Collision.ClosestPointInBox(ref p, ref obb, out var q); AssertVector(Vector3.UnitX, q); }
        [TestMethod] public void Collision_Overload_017_ClosestPointInBox() { var p = new Vector3(2f, 0f, 0f); var center = Vector3.Zero; var extents = Vector3.One; var transform = Matrix4x4.Identity; Collision.ClosestPointInBox(ref p, ref center, ref extents, ref transform, out var q); AssertVector(Vector3.UnitX, q); }
        [TestMethod] public void Collision_Overload_018_ClosestPointInTriangle() { var tri = CoverageTriangle(); var p = new Vector3(0.5f, 0.5f, 2f); Collision.ClosestPointInTriangle(ref p, ref tri, out var q); AssertVector(new Vector3(0.5f, 0.5f, 0f), q); }
        [TestMethod] public void Collision_Overload_019_ClosestPointInTriangle() { var tri = CoverageTriangle(); var p = new Vector3(0.5f, 0.5f, 2f); Collision.ClosestPointInTriangle(ref p, ref tri, out float d, out var q); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), q); }
        [TestMethod] public void Collision_Overload_020_ClosestPointInMesh() { var tri = CoverageTriangle(); Triangle[] mesh = [tri]; var p = new Vector3(0.5f, 0.5f, 2f); Collision.ClosestPointInMesh(ref p, mesh, out var closest, out var q); Assert.AreEqual(tri, closest); AssertVector(new Vector3(0.5f, 0.5f, 0f), q); }
        [TestMethod] public void Collision_Overload_021_ClosestPointInMesh() { var tri = CoverageTriangle(); Triangle[] mesh = [tri]; var p = new Vector3(0.5f, 0.5f, 2f); Collision.ClosestPointInMesh(ref p, mesh, out var closest, out float d, out var q); Assert.AreEqual(tri, closest); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), q); }
        [TestMethod] public void Collision_Overload_022_DistancePlanePoint() { var plane = new Plane(Vector3.UnitY, 0f); var p = new Vector3(0f, 2f, 0f); Assert.AreEqual(2f, Collision.DistancePlanePoint(ref plane, ref p), 1e-5f); }
        [TestMethod] public void Collision_Overload_023_DistanceBoxPoint() { var box = CoverageBox(); var p = new Vector3(3f, 0f, 0f); Assert.AreEqual(2f, Collision.DistanceBoxPoint(ref box, ref p), 1e-5f); }
        [TestMethod] public void Collision_Overload_024_DistanceBoxBox() { var a = CoverageBox(); var b = new BoundingBox(new Vector3(3f, -1f, -1f), new Vector3(5f, 1f, 1f)); Assert.AreEqual(2f, Collision.DistanceBoxBox(ref a, ref b), 1e-5f); }
        [TestMethod] public void Collision_Overload_025_DistanceSpherePoint() { var sphere = new BoundingSphere(Vector3.Zero, 1f); var p = new Vector3(3f, 0f, 0f); Assert.AreEqual(2f, Collision.DistanceSpherePoint(ref sphere, ref p), 1e-5f); }
        [TestMethod] public void Collision_Overload_026_DistanceSphereSphere() { var a = new BoundingSphere(Vector3.Zero, 1f); var b = new BoundingSphere(new Vector3(4f, 0f, 0f), 1f); Assert.AreEqual(2f, Collision.DistanceSphereSphere(ref a, ref b), 1e-5f); }

        [TestMethod] public void Collision_Overload_027_RayIntersectsPoint() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var hit = new Vector3(2f, 0f, 0f); var miss = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsPoint(ref ray, ref hit)); Assert.IsFalse(Collision.RayIntersectsPoint(ref ray, ref miss)); AssertNegativeRayOverloads(); }
        [TestMethod] public void Collision_Overload_028_RayIntersectsRay() { var a = new Ray(Vector3.Zero, Vector3.UnitX); var b = new Ray(Vector3.Zero, Vector3.UnitY); Assert.IsTrue(Collision.RayIntersectsRay(ref a, ref b, out var p)); AssertVector(Vector3.Zero, p); }
        [TestMethod] public void Collision_Overload_029_RayIntersectsRay() { var p1 = Vector3.Zero; var d1 = Vector3.UnitX; var p2 = Vector3.Zero; var d2 = Vector3.UnitY; Assert.IsTrue(Collision.RayIntersectsRay(ref p1, ref d1, ref p2, ref d2, out var p)); AssertVector(Vector3.Zero, p); }
        [TestMethod] public void Collision_Overload_030_RayIntersectsSegment() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var segment = new Segment(new Vector3(2f, -1f, 0f), new Vector3(2f, 1f, 0f)); Assert.IsTrue(Collision.RayIntersectsSegment(ref ray, ref segment, out var p)); AssertVector(new Vector3(2f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_031_RayIntersectsSegment() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var segment = new Segment(new Vector3(2f, -1f, 0f), new Vector3(2f, 1f, 0f)); Assert.IsTrue(Collision.RayIntersectsSegment(ref ray, ref segment, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(2f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_032_RayIntersectsSegment() { var rp = Vector3.Zero; var rd = Vector3.UnitX; var sp = new Vector3(2f, -1f, 0f); var sd = new Vector3(0f, 2f, 0f); var len = 2f; Assert.IsTrue(Collision.RayIntersectsSegment(ref rp, ref rd, ref sp, ref sd, ref len, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(2f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_033_RayIntersectsSegment() { var ray = new PickingRay(new Ray(Vector3.Zero, Vector3.UnitX)); var segment = new Segment(new Vector3(2f, -1f, 0f), new Vector3(2f, 1f, 0f)); Assert.IsTrue(Collision.RayIntersectsSegment(ref ray, ref segment, out var p)); AssertVector(new Vector3(2f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_034_RayIntersectsSegment() { var ray = new PickingRay(new Ray(Vector3.Zero, Vector3.UnitX)); var segment = new Segment(new Vector3(2f, -1f, 0f), new Vector3(2f, 1f, 0f)); Assert.IsTrue(Collision.RayIntersectsSegment(ref ray, ref segment, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(2f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_035_RayIntersectsPlane() { var p = Vector3.Zero; var d = Vector3.UnitX; var plane = new Plane(Vector3.UnitX, -2f); Assert.IsTrue(Collision.RayIntersectsPlane(ref p, ref d, ref plane, out float t)); Assert.AreEqual(2f, t, 1e-5f); }
        [TestMethod] public void Collision_Overload_036_RayIntersectsPlane() { var p = Vector3.Zero; var d = Vector3.UnitX; var plane = new Plane(Vector3.UnitX, -2f); Assert.IsTrue(Collision.RayIntersectsPlane(ref p, ref d, ref plane, out float t, out var hit)); Assert.AreEqual(2f, t, 1e-5f); AssertVector(new Vector3(2f, 0f, 0f), hit); }
        [TestMethod] public void Collision_Overload_037_RayIntersectsPlane() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var plane = new Plane(Vector3.UnitX, -2f); Assert.IsTrue(Collision.RayIntersectsPlane(ref ray, ref plane, out float t)); Assert.AreEqual(2f, t, 1e-5f); }
        [TestMethod] public void Collision_Overload_038_RayIntersectsPlane() { var ray = new Ray(Vector3.Zero, Vector3.UnitX); var plane = new Plane(Vector3.UnitX, -2f); Assert.IsTrue(Collision.RayIntersectsPlane(ref ray, ref plane, out float t, out var hit)); Assert.AreEqual(2f, t, 1e-5f); AssertVector(new Vector3(2f, 0f, 0f), hit); }

        [TestMethod] public void Collision_Overload_039_RayIntersectsTriangle() { var rp = new Vector3(0.5f, 0.5f, 2f); var rd = -Vector3.UnitZ; var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsTriangle(ref rp, ref rd, ref a, ref b, ref c, out float d)); Assert.AreEqual(2f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_040_RayIntersectsTriangle() { var rp = new Vector3(0.5f, 0.5f, 2f); var rd = -Vector3.UnitZ; var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsTriangle(ref rp, ref rd, ref a, ref b, ref c, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), p); }
        [TestMethod] public void Collision_Overload_041_RayIntersectsTriangle() { var ray = new Ray(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref a, ref b, ref c, out float d)); Assert.AreEqual(2f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_042_RayIntersectsTriangle() { var ray = new Ray(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref a, ref b, ref c, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), p); }
        [TestMethod] public void Collision_Overload_043_RayIntersectsTriangle() { var ray = new Ray(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var tri = CoverageTriangle(); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref tri, out float d)); Assert.AreEqual(2f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_044_RayIntersectsTriangle() { var ray = new Ray(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var tri = CoverageTriangle(); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref tri, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), p); }
        [TestMethod] public void Collision_Overload_045_RayIntersectsTriangle() { var ray = new PickingRay(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref a, ref b, ref c, out float d)); Assert.AreEqual(2f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_046_RayIntersectsTriangle() { var ray = new PickingRay(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var a = Vector3.Zero; var b = new Vector3(2f, 0f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref a, ref b, ref c, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), p); }
        [TestMethod] public void Collision_Overload_047_RayIntersectsTriangle() { var ray = new PickingRay(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var tri = CoverageTriangle(); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref tri, out float d)); Assert.AreEqual(2f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_048_RayIntersectsTriangle() { var ray = new PickingRay(new Vector3(0.5f, 0.5f, 2f), -Vector3.UnitZ); var tri = CoverageTriangle(); Assert.IsTrue(Collision.RayIntersectsTriangle(ref ray, ref tri, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), p); }

        [TestMethod] public void Collision_Overload_049_RayIntersectsBox() { var rp = new Vector3(-2f, 0f, 0f); var rd = Vector3.UnitX; var box = CoverageBox(); Assert.IsTrue(Collision.RayIntersectsBox(ref rp, ref rd, ref box, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_050_RayIntersectsBox() { var rp = new Vector3(-2f, 0f, 0f); var rd = Vector3.UnitX; var box = CoverageBox(); Assert.IsTrue(Collision.RayIntersectsBox(ref rp, ref rd, ref box, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_051_RayIntersectsBox() { var ray = new Ray(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var box = CoverageBox(); Assert.IsTrue(Collision.RayIntersectsBox(ref ray, ref box, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_052_RayIntersectsBox() { var ray = new Ray(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var box = CoverageBox(); Assert.IsTrue(Collision.RayIntersectsBox(ref ray, ref box, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_053_RayIntersectsBox() { var ray = new PickingRay(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var box = CoverageBox(); Assert.IsTrue(Collision.RayIntersectsBox(ref ray, ref box, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_054_RayIntersectsBox() { var ray = new PickingRay(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var box = CoverageBox(); Assert.IsTrue(Collision.RayIntersectsBox(ref ray, ref box, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_055_RayIntersectsSphere() { var rp = new Vector3(-2f, 0f, 0f); var rd = Vector3.UnitX; var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref rp, ref rd, ref sphere, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_056_RayIntersectsSphere() { var rp = new Vector3(-2f, 0f, 0f); var rd = Vector3.UnitX; var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref rp, ref rd, ref sphere, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_057_RayIntersectsSphere() { var ray = new Ray(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref ray, ref sphere, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_058_RayIntersectsSphere() { var ray = new Ray(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref ray, ref sphere, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_059_RayIntersectsSphere() { var ray = new PickingRay(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref ray, ref sphere, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_060_RayIntersectsSphere() { var ray = new PickingRay(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.IsTrue(Collision.RayIntersectsSphere(ref ray, ref sphere, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_061_RayIntersectsFrustum() { var pos = new Vector3(-2f, 0f, 0f); var dir = Vector3.UnitX; var f = CoverageFrustum(); Assert.IsTrue(Collision.RayIntersectsFrustum(ref pos, ref dir, ref f, out float? entering, out float? exiting)); Assert.IsTrue(entering.HasValue); Assert.IsTrue(exiting.HasValue); Assert.AreEqual(1f, entering.Value, 1e-4f); Assert.AreEqual(3f, exiting.Value, 1e-4f); }
        [TestMethod] public void Collision_Overload_062_RayIntersectsFrustum() { var ray = new Ray(new Vector3(-2f, 0f, 0f), Vector3.UnitX); var f = CoverageFrustum(); Assert.IsTrue(Collision.RayIntersectsFrustum(ref ray, ref f, out float? entering, out float? exiting)); Assert.IsTrue(entering.HasValue); Assert.IsTrue(exiting.HasValue); Assert.AreEqual(1f, entering.Value, 1e-4f); Assert.AreEqual(3f, exiting.Value, 1e-4f); }
        [TestMethod] public void Collision_Overload_063_RayIntersectsCircle3D() { var ray = new Ray(new Vector3(0f, 0f, -2f), Vector3.UnitZ); var center = Vector3.Zero; var normal = Vector3.UnitZ; Assert.IsTrue(Collision.RayIntersectsCircle3D(ref ray, ref center, ref normal, 3f, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(Vector3.Zero, p); }

        [TestMethod] public void Collision_Overload_064_SegmentIntersectsBox() { var box = CoverageBox(); var segment = new Segment(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)); Assert.IsTrue(Collision.SegmentIntersectsBox(ref segment, ref box, out float d)); Assert.AreEqual(1f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_065_SegmentIntersectsBox() { var box = CoverageBox(); var segment = new Segment(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)); Assert.IsTrue(Collision.SegmentIntersectsBox(ref segment, ref box, out float d, out var p)); Assert.AreEqual(1f, d, 1e-5f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_066_SegmentIntersectsTriangle() { var tri = CoverageTriangle(); var segment = new Segment(new Vector3(0.5f, 0.5f, 2f), new Vector3(0.5f, 0.5f, -2f)); Assert.IsTrue(Collision.SegmentIntersectsTriangle(ref segment, ref tri, out float d)); Assert.AreEqual(2f, d, 1e-5f); }
        [TestMethod] public void Collision_Overload_067_SegmentIntersectsTriangle() { var tri = CoverageTriangle(); var segment = new Segment(new Vector3(0.5f, 0.5f, 2f), new Vector3(0.5f, 0.5f, -2f)); Assert.IsTrue(Collision.SegmentIntersectsTriangle(ref segment, ref tri, out float d, out var p)); Assert.AreEqual(2f, d, 1e-5f); AssertVector(new Vector3(0.5f, 0.5f, 0f), p); }

        [TestMethod] public void Collision_Overload_073_PlaneIntersectsPoint() { var plane = new Plane(Vector3.UnitY, 0f); var front = new Vector3(0f, 2f, 0f); var back = new Vector3(0f, -2f, 0f); var on = Vector3.Zero; Assert.AreEqual(PlaneIntersectionType.Front, Collision.PlaneIntersectsPoint(ref plane, ref front)); Assert.AreEqual(PlaneIntersectionType.Back, Collision.PlaneIntersectsPoint(ref plane, ref back)); Assert.AreEqual(PlaneIntersectionType.Intersecting, Collision.PlaneIntersectsPoint(ref plane, ref on)); }
        [TestMethod] public void Collision_Overload_074_PlaneIntersectsPoints() { var plane = new Plane(Vector3.UnitY, 0f); Assert.AreEqual(PlaneIntersectionType.Front, Collision.PlaneIntersectsPoints(ref plane, [new Vector3(0f, 1f, 0f), new Vector3(0f, 2f, 0f)])); Assert.AreEqual(PlaneIntersectionType.Intersecting, Collision.PlaneIntersectsPoints(ref plane, [new Vector3(0f, -1f, 0f), new Vector3(0f, 1f, 0f)])); }
        [TestMethod] public void Collision_Overload_075_PlaneIntersectsPlane() { var x = new Plane(Vector3.UnitX, 0f); var y = new Plane(Vector3.UnitY, 0f); var parallel = new Plane(Vector3.UnitX, -2f); Assert.IsTrue(Collision.PlaneIntersectsPlane(ref x, ref y)); Assert.IsFalse(Collision.PlaneIntersectsPlane(ref x, ref parallel)); }
        [TestMethod] public void Collision_Overload_076_PlaneIntersectsPlane() { var x = new Plane(Vector3.UnitX, 0f); var y = new Plane(Vector3.UnitY, 0f); Assert.IsTrue(Collision.PlaneIntersectsPlane(ref x, ref y, out var line)); AssertVector(Vector3.Zero, line.Position); Assert.AreEqual(1f, line.Direction.Length(), 1e-5f); }
        [TestMethod] public void Collision_Overload_077_PlaneIntersectsTriangle() { var plane = new Plane(Vector3.UnitY, 0f); var a = new Vector3(0f, 1f, 0f); var b = new Vector3(2f, 1f, 0f); var c = new Vector3(0f, 2f, 0f); Assert.AreEqual(PlaneIntersectionType.Front, Collision.PlaneIntersectsTriangle(ref plane, ref a, ref b, ref c)); var tri = CoverageTriangle(); Assert.AreEqual(PlaneIntersectionType.Intersecting, Collision.PlaneIntersectsTriangle(ref plane, ref tri.Point1, ref tri.Point2, ref tri.Point3)); }
        [TestMethod] public void Collision_Overload_078_PlaneIntersectsBox() { var plane = new Plane(Vector3.UnitX, 0f); var box = CoverageBox(); Assert.AreEqual(PlaneIntersectionType.Intersecting, Collision.PlaneIntersectsBox(ref plane, ref box)); }
        [TestMethod] public void Collision_Overload_079_PlaneIntersectsSphere() { var plane = new Plane(Vector3.UnitY, 0f); var sphere = new BoundingSphere(Vector3.Zero, 1f); Assert.AreEqual(PlaneIntersectionType.Intersecting, Collision.PlaneIntersectsSphere(ref plane, ref sphere)); }
        [TestMethod] public void Collision_Overload_080_PlaneIntersectsCylinder() { var cylinder = new BoundingCylinder(Vector3.Zero, 0.5f, 2f); var plane = new Plane(Vector3.UnitY, 0f); Assert.IsTrue(Collision.PlaneIntersectsCylinder(ref cylinder, ref plane)); }
        [TestMethod] public void Collision_Overload_081_PlaneIntersectsCapsule() { var capsule = new BoundingCapsule(Vector3.Zero, 0.5f, 2f); var plane = new Plane(Vector3.UnitY, 0f); Assert.IsTrue(Collision.PlaneIntersectsCapsule(ref capsule, ref plane)); }
        [TestMethod] public void Collision_Overload_082_PlanesIntersectsPoint() { var x = new Plane(Vector3.UnitX, -1f); var y = new Plane(Vector3.UnitY, -2f); var z = new Plane(Vector3.UnitZ, -3f); Collision.PlanesIntersectsPoint(ref x, ref y, ref z, out var p); AssertVector(new Vector3(1f, 2f, 3f), p); }


        [TestMethod] public void Collision_Overload_001_Intersects() { Assert.IsTrue(Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, i2.Object, IntersectDetectionMode.Sphere)); Assert.IsFalse(Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, i3.Object, IntersectDetectionMode.Sphere)); Assert.IsTrue(Collision.Intersects(i1.Object, IntersectDetectionMode.Box, i2.Object, IntersectDetectionMode.Box)); AssertNegativeShapeResultOverloads(); }
        [TestMethod] public void Collision_Overload_002_Intersects() { Assert.IsTrue(Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph2)); Assert.IsFalse(Collision.Intersects(i1.Object, IntersectDetectionMode.Sphere, sph3)); Assert.IsTrue(Collision.Intersects(i1.Object, IntersectDetectionMode.Mesh, mesh2)); }
        [TestMethod] public void Collision_Overload_003_Intersects() { Assert.IsTrue(Collision.Intersects(sph1, sph2)); Assert.IsFalse(Collision.Intersects(sph1, sph3)); Assert.IsTrue(Collision.Intersects(box1, box2)); }
        [TestMethod] public void Collision_Overload_068_TriangleIntersectsPoint() { var tri = CoverageTriangle(); var inside = new Vector3(0.5f, 0.5f, 0f); var outside = new Vector3(3f, 3f, 0f); Assert.IsTrue(Collision.TriangleIntersectsPoint(ref tri, ref inside)); Assert.IsFalse(Collision.TriangleIntersectsPoint(ref tri, ref outside)); }
        [TestMethod] public void Collision_Overload_069_TriangleIntersectsTriangle() { var a = CoverageTriangle(); var b = CoverageTriangle(); var c = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.TriangleIntersectsTriangle(ref a, ref b)); Assert.IsFalse(Collision.TriangleIntersectsTriangle(ref a, ref c)); }
        [TestMethod] public void Collision_Overload_070_TriangleIntersectsTriangle() { var a = CoverageTriangle(); var b = CoverageTriangle(); var c = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.TriangleIntersectsTriangle(ref a, ref b, out var segment)); Assert.IsFalse(segment.HasValue); var crossing = new Triangle(new Vector3(0.5f, -1f, -1f), new Vector3(0.5f, 2f, -1f), new Vector3(0.5f, 0f, 1f)); Assert.IsTrue(Collision.TriangleIntersectsTriangle(ref a, ref crossing, out segment)); Assert.IsTrue(segment.HasValue); AssertSegment(new Segment(new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, 1f, 0f)), segment.Value); Assert.IsFalse(Collision.TriangleIntersectsTriangle(ref a, ref c, out segment)); Assert.IsFalse(segment.HasValue); }
        [TestMethod] public void Collision_Overload_071_TriangleIntersectsMesh() { var tri = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.TriangleIntersectsMesh(ref tri, [tri, far], out var hits)); Assert.AreEqual(1, hits.Length); Assert.AreEqual(tri, hits[0]); Assert.IsFalse(Collision.TriangleIntersectsMesh(ref far, [tri], out hits)); Assert.AreEqual(0, hits.Length); }
        [TestMethod] public void Collision_Overload_072_TriangleIntersectsMesh() { var tri = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.TriangleIntersectsMesh(ref tri, [tri, far], out var hits, out var segments)); Assert.AreEqual(1, hits.Length); Assert.AreEqual(tri, hits[0]); Assert.AreEqual(0, segments.Length); var crossing = new Triangle(new Vector3(0.5f, -1f, -1f), new Vector3(0.5f, 2f, -1f), new Vector3(0.5f, 0f, 1f)); Assert.IsTrue(Collision.TriangleIntersectsMesh(ref tri, [crossing], out hits, out segments)); Assert.AreEqual(1, hits.Length); Assert.AreEqual(crossing, hits[0]); Assert.AreEqual(1, segments.Length); AssertSegment(new Segment(new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, 1f, 0f)), segments[0]); Assert.IsFalse(Collision.TriangleIntersectsMesh(ref far, [tri], out hits, out segments)); Assert.AreEqual(0, hits.Length); Assert.AreEqual(0, segments.Length); }

        [TestMethod] public void Collision_Overload_083_BoxIntersectsBox() { var a = CoverageBox(); var b = new BoundingBox(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(2f, 0.5f, 0.5f)); var far = new BoundingBox(new Vector3(3f), new Vector3(4f)); Assert.IsTrue(Collision.BoxIntersectsBox(ref a, ref b)); Assert.IsFalse(Collision.BoxIntersectsBox(ref a, ref far)); }
        [TestMethod] public void Collision_Overload_084_BoxIntersectsSphere() { var box = CoverageBox(); var near = new BoundingSphere(Vector3.Zero, 0.5f); var far = new BoundingSphere(new Vector3(3f, 0f, 0f), 0.5f); Assert.IsTrue(Collision.BoxIntersectsSphere(ref box, ref near)); Assert.IsFalse(Collision.BoxIntersectsSphere(ref box, ref far)); }
        [TestMethod] public void Collision_Overload_085_BoxIntersectsFrustum() { var box = new BoundingBox(new Vector3(-2f), new Vector3(2f)); var far = new BoundingBox(new Vector3(4f), new Vector3(5f)); var f = CoverageFrustum(); Assert.IsTrue(Collision.BoxIntersectsFrustum(ref box, ref f)); Assert.IsFalse(Collision.BoxIntersectsFrustum(ref far, ref f)); }
        [TestMethod] public void Collision_Overload_086_BoxIntersectsTriangle() { var box = CoverageBox(); var tri = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.BoxIntersectsTriangle(ref box, ref tri)); Assert.IsFalse(Collision.BoxIntersectsTriangle(ref box, ref far)); }
        [TestMethod] public void Collision_Overload_087_BoxIntersectsMesh() { var box = CoverageBox(); var tri = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.BoxIntersectsMesh(ref box, [tri])); Assert.IsFalse(Collision.BoxIntersectsMesh(ref box, [far])); }
        [TestMethod] public void Collision_Overload_088_BoxIntersectsPlane() { var box = CoverageBox(); var crossing = new Plane(Vector3.UnitX, 0f); var separate = new Plane(Vector3.UnitX, -3f); Assert.IsTrue(Collision.BoxIntersectsPlane(ref box, ref crossing)); Assert.IsFalse(Collision.BoxIntersectsPlane(ref box, ref separate)); }
        [TestMethod] public void Collision_Overload_089_SphereIntersectsTriangle() { var tri = CoverageTriangle(); var hit = new BoundingSphere(new Vector3(0.5f, 0.5f, 0f), 0.25f); var miss = new BoundingSphere(new Vector3(4f, 4f, 0f), 0.25f); Assert.IsTrue(Collision.SphereIntersectsTriangle(ref hit, ref tri.Point1, ref tri.Point2, ref tri.Point3)); Assert.IsFalse(Collision.SphereIntersectsTriangle(ref miss, ref tri.Point1, ref tri.Point2, ref tri.Point3)); }
        [TestMethod] public void Collision_Overload_090_SphereIntersectsTriangle() { var tri = CoverageTriangle(); var hit = new BoundingSphere(new Vector3(0.5f, 0.5f, 0f), 0.25f); var miss = new BoundingSphere(new Vector3(4f, 4f, 0f), 0.25f); Assert.IsTrue(Collision.SphereIntersectsTriangle(ref hit, ref tri)); Assert.IsFalse(Collision.SphereIntersectsTriangle(ref miss, ref tri)); }
        [TestMethod] public void Collision_Overload_091_SphereIntersectsTriangle() { var tri = CoverageTriangle(); var hit = new BoundingSphere(new Vector3(0.5f, 0.5f, 1f), 1f); Assert.IsTrue(Collision.SphereIntersectsTriangle(ref hit, ref tri, out var pick)); AssertVector(new Vector3(0.5f, 0.5f, 0f), pick.Position); Assert.AreEqual(1f, pick.Distance, 1e-5f); Assert.AreEqual(tri, pick.Primitive); }
        [TestMethod] public void Collision_Overload_092_SphereIntersectsSphere() { var a = new BoundingSphere(Vector3.Zero, 1f); var overlap = new BoundingSphere(new Vector3(1.5f, 0f, 0f), 1f); var far = new BoundingSphere(new Vector3(4f, 0f, 0f), 1f); Assert.IsTrue(Collision.SphereIntersectsSphere(ref a, ref overlap)); Assert.IsFalse(Collision.SphereIntersectsSphere(ref a, ref far)); }
        [TestMethod] public void Collision_Overload_093_SphereIntersectsBox() { var sphere = new BoundingSphere(Vector3.Zero, 1f); var box = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)); var far = new BoundingBox(new Vector3(3f), new Vector3(4f)); Assert.IsTrue(Collision.SphereIntersectsBox(ref sphere, ref box)); Assert.IsFalse(Collision.SphereIntersectsBox(ref sphere, ref far)); }
        [TestMethod] public void Collision_Overload_094_SphereIntersectsFrustum() { var f = CoverageFrustum(); var hit = new BoundingSphere(Vector3.Zero, 0.25f); var far = new BoundingSphere(new Vector3(4f, 0f, 0f), 0.25f); Assert.IsTrue(Collision.SphereIntersectsFrustum(ref hit, ref f)); Assert.IsFalse(Collision.SphereIntersectsFrustum(ref far, ref f)); }
        [TestMethod] public void Collision_Overload_095_SphereIntersectsPlane() { var sphere = new BoundingSphere(Vector3.Zero, 1f); var crossing = new Plane(Vector3.UnitY, 0f); var far = new Plane(Vector3.UnitY, -3f); Assert.IsTrue(Collision.SphereIntersectsPlane(ref sphere, ref crossing)); Assert.IsFalse(Collision.SphereIntersectsPlane(ref sphere, ref far)); }
        [TestMethod] public void Collision_Overload_096_SphereIntersectsMesh() { var tri = CoverageTriangle(); var sphere = new BoundingSphere(new Vector3(0.5f, 0.5f, 1f), 1f); Assert.IsTrue(Collision.SphereIntersectsMesh(ref sphere, [tri], out var pick)); AssertVector(new Vector3(0.5f, 0.5f, 0f), pick.Position); Assert.AreEqual(1f, pick.Distance, 1e-5f); Assert.AreEqual(tri, pick.Primitive); }
        [TestMethod] public void Collision_Overload_097_SphereIntersectsMeshAll() { var tri = CoverageTriangle(); var small = new Triangle(Vector3.Zero, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f)); var sphere = new BoundingSphere(new Vector3(0.25f, 0.25f, 1f), 2f); Assert.IsTrue(Collision.SphereIntersectsMeshAll(ref sphere, [tri, small], out var picks)); Assert.AreEqual(2, picks.Length); Assert.AreEqual(tri, picks[0].Primitive); Assert.AreEqual(small, picks[1].Primitive); Assert.AreEqual(1f, picks[0].Distance, 1e-5f); Assert.AreEqual(1f, picks[1].Distance, 1e-5f); }

        [TestMethod] public void Collision_Overload_098_FrustumIntersectsFrustum() { var a = CoverageFrustum(); var b = CoverageFrustum(); var far = new BoundingFrustum(Matrix4x4.Translation(new Vector3(5f, 0f, 0f))); Assert.IsTrue(Collision.FrustumIntersectsFrustum(ref a, ref b)); Assert.IsFalse(Collision.FrustumIntersectsFrustum(ref a, ref far)); }
        [TestMethod] public void Collision_Overload_099_FrustumIntersectsSegment() { var f = CoverageFrustum(); var a = new Vector3(-2f, 0f, 0f); var b = new Vector3(2f, 0f, 0f); Assert.IsTrue(Collision.FrustumIntersectsSegment(ref f, ref a, ref b, out float d)); Assert.AreEqual(1f, d, 1e-4f); }
        [TestMethod] public void Collision_Overload_100_FrustumIntersectsSegment() { var f = CoverageFrustum(); var a = new Vector3(-2f, 0f, 0f); var b = new Vector3(2f, 0f, 0f); Assert.IsTrue(Collision.FrustumIntersectsSegment(ref f, ref a, ref b, out float d, out var p)); Assert.AreEqual(1f, d, 1e-4f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_101_FrustumIntersectsSegment() { var f = CoverageFrustum(); var a = new Vector3(-2f, 0f, 0f); var b = new Vector3(2f, 0f, 0f); Assert.IsTrue(Collision.FrustumIntersectsSegment(ref f, ref a, ref b, out Vector3? entering, out Vector3? exiting)); Assert.IsTrue(entering.HasValue); Assert.IsTrue(exiting.HasValue); AssertVector(new Vector3(-1f, 0f, 0f), entering.Value); AssertVector(new Vector3(1f, 0f, 0f), exiting.Value); }
        [TestMethod] public void Collision_Overload_102_FrustumIntersectsSegment() { var f = CoverageFrustum(); var segment = new Segment(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)); Assert.IsTrue(Collision.FrustumIntersectsSegment(ref f, ref segment)); }
        [TestMethod] public void Collision_Overload_103_FrustumIntersectsSegment() { var f = CoverageFrustum(); var segment = new Segment(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)); Assert.IsTrue(Collision.FrustumIntersectsSegment(ref f, ref segment, out float d, out var p)); Assert.AreEqual(1f, d, 1e-4f); AssertVector(new Vector3(-1f, 0f, 0f), p); }
        [TestMethod] public void Collision_Overload_104_FrustumIntersectsSegment() { var f = CoverageFrustum(); var segment = new Segment(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)); Assert.IsTrue(Collision.FrustumIntersectsSegment(ref f, ref segment, out Vector3? entering, out Vector3? exiting)); Assert.IsTrue(entering.HasValue); Assert.IsTrue(exiting.HasValue); AssertVector(new Vector3(-1f, 0f, 0f), entering.Value); AssertVector(new Vector3(1f, 0f, 0f), exiting.Value); }
        [TestMethod] public void Collision_Overload_105_MeshIntersectsPoint() { var tri = CoverageTriangle(); var inside = new Vector3(0.5f, 0.5f, 0f); var outside = new Vector3(3f, 3f, 0f); Assert.IsTrue(Collision.MeshIntersectsPoint([tri], ref inside)); Assert.IsFalse(Collision.MeshIntersectsPoint([tri], ref outside)); }
        [TestMethod] public void Collision_Overload_106_MeshIntersectsMesh() { var tri = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.IsTrue(Collision.MeshIntersectsMesh([tri], [tri], out var hits, out var segments)); Assert.AreEqual(1, hits.Length); Assert.AreEqual(tri, hits[0]); Assert.AreEqual(0, segments.Length); var crossing = new Triangle(new Vector3(0.5f, -1f, -1f), new Vector3(0.5f, 2f, -1f), new Vector3(0.5f, 0f, 1f)); Assert.IsTrue(Collision.MeshIntersectsMesh([tri], [crossing], out hits, out segments)); Assert.AreEqual(1, hits.Length); Assert.AreEqual(crossing, hits[0]); Assert.AreEqual(1, segments.Length); AssertSegment(new Segment(new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, 1f, 0f)), segments[0]); Assert.IsFalse(Collision.MeshIntersectsMesh([tri], [far], out hits, out segments)); Assert.AreEqual(0, hits.Length); Assert.AreEqual(0, segments.Length); }

        [TestMethod] public void Collision_Overload_107_BoxContainsPoint() { var box = CoverageBox(); var inside = Vector3.Zero; var boundary = Vector3.UnitX; var outside = new Vector3(2f, 0f, 0f); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsPoint(ref box, ref inside)); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsPoint(ref box, ref boundary)); Assert.AreEqual(ContainmentType.Disjoint, Collision.BoxContainsPoint(ref box, ref outside)); }
        [TestMethod] public void Collision_Overload_108_BoxContainsBox() { var box = CoverageBox(); var inside = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)); var overlap = new BoundingBox(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(2f, 0.5f, 0.5f)); var far = new BoundingBox(new Vector3(3f), new Vector3(4f)); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsBox(ref box, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.BoxContainsBox(ref box, ref overlap)); Assert.AreEqual(ContainmentType.Disjoint, Collision.BoxContainsBox(ref box, ref far)); }
        [TestMethod] public void Collision_Overload_109_BoxContainsSphere() { var box = CoverageBox(); var inside = new BoundingSphere(Vector3.Zero, 0.5f); var overlap = new BoundingSphere(new Vector3(1.5f, 0f, 0f), 1f); var far = new BoundingSphere(new Vector3(3f, 0f, 0f), 0.5f); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsSphere(ref box, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.BoxContainsSphere(ref box, ref overlap)); Assert.AreEqual(ContainmentType.Disjoint, Collision.BoxContainsSphere(ref box, ref far)); }
        [TestMethod] public void Collision_Overload_110_BoxContainsFrustum() { var box = new BoundingBox(new Vector3(-2f), new Vector3(2f)); var small = CoverageFrustum(); var tight = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)); var f = CoverageFrustum(); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsFrustum(ref box, ref small)); Assert.AreEqual(ContainmentType.Intersects, Collision.BoxContainsFrustum(ref tight, ref f)); }
        [TestMethod] public void Collision_Overload_111_BoxContainsTriangle() { var box = CoverageBox(); var inside = new Triangle(Vector3.Zero, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f)); var crossing = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsTriangle(ref box, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.BoxContainsTriangle(ref box, ref crossing)); Assert.AreEqual(ContainmentType.Disjoint, Collision.BoxContainsTriangle(ref box, ref far)); }
        [TestMethod] public void Collision_Overload_112_BoxContainsMesh() { var box = CoverageBox(); var inside = new Triangle(Vector3.Zero, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f)); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.AreEqual(ContainmentType.Contains, Collision.BoxContainsMesh(ref box, [inside])); Assert.AreEqual(ContainmentType.Intersects, Collision.BoxContainsMesh(ref box, [CoverageTriangle()])); Assert.AreEqual(ContainmentType.Disjoint, Collision.BoxContainsMesh(ref box, [far])); }
        [TestMethod] public void Collision_Overload_113_SphereContainsPoint() { var sphere = new BoundingSphere(Vector3.Zero, 1f); var inside = Vector3.Zero; var boundary = Vector3.UnitX; var outside = new Vector3(2f, 0f, 0f); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsPoint(ref sphere, ref inside)); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsPoint(ref sphere, ref boundary)); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsPoint(ref sphere, ref outside)); }
        [TestMethod] public void Collision_Overload_114_SphereContainsTriangle() { var sphere = new BoundingSphere(Vector3.Zero, 3f); var a = new Vector3(0.1f, 0f, 0f); var b = new Vector3(0f, 0.1f, 0f); var c = new Vector3(0f, 0f, 0.1f); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsTriangle(ref sphere, ref a, ref b, ref c)); var far = new BoundingSphere(new Vector3(5f), 0.5f); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsTriangle(ref far, ref a, ref b, ref c)); }
        [TestMethod] public void Collision_Overload_115_SphereContainsTriangle() { var tri = CoverageTriangle(); var large = new BoundingSphere(Vector3.Zero, 4f); var touching = new BoundingSphere(Vector3.Zero, 0.5f); var far = new BoundingSphere(new Vector3(5f), 0.5f); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsTriangle(ref large, ref tri)); Assert.AreEqual(ContainmentType.Intersects, Collision.SphereContainsTriangle(ref touching, ref tri)); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsTriangle(ref far, ref tri)); }
        [TestMethod] public void Collision_Overload_116_SphereContainsBox() { var sphere = new BoundingSphere(Vector3.Zero, 2f); var inside = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)); var overlap = CoverageBox(); var far = new BoundingBox(new Vector3(4f), new Vector3(5f)); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsBox(ref sphere, ref inside)); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsBox(ref sphere, ref overlap)); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsBox(ref sphere, ref far)); }
        [TestMethod] public void Collision_Overload_117_SphereContainsSphere() { var sphere = new BoundingSphere(Vector3.Zero, 2f); var inside = new BoundingSphere(Vector3.Zero, 1f); var overlap = new BoundingSphere(new Vector3(2f, 0f, 0f), 1f); var far = new BoundingSphere(new Vector3(4f, 0f, 0f), 1f); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsSphere(ref sphere, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.SphereContainsSphere(ref sphere, ref overlap)); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsSphere(ref sphere, ref far)); }
        [TestMethod] public void Collision_Overload_118_SphereContainsFrustum() { var f = CoverageFrustum(); var large = new BoundingSphere(Vector3.Zero, 4f); var small = new BoundingSphere(Vector3.Zero, 0.5f); var far = new BoundingSphere(new Vector3(4f, 0f, 0f), 0.25f); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsFrustum(ref large, ref f)); Assert.AreEqual(ContainmentType.Intersects, Collision.SphereContainsFrustum(ref small, ref f)); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsFrustum(ref far, ref f)); }
        [TestMethod] public void Collision_Overload_119_SphereContainsMesh() { var tri = CoverageTriangle(); var large = new BoundingSphere(Vector3.Zero, 4f); var near = new BoundingSphere(Vector3.Zero, 0.5f); var far = new BoundingSphere(new Vector3(5f, 0f, 0f), 0.5f); Assert.AreEqual(ContainmentType.Contains, Collision.SphereContainsMesh(ref large, [tri])); Assert.AreEqual(ContainmentType.Intersects, Collision.SphereContainsMesh(ref near, [tri])); Assert.AreEqual(ContainmentType.Disjoint, Collision.SphereContainsMesh(ref far, [tri])); }
        [TestMethod] public void Collision_Overload_120_FrustumContainsPoint() { var f = CoverageFrustum(); var inside = Vector3.Zero; var boundary = Vector3.UnitX; var outside = new Vector3(3f, 0f, 0f); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsPoint(ref f, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsPoint(ref f, ref boundary)); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsPoint(ref f, ref outside)); }
        [TestMethod] public void Collision_Overload_121_FrustumContainsSphere() { var f = CoverageFrustum(); var inside = new BoundingSphere(Vector3.Zero, 0.25f); var crossing = new BoundingSphere(Vector3.Zero, 2f); var far = new BoundingSphere(new Vector3(3f, 0f, 0f), 0.25f); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsSphere(ref f, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsSphere(ref f, ref crossing)); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsSphere(ref f, ref far)); }
        [TestMethod] public void Collision_Overload_122_FrustumContainsBox() { var f = CoverageFrustum(); var inside = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)); var crossing = new BoundingBox(new Vector3(-2f), new Vector3(2f)); var far = new BoundingBox(new Vector3(4f), new Vector3(5f)); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsBox(ref f, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsBox(ref f, ref crossing)); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsBox(ref f, ref far)); }
        [TestMethod] public void Collision_Overload_123_FrustumContainsFrustum() { var a = CoverageFrustum(); var same = CoverageFrustum(); var far = new BoundingFrustum(Matrix4x4.Translation(new Vector3(5f, 0f, 0f))); Assert.AreEqual(ContainmentType.Contains, Collision.FrustumContainsFrustum(ref a, ref same)); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsFrustum(ref a, ref far)); }
        [TestMethod] public void Collision_Overload_124_FrustumContainsTriangle() { var f = CoverageFrustum(); var inside = new Triangle(Vector3.Zero, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f)); var crossing = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.AreEqual(ContainmentType.Contains, Collision.FrustumContainsTriangle(ref f, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsTriangle(ref f, ref crossing)); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsTriangle(ref f, ref far)); }
        [TestMethod] public void Collision_Overload_125_FrustumContainsMesh() { var f = CoverageFrustum(); var inside = new Triangle(Vector3.Zero, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f)); var crossing = CoverageTriangle(); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.AreEqual(ContainmentType.Contains, Collision.FrustumContainsMesh(ref f, [inside])); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsMesh(ref f, [crossing])); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsMesh(ref f, [far])); }
        [TestMethod] public void Collision_Overload_126_FrustumContainsPoints() { var f = CoverageFrustum(); Assert.AreEqual(ContainmentType.Contains, Collision.FrustumContainsPoints(ref f, [Vector3.Zero, new Vector3(0.5f, 0.5f, 0f)])); Assert.AreEqual(ContainmentType.Intersects, Collision.FrustumContainsPoints(ref f, [Vector3.Zero, new Vector3(3f, 0f, 0f)])); Assert.AreEqual(ContainmentType.Disjoint, Collision.FrustumContainsPoints(ref f, [new Vector3(3f, 0f, 0f), new Vector3(4f, 0f, 0f)])); }
        [TestMethod] public void Collision_Overload_127_CapsuleContainsPoint() { var capsule = new BoundingCapsule(Vector3.Zero, 0.5f, 2f); var inside = new Vector3(0f, -1f, 0f); var axial = Vector3.Zero; var outside = new Vector3(0f, 3f, 0f); Assert.AreEqual(ContainmentType.Contains, Collision.CapsuleContainsPoint(ref capsule, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.CapsuleContainsPoint(ref capsule, ref axial)); Assert.AreEqual(ContainmentType.Disjoint, Collision.CapsuleContainsPoint(ref capsule, ref outside)); }
        [TestMethod] public void Collision_Overload_128_CylinderContainsPoint() { var cylinder = new BoundingCylinder(Vector3.Zero, 0.5f, 2f); var inside = Vector3.Zero; var boundary = new Vector3(0.5f, 0f, 0f); var outside = new Vector3(2f, 0f, 0f); Assert.AreEqual(ContainmentType.Contains, Collision.CylinderContainsPoint(ref cylinder, ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.CylinderContainsPoint(ref cylinder, ref boundary)); Assert.AreEqual(ContainmentType.Disjoint, Collision.CylinderContainsPoint(ref cylinder, ref outside)); }
        [TestMethod] public void Collision_Overload_129_MeshContainsSphere() { var tri = CoverageTriangle(); var inside = new BoundingSphere(new Vector3(0.5f, 0.5f, 0f), 0f); var overlap = new BoundingSphere(new Vector3(1f, 1f, 0f), 1f); var far = new BoundingSphere(new Vector3(4f, 0f, 0f), 0.25f); Assert.AreEqual(ContainmentType.Intersects, Collision.MeshContainsSphere([tri], ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.MeshContainsSphere([tri], ref overlap)); Assert.AreEqual(ContainmentType.Disjoint, Collision.MeshContainsSphere([tri], ref far)); }
        [TestMethod] public void Collision_Overload_130_MeshContainsBox() { var tri = CoverageTriangle(); var inside = new BoundingBox(new Vector3(0.25f, 0.25f, 0f), new Vector3(0.75f, 0.75f, 0f)); var overlap = new BoundingBox(new Vector3(1f, 1f, 0f), new Vector3(3f, 3f, 0f)); var far = new BoundingBox(new Vector3(4f), new Vector3(5f)); Assert.AreEqual(ContainmentType.Contains, Collision.MeshContainsBox([tri], ref inside)); Assert.AreEqual(ContainmentType.Intersects, Collision.MeshContainsBox([tri], ref overlap)); Assert.AreEqual(ContainmentType.Disjoint, Collision.MeshContainsBox([tri], ref far)); }
        [TestMethod] public void Collision_Overload_131_MeshContainsFrustum() { var tri = CoverageTriangle(); var f = CoverageFrustum(); Assert.AreEqual(ContainmentType.Intersects, Collision.MeshContainsFrustum([tri], ref f)); var far = new BoundingFrustum(Matrix4x4.Translation(new Vector3(5f, 0f, 0f))); Assert.AreEqual(ContainmentType.Disjoint, Collision.MeshContainsFrustum([tri], ref far)); }
        [TestMethod] public void Collision_Overload_132_MeshContainsMesh() { var tri = CoverageTriangle(); var inside = new Triangle(new Vector3(0.25f, 0.25f, 0f), new Vector3(0.5f, 0.25f, 0f), new Vector3(0.25f, 0.5f, 0f)); var far = new Triangle(new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f), new Vector3(4f, 1f, 0f)); Assert.AreEqual(ContainmentType.Intersects, Collision.MeshContainsMesh([tri], [inside])); Assert.AreEqual(ContainmentType.Disjoint, Collision.MeshContainsMesh([tri], [far])); }


        private static void AssertNegativeRayOverloads()
        {
            var ray = new Ray(Vector3.Zero, Vector3.UnitX);
            var parallelRay = new Ray(new Vector3(0f, -1f, 1f), Vector3.UnitY);
            Assert.IsFalse(Collision.RayIntersectsRay(ref ray, ref parallelRay, out var rayHit)); AssertVector(Vector3.Zero, rayHit);
            var rp1 = Vector3.Zero; var rd1 = Vector3.UnitX; var rp2 = new Vector3(0f, -1f, 1f); var rd2 = Vector3.UnitY;
            Assert.IsFalse(Collision.RayIntersectsRay(ref rp1, ref rd1, ref rp2, ref rd2, out rayHit)); AssertVector(Vector3.Zero, rayHit);

            var segmentMiss = new Segment(new Vector3(2f, 2f, 0f), new Vector3(2f, 3f, 0f));
            Assert.IsFalse(Collision.RayIntersectsSegment(ref ray, ref segmentMiss, out rayHit)); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsSegment(ref ray, ref segmentMiss, out float segmentDistance, out rayHit)); Assert.AreEqual(float.MaxValue, segmentDistance); AssertVector(Vector3.Zero, rayHit);
            var rawSegmentStart = new Vector3(2f, 2f, 0f); var rawSegmentDirection = Vector3.UnitY; var rawSegmentLength = 1f;
            Assert.IsFalse(Collision.RayIntersectsSegment(ref rp1, ref rd1, ref rawSegmentStart, ref rawSegmentDirection, ref rawSegmentLength, out segmentDistance, out rayHit)); Assert.AreEqual(float.MaxValue, segmentDistance); AssertVector(Vector3.Zero, rayHit);
            var picking = new PickingRay(ray);
            Assert.IsFalse(Collision.RayIntersectsSegment(ref picking, ref segmentMiss, out rayHit)); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsSegment(ref picking, ref segmentMiss, out segmentDistance, out rayHit)); Assert.AreEqual(float.MaxValue, segmentDistance); AssertVector(Vector3.Zero, rayHit);

            var parallelPlane = new Plane(Vector3.UnitY, -2f);
            Assert.IsFalse(Collision.RayIntersectsPlane(ref rp1, ref rd1, ref parallelPlane, out float planeDistance)); Assert.AreEqual(0f, planeDistance, 1e-5f);
            Assert.IsFalse(Collision.RayIntersectsPlane(ref rp1, ref rd1, ref parallelPlane, out planeDistance, out rayHit)); Assert.AreEqual(float.MaxValue, planeDistance); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsPlane(ref ray, ref parallelPlane, out planeDistance)); Assert.AreEqual(0f, planeDistance, 1e-5f);
            Assert.IsFalse(Collision.RayIntersectsPlane(ref ray, ref parallelPlane, out planeDistance, out rayHit)); Assert.AreEqual(float.MaxValue, planeDistance); AssertVector(Vector3.Zero, rayHit);

            var tri = CoverageTriangle(); var triMissRay = new Ray(new Vector3(3f, 3f, 2f), -Vector3.UnitZ);
            var triPos = triMissRay.Position; var triDir = triMissRay.Direction; var a = tri.Point1; var b = tri.Point2; var c = tri.Point3;
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref triPos, ref triDir, ref a, ref b, ref c, out float triDistance)); Assert.AreEqual(float.MaxValue, triDistance);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref triPos, ref triDir, ref a, ref b, ref c, out triDistance, out rayHit)); Assert.AreEqual(float.MaxValue, triDistance); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref triMissRay, ref a, ref b, ref c, out triDistance)); Assert.AreEqual(float.MaxValue, triDistance);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref triMissRay, ref a, ref b, ref c, out triDistance, out rayHit)); Assert.AreEqual(float.MaxValue, triDistance); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref triMissRay, ref tri, out triDistance)); Assert.AreEqual(float.MaxValue, triDistance);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref triMissRay, ref tri, out triDistance, out rayHit)); Assert.AreEqual(float.MaxValue, triDistance); AssertVector(Vector3.Zero, rayHit);
            var pickTriRay = new PickingRay(triMissRay);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref pickTriRay, ref a, ref b, ref c, out triDistance)); Assert.AreEqual(float.MaxValue, triDistance);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref pickTriRay, ref a, ref b, ref c, out triDistance, out rayHit)); Assert.AreEqual(float.MaxValue, triDistance); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref pickTriRay, ref tri, out triDistance)); Assert.AreEqual(float.MaxValue, triDistance);
            Assert.IsFalse(Collision.RayIntersectsTriangle(ref pickTriRay, ref tri, out triDistance, out rayHit)); Assert.AreEqual(float.MaxValue, triDistance); AssertVector(Vector3.Zero, rayHit);

            var box = CoverageBox(); var boxMissRay = new Ray(new Vector3(-2f, 3f, 0f), Vector3.UnitX); var boxPos = boxMissRay.Position; var boxDir = boxMissRay.Direction;
            Assert.IsFalse(Collision.RayIntersectsBox(ref boxPos, ref boxDir, ref box, out float boxDistance)); Assert.AreEqual(float.MaxValue, boxDistance);
            Assert.IsFalse(Collision.RayIntersectsBox(ref boxPos, ref boxDir, ref box, out boxDistance, out rayHit)); Assert.AreEqual(float.MaxValue, boxDistance); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsBox(ref boxMissRay, ref box, out boxDistance)); Assert.AreEqual(float.MaxValue, boxDistance);
            Assert.IsFalse(Collision.RayIntersectsBox(ref boxMissRay, ref box, out boxDistance, out rayHit)); Assert.AreEqual(float.MaxValue, boxDistance); AssertVector(Vector3.Zero, rayHit);
            var boxPickRay = new PickingRay(boxMissRay);
            Assert.IsFalse(Collision.RayIntersectsBox(ref boxPickRay, ref box, out boxDistance)); Assert.AreEqual(float.MaxValue, boxDistance);
            Assert.IsFalse(Collision.RayIntersectsBox(ref boxPickRay, ref box, out boxDistance, out rayHit)); Assert.AreEqual(float.MaxValue, boxDistance); AssertVector(Vector3.Zero, rayHit);

            var sphere = new BoundingSphere(Vector3.Zero, 1f); var spherePos = boxMissRay.Position; var sphereDir = boxMissRay.Direction;
            Assert.IsFalse(Collision.RayIntersectsSphere(ref spherePos, ref sphereDir, ref sphere, out float sphereDistance)); Assert.AreEqual(0f, sphereDistance, 1e-5f);
            Assert.IsFalse(Collision.RayIntersectsSphere(ref spherePos, ref sphereDir, ref sphere, out sphereDistance, out rayHit)); Assert.AreEqual(float.MaxValue, sphereDistance); AssertVector(Vector3.Zero, rayHit);
            Assert.IsFalse(Collision.RayIntersectsSphere(ref boxMissRay, ref sphere, out sphereDistance)); Assert.AreEqual(0f, sphereDistance, 1e-5f);
            Assert.IsFalse(Collision.RayIntersectsSphere(ref boxMissRay, ref sphere, out sphereDistance, out rayHit)); Assert.AreEqual(float.MaxValue, sphereDistance); AssertVector(Vector3.Zero, rayHit);
            var spherePickRay = new PickingRay(boxMissRay);
            Assert.IsFalse(Collision.RayIntersectsSphere(ref spherePickRay, ref sphere, out sphereDistance)); Assert.AreEqual(float.MaxValue, sphereDistance);
            Assert.IsFalse(Collision.RayIntersectsSphere(ref spherePickRay, ref sphere, out sphereDistance, out rayHit)); Assert.AreEqual(float.MaxValue, sphereDistance); AssertVector(Vector3.Zero, rayHit);

            var frustum = CoverageFrustum(); var frustumPos = new Vector3(-2f, 3f, 0f); var frustumDir = Vector3.UnitX;
            Assert.IsFalse(Collision.RayIntersectsFrustum(ref frustumPos, ref frustumDir, ref frustum, out float? enter, out float? exit)); Assert.IsFalse(enter.HasValue); Assert.IsFalse(exit.HasValue);
            var frustumRay = new Ray(frustumPos, frustumDir);
            Assert.IsFalse(Collision.RayIntersectsFrustum(ref frustumRay, ref frustum, out enter, out exit)); Assert.IsFalse(enter.HasValue); Assert.IsFalse(exit.HasValue);
            var circleRay = new Ray(new Vector3(0f, 0f, -2f), Vector3.UnitZ); var circleCenter = new Vector3(3f, 0f, 0f); var circleNormal = Vector3.UnitZ;
            Assert.IsFalse(Collision.RayIntersectsCircle3D(ref circleRay, ref circleCenter, ref circleNormal, 1f, out float circleDistance, out rayHit)); Assert.AreEqual(float.MaxValue, circleDistance); AssertVector(Vector3.Zero, rayHit);

            var shortBoxSegment = new Segment(new Vector3(-2f, 0f, 0f), new Vector3(-1.5f, 0f, 0f));
            Assert.IsFalse(Collision.SegmentIntersectsBox(ref shortBoxSegment, ref box, out float shortDistance)); Assert.AreEqual(float.MaxValue, shortDistance);
            Assert.IsFalse(Collision.SegmentIntersectsBox(ref shortBoxSegment, ref box, out shortDistance, out rayHit)); Assert.AreEqual(float.MaxValue, shortDistance); AssertVector(Vector3.Zero, rayHit);
            var shortTriSegment = new Segment(new Vector3(0.5f, 0.5f, 2f), new Vector3(0.5f, 0.5f, 1f));
            Assert.IsFalse(Collision.SegmentIntersectsTriangle(ref shortTriSegment, ref tri, out float shortTriDistance)); Assert.AreEqual(float.MaxValue, shortTriDistance);
            Assert.IsFalse(Collision.SegmentIntersectsTriangle(ref shortTriSegment, ref tri, out shortTriDistance, out rayHit)); Assert.AreEqual(float.MaxValue, shortTriDistance); AssertVector(Vector3.Zero, rayHit);
        }

        private static void AssertNegativeShapeResultOverloads()
        {
            var tri = CoverageTriangle(); var farSphere = new BoundingSphere(new Vector3(4f, 4f, 0f), 0.25f);
            Assert.IsFalse(Collision.SphereIntersectsTriangle(ref farSphere, ref tri, out var pick)); AssertVector(Vector3.Zero, pick.Position); Assert.AreEqual(float.MaxValue, pick.Distance); Assert.AreEqual(default(Triangle), pick.Primitive);
            var farMeshSphere = new BoundingSphere(new Vector3(5f, 5f, 0f), 0.25f);
            Assert.IsFalse(Collision.SphereIntersectsMesh(ref farMeshSphere, [tri], out pick)); AssertVector(Vector3.Zero, pick.Position); Assert.AreEqual(float.MaxValue, pick.Distance); Assert.AreEqual(default(Triangle), pick.Primitive);
            Assert.IsFalse(Collision.SphereIntersectsMeshAll(ref farMeshSphere, [tri], out var picks)); Assert.AreEqual(0, picks.Length);

            var frustum = CoverageFrustum(); var p1 = new Vector3(-2f, 3f, 0f); var p2 = new Vector3(2f, 3f, 0f);
            Assert.IsFalse(Collision.FrustumIntersectsSegment(ref frustum, ref p1, ref p2, out float d)); Assert.AreEqual(float.MaxValue, d);
            Assert.IsFalse(Collision.FrustumIntersectsSegment(ref frustum, ref p1, ref p2, out d, out var point)); Assert.AreEqual(float.MaxValue, d); AssertVector(Vector3.Zero, point);
            Assert.IsFalse(Collision.FrustumIntersectsSegment(ref frustum, ref p1, ref p2, out Vector3? entering, out Vector3? exiting)); Assert.IsFalse(entering.HasValue); Assert.IsFalse(exiting.HasValue);
            var outsideSegment = new Segment(p1, p2);
            Assert.IsFalse(Collision.FrustumIntersectsSegment(ref frustum, ref outsideSegment));
            Assert.IsFalse(Collision.FrustumIntersectsSegment(ref frustum, ref outsideSegment, out d, out point)); Assert.AreEqual(float.MaxValue, d); AssertVector(Vector3.Zero, point);
            Assert.IsFalse(Collision.FrustumIntersectsSegment(ref frustum, ref outsideSegment, out entering, out exiting)); Assert.IsFalse(entering.HasValue); Assert.IsFalse(exiting.HasValue);
        }

        private static BoundingBox CoverageBox() => new(new Vector3(-1f), Vector3.One);
        private static Triangle CoverageTriangle() => new(Vector3.Zero, new Vector3(2f, 0f, 0f), new Vector3(0f, 2f, 0f));
        private static BoundingFrustum CoverageFrustum() => new(Matrix4x4.Identity);
        private static void AssertSegment(Segment expected, Segment actual)
        {
            bool forward = MathF.Abs(expected.Point1.X - actual.Point1.X) <= 1e-5f && MathF.Abs(expected.Point1.Y - actual.Point1.Y) <= 1e-5f && MathF.Abs(expected.Point1.Z - actual.Point1.Z) <= 1e-5f && MathF.Abs(expected.Point2.X - actual.Point2.X) <= 1e-5f && MathF.Abs(expected.Point2.Y - actual.Point2.Y) <= 1e-5f && MathF.Abs(expected.Point2.Z - actual.Point2.Z) <= 1e-5f;
            bool reverse = MathF.Abs(expected.Point1.X - actual.Point2.X) <= 1e-5f && MathF.Abs(expected.Point1.Y - actual.Point2.Y) <= 1e-5f && MathF.Abs(expected.Point1.Z - actual.Point2.Z) <= 1e-5f && MathF.Abs(expected.Point2.X - actual.Point1.X) <= 1e-5f && MathF.Abs(expected.Point2.Y - actual.Point1.Y) <= 1e-5f && MathF.Abs(expected.Point2.Z - actual.Point1.Z) <= 1e-5f;
            Assert.IsTrue(forward || reverse);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.X, actual.X, 1e-5f);
            Assert.AreEqual(expected.Y, actual.Y, 1e-5f);
            Assert.AreEqual(expected.Z, actual.Z, 1e-5f);
        }
    }
}
