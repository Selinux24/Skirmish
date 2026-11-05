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
    public class Position3Tests
    {
        public TestContext TestContext { get; set; }

        static readonly string positionString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void PositionZeroTest()
        {
            var res = Vector3.Zero;

            Assert.AreEqual(new Vector3(0, 0, 0), res);
        }
        [TestMethod()]
        public void PositionValueTest()
        {
            var res = new Vector3(2);

            Assert.AreEqual(new Vector3(2, 2, 2), res);
        }
        [TestMethod()]
        public void PositionArrayTest()
        {
            var res = new Vector3([1, 2, 3]);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void PositionBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Vector3(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3([1, 2, 3, 4]));
        }
        [TestMethod()]
        public void PositionComponentsTest()
        {
            var res = new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void PositionSettesTest()
        {
            var res = new Vector3
            {
                X = 1,
                Y = 2,
                Z = 3
            };

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }

        [TestMethod()]
        public void PositionEqualsTest()
        {
            var res = Vector3.Zero == new Vector3(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void PositionDistinctTest()
        {
            var res = Vector3.Zero != new Vector3(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void PositionAddTest()
        {
            var res = new Vector3(1, 2, 3) + new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 4, 6), res);

            res = new Vector3(1, 2, 3) + 1;
            Assert.AreEqual(new Vector3(2, 3, 4), res);

            res = 1 + new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 3, 4), res);
        }
        [TestMethod()]
        public void PositionSubstracTest()
        {
            var res = new Vector3(1, 2, 3) - new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(0, 0, 0), res);

            res = new Vector3(1, 2, 3) - 1;
            Assert.AreEqual(new Vector3(0, 1, 2), res);

            res = 1 - new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(0, -1, -2), res);
        }
        [TestMethod()]
        public void PositionMultiplyTest()
        {
            var res = new Vector3(1, 2, 3) * new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 4, 9), res);

            res = new Vector3(1, 2, 3) * 2;
            Assert.AreEqual(new Vector3(2, 4, 6), res);

            res = 2 * new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 4, 6), res);
        }
        [TestMethod()]
        public void PositionDivideTest()
        {
            var res = new Vector3(1, 2, 3) / new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 1, 1), res);

            res = new Vector3(1, 2, 3) / 2;
            Assert.AreEqual(new Vector3(0.5f, 1, 1.5f), res);

            res = 2 / new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 1, 2f / 3f), res);
        }
        [TestMethod()]
        public void PositionPositiveTest()
        {
            var res = +new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void PositionNegateTest()
        {
            var res = -new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(-1, -2, -3), res);
        }

        [TestMethod()]
        public void PositionToVector3Test()
        {
            Vector3 res1 = new(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res1);

            Vector3 res2 = new(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res2);
        }
        [TestMethod()]
        public void PositionToStringTest()
        {
            string res1 = ContentHelper.WritePosition3(new Vector3(0.1f, 0.2f, 0.3f));
            Assert.AreEqual(positionString, res1);

            Vector3? res2 = ContentHelper.ReadPosition3(positionString);
            Assert.AreEqual(new Vector3(0.1f, 0.2f, 0.3f), res2);

            res2 = ContentHelper.ReadPosition3("Zero");
            Assert.AreEqual(new Vector3(0), res2);
            res2 = ContentHelper.ReadPosition3("Max");
            Assert.AreEqual(new Vector3(float.MaxValue), res2);
            res2 = ContentHelper.ReadPosition3("Min");
            Assert.AreEqual(new Vector3(float.MinValue), res2);

            res2 = ContentHelper.ReadPosition3("Nothing parseable");
            Assert.AreEqual(new Vector3(0), res2);
        }
    }
}