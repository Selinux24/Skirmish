using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System.Linq;

namespace Engine.Common.Tests
{
    [TestClass()]
    public class IntersectionTests
    {
        BoundingSphere sph1 = new BoundingSphere(Vector3.Zero, 1f);
        BoundingSphere sph2 = new BoundingSphere(Vector3.Zero, 30f);
        BoundingSphere sph3 = new BoundingSphere(Vector3.Zero, 0.5f);
        BoundingSphere sph4 = new BoundingSphere(Vector3.One, 1f);
        BoundingSphere sph5 = new BoundingSphere(Vector3.One * 3, 1f);

        BoundingBox box1 = new BoundingBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
        BoundingBox box2 = new BoundingBox(Vector3.One * -1f, Vector3.One * 1f);
        BoundingBox box3 = new BoundingBox(Vector3.One * -0.25f, Vector3.One * 0.25f);
        BoundingBox box4 = new BoundingBox((Vector3.One * -0.5f) + Vector3.One, (Vector3.One * 0.5f) + Vector3.One);
        BoundingBox box5 = new BoundingBox((Vector3.One * -0.5f) + (Vector3.One * 3f), (Vector3.One * 0.5f) + (Vector3.One * 3f));
        BoundingBox box6 = new BoundingBox((Vector3.One * -0.5f) + (Vector3.One * 0.75f), (Vector3.One * 0.75f) + (Vector3.One * 3f));

        static Vector3 frustOrigin1 = Vector3.BackwardLH * +0.5f;
        static Vector3 frustOrigin2 = Vector3.BackwardLH * +1.0f;
        static Vector3 frustOrigin3 = Vector3.BackwardLH * +.25f;
        static Vector3 frustOrigin4 = (Vector3.BackwardLH * +0.5f) + Vector3.One * 0.5f;
        static Vector3 frustOrigin5 = (Vector3.BackwardLH * +0.5f) + (Vector3.One * 3f);

