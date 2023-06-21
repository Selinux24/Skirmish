using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class RotationQTests
    {
        static TestContext _testContext;

        static RotationQ rotarion0 = RotationQ.RotationAxis(Direction3.Up, MathUtil.DegreesToRadians(0));
        static RotationQ rotarion33 = RotationQ.RotationAxis(Direction3.Up, MathUtil.DegreesToRadians(33));
        static RotationQ rotarion90 = RotationQ.RotationAxis(Direction3.Up, MathUtil.DegreesToRadians(90));

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
        public void RotationZeroTest()
        {
            var res = RotationQ.Zero;

            Assert.AreEqual(res, new RotationQ(0, 0, 0, 0));
        }
        [TestMethod()]
        public void RotationOneTest()
        {
            var res = RotationQ.One;

            Assert.AreEqual(res, new RotationQ(1, 1, 1, 1));
        }
        [TestMethod()]
        public void RotationIdentityTest()
        {
            var res = RotationQ.Identity;

            Assert.AreEqual(res, new RotationQ(0, 0, 0, 1));
        }
        [TestMethod()]
        public void RotationValueTest()
        {
            var res = new RotationQ(2);

            Assert.AreEqual(res, new RotationQ(2, 2, 2, 2));
        }
        [TestMethod()]
        public void RotationArrayTest()
        {
            var res = new RotationQ(new float[] { 1, 2, 3, 4 });

            Assert.AreEqual(res, new RotationQ(1, 2, 3, 4));
        }
        [TestMethod()]
        public void RotationBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RotationQ(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new RotationQ(new float[] { }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new RotationQ(new float[] { 1, 2, 3 }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new RotationQ(new float[] { 1, 2, 3, 4, 5 }));
        }
        [TestMethod()]
        public void RotationComponentsTest()
        {
            var res = new RotationQ(1, 2, 3, 4);

            Assert.AreEqual(res, new RotationQ(1, 2, 3, 4));
        }
        [TestMethod()]
        public void RotationSettesTest()
        {
            var res = new RotationQ();
            res.X = 1;
            res.Y = 2;
            res.Z = 3;
            res.W = 4;

            Assert.AreEqual(res, new RotationQ(1, 2, 3, 4));
        }

        [TestMethod()]
        public void RotationEqualsTest()
        {
            var res = RotationQ.Zero == new RotationQ(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void RotationDistinctTest()
        {
            var res = RotationQ.Zero != new RotationQ(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void RotationAddTest()
        {
            var res = new RotationQ(1, 2, 3, 4) + new RotationQ(1, 2, 3, 4);
            Assert.AreEqual(res, new RotationQ(2, 4, 6, 8));
        }
        [TestMethod()]
        public void RotationSubstracTest()
        {
            var res = new RotationQ(1, 2, 3, 4) - new RotationQ(1, 2, 3, 4);
            Assert.AreEqual(res, new RotationQ(0, 0, 0, 0));
        }
        [TestMethod()]
        public void RotationMultiplyTest()
        {
            RotationQ res = new RotationQ(1, 2, 3, 4) * new RotationQ(1, 2, 3, 4);
            RotationQ expected = new Quaternion(1, 2, 3, 4) * new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(expected, res);

            res = new RotationQ(1, 2, 3, 4) * 2;
            expected = new Quaternion(1, 2, 3, 4) * 2;
            Assert.AreEqual(res, expected);

            res = 2 * new RotationQ(1, 2, 3, 4);
            expected = 2 * new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(res, expected);
        }
        [TestMethod()]
        public void RotationNegateTest()
        {
            var res = -new RotationQ(1, 2, 3, 4);

            Assert.AreEqual(res, new RotationQ(-1, -2, -3, -4));
        }

        [TestMethod()]
        public void RotationToQuaternionTest()
        {
            Quaternion res1 = new RotationQ(1, 2, 3, 4);
            Assert.AreEqual(res1, new Quaternion(1, 2, 3, 4));

            RotationQ res2 = new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(res2, new RotationQ(1, 2, 3, 4));
        }
        [TestMethod()]
        public void RotationToStringTest()
        {
            string res1 = new RotationQ(1, 2, 3, 4);
            Assert.AreEqual(res1, "1 2 3 4");

            RotationQ res2 = "1 2 3 4";
            Assert.AreEqual(res2, new RotationQ(1, 2, 3, 4));

            res2 = "Identity";
            Assert.AreEqual(res2, RotationQ.Identity);
            res2 = "Rot0";
            Assert.AreEqual(res2, rotarion0);
            res2 = "Rot33";
            Assert.AreEqual(res2, rotarion33);
            res2 = "Rot90";
            Assert.AreEqual(res2, rotarion90);

            res2 = "Nothing parseable";
            Assert.AreEqual(res2, RotationQ.Identity);
        }
    }
}