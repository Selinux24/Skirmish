using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Position4Tests
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
            var res = Position4.Zero;

            Assert.AreEqual(res, new Position4(0, 0, 0, 0));
        }
        [TestMethod()]
        public void PositionValueTest()
        {
            var res = new Position4(2);

            Assert.AreEqual(res, new Position4(2, 2, 2, 2));
        }
        [TestMethod()]
        public void PositionArrayTest()
        {
            var res = new Position4(new float[] { 1, 2, 3, 4 });

            Assert.AreEqual(res, new Position4(1, 2, 3, 4));
        }
        [TestMethod()]
        public void PositionBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Position4(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position4(new float[] { }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position4(new float[] { 1, 2, 3 }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position4(new float[] { 1, 2, 3, 4, 5 }));
        }
        [TestMethod()]
        public void PositionComponentsTest()
        {
            var res = new Position4(1, 2, 3, 4);

            Assert.AreEqual(res, new Position4(1, 2, 3, 4));
        }
        [TestMethod()]
        public void PositionSettesTest()
        {
            var res = new Position4();
            res.X = 1;
            res.Y = 2;
            res.Z = 3;
            res.W = 4;

            Assert.AreEqual(res, new Position4(1, 2, 3, 4));
        }

        [TestMethod()]
        public void PositionEqualsTest()
        {
            var res = Position4.Zero == new Position4(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PositionDistinctTest()
        {
            var res = Position4.Zero != new Position4(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void PositionAddTest()
        {
            var res = new Position4(1, 2, 3, 4) + new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(2, 4, 6, 8));

            res = new Position4(1, 2, 3, 4) + 1;
            Assert.AreEqual(res, new Position4(2, 3, 4, 5));

            res = 1 + new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(2, 3, 4, 5));
        }
        [TestMethod()]
        public void PositionSubstracTest()
        {
            var res = new Position4(1, 2, 3, 4) - new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(0, 0, 0, 0));

            res = new Position4(1, 2, 3, 4) - 1;
            Assert.AreEqual(res, new Position4(0, 1, 2, 3));

            res = 1 - new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(0, -1, -2, -3));
        }
        [TestMethod()]
        public void PositionMultiplyTest()
        {
            var res = new Position4(1, 2, 3, 4) * new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(1, 4, 9, 16));

            res = new Position4(1, 2, 3, 4) * 2;
            Assert.AreEqual(res, new Position4(2, 4, 6, 8));

            res = 2 * new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(2, 4, 6, 8));
        }
        [TestMethod()]
        public void PositionDivideTest()
        {
            var res = new Position4(1, 2, 3, 4) / new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(1, 1, 1, 1));

            res = new Position4(1, 2, 3, 4) / 2;
            Assert.AreEqual(res, new Position4(0.5f, 1, 1.5f, 2));

            res = 2 / new Position4(1, 2, 3, 4);
            Assert.AreEqual(res, new Position4(2, 1, 2f / 3f, 0.5f));
        }
        [TestMethod()]
        public void PositionPositiveTest()
        {
            var res = +new Position4(1, 2, 3, 4);

            Assert.AreEqual(res, new Position4(1, 2, 3, 4));
        }
        [TestMethod()]
        public void PositionNegateTest()
        {
            var res = -new Position4(1, 2, 3, 4);

            Assert.AreEqual(res, new Position4(-1, -2, -3, -4));
        }

        [TestMethod()]
        public void PositionToVector3Test()
        {
            Vector4 res1 = new Position4(1, 2, 3, 4);
            Assert.AreEqual(res1, new Vector4(1, 2, 3, 4));

            Position4 res2 = new Vector4(1, 2, 3, 4);
            Assert.AreEqual(res2, new Position4(1, 2, 3, 4));
        }
        [TestMethod()]
        public void PositionToStringTest()
        {
            string res1 = new Position4(1, 2, 3, 4);
            Assert.AreEqual(res1, "1 2 3 4");

            Position4 res2 = "1 2 3 4";
            Assert.AreEqual(res2, new Position4(1, 2, 3, 4));

            res2 = "Zero";
            Assert.AreEqual(res2, new Position4(0));
            res2 = "Max";
            Assert.AreEqual(res2, new Position4(float.MaxValue));
            res2 = "Min";
            Assert.AreEqual(res2, new Position4(float.MinValue));

            res2 = "Nothing parseable";
            Assert.AreEqual(res2, new Position4(0));
        }
    }
}