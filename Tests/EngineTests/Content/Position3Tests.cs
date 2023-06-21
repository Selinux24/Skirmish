using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Position3Tests
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
        public void PositionZeroTest()
        {
            var res = Position3.Zero;

            Assert.AreEqual(res, new Position3(0, 0, 0));
        }
        [TestMethod()]
        public void PositionValueTest()
        {
            var res = new Position3(2);

            Assert.AreEqual(res, new Position3(2, 2, 2));
        }
        [TestMethod()]
        public void PositionArrayTest()
        {
            var res = new Position3(new float[] { 1, 2, 3 });

            Assert.AreEqual(res, new Position3(1, 2, 3));
        }
        [TestMethod()]
        public void PositionBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Position3(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position3(new float[] { }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position3(new float[] { 1, 2, 3, 4 }));
        }
        [TestMethod()]
        public void PositionComponentsTest()
        {
            var res = new Position3(1, 2, 3);

            Assert.AreEqual(res, new Position3(1, 2, 3));
        }
        [TestMethod()]
        public void PositionSettesTest()
        {
            var res = new Position3();
            res.X = 1;
            res.Y = 2;
            res.Z = 3;

            Assert.AreEqual(res, new Position3(1, 2, 3));
        }

        [TestMethod()]
        public void PositionEqualsTest()
        {
            var res = Position3.Zero == new Position3(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PositionDistinctTest()
        {
            var res = Position3.Zero != new Position3(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void PositionAddTest()
        {
            var res = new Position3(1, 2, 3) + new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(2, 4, 6));

            res = new Position3(1, 2, 3) + 1;
            Assert.AreEqual(res, new Position3(2, 3, 4));

            res = 1 + new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(2, 3, 4));
        }
        [TestMethod()]
        public void PositionSubstracTest()
        {
            var res = new Position3(1, 2, 3) - new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(0, 0, 0));

            res = new Position3(1, 2, 3) - 1;
            Assert.AreEqual(res, new Position3(0, 1, 2));

            res = 1 - new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(0, -1, -2));
        }
        [TestMethod()]
        public void PositionMultiplyTest()
        {
            var res = new Position3(1, 2, 3) * new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(1, 4, 9));

            res = new Position3(1, 2, 3) * 2;
            Assert.AreEqual(res, new Position3(2, 4, 6));

            res = 2 * new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(2, 4, 6));
        }
        [TestMethod()]
        public void PositionDivideTest()
        {
            var res = new Position3(1, 2, 3) / new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(1, 1, 1));

            res = new Position3(1, 2, 3) / 2;
            Assert.AreEqual(res, new Position3(0.5f, 1, 1.5f));

            res = 2 / new Position3(1, 2, 3);
            Assert.AreEqual(res, new Position3(2, 1, 2f / 3f));
        }
        [TestMethod()]
        public void PositionPositiveTest()
        {
            var res = +new Position3(1, 2, 3);

            Assert.AreEqual(res, new Position3(1, 2, 3));
        }
        [TestMethod()]
        public void PositionNegateTest()
        {
            var res = -new Position3(1, 2, 3);

            Assert.AreEqual(res, new Position3(-1, -2, -3));
        }

        [TestMethod()]
        public void PositionToVector3Test()
        {
            Vector3 res1 = new Position3(1, 2, 3);
            Assert.AreEqual(res1, new Vector3(1, 2, 3));

            Position3 res2 = new Vector3(1, 2, 3);
            Assert.AreEqual(res2, new Position3(1, 2, 3));
        }
        [TestMethod()]
        public void PositionToStringTest()
        {
            string res1 = new Position3(1, 2, 3);
            Assert.AreEqual(res1, "1 2 3");

            Position3 res2 = "1 2 3";
            Assert.AreEqual(res2, new Position3(1, 2, 3));

            res2 = "Zero";
            Assert.AreEqual(res2, new Position3(0));
            res2 = "Max";
            Assert.AreEqual(res2, new Position3(float.MaxValue));
            res2 = "Min";
            Assert.AreEqual(res2, new Position3(float.MinValue));

            res2 = "Nothing parseable";
            Assert.AreEqual(res2, new Position3(0));
        }
    }
}