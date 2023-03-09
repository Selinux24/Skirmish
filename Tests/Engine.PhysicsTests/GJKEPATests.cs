using Engine.Physics.GJKEPA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class GJKEPATests
    {
        static TestContext _testContext;

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

        public static IEnumerable<object[]> TestData
        {
            get
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return new object[] { i };
                }
            }
        }

        [TestMethod()]
        [DynamicData(nameof(TestData))]
        public void CubeCubeTest(int i)
        {
            var s1 = new CubeShape();
            var s2 = new CubeShape();

            Matrix rot1 = Matrix.Identity;
            Matrix rot2 = Matrix.Identity;

            Vector3 pos1 = new Vector3(1.0f + i / 10.0f, 0.1f, 0.0f);
            Vector3 pos2 = new Vector3(-(float)i / 10.0f, 0.0f, 0.0f);

            GJKEPASolver.Detect(s1, s2, rot1, rot2, pos1, pos2, out _, out _, out _, out var separation);

            float analyticalDistance = 2.0f * i / 10.0f;

            Assert.IsFalse(Math.Abs(analyticalDistance - separation) > 1e-5f, "Distance does not match analytical result.");
        }

        [TestMethod()]
        [DynamicData(nameof(TestData))]
        public void CubeSphereTest(int i)
        {
            var s1 = new CubeShape();
            var s2 = new SphereShape();

            Matrix rot1 = Matrix.RotationX(i);
            Matrix rot2 = Matrix.RotationX(-0.7f * i);

            Vector3 pos1 = new Vector3(0.1f, 0.1f, 0.2f);
            Vector3 pos2 = new Vector3(0.8f, 0.3f, 0.4f);

            GJKEPASolver.Detect(s1, s2, rot1, rot2, pos1, pos2, out _, out _, out _, out _);
        }

        [TestMethod()]
        [DynamicData(nameof(TestData))]
        public void SphereSphereTest(int i)
        {
            var s1 = new SphereShape();
            var s2 = new SphereShape();

            Matrix rot1 = Matrix.RotationX(i);
            Matrix rot2 = Matrix.RotationY(-i);

            Vector3 pos1 = new Vector3(0.1f + i / 1e5f, 0.1f, 0.2f);
            Vector3 pos2 = new Vector3(0.8f, 0.3f, 0.4f);

            GJKEPASolver.Detect(s1, s2, rot1, rot2, pos1, pos2, out _, out _, out _, out var separation);

            float analyticalDistance = (pos2 - pos1).Length() - 1.0f;

            Assert.IsFalse(Math.Abs(analyticalDistance - separation) > 1e-5f, "Distance does not match analytical result.");
        }
    }
}
