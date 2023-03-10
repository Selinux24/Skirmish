using Engine.Physics.GJK2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class GJK2Tests
    {
        static TestContext _testContext;

        static float toleranze = Solver.EPA_TOLERANCE;

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
        public void CubeCubeTest()
        {
            var s1 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(Vector3.Zero, mtv);
        }
        [TestMethod()]
        public void CubeCube2Test()
        {
            var s1 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(Vector3.Zero, mtv);
        }
        [TestMethod()]
        public void CubeCube3Test()
        {
            var s1 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(true, contact);
            Assert.AreEqual(new Vector3(-1, 0, 0), mtv);
        }

        [TestMethod()]
        public void CubeSphereTest()
        {
            var s1 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new Sphere { R = 1, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(Vector3.NearEqual(Vector3.Zero, mtv, new Vector3(toleranze)));
        }
        [TestMethod()]
        public void CubeSphere2Test()
        {
            var s1 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new Sphere { R = 1, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(Vector3.Zero, mtv);
        }
        [TestMethod()]
        public void CubeSphere3Test()
        {
            var s1 = new BBox { Min = -Vector3.One, Max = Vector3.One, Position = Vector3.Zero, RotationScale = Matrix.Identity };
            var s2 = new Sphere { R = 1, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(true, contact);
            Assert.IsTrue(Vector3.NearEqual(new Vector3(-1, 0, 0), mtv, new Vector3(toleranze)));
        }

        [TestMethod()]
        public void SphereSphereTest()
        {
            var s1 = new Sphere { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new Sphere { R = 1, Position = new Vector3(2, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(true, contact);
            //Assert.IsTrue(Vector3.NearEqual(Vector3.Zero, mtv, new Vector3(toleranze)))
        }
        [TestMethod()]
        public void SphereSphere2Test()
        {
            var s1 = new Sphere { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new Sphere { R = 1, Position = new Vector3(3, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(false, contact);
            Assert.AreEqual(Vector3.Zero, mtv);
        }
        [TestMethod()]
        public void SphereSphere3Test()
        {
            var s1 = new Sphere { R = 1, Position = new Vector3(0, 0, 0), RotationScale = Matrix.Identity };
            var s2 = new Sphere { R = 1, Position = new Vector3(1, 0, 0), RotationScale = Matrix.Identity };

            bool contact = Solver.GJK(s1, s2, true, out var mtv);

            Assert.AreEqual(true, contact);
            //Assert.IsTrue(Vector3.NearEqual(new Vector3(-1, 0, 0), mtv, new Vector3(toleranze)))
        }
    }
}
