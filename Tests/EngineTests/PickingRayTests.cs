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
        static PickingHullTypes testRayParams;
        static float testRayLength;
        static PickingHullTypes facingOnlyParams;
        static PickingHullTypes allFacesParams;
        static float badRayLength;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            position = Vector3.One;
            direction = Vector3.Normalize(Vector3.One);
            testRay = new Ray(position, direction);
            testRayParams = PickingHullTypes.Geometry;
            testRayLength = 100f;

            facingOnlyParams = PickingHullTypes.FacingOnly;
            allFacesParams = PickingHullTypes.None;

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
            var p = new PickingRay(testRay);

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(PickingHullTypes.Default, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest2()
        {
            var p = new PickingRay(testRay, testRayParams);

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest3()
        {
            var p = new PickingRay(testRay, testRayParams, testRayLength);

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(testRayLength, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest4()
        {
            var p = new PickingRay(position, direction);

            Assert.AreEqual(position, p.Position);
            Assert.AreEqual(direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(PickingHullTypes.Default, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest5()
        {
            var p = new PickingRay(position, direction, testRayParams);

            Assert.AreEqual(position, p.Position);
            Assert.AreEqual(direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }
        [TestMethod()]
        public void RayConstructorTest6()
        {
            var p = new PickingRay(position, direction, testRayParams, testRayLength);

            Assert.AreEqual(position, p.Position);
            Assert.AreEqual(direction, p.Direction);
            Assert.AreEqual(testRayLength, p.RayLength);
            Assert.AreEqual(testRayParams, p.RayPickingParams);
        }

        [TestMethod()]
        public void FacingOnlyTest()
        {
            var p = new PickingRay(position, direction, facingOnlyParams);

            Assert.IsTrue(p.FacingOnly);
        }
        [TestMethod()]
        public void AllFacesTest()
        {
            var p = new PickingRay(position, direction, allFacesParams);

            Assert.IsFalse(p.FacingOnly);
        }

        [TestMethod()]
        public void MaxDistanceTest()
        {
            var p = new PickingRay(position, direction, allFacesParams, badRayLength);

            Assert.AreEqual(badRayLength, p.RayLength);
            Assert.AreEqual(float.MaxValue, p.MaxDistance);
        }

        [TestMethod()]
        public void ImplicitConversionTest1()
        {
            var ray = new Ray(position, direction);
            PickingRay p = ray;

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
            Assert.AreEqual(float.MaxValue, p.RayLength);
            Assert.AreEqual(PickingHullTypes.Default, p.RayPickingParams);
        }
        [TestMethod()]
        public void ImplicitConversionTest2()
        {
            var ray = new PickingRay(position, direction);
            Ray p = ray;

            Assert.AreEqual(testRay.Position, p.Position);
            Assert.AreEqual(testRay.Direction, p.Direction);
        }
    }
}
