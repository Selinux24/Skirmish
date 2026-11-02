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
    public class CntDirection3Tests
    {
        public TestContext TestContext { get; set; }

        static readonly string directionString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void DirectionZeroTest()
        {
            var res = CntDirection3.Zero;

            Assert.AreEqual(new CntDirection3(0, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionUpTest()
        {
            var res = CntDirection3.Up;

            Assert.AreEqual(new CntDirection3(0, 1, 0), res);
        }
        [TestMethod()]
        public void DirectionDownTest()
        {
            var res = CntDirection3.Down;

            Assert.AreEqual(new CntDirection3(0, -1, 0), res);
        }
        [TestMethod()]
        public void DirectionLeftTest()
        {
            var res = CntDirection3.Left;

            Assert.AreEqual(new CntDirection3(-1, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionRightTest()
        {
            var res = CntDirection3.Right;

            Assert.AreEqual(new CntDirection3(1, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionForwardLHTest()
        {
            var res = CntDirection3.ForwardLH;

            Assert.AreEqual(new CntDirection3(0, 0, 1), res);
        }
        [TestMethod()]
        public void DirectionBackwardLHTest()
        {
            var res = CntDirection3.BackwardLH;

            Assert.AreEqual(new CntDirection3(0, 0, -1), res);
        }
        [TestMethod()]
        public void DirectionForwardRHTest()
        {
            var res = CntDirection3.ForwardRH;

            Assert.AreEqual(new CntDirection3(0, 0, -1), res);
        }
        [TestMethod()]
        public void DirectionBackwardRHTest()
        {
            var res = CntDirection3.BackwardRH;

            Assert.AreEqual(new CntDirection3(0, 0, 1), res);
        }

        [TestMethod()]
        public void DirectionValueTest()
        {
            var res = new CntDirection3(2);

            Assert.AreEqual(new CntDirection3(2, 2, 2), res);
        }
        [TestMethod()]
        public void DirectionArrayTest()
        {
            var res = new CntDirection3([1, 2, 3]);

            Assert.AreEqual(new CntDirection3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new CntDirection3(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntDirection3([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntDirection3([1, 2, 3, 4]));
        }
        [TestMethod()]
        public void DirectionComponentsTest()
        {
            var res = new CntDirection3(1, 2, 3);

            Assert.AreEqual(new CntDirection3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionSettesTest()
        {
            var res = new CntDirection3
            {
                X = 1,
                Y = 2,
                Z = 3
            };

            Assert.AreEqual(new CntDirection3(1, 2, 3), res);
        }

        [TestMethod()]
        public void DirectionEqualsTest()
        {
            var res = CntDirection3.Zero == new CntDirection3(0, 0, 0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void DirectionDistinctTest()
        {
            var res = CntDirection3.Zero != CntDirection3.Up;

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void DirectionAddTest()
        {
            var res = new CntDirection3(1, 2, 3) + new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(2, 4, 6), res);

            res = new CntDirection3(1, 2, 3) + 1;
            Assert.AreEqual(new CntDirection3(2, 3, 4), res);

            res = 1 + new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(2, 3, 4), res);
        }
        [TestMethod()]
        public void DirectionSubstracTest()
        {
            var res = new CntDirection3(1, 2, 3) - new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(0, 0, 0), res);

            res = new CntDirection3(1, 2, 3) - 1;
            Assert.AreEqual(new CntDirection3(0, 1, 2), res);

            res = 1 - new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(0, -1, -2), res);
        }
        [TestMethod()]
        public void DirectionMultiplyTest()
        {
            var res = new CntDirection3(1, 2, 3) * new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(1, 4, 9), res);

            res = new CntDirection3(1, 2, 3) * 2;
            Assert.AreEqual(new CntDirection3(2, 4, 6), res);

            res = 2 * new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(2, 4, 6), res);
        }
        [TestMethod()]
        public void DirectionDivideTest()
        {
            var res = new CntDirection3(1, 2, 3) / new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(1, 1, 1), res);

            res = new CntDirection3(1, 2, 3) / 2;
            Assert.AreEqual(new CntDirection3(0.5f, 1, 1.5f), res);

            res = 2 / new CntDirection3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(2, 1, 2f / 3f), res);
        }
        [TestMethod()]
        public void DirectionPositiveTest()
        {
            var res = +new CntDirection3(1, 2, 3);

            Assert.AreEqual(new CntDirection3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionNegateTest()
        {
            var res = -new CntDirection3(1, 2, 3);

            Assert.AreEqual(new CntDirection3(-1, -2, -3), res);
        }

        [TestMethod()]
        public void DirectionToVector3Test()
        {
            Vector3 res1 = new CntDirection3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res1);

            CntDirection3 res2 = new Vector3(1, 2, 3);
            Assert.AreEqual(new CntDirection3(1, 2, 3), res2);
        }
        [TestMethod()]
        public void DirectionToStringTest()
        {
            string res1 = new CntDirection3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(directionString, res1);

            CntDirection3 res2 = directionString;
            Assert.AreEqual(new CntDirection3(0.1f, 0.2f, 0.3f), res2);

            res2 = "Up";
            Assert.AreEqual(CntDirection3.Up, res2);
            res2 = "Down";
            Assert.AreEqual(CntDirection3.Down, res2);
            res2 = "Left";
            Assert.AreEqual(CntDirection3.Left, res2);
            res2 = "Right";
            Assert.AreEqual(CntDirection3.Right, res2);
            res2 = "Forward";
            Assert.AreEqual(CntDirection3.ForwardLH, res2);
            res2 = "Backward";
            Assert.AreEqual(CntDirection3.BackwardLH, res2);
            res2 = "ForwardLH";
            Assert.AreEqual(CntDirection3.ForwardLH, res2);
            res2 = "BackwardLH";
            Assert.AreEqual(CntDirection3.BackwardLH, res2);
            res2 = "ForwardRH";
            Assert.AreEqual(CntDirection3.ForwardRH, res2);
            res2 = "BackwardRH";
            Assert.AreEqual(CntDirection3.BackwardRH, res2);

            res2 = "Nothing parseable";
            Assert.AreEqual(CntDirection3.ForwardLH, res2);
        }
    }
}