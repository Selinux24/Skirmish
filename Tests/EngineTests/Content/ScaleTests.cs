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
    public class ScaleTests
    {
        public TestContext TestContext { get; set; }

        static readonly float scale1by5 = 1f / 5f;
        static readonly float scale1by4 = 1f / 4f;
        static readonly float scale1by3 = 1f / 3f;
        static readonly float scale1by2 = 1f / 2f;

        static readonly string scaleString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void ScaleZeroTest()
        {
            var res = Vector3.Zero;

            Assert.AreEqual(new Vector3(0, 0, 0), res);
        }
        [TestMethod()]
        public void ScaleOneTest()
        {
            var res = Vector3.One;

            Assert.AreEqual(new Vector3(1, 1, 1), res);
        }
        [TestMethod()]
        public void ScaleValueTest()
        {
            var res = new Vector3(2);

            Assert.AreEqual(new Vector3(2, 2, 2), res);
        }
        [TestMethod()]
        public void ScaleArrayTest()
        {
            var res = new Vector3([1, 2, 3]);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void ScaleBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Vector3(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3([1, 2, 3, 4]));
        }
        [TestMethod()]
        public void ScaleComponentsTest()
        {
            var res = new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void ScaleSettesTest()
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
        public void ScaleEqualsTest()
        {
            var res = Vector3.Zero == new Vector3(0, 0, 0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void ScaleDistinctTest()
        {
            var res = Vector3.Zero != Vector3.One;

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void ScaleAddTest()
        {
            var res = new Vector3(1, 2, 3) + new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 4, 6), res);

            res = new Vector3(1, 2, 3) + 1;
            Assert.AreEqual(new Vector3(2, 3, 4), res);

            res = 1 + new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 3, 4), res);
        }
        [TestMethod()]
        public void ScaleSubstracTest()
        {
            var res = new Vector3(1, 2, 3) - new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(0, 0, 0), res);

            res = new Vector3(1, 2, 3) - 1;
            Assert.AreEqual(new Vector3(0, 1, 2), res);

            res = 1 - new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(0, -1, -2), res);
        }
        [TestMethod()]
        public void ScaleMultiplyTest()
        {
            var res = new Vector3(1, 2, 3) * new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 4, 9), res);

            res = new Vector3(1, 2, 3) * 2;
            Assert.AreEqual(new Vector3(2, 4, 6), res);

            res = 2 * new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 4, 6), res);
        }
        [TestMethod()]
        public void ScaleDivideTest()
        {
            var res = new Vector3(1, 2, 3) / new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 1, 1), res);

            res = new Vector3(1, 2, 3) / 2;
            Assert.AreEqual(new Vector3(0.5f, 1, 1.5f), res);

            res = 2 / new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 1, 2f / 3f), res);
        }
        [TestMethod()]
        public void ScalePositiveTest()
        {
            var res = +new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void ScaleNegateTest()
        {
            var res = -new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(-1, -2, -3), res);
        }

        [TestMethod()]
        public void ScaleToVector3Test()
        {
            Vector3 res1 = new(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res1);

            Vector3 res2 = new(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res2);
        }
        [TestMethod()]
        public void ScaleToStringTest()
        {
            string res1 = ContentHelper.WriteScale(new Vector3(0.1f, 0.2f, 0.3f));
            Assert.AreEqual(scaleString, res1);

            Vector3? res2 = ContentHelper.ReadScale(scaleString);
            Assert.AreEqual(new Vector3(0.1f, 0.2f, 0.3f), res2);

            res2 = ContentHelper.ReadScale("One");
            Assert.AreEqual(new Vector3(1), res2);
            res2 = ContentHelper.ReadScale("Two");
            Assert.AreEqual(new Vector3(2), res2);

            res2 = ContentHelper.ReadScale("1/5");
            Assert.AreEqual(new Vector3(scale1by5), res2);
            res2 = ContentHelper.ReadScale("1/4");
            Assert.AreEqual(new Vector3(scale1by4), res2);
            res2 = ContentHelper.ReadScale("1/3");
            Assert.AreEqual(new Vector3(scale1by3), res2);
            res2 = ContentHelper.ReadScale("1/2");
            Assert.AreEqual(new Vector3(scale1by2), res2);

            res2 = ContentHelper.ReadScale("Nothing parseable");
            Assert.AreEqual(new Vector3(1), res2);
        }
    }
}