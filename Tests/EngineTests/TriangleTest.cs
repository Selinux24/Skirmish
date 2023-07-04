using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class TriangleTest
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

        [TestMethod()]
        public void ConstructorTest()
        {
            Vector3 p1 = new(1, 0, 1);
            Vector3 p2 = new(-1, 0, -1);
            Vector3 p3 = new(-1, 0, 1);
            Vector3 c = Vector3.Multiply(p1 + p2 + p3, 1.0f / 3.0f);
            Vector3 n = new(0, 1, 0);

            Triangle t1 = new();
            Triangle t2 = new(p1, p2, p3);
            Triangle t3 = new(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, p3.X, p3.Y, p3.Z);
            Triangle t4 = new(new[] { p1, p2, p3 });

            Assert.IsTrue(t1 != t2);
            Assert.IsFalse(t1 == t2);
            Assert.IsFalse(t1.Equals(t2));
            Assert.IsFalse(t1.Equals((object)t2));

            Assert.IsTrue(t2 == t3);
            Assert.IsFalse(t2 != t3);
            Assert.IsTrue(t2.Equals(t3));
            Assert.IsTrue(t2.Equals((object)t3));

            Assert.AreEqual(Vector3.Zero, t1.Point1);
            Assert.AreEqual(Vector3.Zero, t1.Point2);
            Assert.AreEqual(Vector3.Zero, t1.Point3);
            Assert.AreEqual(Vector3.Zero, t1.Center);
            Assert.AreEqual(Vector3.Zero, t1.Normal);

            Assert.AreEqual(p1, t2.Point1);
            Assert.AreEqual(p2, t2.Point2);
            Assert.AreEqual(p3, t2.Point3);
            Assert.AreEqual(c, t2.Center);
            Assert.AreEqual(n, t2.Normal);

            Assert.AreEqual(p1, t3.Point1);
            Assert.AreEqual(p2, t3.Point2);
            Assert.AreEqual(p3, t3.Point3);
            Assert.AreEqual(c, t3.Center);
            Assert.AreEqual(n, t3.Normal);

            Assert.AreEqual(p1, t4.Point1);
            Assert.AreEqual(p2, t4.Point2);
            Assert.AreEqual(p3, t4.Point3);
            Assert.AreEqual(c, t4.Center);
            Assert.AreEqual(n, t4.Normal);
        }
    }
}
