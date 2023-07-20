using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Position4Tests
    {
        static TestContext _testContext;

        static readonly string positionString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

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

            Assert.AreEqual(new Position4(0, 0, 0, 0), res);
        }
        [TestMethod()]
        public void PositionValueTest()
        {
            var res = new Position4(2);

            Assert.AreEqual(new Position4(2, 2, 2, 2), res);
        }
        [TestMethod()]
        public void PositionArrayTest()
        {
            var res = new Position4(new float[] { 1, 2, 3, 4 });

            Assert.AreEqual(new Position4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Position4(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position4(Array.Empty<float>()));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position4(new float[] { 1, 2, 3 }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Position4(new float[] { 1, 2, 3, 4, 5 }));
        }
        [TestMethod()]
        public void PositionComponentsTest()
        {
            var res = new Position4(1, 2, 3, 4);

            Assert.AreEqual(new Position4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionSettesTest()
        {
            var res = new Position4
            {
                X = 1,
                Y = 2,
                Z = 3,
                W = 4
            };

            Assert.AreEqual(new Position4(1, 2, 3, 4), res);
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
            Assert.AreEqual(new Position4(2, 4, 6, 8), res);

            res = new Position4(1, 2, 3, 4) + 1;
            Assert.AreEqual(new Position4(2, 3, 4, 5), res);

            res = 1 + new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(2, 3, 4, 5), res);
        }
        [TestMethod()]
        public void PositionSubstracTest()
        {
            var res = new Position4(1, 2, 3, 4) - new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(0, 0, 0, 0), res);

            res = new Position4(1, 2, 3, 4) - 1;
            Assert.AreEqual(new Position4(0, 1, 2, 3), res);

            res = 1 - new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(0, -1, -2, -3), res);
        }
        [TestMethod()]
        public void PositionMultiplyTest()
        {
            var res = new Position4(1, 2, 3, 4) * new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(1, 4, 9, 16), res);

            res = new Position4(1, 2, 3, 4) * 2;
            Assert.AreEqual(new Position4(2, 4, 6, 8), res);

            res = 2 * new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(2, 4, 6, 8), res);
        }
        [TestMethod()]
        public void PositionDivideTest()
        {
            var res = new Position4(1, 2, 3, 4) / new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(1, 1, 1, 1), res);

            res = new Position4(1, 2, 3, 4) / 2;
            Assert.AreEqual(new Position4(0.5f, 1, 1.5f, 2), res);

            res = 2 / new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(2, 1, 2f / 3f, 0.5f), res);
        }
        [TestMethod()]
        public void PositionPositiveTest()
        {
            var res = +new Position4(1, 2, 3, 4);

            Assert.AreEqual(new Position4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionNegateTest()
        {
            var res = -new Position4(1, 2, 3, 4);

            Assert.AreEqual(new Position4(-1, -2, -3, -4), res);
        }

        [TestMethod()]
        public void PositionToVector3Test()
        {
            Vector4 res1 = new Position4(1, 2, 3, 4);
            Assert.AreEqual(new Vector4(1, 2, 3, 4), res1);

            Position4 res2 = new Vector4(1, 2, 3, 4);
            Assert.AreEqual(new Position4(1, 2, 3, 4), res2);
        }
        [TestMethod()]
        public void PositionToStringTest()
        {
            string res1 = new Position4(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(positionString, res1);

            Position4 res2 = positionString;
            Assert.AreEqual(new Position4(0.1f, 0.2f, 0.3f, 0.4f), res2);

            res2 = "Zero";
            Assert.AreEqual(new Position4(0), res2);
            res2 = "Max";
            Assert.AreEqual(new Position4(float.MaxValue), res2);
            res2 = "Min";
            Assert.AreEqual(new Position4(float.MinValue), res2);

            res2 = "Nothing parseable";
            Assert.AreEqual(new Position4(0), res2);
        }
    }
}