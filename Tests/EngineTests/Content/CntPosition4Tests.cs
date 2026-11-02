using Engine.Content;
using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EngineTests.Content
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class CntPosition4Tests
    {
        public TestContext TestContext { get; set; }

        static readonly string positionString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void PositionZeroTest()
        {
            var res = CntPosition4.Zero;

            Assert.AreEqual(new CntPosition4(0, 0, 0, 0), res);
        }
        [TestMethod()]
        public void PositionValueTest()
        {
            var res = new CntPosition4(2);

            Assert.AreEqual(new CntPosition4(2, 2, 2, 2), res);
        }
        [TestMethod()]
        public void PositionArrayTest()
        {
            var res = new CntPosition4([1, 2, 3, 4]);

            Assert.AreEqual(new CntPosition4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new CntPosition4(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntPosition4([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntPosition4([1, 2, 3]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntPosition4([1, 2, 3, 4, 5]));
        }
        [TestMethod()]
        public void PositionComponentsTest()
        {
            var res = new CntPosition4(1, 2, 3, 4);

            Assert.AreEqual(new CntPosition4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionSettesTest()
        {
            var res = new CntPosition4
            {
                X = 1,
                Y = 2,
                Z = 3,
                W = 4
            };

            Assert.AreEqual(new CntPosition4(1, 2, 3, 4), res);
        }

        [TestMethod()]
        public void PositionEqualsTest()
        {
            var res = CntPosition4.Zero == new CntPosition4(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PositionDistinctTest()
        {
            var res = CntPosition4.Zero != new CntPosition4(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void PositionAddTest()
        {
            var res = new CntPosition4(1, 2, 3, 4) + new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(2, 4, 6, 8), res);

            res = new CntPosition4(1, 2, 3, 4) + 1;
            Assert.AreEqual(new CntPosition4(2, 3, 4, 5), res);

            res = 1 + new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(2, 3, 4, 5), res);
        }
        [TestMethod()]
        public void PositionSubstracTest()
        {
            var res = new CntPosition4(1, 2, 3, 4) - new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(0, 0, 0, 0), res);

            res = new CntPosition4(1, 2, 3, 4) - 1;
            Assert.AreEqual(new CntPosition4(0, 1, 2, 3), res);

            res = 1 - new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(0, -1, -2, -3), res);
        }
        [TestMethod()]
        public void PositionMultiplyTest()
        {
            var res = new CntPosition4(1, 2, 3, 4) * new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(1, 4, 9, 16), res);

            res = new CntPosition4(1, 2, 3, 4) * 2;
            Assert.AreEqual(new CntPosition4(2, 4, 6, 8), res);

            res = 2 * new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(2, 4, 6, 8), res);
        }
        [TestMethod()]
        public void PositionDivideTest()
        {
            var res = new CntPosition4(1, 2, 3, 4) / new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(1, 1, 1, 1), res);

            res = new CntPosition4(1, 2, 3, 4) / 2;
            Assert.AreEqual(new CntPosition4(0.5f, 1, 1.5f, 2), res);

            res = 2 / new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(2, 1, 2f / 3f, 0.5f), res);
        }
        [TestMethod()]
        public void PositionPositiveTest()
        {
            var res = +new CntPosition4(1, 2, 3, 4);

            Assert.AreEqual(new CntPosition4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionNegateTest()
        {
            var res = -new CntPosition4(1, 2, 3, 4);

            Assert.AreEqual(new CntPosition4(-1, -2, -3, -4), res);
        }

        [TestMethod()]
        public void PositionToVector3Test()
        {
            Vector4 res1 = new CntPosition4(1, 2, 3, 4);
            Assert.AreEqual(new Vector4(1, 2, 3, 4), res1);

            CntPosition4 res2 = new Vector4(1, 2, 3, 4);
            Assert.AreEqual(new CntPosition4(1, 2, 3, 4), res2);
        }
        [TestMethod()]
        public void PositionToStringTest()
        {
            string res1 = new CntPosition4(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(positionString, res1);

            CntPosition4 res2 = positionString;
            Assert.AreEqual(new CntPosition4(0.1f, 0.2f, 0.3f, 0.4f), res2);

            res2 = "Zero";
            Assert.AreEqual(new CntPosition4(0), res2);
            res2 = "Max";
            Assert.AreEqual(new CntPosition4(float.MaxValue), res2);
            res2 = "Min";
            Assert.AreEqual(new CntPosition4(float.MinValue), res2);

            res2 = "Nothing parseable";
            Assert.AreEqual(new CntPosition4(0), res2);
        }
    }
}