        BoundingFrustum frustum1 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin1, frustOrigin1 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 1.0f));
        BoundingFrustum frustum2 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin2, frustOrigin2 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 2.0f));
        BoundingFrustum frustum3 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin3, frustOrigin3 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 0.5f));
        BoundingFrustum frustum4 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin4, frustOrigin4 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 1.0f));
        BoundingFrustum frustum5 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin5, frustOrigin5 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 1.0f));

        Triangle tri1 = new Triangle(new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, -0.5f));
        Triangle tri2 = new Triangle(new Vector3(-1.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(-1.0f, 0.0f, -1.0f));
        Triangle tri3 = new Triangle(new Vector3(-.25f, 0.0f, .25f), new Vector3(.25f, 0.0f, .25f), new Vector3(-.25f, 0.0f, -.25f));
        Triangle tri4 = new Triangle(new Vector3(+0.5f, 0.0f, 1.5f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(+0.5f, 0.0f, +0.5f));
        Triangle tri5 = new Triangle(new Vector3(+2.5f, 0.0f, 3.5f), new Vector3(3.5f, 0.0f, 3.5f), new Vector3(+2.5f, 0.0f, +2.5f));
        Triangle tri6 = new Triangle(new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(-0.5f, -0.5f, 0.0f));
        Triangle tri7 = new Triangle(new Vector3(-0.5f, 10.0f, 0.5f), new Vector3(0.5f, 10.0f, 0.5f), new Vector3(-0.5f, 10.0f, -0.5f));
        Triangle tri8 = new Triangle(new Vector3(-0.5f, -10.0f, 0.5f), new Vector3(0.5f, -10.0f, 0.5f), new Vector3(-0.5f, -10.0f, -0.5f));

        Ray rayIn = new Ray(Vector3.One * -10f, Vector3.One * 20f);
        Ray rayOut = new Ray(Vector3.One * -10f, Vector3.ForwardLH * 10f);

        Vector3 pointIn = Vector3.Zero;
        Vector3 pointOver = Vector3.Up;
        Vector3 pointBelow = Vector3.Down;

        Vector3 pointRayLeft = (Vector3.One * -10f) - Vector3.Up;
        Vector3 pointRayCenter = new Vector3(-1, 1, -1);
        Vector3 pointRayRigh = (Vector3.One * +10f) + Vector3.Up;

        Vector3 triCenterPlane = new Vector3(0.2f, 0.0f, 0.2f);
        Vector3 triCenterOver = new Vector3(0.2f, 1.0f, 0.2f);
        Vector3 triCenterBelow = new Vector3(0.2f, -1.0f, 0.2f);

        Vector3 triP1Perfect = new Vector3(-0.5f, 0.0f, 0.5f);
        Vector3 triP1 = new Vector3(-0.8f, 1.0f, 0.8f);
        Vector3 triP2Perfect = new Vector3(0.5f, 0.0f, 0.5f);
        Vector3 triP2 = new Vector3(0.8f, 1.0f, 0.8f);
        Vector3 triP3Perfect = new Vector3(-0.5f, 0.0f, -0.5f);
        Vector3 triP3 = new Vector3(-0.8f, 1.0f, -0.8f);

        Vector3 triS1Perfect = new Vector3(0.0f, 0.0f, 0.5f);
        Vector3 triS1 = new Vector3(0.0f, 1.0f, 0.8f);
        Vector3 triS2Perfect = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 triS2 = new Vector3(0.2f, 1.0f, -0.2f);
        Vector3 triS3Perfect = new Vector3(-0.5f, 0.0f, 0.0f);
        Vector3 triS3 = new Vector3(-0.8f, 1.0f, 0.0f);

        [TestMethod()]
        public void SphereIntersectsSphereContainsTest()
        {
            var res = Intersection.SphereIntersectsSphere(sph1, sph2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsSphereContainedTest()
        {
            var res = Intersection.SphereIntersectsSphere(sph1, sph3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsSphereIntersectedTest()
        {
            var res = Intersection.SphereIntersectsSphere(sph1, sph4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsSphereNotIntersectedTest()
        {
            var res = Intersection.SphereIntersectsSphere(sph1, sph5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void BoxIntersectsBoxContainsTest()
        {
            var res = Intersection.BoxIntersectsBox(box1, box2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsBoxContainedTest()
        {
            var res = Intersection.BoxIntersectsBox(box1, box3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsBoxIntersectedTest()
        {
            var res = Intersection.BoxIntersectsBox(box1, box4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsBoxNotIntersectedTest()
        {
            var res = Intersection.BoxIntersectsBox(box1, box5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void FrustumIntersectsFrustumContainsTest()
        {
            var res = Intersection.FrustumIntersectsFrustum(frustum1, frustum2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void FrustumIntersectsFrustumContainedTest()
        {
            var res = Intersection.FrustumIntersectsFrustum(frustum1, frustum3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void FrustumIntersectsFrustumIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsFrustum(frustum1, frustum4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void FrustumIntersectsFrustumNotIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsFrustum(frustum1, frustum5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void MeshIntersectsMeshContainsTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            // A mesh is not a volume, box 1 is into the box 2, but no mesh 2 triangle intersects mesh 1 triangle
            Assert.IsFalse(res);
            Assert.IsFalse(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshContainedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            // A mesh is not a volume, box 1 contains box 3, but no mesh 3 triangle intersects mesh 1 triangle
            Assert.IsFalse(res);
            Assert.IsFalse(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshIntersectedPerfectFaceTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            // It's a perfect face intersection between box 1 and box 4 meshes
            Assert.IsTrue(res);
            Assert.IsTrue(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshNotIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            Assert.IsFalse(res);
            Assert.IsFalse(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box6);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            Assert.IsTrue(res);
            Assert.IsTrue(tris.Any());
            Assert.IsTrue(segments.Any());
        }

        [TestMethod()]
        public void SphereIntersectsBoxContainsTest()
        {
            var res = Intersection.SphereIntersectsBox(sph1, box2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsBoxContainedTest()
        {
            var res = Intersection.SphereIntersectsBox(sph1, box3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsBoxIntersectedTest()
        {
            var res = Intersection.SphereIntersectsBox(sph1, box4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsBoxNotIntersectedTest()
        {
            var res = Intersection.SphereIntersectsBox(sph1, box5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void BoxIntersectsSphereContainsTest()
        {
            var res = Intersection.BoxIntersectsSphere(box1, sph2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsSphereContainedTest()
        {
            var res = Intersection.BoxIntersectsSphere(box1, sph3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsSphereIntersectedTest()
        {
            var res = Intersection.BoxIntersectsSphere(box1, sph4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsSphereNotIntersectedTest()
        {
            var res = Intersection.BoxIntersectsSphere(box1, sph5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void SphereIntersectsFrustumContainsTest()
        {
            var res = Intersection.SphereIntersectsFrustum(sph1, frustum2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsFrustumContainedTest()
        {
            var res = Intersection.SphereIntersectsFrustum(sph1, frustum3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsFrustumIntersectedTest()
        {
            var res = Intersection.SphereIntersectsFrustum(sph1, frustum4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsFrustumNotIntersectedTest()
        {
            var res = Intersection.SphereIntersectsFrustum(sph1, frustum5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void BoxIntersectsFrustumContainsTest()
        {
            var res = Intersection.BoxIntersectsFrustum(box1, frustum2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsFrustumContainedTest()
        {
            var res = Intersection.BoxIntersectsFrustum(box1, frustum3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsFrustumIntersectedTest()
        {
            var res = Intersection.BoxIntersectsFrustum(box1, frustum4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsFrustumNotIntersectedTest()
        {
            var res = Intersection.BoxIntersectsFrustum(box1, frustum5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void SphereIntersectsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, p.Distance);
            Assert.AreNotEqual(Vector3.Zero, p.Position);
        }
        [TestMethod()]
        public void SphereIntersectsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, p.Distance);
            Assert.AreNotEqual(Vector3.Zero, p.Position);
        }
        [TestMethod()]
        public void SphereIntersectsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, p.Distance);
            Assert.AreNotEqual(Vector3.Zero, p.Position);
        }
        [TestMethod()]
        public void SphereIntersectsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, p.Distance);
            Assert.AreEqual(Vector3.Zero, p.Position);
        }

        [TestMethod()]
        public void SphereIntersectsMeshAllContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.IsTrue(p.Any());
            Assert.IsFalse(p.Any(r => r.Distance == float.MaxValue));
            Assert.IsFalse(p.Any(r => r.Position == Vector3.Zero));
        }
        [TestMethod()]
        public void SphereIntersectsMeshAllContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.IsTrue(p.Any());
            Assert.IsFalse(p.Any(r => r.Distance == float.MaxValue));
            Assert.IsFalse(p.Any(r => r.Position == Vector3.Zero));
        }
        [TestMethod()]
        public void SphereIntersectsMeshAllIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.IsTrue(p.Any());
            Assert.IsFalse(p.Any(r => r.Distance == float.MaxValue));
            Assert.IsFalse(p.Any(r => r.Position == Vector3.Zero));
        }
        [TestMethod()]
        public void SphereIntersectsMeshAllNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsFalse(res);
            Assert.IsFalse(p.Any());
        }

        [TestMethod()]
        public void BoxIntersectsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            // A mesh is not a volume, box 1 is into the box 2 mesh, but no mesh triangle intersects box 1
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void BoxIntersectsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            // All mesh 3 triangles were into the box 1 volume
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTriangleContainsTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleContainedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleIntersectedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleIntersectedTest2()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri6);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleNotIntersectedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTriangleResultContainsTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri2, out bool coplanar, out var segment);

            Assert.IsTrue(res);
            Assert.IsTrue(coplanar);
            Assert.IsTrue(segment.Point1 == segment.Point2);
            Assert.AreEqual(Vector3.Zero, segment.Point1);
            Assert.AreEqual(Vector3.Zero, segment.Point2);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultContainedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri3, out bool coplanar, out var segment);

            Assert.IsTrue(res);
            Assert.IsTrue(coplanar);
            Assert.IsTrue(segment.Point1 == segment.Point2);
            Assert.AreEqual(Vector3.Zero, segment.Point1);
            Assert.AreEqual(Vector3.Zero, segment.Point2);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultIntersectedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri4, out bool coplanar, out var segment);

            Assert.IsTrue(res);
            Assert.IsTrue(coplanar);
            Assert.IsTrue(segment.Point1 == segment.Point2);
            Assert.AreEqual(Vector3.Zero, segment.Point1);
            Assert.AreEqual(Vector3.Zero, segment.Point2);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultIntersectedTest2()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri6, out bool coplanar, out var segment);

            Assert.IsTrue(res);
            Assert.IsFalse(coplanar);
            Assert.IsFalse(segment.Point1 == segment.Point2);
            Assert.AreEqual(new Vector3(-0.5f, 0, 0), segment.Point1);
            Assert.AreEqual(new Vector3(0, 0, 0), segment.Point2);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultNotIntersectedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri5, out bool coplanar, out var segment);

            Assert.IsFalse(res);
            Assert.IsTrue(coplanar);
            Assert.IsTrue(segment.Point1 == segment.Point2);
            Assert.AreEqual(Vector3.Zero, segment.Point1);
            Assert.AreEqual(Vector3.Zero, segment.Point2);
        }

        [TestMethod()]
        public void SphereIntersectsTriangleContainsTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsTriangleContainedTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsTriangleIntersectedTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void SphereIntersectsTriangleNotIntersectedTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void BoxIntersectsTriangleContainsTest()
        {
            var res = Intersection.BoxIntersectsTriangle(box1, tri2);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsTriangleContainedTest()
        {
            var res = Intersection.BoxIntersectsTriangle(box1, tri3);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsTriangleIntersectedTest()
        {
            var res = Intersection.BoxIntersectsTriangle(box1, tri4);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsTriangleNotIntersectedTest()
        {
            var res = Intersection.BoxIntersectsTriangle(box1, tri5);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles);

            // A mesh is not a volume. Tri 1 is into the box 2, but not intersections were possible
            Assert.IsFalse(res);
            Assert.IsFalse(triangles.Any());
        }
        [TestMethod()]
        public void TriangleIntersectsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles);

            Assert.IsTrue(res);
            Assert.IsTrue(triangles.Any());
        }
        [TestMethod()]
        public void TriangleIntersectsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles);

            Assert.IsFalse(res);
            Assert.IsFalse(triangles.Any());
        }

        [TestMethod()]
        public void RayIntersectsBoxIntersectedTest()
        {
            var res = Intersection.RayIntersectsBox(rayIn, box1);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void RayIntersectsBoxNotIntersectedTest()
        {
            var res = Intersection.RayIntersectsBox(rayOut, box1);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void RayIntersectsBoxResultIntersectedTest()
        {
            var res = Intersection.RayIntersectsBox(rayIn, box1, out float distance);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, distance);
        }
        [TestMethod()]
        public void RayIntersectsBoxResultNotIntersectedTest()
        {
            var res = Intersection.RayIntersectsBox(rayOut, box1, out float distance);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, distance);
        }

        [TestMethod()]
        public void RayIntersectsTriangleIntersectedTest()
        {
            var res = Intersection.RayIntersectsTriangle(rayIn, tri1, out Vector3 position, out float distance);

            Assert.IsTrue(res);
            Assert.AreEqual(Vector3.Distance(rayIn.Position, Vector3.Zero), distance);
            Assert.AreEqual(Vector3.Zero, position);
        }
        [TestMethod()]
        public void RayIntersectsTriangleNotIntersectedTest()
        {
            var res = Intersection.RayIntersectsTriangle(rayOut, tri1, out Vector3 position, out float distance);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, distance);
            Assert.AreEqual(Vector3.Zero, position);
        }

        [TestMethod()]
        public void PointInTriangleContainedTest()
        {
            var res = Intersection.PointInTriangle(pointIn, tri1);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PointInTriangleOverTest()
        {
            var res = Intersection.PointInTriangle(pointOver, tri1);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void PointInTriangleBelowTest()
        {
            var res = Intersection.PointInTriangle(pointBelow, tri1);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void PointInMeshContainedTest()
        {
            var res = Intersection.PointInMesh(pointIn, new[] { tri1 });

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PointInMeshOverTest()
        {
            var res = Intersection.PointInMesh(pointOver, new[] { tri1 });

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void PointInMeshBelowTest()
        {
            var res = Intersection.PointInMesh(pointBelow, new[] { tri1 });

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void ClosestPointInRayLeftTest()
        {
            var p = Intersection.ClosestPointInRay(rayIn, pointRayLeft);

            Assert.AreEqual(rayIn.Position, p);
        }
        [TestMethod()]
        public void ClosestPointInRayCenterTest()
        {
            var p = Intersection.ClosestPointInRay(rayIn, pointRayCenter);

            float d = Vector3.Dot(pointRayCenter - rayIn.Position, Vector3.Normalize(rayIn.Direction));
            Vector3 tp = rayIn.Position + (Vector3.Normalize(rayIn.Direction) * d);
            Assert.AreEqual(tp, p);
        }
        [TestMethod()]
        public void ClosestPointInRayRightTest()
        {
            var p = Intersection.ClosestPointInRay(rayIn, pointRayRigh);

            Vector3 tp = rayIn.Position + rayIn.Direction;
            Assert.AreEqual(tp, p);
        }

        [TestMethod()]
        public void ClosestPointInRayResultLeftTest()
        {
            var p = Intersection.ClosestPointInRay(rayIn, pointRayLeft, out float d);

            Vector3 tp = rayIn.Position;
            Assert.AreEqual(tp, p);
            Assert.AreEqual(Vector3.Distance(pointRayLeft, p), d);
        }
        [TestMethod()]
        public void ClosestPointInRayResultCenterTest()
        {
            var p = Intersection.ClosestPointInRay(rayIn, pointRayCenter, out float d);

            float td = Vector3.Dot(pointRayCenter - rayIn.Position, Vector3.Normalize(rayIn.Direction));
            Vector3 tp = rayIn.Position + (Vector3.Normalize(rayIn.Direction) * td);
            Assert.AreEqual(tp, p);
            Assert.AreEqual(Vector3.Distance(pointRayCenter, p), d);
        }
        [TestMethod()]
        public void ClosestPointInRayResultRightTest()
        {
            var p = Intersection.ClosestPointInRay(rayIn, pointRayRigh, out float d);

            Vector3 tp = rayIn.Position + rayIn.Direction;
            Assert.AreEqual(tp, p);
            Assert.AreEqual(Vector3.Distance(pointRayRigh, p), d);
        }

        [TestMethod()]
        public void ClosestPointInTriangleCenterTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triCenterPlane, tri1);

            Assert.AreEqual(triCenterPlane, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleCenterOverTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triCenterOver, tri1);

            Assert.AreEqual(triCenterPlane, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleCenterBelowTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triCenterBelow, tri1);

            Assert.AreEqual(triCenterPlane, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleP1PerfectTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triP1Perfect, tri1);

            Assert.AreEqual(triP1Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleP1Test()
        {
            var closest = Intersection.ClosestPointInTriangle(triP1, tri1);

            Assert.AreEqual(triP1Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleP2PerfectTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triP2Perfect, tri1);

            Assert.AreEqual(triP2Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleP2Test()
        {
            var closest = Intersection.ClosestPointInTriangle(triP2, tri1);

            Assert.AreEqual(triP2Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleP3PerfectTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triP3Perfect, tri1);

            Assert.AreEqual(triP3Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleP3Test()
        {
            var closest = Intersection.ClosestPointInTriangle(triP3, tri1);

            Assert.AreEqual(triP3Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleS1PerfectTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triS1Perfect, tri1);

            Assert.AreEqual(triS1Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleS1Test()
        {
            var closest = Intersection.ClosestPointInTriangle(triS1, tri1);

            Assert.AreEqual(triS1Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleS2PerfectTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triS2Perfect, tri1);

            Assert.AreEqual(triS2Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleS2Test()
        {
            var closest = Intersection.ClosestPointInTriangle(triS2, tri1);

            Assert.AreEqual(triS2Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleS3PerfectTest()
        {
            var closest = Intersection.ClosestPointInTriangle(triS3Perfect, tri1);

            Assert.AreEqual(triS3Perfect, closest);
        }
        [TestMethod()]
        public void ClosestPointInTriangleS3Test()
        {
            var closest = Intersection.ClosestPointInTriangle(triS3, tri1);

            Assert.AreEqual(triS3Perfect, closest);
        }

        [TestMethod()]
        public void ClosestPointInMeshCenterTest()
        {
            Intersection.ClosestPointInMesh(triCenterPlane, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.IsTrue(tri1 == closest || tri3 == closest);
            Assert.AreEqual(triCenterPlane, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshCenterOverTest()
        {
            Intersection.ClosestPointInMesh(triCenterOver, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.IsTrue(tri1 == closest || tri3 == closest);
            Assert.AreEqual(triCenterPlane, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshCenterBelowTest()
        {
            Intersection.ClosestPointInMesh(triCenterBelow, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.IsTrue(tri1 == closest || tri3 == closest);
            Assert.AreEqual(triCenterPlane, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP1PerfectTest()
        {
            Intersection.ClosestPointInMesh(triP1Perfect, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP1Test()
        {
            Intersection.ClosestPointInMesh(triP1, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP2PerfectTest()
        {
            Intersection.ClosestPointInMesh(triP2Perfect, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP2Test()
        {
            Intersection.ClosestPointInMesh(triP2, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP3PerfectTest()
        {
            Intersection.ClosestPointInMesh(triP3Perfect, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP3Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP3Test()
        {
            Intersection.ClosestPointInMesh(triP3, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP3Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS1PerfectTest()
        {
            Intersection.ClosestPointInMesh(triS1Perfect, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS1Test()
        {
            Intersection.ClosestPointInMesh(triS1, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS2PerfectTest()
        {
            Intersection.ClosestPointInMesh(triS2Perfect, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS2Test()
        {
            Intersection.ClosestPointInMesh(triS2, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS3PerfectTest()
        {
            Intersection.ClosestPointInMesh(triS3Perfect, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS3Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS3Test()
        {
            Intersection.ClosestPointInMesh(triS3, new[] { tri1, tri3, tri7, tri8 }, out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS3Perfect, closestPoint);
        }

        [TestMethod()]
        public void BoxContainsBoxContainsTest()
        {
            var res = Intersection.BoxContainsBox(box1, box2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsBoxContainedTest()
        {
            var res = Intersection.BoxContainsBox(box1, box3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void BoxContainsBoxIntersectedTest()
        {
            var res = Intersection.BoxContainsBox(box1, box4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsBoxNotIntersectedTest()
        {
            var res = Intersection.BoxContainsBox(box1, box5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void BoxContainsSphereContainsTest()
        {
            var res = Intersection.BoxContainsSphere(box1, sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsSphereContainedTest()
        {
            var res = Intersection.BoxContainsSphere(box1, sph3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void BoxContainsSphereIntersectedTest()
        {
            var res = Intersection.BoxContainsSphere(box1, sph4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsSphereNotIntersectedTest()
        {
            var res = Intersection.BoxContainsSphere(box1, sph5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void BoxContainsFrustumContainsTest()
        {
            var res = Intersection.BoxContainsFrustum(box1, frustum2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsFrustumContainedTest()
        {
            var res = Intersection.BoxContainsFrustum(box2, frustum3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void BoxContainsFrustumIntersectedTest()
        {
            var res = Intersection.BoxContainsFrustum(box1, frustum4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsFrustumNotIntersectedTest()
        {
            var res = Intersection.BoxContainsFrustum(box1, frustum5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void BoxContainsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            // A mesh is not a volume, box 1 is into the box 2 mesh, but no mesh triangle intersects box 1
            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
        [TestMethod()]
        public void BoxContainsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            // All mesh 3 triangles were into the box 1 volume
            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void BoxContainsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void BoxContainsTriangleContainsTest()
        {
            var res = Intersection.BoxContainsTriangle(box1, tri2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsTriangleContainedTest()
        {
            var res = Intersection.BoxContainsTriangle(box1, tri3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void BoxContainsTriangleIntersectedTest()
        {
            var res = Intersection.BoxContainsTriangle(box1, tri4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsTriangleNotIntersectedTest()
        {
            var res = Intersection.BoxContainsTriangle(box1, tri5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void SphereContainsBoxContainsTest()
        {
            var res = Intersection.SphereContainsBox(sph1, box2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsBoxContainedTest()
        {
            var res = Intersection.SphereContainsBox(sph1, box3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void SphereContainsBoxIntersectedTest()
        {
            var res = Intersection.SphereContainsBox(sph1, box4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsBoxNotIntersectedTest()
        {
            var res = Intersection.SphereContainsBox(sph1, box5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void SphereContainsSphereContainsTest()
        {
            var res = Intersection.SphereContainsSphere(sph1, sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsSphereContainedTest()
        {
            var res = Intersection.SphereContainsSphere(sph1, sph3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void SphereContainsSphereIntersectedTest()
        {
            var res = Intersection.SphereContainsSphere(sph1, sph4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsSphereNotIntersectedTest()
        {
            var res = Intersection.SphereContainsSphere(sph1, sph5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void SphereContainsFrustumContainsTest()
        {
            var res = Intersection.SphereContainsFrustum(sph1, frustum2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsFrustumContainedTest()
        {
            var res = Intersection.SphereContainsFrustum(sph2, frustum3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void SphereContainsFrustumIntersectedTest()
        {
            var res = Intersection.SphereContainsFrustum(sph1, frustum4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsFrustumNotIntersectedTest()
        {
            var res = Intersection.SphereContainsFrustum(sph1, frustum5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void SphereContainsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void SphereContainsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void SphereContainsTriangleContainsTest()
        {
            var res = Intersection.SphereContainsTriangle(sph1, tri2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsTriangleContainedTest()
        {
            var res = Intersection.SphereContainsTriangle(sph1, tri3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void SphereContainsTriangleIntersectedTest()
        {
            var res = Intersection.SphereContainsTriangle(sph1, tri4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsTriangleNotIntersectedTest()
        {
            var res = Intersection.SphereContainsTriangle(sph1, tri5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void FrustumContainsTriangleContainsTest()
        {
            var res = Intersection.FrustumContainsTriangle(frustum1, tri2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsTriangleContainedTest()
        {
            var res = Intersection.FrustumContainsTriangle(frustum1, tri3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void FrustumContainsTriangleIntersectedTest()
        {
            var res = Intersection.FrustumContainsTriangle(frustum2, tri4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsTriangleNotIntersectedTest()
        {
            var res = Intersection.FrustumContainsTriangle(frustum1, tri5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void FrustumContainsSphereContainsTest()
        {
            var res = Intersection.FrustumContainsSphere(frustum1, sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsSphereContainedTest()
        {
            var res = Intersection.FrustumContainsSphere(frustum2, sph3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void FrustumContainsSphereIntersectedTest()
        {
            var res = Intersection.FrustumContainsSphere(frustum2, sph4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsSphereNotIntersectedTest()
        {
            var res = Intersection.FrustumContainsSphere(frustum1, sph5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void FrustumContainsBoxContainsTest()
        {
            var res = Intersection.FrustumContainsBox(frustum1, box2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsBoxContainedTest()
        {
            var res = Intersection.FrustumContainsBox(frustum2, box3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void FrustumContainsBoxIntersectedTest()
        {
            var res = Intersection.FrustumContainsBox(frustum2, box4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsBoxNotIntersectedTest()
        {
            var res = Intersection.FrustumContainsBox(frustum1, box5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void FrustumContainsFrustumContainsTest()
        {
            var res = Intersection.FrustumContainsFrustum(frustum1, frustum2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsFrustumContainedTest()
        {
            var res = Intersection.FrustumContainsFrustum(frustum1, frustum3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void FrustumContainsFrustumIntersectedTest()
        {
            var res = Intersection.FrustumContainsFrustum(frustum1, frustum4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsFrustumNotIntersectedTest()
        {
            var res = Intersection.FrustumContainsFrustum(frustum1, frustum5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void FrustumContainsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.FrustumContainsMesh(frustum1, mesh);

            // A mesh is not a volume, frustum 1 is into the box 2 mesh, but no mesh triangle intersects frustum 1
            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
        [TestMethod()]
        public void FrustumContainsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.FrustumContainsMesh(frustum1, mesh);

            // All mesh 3 triangles were into the frustum 1 volume
            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void FrustumContainsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.FrustumContainsMesh(frustum2, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.FrustumContainsMesh(frustum1, mesh);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void MeshContainsSphereContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsSphere(mesh, sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsSphereContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsSphere(mesh, sph3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsSphereIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsSphere(mesh, sph4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsSphereNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsSphere(mesh, sph5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void MeshContainsMeshContainsTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            // A mesh is not a volume, box 1 is into the box 2, but no mesh 2 triangle intersects mesh 1 triangle
            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsMeshContainedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box3);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            // A mesh is not a volume, box 1 contains box 3, but no mesh 3 triangle intersects mesh 1 triangle
            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsMeshIntersectedPerfectFaceTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box4);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            // It's a perfect face intersection between box 1 and box 4 meshes
            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsMeshNotIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box5);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
        [TestMethod()]
        public void MeshContainsMeshIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(Topology.TriangleList, box1);
            var mesh2 = Triangle.ComputeTriangleList(Topology.TriangleList, box6);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }

        [TestMethod()]
        public void MeshContainsBoxContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsBox(mesh, box2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsBoxContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsBox(mesh, box3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsBoxIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsBox(mesh, box4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsBoxNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsBox(mesh, box5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void MeshContainsFrustumContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsFrustum(mesh, frustum2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsFrustumContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box2);

            var res = Intersection.MeshContainsFrustum(mesh, frustum3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsFrustumIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsFrustum(mesh, frustum4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsFrustumNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(Topology.TriangleList, box1);

            var res = Intersection.MeshContainsFrustum(mesh, frustum5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
    }
}