﻿using Engine.Physics;
using Engine.Physics.Colliders;
using Engine.Physics.GJK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class GJKTests
    {
        static TestContext _testContext;

        static readonly float toleranze = Solver.EPA_TOLERANCE;

        static BoxCollider BoxFromExtents(Vector3 extents, Matrix transform)
        {
            var collider = new BoxCollider(extents);
            var body = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            collider.Attach(body);

            return collider;
        }
        static SphereCollider SphereFromRadius(float r, Matrix transform)
        {
            var collider = new SphereCollider(r);
            var body = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            collider.Attach(body);

            return collider;
        }
        static MeshCollider MeshFromExtents(Vector3 extents, Matrix transform)
        {
            var triangles = Triangle.ComputeTriangleList(Topology.TriangleList, new BoundingBox(-extents, extents));
            var collider = new MeshCollider(triangles);
            var body = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            collider.Attach(body);

            return collider;
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void SphereSphere1Test()
        {
            var s1 = SphereFromRadius(1, Matrix.Identity);
            var s2 = SphereFromRadius(1, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(penetration >= 0);
        }
        [TestMethod()]
        public void SphereSphere2Test()
        {
            var s1 = SphereFromRadius(1, Matrix.Identity);
            var s2 = SphereFromRadius(1, Matrix.Translation(new Vector3(3, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
        }
        [TestMethod()]
        public void SphereSphere3Test()
        {
            var s1 = SphereFromRadius(1, Matrix.Identity);
            var s2 = SphereFromRadius(1, Matrix.Translation(new Vector3(1, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(penetration >= 0);
        }
        [TestMethod()]
        public void SphereSphere4Test()
        {
            var s1 = SphereFromRadius(1, Matrix.Identity);
            var s2 = SphereFromRadius(2, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(penetration >= 0);
        }

        [TestMethod()]
        public void CubeSphere1Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = SphereFromRadius(1, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeSphere2Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = SphereFromRadius(1, Matrix.Translation(new Vector3(3, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeSphere3Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = SphereFromRadius(1, Matrix.Translation(new Vector3(1, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeSphere4Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = SphereFromRadius(2, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }

        [TestMethod()]
        public void CubeCube1Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeCube2Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One, Matrix.Translation(new Vector3(3, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeCube3Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One, Matrix.Translation(new Vector3(1, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeCube4Test()
        {
            var s1 = BoxFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One * 2f, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }

        [TestMethod()]
        public void CubePolytope1Test()
        {
            var s1 = MeshFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubePolytope2Test()
        {
            var s1 = MeshFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One, Matrix.Translation(new Vector3(3, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubePolytope3Test()
        {
            var s1 = MeshFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One, Matrix.Translation(new Vector3(1, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
        [TestMethod()]
        public void CubePolytope4Test()
        {
            var s1 = MeshFromExtents(Vector3.One, Matrix.Identity);
            var s2 = BoxFromExtents(Vector3.One * 2f, Matrix.Translation(new Vector3(2, 0, 0)));

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
    }
}
