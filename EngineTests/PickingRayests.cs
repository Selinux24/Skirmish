using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class PickingRayTests
    {
        static TestContext _testContext;

        static Vector3 position;
        static Vector3 direction;
        static Ray testRay;
        static RayPickingParams testRayParams;
        static float testRayLength;
        static RayPickingParams facingOnlyParams;
        static RayPickingParams allFacesParams;
        static float badRayLength;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            position = Vector3.One;
            direction = Vector3.Normalize(Vector3.One);
            testRay = new Ray(position, direction);
            testRayParams = RayPickingParams.Geometry;
            testRayLength = 100f;

            facingOnlyParams = RayPickingParams.FacingOnly;
            allFacesParams = RayPickingParams.AllTriangles;

            badRayLength = -1;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void RayConstructorTest1()
        {
            PickingRay p = new PickingRay(testRay);

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(RayPickingParams.Default, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest2()
        {
            PickingRay p = new PickingRay(testRay, testRayParams);

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest3()
        {
            PickingRay p = new PickingRay(testRay, testRayParams, testRayLength);

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(testRayLength, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest4()
        {
            PickingRay p = new PickingRay(position, direction);

            Assert.AreEqual(position, p.Position);
            Assert.AreEqual(direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(RayPickingParams.Default, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest5()
        {
            PickingRay p = new PickingRay(position, direction, testRayParams);

            Assert.AreEqual(position, p.Position);
            Assert.AreEqual(direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest6()
        {
            PickingRay p = new PickingRay(position, direction, testRayParams, testRayLength);

            Assert.AreEqual(position, p.Position);
            Assert.AreEqual(direction, p.Direction);
            Assert.AreEqual(testRayLength, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }

        [TestMethod()]
        public void FacingOnlyTest()
        {
            PickingRay p = new PickingRay(position, direction, facingOnlyParams);

            Assert.IsTrue(p.FacingOnly);
        }
        [TestMethod()]
        public void AllFacesTest()
        {
            PickingRay p = new PickingRay(position, direction, allFacesParams);

            Assert.IsFalse(p.FacingOnly);
        }

        [TestMethod()]
        public void MaxDistanceTest()
        {
            PickingRay p = new PickingRay(position, direction, allFacesParams, badRayLength);

            Assert.AreEqual(badRayLength, p.RayLength);
            Assert.AreEqual(float.MaxValue, p.MaxDistance);
        }

        [TestMethod()]
        public void ImplicitConversionTest1()
        {
            Ray ray = new Ray(position, direction);
            PickingRay p = ray;

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(RayPickingParams.Default, p.RayPickingParams);
        }
        [TestMethod()]
        public void ImplicitConversionTest2()
        {
            PickingRay ray = new PickingRay(position, direction);
            Ray p = ray;

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
        }
    }
}
