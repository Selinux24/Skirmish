using Engine.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EngineTests.Content
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Direction3Tests
    {
        static TestContext _testContext;

        static readonly string directionString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);

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
        public void DirectionZeroTest()
        {
            var res = Direction3.Zero;

            Assert.AreEqual(new Direction3(0, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionUpTest()
        {
            var res = Direction3.Up;

            Assert.AreEqual(new Direction3(0, 1, 0), res);
        }
        [TestMethod()]
        public void DirectionDownTest()
        {
            var res = Direction3.Down;

            Assert.AreEqual(new Direction3(0, -1, 0), res);
        }
        [TestMethod()]
        public void DirectionLeftTest()
        {
            var res = Direction3.Left;

            Assert.AreEqual(new Direction3(-1, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionRightTest()
        {
            var res = Direction3.Right;

            Assert.AreEqual(new Direction3(1, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionForwardLHTest()
        {
            var res = Direction3.ForwardLH;

            Assert.AreEqual(new Direction3(0, 0, 1), res);
        }
        [TestMethod()]
        public void DirectionBackwardLHTest()
        {
            var res = Direction3.BackwardLH;

            Assert.AreEqual(new Direction3(0, 0, -1), res);
        }
        [TestMethod()]
        public void DirectionForwardRHTest()
        {
            var res = Direction3.ForwardRH;

            Assert.AreEqual(new Direction3(0, 0, -1), res);
        }
        [TestMethod()]
        public void DirectionBackwardRHTest()
        {
            var res = Direction3.BackwardRH;

            Assert.AreEqual(new Direction3(0, 0, 1), res);
        }

        [TestMethod()]
        public void DirectionValueTest()
        {
            var res = new Direction3(2);

            Assert.AreEqual(new Direction3(2, 2, 2), res);
        }
        [TestMethod()]
        public void DirectionArrayTest()
        {
            var res = new Direction3([1, 2, 3]);

            Assert.AreEqual(new Direction3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Direction3(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Direction3([]));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Direction3([1, 2, 3, 4]));
        }
        [TestMethod()]
        public void DirectionComponentsTest()
        {
            var res = new Direction3(1, 2, 3);

            Assert.AreEqual(new Direction3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionSettesTest()
        {
            var res = new Direction3
            {
                X = 1,
                Y = 2,
                Z = 3
            };

            Assert.AreEqual(new Direction3(1, 2, 3), res);
        }

        [TestMethod()]
        public void DirectionEqualsTest()
        {
            var res = Direction3.Zero == new Direction3(0, 0, 0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void DirectionDistinctTest()
        {
            var res = Direction3.Zero != Direction3.Up;

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void DirectionAddTest()
        {
            var res = new Direction3(1, 2, 3) + new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(2, 4, 6), res);

            res = new Direction3(1, 2, 3) + 1;
            Assert.AreEqual(new Direction3(2, 3, 4), res);

            res = 1 + new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(2, 3, 4), res);
        }
        [TestMethod()]
        public void DirectionSubstracTest()
        {
            var res = new Direction3(1, 2, 3) - new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(0, 0, 0), res);

            res = new Direction3(1, 2, 3) - 1;
            Assert.AreEqual(new Direction3(0, 1, 2), res);

            res = 1 - new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(0, -1, -2), res);
        }
        [TestMethod()]
        public void DirectionMultiplyTest()
        {
            var res = new Direction3(1, 2, 3) * new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(1, 4, 9), res);

            res = new Direction3(1, 2, 3) * 2;
            Assert.AreEqual(new Direction3(2, 4, 6), res);

            res = 2 * new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(2, 4, 6), res);
        }
        [TestMethod()]
        public void DirectionDivideTest()
        {
            var res = new Direction3(1, 2, 3) / new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(1, 1, 1), res);

            res = new Direction3(1, 2, 3) / 2;
            Assert.AreEqual(new Direction3(0.5f, 1, 1.5f), res);

            res = 2 / new Direction3(1, 2, 3);
            Assert.AreEqual(new Direction3(2, 1, 2f / 3f), res);
        }
        [TestMethod()]
        public void DirectionPositiveTest()
        {
            var res = +new Direction3(1, 2, 3);

            Assert.AreEqual(new Direction3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionNegateTest()
        {
            var res = -new Direction3(1, 2, 3);

            Assert.AreEqual(new Direction3(-1, -2, -3), res);
        }

        [TestMethod()]
        public void DirectionToVector3Test()
        {
            Vector3 res1 = new Direction3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res1);

            Direction3 res2 = new Vector3(1, 2, 3);
            Assert.AreEqual(new Direction3(1, 2, 3), res2);
        }
        [TestMethod()]
        public void DirectionToStringTest()
        {
            string res1 = new Direction3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(directionString, res1);

            Direction3 res2 = directionString;
            Assert.AreEqual(new Direction3(0.1f, 0.2f, 0.3f), res2);

            res2 = "Up";
            Assert.AreEqual(Direction3.Up, res2);
            res2 = "Down";
            Assert.AreEqual(Direction3.Down, res2);
            res2 = "Left";
            Assert.AreEqual(Direction3.Left, res2);
            res2 = "Right";
            Assert.AreEqual(Direction3.Right, res2);
            res2 = "Forward";
            Assert.AreEqual(Direction3.ForwardLH, res2);
            res2 = "Backward";
            Assert.AreEqual(Direction3.BackwardLH, res2);
            res2 = "ForwardLH";
            Assert.AreEqual(Direction3.ForwardLH, res2);
            res2 = "BackwardLH";
            Assert.AreEqual(Direction3.BackwardLH, res2);
            res2 = "ForwardRH";
            Assert.AreEqual(Direction3.ForwardRH, res2);
            res2 = "BackwardRH";
            Assert.AreEqual(Direction3.BackwardRH, res2);

            res2 = "Nothing parseable";
            Assert.AreEqual(Direction3.ForwardLH, res2);
        }
    }
}