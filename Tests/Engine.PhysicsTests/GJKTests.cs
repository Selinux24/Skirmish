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
        public void CubeSphere1Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeSphere2Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeSphere3Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeSphere4Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Scaling(2) };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }

        [TestMethod()]
        public void SphereSphere1Test()
        {
            var s1 = new SphereCollider { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(penetration >= 0);
        }
        [TestMethod()]
        public void SphereSphere2Test()
        {
            var s1 = new SphereCollider { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
        }
        [TestMethod()]
        public void SphereSphere3Test()
        {
            var s1 = new SphereCollider { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(penetration >= 0);
        }
        [TestMethod()]
        public void SphereSphere4Test()
        {
            var s1 = new SphereCollider { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new SphereCollider { R = 1, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Scaling(2) };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(penetration >= 0);
        }

        [TestMethod()]
        public void CubeCube1Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeCube2Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeCube3Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
        [TestMethod()]
        public void CubeCube4Test()
        {
            var s1 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Scaling(2) };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }

        [TestMethod()]
        public void PolytopeCube1Test()
        {
            var vertices = new BoundingBox(-Vector3.One, Vector3.One).GetCorners();
            var s1 = new PolytopeCollider { Points = vertices, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void PolytopeCube2Test()
        {
            var vertices = new BoundingBox(-Vector3.One, Vector3.One).GetCorners();
            var s1 = new PolytopeCollider { Points = vertices, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(0, penetration, toleranze);
        }
        [TestMethod()]
        public void PolytopeCube3Test()
        {
            var vertices = new BoundingBox(-Vector3.One, Vector3.One).GetCorners();
            var s1 = new PolytopeCollider { Points = vertices, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
        [TestMethod()]
        public void PolytopeCube4Test()
        {
            var vertices = new BoundingBox(-Vector3.One, Vector3.One).GetCorners();
            var s1 = new PolytopeCollider { Points = vertices, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BoxCollider { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Scaling(2) };

            bool contact = Solver.GJK(s1, s2, true, out _, out _, out var penetration);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(1, penetration, toleranze);
        }
    }
}
