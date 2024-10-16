﻿using Engine;
using Engine.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests.Common
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class IntersectionTests
    {
        static TestContext _testContext;

        static BoundingSphere sph1;
        static BoundingSphere sph2;
        static BoundingSphere sph3;
        static BoundingSphere sph4;
        static BoundingSphere sph5;

        static BoundingBox box1;
        static BoundingBox box2;
        static BoundingBox box3;
        static BoundingBox box4;
        static BoundingBox box5;
        static BoundingBox box6;

        static BoundingFrustum frustum1;
        static BoundingFrustum frustum2;
        static BoundingFrustum frustum3;
        static BoundingFrustum frustum4;
        static BoundingFrustum frustum5;

        static Triangle tri1;
        static Triangle tri2;
        static Triangle tri3;
        static Triangle tri4;
        static Triangle tri5;
        static Triangle tri6;
        static Triangle tri7;
        static Triangle tri8;

        static float triToTriDeltaInto;
        static float triToTriDeltaExact;
        static float triToTriDeltaOuter;
        static Triangle triOrigin;

        static Ray rayIn;
        static Ray rayOut;

        static Vector3 pointIn;
        static Vector3 pointOver;
        static Vector3 pointBelow;

        static Vector3 pointRayLeft;
        static Vector3 pointRayCenter;
        static Vector3 pointRayRigh;

        static Vector3 triCenterPlane;
        static Vector3 triCenterOver;
        static Vector3 triCenterBelow;

        static Vector3 triP1Perfect;
        static Vector3 triP1;
        static Vector3 triP2Perfect;
        static Vector3 triP2;
        static Vector3 triP3Perfect;
        static Vector3 triP3;

        static Vector3 triS1Perfect;
        static Vector3 triS1;
        static Vector3 triS2Perfect;
        static Vector3 triS2;
        static Vector3 triS3Perfect;
        static Vector3 triS3;

        static Vector3 segment1A;
        static Vector3 segment1B;
        static Vector3 segment2A;
        static Vector3 segment2B;
        static Vector3 segment3A;
        static Vector3 segment3B;
        static Vector3 segment4A;
        static Vector3 segment4B;
        static Vector3 segment5A;
        static Vector3 segment5B;
        static Vector3 segment6A;
        static Vector3 segment6B;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            sph1 = new BoundingSphere(Vector3.Zero, 1f);
            sph2 = new BoundingSphere(Vector3.Zero, 30f);
            sph3 = new BoundingSphere(Vector3.Zero, 0.5f);
            sph4 = new BoundingSphere(Vector3.One, 1f);
            sph5 = new BoundingSphere(Vector3.One * 3, 1f);

            box1 = new BoundingBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
            box2 = new BoundingBox(Vector3.One * -1f, Vector3.One * 1f);
            box3 = new BoundingBox(Vector3.One * -0.25f, Vector3.One * 0.25f);
            box4 = new BoundingBox((Vector3.One * -0.5f) + Vector3.One, (Vector3.One * 0.5f) + Vector3.One);
            box5 = new BoundingBox((Vector3.One * -0.5f) + (Vector3.One * 3f), (Vector3.One * 0.5f) + (Vector3.One * 3f));
            box6 = new BoundingBox((Vector3.One * -0.5f) + (Vector3.One * 0.75f), (Vector3.One * 0.75f) + (Vector3.One * 3f));

            Vector3 frustOrigin1 = Vector3.BackwardLH * +0.5f;
            Vector3 frustOrigin2 = Vector3.BackwardLH * +1.0f;
            Vector3 frustOrigin3 = Vector3.BackwardLH * +.25f;
            Vector3 frustOrigin4 = (Vector3.BackwardLH * +0.5f) + Vector3.One * 0.5f;
            Vector3 frustOrigin5 = (Vector3.BackwardLH * +0.5f) + (Vector3.One * 3f);

            frustum1 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin1, frustOrigin1 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 1.0f));
            frustum2 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin2, frustOrigin2 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 2.0f));
            frustum3 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin3, frustOrigin3 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 0.5f));
            frustum4 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin4, frustOrigin4 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 1.0f));
            frustum5 = new BoundingFrustum(Matrix.LookAtLH(frustOrigin5, frustOrigin5 + Vector3.ForwardLH, Vector3.Up) * Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, 1.0f));

            tri1 = new Triangle(new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, -0.5f));
            tri2 = new Triangle(new Vector3(-1.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(-1.0f, 0.0f, -1.0f));
            tri3 = new Triangle(new Vector3(-.25f, 0.0f, .25f), new Vector3(.25f, 0.0f, .25f), new Vector3(-.25f, 0.0f, -.25f));
            tri4 = new Triangle(new Vector3(+0.5f, 0.0f, 1.5f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(+0.5f, 0.0f, +0.5f));
            tri5 = new Triangle(new Vector3(+2.5f, 0.0f, 3.5f), new Vector3(3.5f, 0.0f, 3.5f), new Vector3(+2.5f, 0.0f, +2.5f));
            tri6 = new Triangle(new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(-0.5f, -0.5f, 0.0f));
            tri7 = new Triangle(new Vector3(-0.5f, 10.0f, 0.5f), new Vector3(0.5f, 10.0f, 0.5f), new Vector3(-0.5f, 10.0f, -0.5f));
            tri8 = new Triangle(new Vector3(-0.5f, -10.0f, 0.5f), new Vector3(0.5f, -10.0f, 0.5f), new Vector3(-0.5f, -10.0f, -0.5f));

            triToTriDeltaInto = 0.9f;
            triToTriDeltaExact = 1.0f;
            triToTriDeltaOuter = 1.1f;
            triOrigin = new Triangle(new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, -0.5f));

            rayIn = new Ray(Vector3.One * -10f, Vector3.One * 20f);
            rayOut = new Ray(Vector3.One * -10f, Vector3.ForwardLH * 10f);

            pointIn = Vector3.Zero;
            pointOver = Vector3.Up;
            pointBelow = Vector3.Down;

            pointRayLeft = (Vector3.One * -10f) - Vector3.Up;
            pointRayCenter = new Vector3(-1, 1, -1);
            pointRayRigh = (Vector3.One * +10f) + Vector3.Up;

            triCenterPlane = new Vector3(0.2f, 0.0f, 0.2f);
            triCenterOver = new Vector3(0.2f, 1.0f, 0.2f);
            triCenterBelow = new Vector3(0.2f, -1.0f, 0.2f);

            triP1Perfect = new Vector3(-0.5f, 0.0f, 0.5f);
            triP1 = new Vector3(-0.8f, 1.0f, 0.8f);
            triP2Perfect = new Vector3(0.5f, 0.0f, 0.5f);
            triP2 = new Vector3(0.8f, 1.0f, 0.8f);
            triP3Perfect = new Vector3(-0.5f, 0.0f, -0.5f);
            triP3 = new Vector3(-0.8f, 1.0f, -0.8f);

            triS1Perfect = new Vector3(0.0f, 0.0f, 0.5f);
            triS1 = new Vector3(0.0f, 1.0f, 0.8f);
            triS2Perfect = new Vector3(0.0f, 0.0f, 0.0f);
            triS2 = new Vector3(0.2f, 1.0f, -0.2f);
            triS3Perfect = new Vector3(-0.5f, 0.0f, 0.0f);
            triS3 = new Vector3(-0.8f, 1.0f, 0.0f);

            segment1A = new Vector3(-0.1f, 0.0f, 0.0f);
            segment1B = new Vector3(+0.1f, 0.0f, 0.0f);

            segment2A = new Vector3(-5.0f, 0.0f, 0.0f);
            segment2B = new Vector3(+5.0f, 0.0f, 0.0f);

            segment3A = new Vector3(+0.0f, 0.0f, 0.0f);
            segment3B = new Vector3(+5.0f, 0.0f, 0.0f);

            segment4A = new Vector3(+5.0f, 0.0f, 0.0f);
            segment4B = new Vector3(+10.0f, 0.0f, 0.0f);

            segment5A = new Vector3(+10.0f, 0.0f, 0.0f);
            segment5B = new Vector3(+5.0f, 0.0f, 0.0f);

            segment6A = new Vector3(-5.0f, 5.0f, 0.0f);
            segment6B = new Vector3(+5.0f, 5.0f, 0.0f);
        }
        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

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
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box2);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            // A mesh is not a volume, box 1 is into the box 2, but no mesh 2 triangle intersects mesh 1 triangle
            Assert.IsFalse(res);
            Assert.IsFalse(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshContainedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box3);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            // A mesh is not a volume, box 1 contains box 3, but no mesh 3 triangle intersects mesh 1 triangle
            Assert.IsFalse(res);
            Assert.IsFalse(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshIntersectedPerfectFaceTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box4);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            // It's a perfect face intersection between box 1 and box 4 meshes
            Assert.IsTrue(res);
            Assert.IsTrue(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshNotIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box5);

            var res = Intersection.MeshIntersectsMesh(mesh1, mesh2, out var tris, out var segments);

            Assert.IsFalse(res);
            Assert.IsFalse(tris.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void MeshIntersectsMeshIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box6);

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
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, p.Distance);
            Assert.AreNotEqual(Vector3.Zero, p.Position);
        }
        [TestMethod()]
        public void SphereIntersectsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, p.Distance);
            Assert.AreNotEqual(Vector3.Zero, p.Position);
        }
        [TestMethod()]
        public void SphereIntersectsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, p.Distance);
            Assert.AreNotEqual(Vector3.Zero, p.Position);
        }
        [TestMethod()]
        public void SphereIntersectsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box5);

            var res = Intersection.SphereIntersectsMesh(sph1, mesh, out var p);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, p.Distance);
            Assert.AreEqual(Vector3.Zero, p.Position);
        }

        [TestMethod()]
        public void SphereIntersectsMeshAllContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.IsTrue(p.Any());
            Assert.IsFalse(p.Any(r => r.Distance == float.MaxValue));
            Assert.IsFalse(p.Any(r => r.Position == Vector3.Zero));
        }
        [TestMethod()]
        public void SphereIntersectsMeshAllContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.IsTrue(p.Any());
            Assert.IsFalse(p.Any(r => r.Distance == float.MaxValue));
            Assert.IsFalse(p.Any(r => r.Position == Vector3.Zero));
        }
        [TestMethod()]
        public void SphereIntersectsMeshAllIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsTrue(res);
            Assert.IsTrue(p.Any());
            Assert.IsFalse(p.Any(r => r.Distance == float.MaxValue));
            Assert.IsFalse(p.Any(r => r.Position == Vector3.Zero));
        }
        [TestMethod()]
        public void SphereIntersectsMeshAllNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box5);

            var res = Intersection.SphereIntersectsMeshAll(sph1, mesh, out var p);

            Assert.IsFalse(res);
            Assert.IsFalse(p.Any());
        }

        [TestMethod()]
        public void BoxIntersectsMeshContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            // A mesh is not a volume, box 1 is into the box 2 mesh, but no mesh triangle intersects box 1
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void BoxIntersectsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            // All mesh 3 triangles were into the box 1 volume
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.BoxIntersectsMesh(box1, mesh);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void BoxIntersectsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box5);

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
        public void TriangleIntersectsTriangleParalellOverOriginTest()
        {
            var triOverOrigin = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellBelowOriginTest()
        {
            var triBelowOrigin = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactOverOriginTest()
        {
            var triOverOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaExact, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);
            Assert.IsFalse(res);

            triOverOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaExact * 0.5f, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactBelowOriginTest()
        {
            var triBelowOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaExact, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);
            Assert.IsFalse(res);

            triBelowOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaExact * 0.5f, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoOverOriginTest()
        {
            var triOverOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaInto, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);
            Assert.IsFalse(res);

            triOverOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaInto * 0.5f, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoBelowOriginTest()
        {
            var triBelowOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaInto, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);
            Assert.IsFalse(res);

            triBelowOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaInto * 0.5f, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterOverOriginTest()
        {
            var triOverOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaOuter, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);
            Assert.IsFalse(res);

            triOverOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaOuter * 0.5f, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverOrigin);
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterBelowOriginTest()
        {
            var triBelowOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaOuter, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);
            Assert.IsFalse(res);

            triBelowOrigin = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaOuter * 0.5f, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowOrigin);
            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactPlaneLeftCoplanarTest()
        {
            var triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaExact, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactPlaneRightCoplanarTest()
        {
            var triRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaExact, 0, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactPlaneForwardCoplanarTest()
        {
            var triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, 0, triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactPlaneBackwardCoplanarTest()
        {
            var triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, 0, -triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactOverLeftCoplanarTest()
        {
            var triOverLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaExact, triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactOverRightCoplanarTest()
        {
            var triOverRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaExact, triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactOverForwardCoplanarTest()
        {
            var triOverForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaExact, triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactOverBackwardCoplanarTest()
        {
            var triOverBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaExact, -triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactBelowLeftCoplanarTest()
        {
            var triBelowLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaExact, -triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactBelowRightCoplanarTest()
        {
            var triBelowRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaExact, -triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactBelowForwardCoplanarTest()
        {
            var triBelowForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaExact, triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellExactBelowBackwardCoplanarTest()
        {
            var triBelowBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaExact, -triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowBackwardCoplanar);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoPlaneLeftCoplanarTest()
        {
            var triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaInto, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoPlaneRightCoplanarTest()
        {
            var triRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaInto, 0, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoPlaneForwardCoplanarTest()
        {
            var triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, 0, triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoPlaneBackwardCoplanarTest()
        {
            var triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, 0, -triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoOverLeftCoplanarTest()
        {
            var triOverLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaInto, triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoOverRightCoplanarTest()
        {
            var triOverRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaInto, triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoOverForwardCoplanarTest()
        {
            var triOverForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaInto, triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoOverBackwardCoplanarTest()
        {
            var triOverBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaInto, -triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoBelowLeftCoplanarTest()
        {
            var triBelowLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaInto, -triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoBelowRightCoplanarTest()
        {
            var triBelowRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaInto, -triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoBelowForwardCoplanarTest()
        {
            var triBelowForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaInto, triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellIntoBelowBackwardCoplanarTest()
        {
            var triBelowBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaInto, -triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowBackwardCoplanar);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterPlaneLeftCoplanarTest()
        {
            var triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaOuter, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterPlaneRightCoplanarTest()
        {
            var triRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaOuter, 0, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterPlaneForwardCoplanarTest()
        {
            var triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, 0, triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterPlaneBackwardCoplanarTest()
        {
            var triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, 0, -triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterOverLeftCoplanarTest()
        {
            var triOverLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaOuter, triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterOverRightCoplanarTest()
        {
            var triOverRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaOuter, triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterOverForwardCoplanarTest()
        {
            var triOverForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaOuter, triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterOverBackwardCoplanarTest()
        {
            var triOverBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, triToTriDeltaOuter, -triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterBelowLeftCoplanarTest()
        {
            var triBelowLeftCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(-triToTriDeltaOuter, -triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterBelowRightCoplanarTest()
        {
            var triBelowRightCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(triToTriDeltaOuter, -triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterBelowForwardCoplanarTest()
        {
            var triBelowForwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaOuter, triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleParalellOuterBelowBackwardCoplanarTest()
        {
            var triBelowBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.Translation(0, -triToTriDeltaOuter, -triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowBackwardCoplanar);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactPlaneLeftCoplanarTest()
        {
            var triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaExact, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);
            Assert.IsFalse(res);

            triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaExact * 0.5f, 0, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactPlaneRightCoplanarTest()
        {
            var triRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaExact, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);
            Assert.IsFalse(res);

            triRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaExact * 0.5f, 0, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactPlaneForwardCoplanarTest()
        {
            var triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, triToTriDeltaExact));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);
            Assert.IsFalse(res);

            triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, triToTriDeltaExact * 0.5f));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactPlaneBackwardCoplanarTest()
        {
            var triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, -triToTriDeltaExact));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);
            Assert.IsFalse(res);

            triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, -triToTriDeltaExact * 0.5f));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactOverLeftCoplanarTest()
        {
            var triOverLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaExact, triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactOverRightCoplanarTest()
        {
            var triOverRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaExact, triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactOverForwardCoplanarTest()
        {
            var triOverForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaExact, triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactOverBackwardCoplanarTest()
        {
            var triOverBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaExact, -triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactBelowLeftCoplanarTest()
        {
            var triBelowLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaExact, -triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactBelowRightCoplanarTest()
        {
            var triBelowRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaExact, -triToTriDeltaExact, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactBelowForwardCoplanarTest()
        {
            var triBelowForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaExact, triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularExactBelowBackwardCoplanarTest()
        {
            var triBelowBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaExact, -triToTriDeltaExact));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowBackwardCoplanar);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoPlaneLeftCoplanarTest()
        {
            var triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaInto, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);
            Assert.IsFalse(res);

            triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaInto * 0.5f, 0, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoPlaneRightCoplanarTest()
        {
            var triRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaInto, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);
            Assert.IsFalse(res);

            triRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaInto * 0.5f, 0, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoPlaneForwardCoplanarTest()
        {
            var triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, triToTriDeltaInto));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);
            Assert.IsFalse(res);

            triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, triToTriDeltaInto * 0.5f));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoPlaneBackwardCoplanarTest()
        {
            var triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, -triToTriDeltaInto));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);
            Assert.IsFalse(res);

            triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, -triToTriDeltaInto * 0.5f));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);
            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoOverLeftCoplanarTest()
        {
            var triOverLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaInto, triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoOverRightCoplanarTest()
        {
            var triOverRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaInto, triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoOverForwardCoplanarTest()
        {
            var triOverForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaInto, triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoOverBackwardCoplanarTest()
        {
            var triOverBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaInto, -triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoBelowLeftCoplanarTest()
        {
            var triBelowLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaInto, -triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoBelowRightCoplanarTest()
        {
            var triBelowRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaInto, -triToTriDeltaInto, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoBelowForwardCoplanarTest()
        {
            var triBelowForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaInto, triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularIntoBelowBackwardCoplanarTest()
        {
            var triBelowBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaInto, -triToTriDeltaInto));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowBackwardCoplanar);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterPlaneLeftCoplanarTest()
        {
            var triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaOuter, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);
            Assert.IsFalse(res);

            triLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaOuter * 0.5f, 0, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triLeftCoplanar);
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterPlaneRightCoplanarTest()
        {
            var triRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaOuter, 0, 0));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);
            Assert.IsFalse(res);

            triRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaOuter * 0.5f, 0, 0));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triRightCoplanar);
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterPlaneForwardCoplanarTest()
        {
            var triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, triToTriDeltaOuter));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);
            Assert.IsFalse(res);

            triForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, triToTriDeltaOuter * 0.5f));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triForwardCoplanar);
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterPlaneBackwardCoplanarTest()
        {
            var triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, -triToTriDeltaOuter));
            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);
            Assert.IsFalse(res);

            triBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, 0, -triToTriDeltaOuter * 0.5f));
            res = Intersection.TriangleIntersectsTriangle(triOrigin, triBackwardCoplanar);
            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterOverLeftCoplanarTest()
        {
            var triOverLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaOuter, triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterOverRightCoplanarTest()
        {
            var triOverRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaOuter, triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterOverForwardCoplanarTest()
        {
            var triOverForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaOuter, triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterOverBackwardCoplanarTest()
        {
            var triOverBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, triToTriDeltaOuter, -triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triOverBackwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterBelowLeftCoplanarTest()
        {
            var triBelowLeftCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(-triToTriDeltaOuter, -triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowLeftCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterBelowRightCoplanarTest()
        {
            var triBelowRightCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(triToTriDeltaOuter, -triToTriDeltaOuter, 0));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowRightCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterBelowForwardCoplanarTest()
        {
            var triBelowForwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaOuter, triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowForwardCoplanar);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void TriangleIntersectsTrianglePerpendicularOuterBelowBackwardCoplanarTest()
        {
            var triBelowBackwardCoplanar = Triangle.Transform(triOrigin, Matrix.RotationAxis(Vector3.UnitZ, MathUtil.PiOverTwo) * Matrix.Translation(0, -triToTriDeltaOuter, -triToTriDeltaOuter));

            var res = Intersection.TriangleIntersectsTriangle(triOrigin, triBelowBackwardCoplanar);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void TriangleIntersectsTriangleResultContainsTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri2, out var segment);

            Assert.IsTrue(res);
            Assert.IsNull(segment);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultContainedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri3, out var segment);

            Assert.IsTrue(res);
            Assert.IsNull(segment);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultIntersectedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri4, out var segment);

            Assert.IsTrue(res);
            Assert.IsNull(segment);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultIntersectedTest2()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri6, out var segment);

            Assert.IsTrue(res);
            Assert.IsNotNull(segment);
            Assert.IsFalse(segment.Value.Point1 == segment.Value.Point2);
            Assert.AreEqual(new Vector3(-0.5f, 0, 0), segment.Value.Point1);
            Assert.AreEqual(new Vector3(0, 0, 0), segment.Value.Point2);
        }
        [TestMethod()]
        public void TriangleIntersectsTriangleResultNotIntersectedTest()
        {
            var res = Intersection.TriangleIntersectsTriangle(tri1, tri5, out var segment);

            Assert.IsFalse(res);
            Assert.IsNull(segment);
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
        public void SphereIntersectsTriangleResultContainsTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri2, out var pick);

            Assert.IsTrue(res);
            Assert.AreEqual(sph1.Center, pick.Position);
            Assert.AreEqual(0, pick.Distance);
            Assert.AreEqual(tri2, pick.Primitive);
        }
        [TestMethod()]
        public void SphereIntersectsTriangleResultContainedTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri3, out var pick);

            Assert.IsTrue(res);
            Assert.AreEqual(sph1.Center, pick.Position);
            Assert.AreEqual(0, pick.Distance);
            Assert.AreEqual(tri3, pick.Primitive);
        }
        [TestMethod()]
        public void SphereIntersectsTriangleResultIntersectedTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri4, out var pick);

            Assert.IsTrue(res);
            Assert.AreEqual(tri4.Point3, pick.Position);
            Assert.AreEqual(Vector3.Distance(sph1.Center, tri4.Point3), pick.Distance);
            Assert.AreEqual(tri4, pick.Primitive);
        }
        [TestMethod()]
        public void SphereIntersectsTriangleResultNotIntersectedTest()
        {
            var res = Intersection.SphereIntersectsTriangle(sph1, tri5, out var pick);

            Assert.IsFalse(res);
            Assert.AreEqual(Vector3.Zero, pick.Position);
            Assert.AreEqual(float.MaxValue, pick.Distance);
            Assert.AreEqual(new Triangle(), pick.Primitive);
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
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles);

            // A mesh is not a volume. Tri 1 is into the box 2, but not intersections were possible
            Assert.IsFalse(res);
            Assert.IsFalse(triangles.Any());
        }
        [TestMethod()]
        public void TriangleIntersectsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles);

            Assert.IsTrue(res);
            Assert.IsTrue(triangles.Any());
        }
        [TestMethod()]
        public void TriangleIntersectsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles);

            Assert.IsFalse(res);
            Assert.IsFalse(triangles.Any());
        }

        [TestMethod()]
        public void TriangleIntersectsMeshResultContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles, out var segments);

            // A mesh is not a volume. Tri 1 is into the box 2, but not intersections were possible
            Assert.IsFalse(res);
            Assert.IsFalse(triangles.Any());
            Assert.IsFalse(segments.Any());
        }
        [TestMethod()]
        public void TriangleIntersectsMeshResultIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles, out var segments);

            Assert.IsTrue(res);
            Assert.IsTrue(triangles.Any());
            Assert.IsTrue(segments.Any());
        }
        [TestMethod()]
        public void TriangleIntersectsMeshResultNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.TriangleIntersectsMesh(tri1, mesh, out var triangles, out var segments);

            Assert.IsFalse(res);
            Assert.IsFalse(triangles.Any());
            Assert.IsFalse(segments.Any());
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
        public void FrustumIntersectsSegmentContainsTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment1A, segment1B);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentContainedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment2A, segment2B);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment3A, segment3B);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentNotIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment4A, segment4B);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentNotIntersectedTest2()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment5A, segment5B);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentNotIntersectedTest3()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment6A, segment6B);

            Assert.IsFalse(res);
        }

        [TestMethod()]
        public void FrustumIntersectsSegmentResultContainsTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment1A, segment1B, out float distance, out Vector3 point);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, distance);
            Assert.AreEqual(Vector3.Zero, point);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultContainedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment2A, segment2B, out float distance, out Vector3 point);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, distance);
            Assert.AreNotEqual(Vector3.Zero, point);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment3A, segment3B, out float distance, out Vector3 point);

            Assert.IsTrue(res);
            Assert.AreNotEqual(float.MaxValue, distance);
            Assert.AreNotEqual(Vector3.Zero, point);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultNotIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment4A, segment4B, out float distance, out Vector3 point);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, distance);
            Assert.AreEqual(Vector3.Zero, point);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultNotIntersectedTest2()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment5A, segment5B, out float distance, out Vector3 point);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, distance);
            Assert.AreEqual(Vector3.Zero, point);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultNotIntersectedTest3()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment6A, segment6B, out float distance, out Vector3 point);

            Assert.IsFalse(res);
            Assert.AreEqual(float.MaxValue, distance);
            Assert.AreEqual(Vector3.Zero, point);
        }

        [TestMethod()]
        public void FrustumIntersectsSegmentResultPointsContainsTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment1A, segment1B, out Vector3? enteringPoint, out Vector3? exitingPoint);

            Assert.IsFalse(res);
            Assert.IsNull(enteringPoint);
            Assert.IsNull(exitingPoint);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultPointsContainedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment2A, segment2B, out Vector3? enteringPoint, out Vector3? exitingPoint);

            Assert.IsTrue(res);
            Assert.IsNotNull(enteringPoint);
            Assert.IsNotNull(exitingPoint);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultPointsIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment3A, segment3B, out Vector3? enteringPoint, out Vector3? exitingPoint);

            Assert.IsTrue(res);
            Assert.IsNotNull(enteringPoint);
            Assert.IsNull(exitingPoint);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultPointsNotIntersectedTest()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment4A, segment4B, out Vector3? enteringPoint, out Vector3? exitingPoint);

            Assert.IsFalse(res);
            Assert.IsNull(enteringPoint);
            Assert.IsNull(exitingPoint);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultPointsNotIntersectedTest2()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment5A, segment5B, out Vector3? enteringPoint, out Vector3? exitingPoint);

            Assert.IsFalse(res);
            Assert.IsNull(enteringPoint);
            Assert.IsNull(exitingPoint);
        }
        [TestMethod()]
        public void FrustumIntersectsSegmentResultPointsNotIntersectedTest3()
        {
            var res = Intersection.FrustumIntersectsSegment(frustum1, segment6A, segment6B, out Vector3? enteringPoint, out Vector3? exitingPoint);

            Assert.IsFalse(res);
            Assert.IsNull(enteringPoint);
            Assert.IsNull(exitingPoint);
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
            var res = Intersection.PointInMesh(pointIn, [tri1]);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PointInMeshOverTest()
        {
            var res = Intersection.PointInMesh(pointOver, [tri1]);

            Assert.IsFalse(res);
        }
        [TestMethod()]
        public void PointInMeshBelowTest()
        {
            var res = Intersection.PointInMesh(pointBelow, [tri1]);

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
            Intersection.ClosestPointInMesh(triCenterPlane, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.IsTrue(tri1 == closest || tri3 == closest);
            Assert.AreEqual(triCenterPlane, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshCenterOverTest()
        {
            Intersection.ClosestPointInMesh(triCenterOver, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.IsTrue(tri1 == closest || tri3 == closest);
            Assert.AreEqual(triCenterPlane, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshCenterBelowTest()
        {
            Intersection.ClosestPointInMesh(triCenterBelow, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.IsTrue(tri1 == closest || tri3 == closest);
            Assert.AreEqual(triCenterPlane, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP1PerfectTest()
        {
            Intersection.ClosestPointInMesh(triP1Perfect, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP1Test()
        {
            Intersection.ClosestPointInMesh(triP1, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP2PerfectTest()
        {
            Intersection.ClosestPointInMesh(triP2Perfect, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP2Test()
        {
            Intersection.ClosestPointInMesh(triP2, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP3PerfectTest()
        {
            Intersection.ClosestPointInMesh(triP3Perfect, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP3Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshP3Test()
        {
            Intersection.ClosestPointInMesh(triP3, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triP3Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS1PerfectTest()
        {
            Intersection.ClosestPointInMesh(triS1Perfect, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS1Test()
        {
            Intersection.ClosestPointInMesh(triS1, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS1Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS2PerfectTest()
        {
            Intersection.ClosestPointInMesh(triS2Perfect, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS2Test()
        {
            Intersection.ClosestPointInMesh(triS2, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS2Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS3PerfectTest()
        {
            Intersection.ClosestPointInMesh(triS3Perfect, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

            Assert.AreEqual(tri1, closest);
            Assert.AreEqual(triS3Perfect, closestPoint);
        }
        [TestMethod()]
        public void ClosestPointInMeshS3Test()
        {
            Intersection.ClosestPointInMesh(triS3, [tri1, tri3, tri7, tri8], out var closest, out var closestPoint);

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
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            // A mesh is not a volume, box 1 is into the box 2 mesh, but no mesh triangle intersects box 1
            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
        [TestMethod()]
        public void BoxContainsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            // All mesh 3 triangles were into the box 1 volume
            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void BoxContainsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.BoxContainsMesh(box1, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void BoxContainsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box5);

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
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void SphereContainsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.SphereContainsMesh(sph1, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void SphereContainsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box5);

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
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.FrustumContainsMesh(frustum1, mesh);

            // A mesh is not a volume, frustum 1 is into the box 2 mesh, but no mesh triangle intersects frustum 1
            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
        [TestMethod()]
        public void FrustumContainsMeshContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box3);

            var res = Intersection.FrustumContainsMesh(frustum1, mesh);

            // All mesh 3 triangles were into the frustum 1 volume
            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void FrustumContainsMeshIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box4);

            var res = Intersection.FrustumContainsMesh(frustum2, mesh);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void FrustumContainsMeshNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box5);

            var res = Intersection.FrustumContainsMesh(frustum1, mesh);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void MeshContainsSphereContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsSphere(mesh, sph2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsSphereContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsSphere(mesh, sph3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsSphereIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsSphere(mesh, sph4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsSphereNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsSphere(mesh, sph5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void MeshContainsMeshContainsTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box2);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            // A mesh is not a volume, box 1 is into the box 2, but no mesh 2 triangle intersects mesh 1 triangle
            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsMeshContainedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box3);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            // A mesh is not a volume, box 1 contains box 3, but no mesh 3 triangle intersects mesh 1 triangle
            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsMeshIntersectedPerfectFaceTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box4);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            // It's a perfect face intersection between box 1 and box 4 meshes
            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsMeshNotIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box5);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }
        [TestMethod()]
        public void MeshContainsMeshIntersectedTest()
        {
            var mesh1 = Triangle.ComputeTriangleList(box1);
            var mesh2 = Triangle.ComputeTriangleList(box6);

            var res = Intersection.MeshContainsMesh(mesh1, mesh2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }

        [TestMethod()]
        public void MeshContainsBoxContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsBox(mesh, box2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsBoxContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsBox(mesh, box3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsBoxIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsBox(mesh, box4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsBoxNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsBox(mesh, box5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [TestMethod()]
        public void MeshContainsFrustumContainsTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsFrustum(mesh, frustum2);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsFrustumContainedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box2);

            var res = Intersection.MeshContainsFrustum(mesh, frustum3);

            Assert.AreEqual(ContainmentType.Contains, res);
        }
        [TestMethod()]
        public void MeshContainsFrustumIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsFrustum(mesh, frustum4);

            Assert.AreEqual(ContainmentType.Intersects, res);
        }
        [TestMethod()]
        public void MeshContainsFrustumNotIntersectedTest()
        {
            var mesh = Triangle.ComputeTriangleList(box1);

            var res = Intersection.MeshContainsFrustum(mesh, frustum5);

            Assert.AreEqual(ContainmentType.Disjoint, res);
        }

        [DataTestMethod()]
        [DataRow(0f, 1f, false)]
        [DataRow(0f, 0.5f, false)]
        [DataRow(0f, 0f, true)]
        [DataRow(0f, -1f, true)]
        [DataRow(0f, -2f, true)]
        [DataRow(0f, -3f, true)]
        [DataRow(MathUtil.PiOverFour, 1f, false)]
        [DataRow(MathUtil.PiOverFour, 0.5f, true)]
        [DataRow(MathUtil.PiOverFour, 0f, true)]
        [DataRow(MathUtil.PiOverFour, -1f, true)]
        [DataRow(MathUtil.PiOverFour, -2f, true)]
        [DataRow(MathUtil.PiOverFour, -3f, true)]
        public void CyliderIntersectsPlaneOverTest(float planeAngle, float cylinderHeight, bool intersects)
        {
            float r = 1;
            float h = 2;
            var cylinder = new BoundingCylinder(Vector3.Up * (cylinderHeight + h * 0.5f), r, h);

            var plane = new Plane(Vector3.Zero, Vector3.TransformNormal(Vector3.Up, Matrix.RotationX(planeAngle)));

            var res = Intersection.CylinderIntersectsPlane(cylinder, plane);

            Assert.AreEqual(intersects, res);
        }
    }
}