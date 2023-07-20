using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Position3Tests
    {
        static TestContext _testContext;

        static readonly string positionString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);

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

            Assert.AreEqual(new Position3(0, 0, 0), res);
        }
        [TestMethod()]
        public void PositionValueTest()
        {
            var res = new Position3(2);

            Assert.AreEqual(new Position3(2, 2, 2), res);
        }
        [TestMethod()]
        public void PositionArrayTest()
        {
            var res = new Position3(new float[] { 1, 2, 3 });

            Assert.AreEqual(new Position3(1, 2, 3), res);
        }
        [TestMethod()]
        public void PositionBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Position3(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position3(Array.Empty<float>()));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position3(new float[] { 1, 2, 3, 4 }));
        }
        [TestMethod()]
        public void PositionComponentsTest()
        {
            var res = new Position3(1, 2, 3);

            Assert.AreEqual(new Position3(1, 2, 3), res);
        }
        [TestMethod()]
        public void PositionSettesTest()
        {
            var res = new Position3
            {
                X = 1,
                Y = 2,
                Z = 3
            };

            Assert.AreEqual(new Position3(1, 2, 3), res);
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
            Assert.AreEqual(new Position3(2, 4, 6), res);

            res = new Position3(1, 2, 3) + 1;
            Assert.AreEqual(new Position3(2, 3, 4), res);

            res = 1 + new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionSubstracTest()
        {
            var res = new Position3(1, 2, 3) - new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(0, 0, 0), res);

            res = new Position3(1, 2, 3) - 1;
            Assert.AreEqual(new Position3(0, 1, 2), res);

            res = 1 - new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(0, -1, -2), res);
        }
        [TestMethod()]
        public void PositionMultiplyTest()
        {
            var res = new Position3(1, 2, 3) * new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(1, 4, 9), res);

            res = new Position3(1, 2, 3) * 2;
            Assert.AreEqual(new Position3(2, 4, 6), res);

            res = 2 * new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(2, 4, 6), res);
        }
        [TestMethod()]
        public void PositionDivideTest()
        {
            var res = new Position3(1, 2, 3) / new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(1, 1, 1), res);

            res = new Position3(1, 2, 3) / 2;
            Assert.AreEqual(new Position3(0.5f, 1, 1.5f), res);

            res = 2 / new Position3(1, 2, 3);
            Assert.AreEqual(new Position3(2, 1, 2f / 3f), res);
        }
        [TestMethod()]
        public void PositionPositiveTest()
        {
            var res = +new Position3(1, 2, 3);

            Assert.AreEqual(new Position3(1, 2, 3), res);
        }
        [TestMethod()]
        public void PositionNegateTest()
        {
            var res = -new Position3(1, 2, 3);

            Assert.AreEqual(new Position3(-1, -2, -3), res);
        }

        [TestMethod()]
        public void PositionToVector3Test()
        {
            Vector3 res1 = new Position3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res1);

            Position3 res2 = new Vector3(1, 2, 3);
            Assert.AreEqual(new Position3(1, 2, 3), res2);
        }
        [TestMethod()]
        public void PositionToStringTest()
        {
            string res1 = new Position3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(positionString, res1);

            Position3 res2 = positionString;
            Assert.AreEqual(new Position3(0.1f, 0.2f, 0.3f), res2);

            res2 = "Zero";
            Assert.AreEqual(new Position3(0), res2);
            res2 = "Max";
            Assert.AreEqual(new Position3(float.MaxValue), res2);
            res2 = "Min";
            Assert.AreEqual(new Position3(float.MinValue), res2);

            res2 = "Nothing parseable";
            Assert.AreEqual(new Position3(0), res2);
        }
    }
}