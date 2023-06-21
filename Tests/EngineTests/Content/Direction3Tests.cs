using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Direction3Tests
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
        public void DirectionZeroTest()
        {
            var res = Direction3.Zero;

            Assert.AreEqual(res, new Direction3(0, 0, 0));
        }
        [TestMethod()]
        public void DirectionUpTest()
        {
            var res = Direction3.Up;

            Assert.AreEqual(res, new Direction3(0, 1, 0));
        }
        [TestMethod()]
        public void DirectionDownTest()
        {
            var res = Direction3.Down;

            Assert.AreEqual(res, new Direction3(0, -1, 0));
        }
        [TestMethod()]
        public void DirectionLeftTest()
        {
            var res = Direction3.Left;

            Assert.AreEqual(res, new Direction3(-1, 0, 0));
        }
        [TestMethod()]
        public void DirectionRightTest()
        {
            var res = Direction3.Right;

            Assert.AreEqual(res, new Direction3(1, 0, 0));
        }
        [TestMethod()]
        public void DirectionForwardLHTest()
        {
            var res = Direction3.ForwardLH;

            Assert.AreEqual(res, new Direction3(0, 0, 1));
        }
        [TestMethod()]
        public void DirectionBackwardLHTest()
        {
            var res = Direction3.BackwardLH;

            Assert.AreEqual(res, new Direction3(0, 0, -1));
        }
        [TestMethod()]
        public void DirectionForwardRHTest()
        {
            var res = Direction3.ForwardRH;

            Assert.AreEqual(res, new Direction3(0, 0, -1));
        }
        [TestMethod()]
        public void DirectionBackwardRHTest()
        {
            var res = Direction3.BackwardRH;

            Assert.AreEqual(res, new Direction3(0, 0, 1));
        }

        [TestMethod()]
        public void DirectionValueTest()
        {
            var res = new Direction3(2);

            Assert.AreEqual(res, new Direction3(2, 2, 2));
        }
        [TestMethod()]
        public void DirectionArrayTest()
        {
            var res = new Direction3(new float[] { 1, 2, 3 });

            Assert.AreEqual(res, new Direction3(1, 2, 3));
        }
        [TestMethod()]
        public void DirectionBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Direction3(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Direction3(new float[] { }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Direction3(new float[] { 1, 2, 3, 4 }));
        }
        [TestMethod()]
        public void DirectionComponentsTest()
        {
            var res = new Direction3(1, 2, 3);

            Assert.AreEqual(res, new Direction3(1, 2, 3));
        }
        [TestMethod()]
        public void DirectionSettesTest()
        {
            var res = new Direction3();
            res.X = 1;
            res.Y = 2;
            res.Z = 3;

            Assert.AreEqual(res, new Direction3(1, 2, 3));
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
            Assert.AreEqual(res, new Direction3(2, 4, 6));

            res = new Direction3(1, 2, 3) + 1;
            Assert.AreEqual(res, new Direction3(2, 3, 4));

            res = 1 + new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(2, 3, 4));
        }
        [TestMethod()]
        public void DirectionSubstracTest()
        {
            var res = new Direction3(1, 2, 3) - new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(0, 0, 0));

            res = new Direction3(1, 2, 3) - 1;
            Assert.AreEqual(res, new Direction3(0, 1, 2));

            res = 1 - new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(0, -1, -2));
        }
        [TestMethod()]
        public void DirectionMultiplyTest()
        {
            var res = new Direction3(1, 2, 3) * new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(1, 4, 9));

            res = new Direction3(1, 2, 3) * 2;
            Assert.AreEqual(res, new Direction3(2, 4, 6));

            res = 2 * new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(2, 4, 6));
        }
        [TestMethod()]
        public void DirectionDivideTest()
        {
            var res = new Direction3(1, 2, 3) / new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(1, 1, 1));

            res = new Direction3(1, 2, 3) / 2;
            Assert.AreEqual(res, new Direction3(0.5f, 1, 1.5f));

            res = 2 / new Direction3(1, 2, 3);
            Assert.AreEqual(res, new Direction3(2, 1, 2f / 3f));
        }
        [TestMethod()]
        public void DirectionPositiveTest()
        {
            var res = +new Direction3(1, 2, 3);

            Assert.AreEqual(res, new Direction3(1, 2, 3));
        }
        [TestMethod()]
        public void DirectionNegateTest()
        {
            var res = -new Direction3(1, 2, 3);

            Assert.AreEqual(res, new Direction3(-1, -2, -3));
        }

        [TestMethod()]
        public void DirectionToVector3Test()
        {
            Vector3 res1 = new Direction3(1, 2, 3);
            Assert.AreEqual(res1, new Vector3(1, 2, 3));

            Direction3 res2 = new Vector3(1, 2, 3);
            Assert.AreEqual(res2, new Direction3(1, 2, 3));
        }
        [TestMethod()]
        public void DirectionToStringTest()
        {
            string res1 = new Direction3(1, 2, 3);
            Assert.AreEqual(res1, "1 2 3");

            Direction3 res2 = "1 2 3";
            Assert.AreEqual(res2, new Direction3(1, 2, 3));

            res2 = "Up";
            Assert.AreEqual(res2, Direction3.Up);
            res2 = "Down";
            Assert.AreEqual(res2, Direction3.Down);
            res2 = "Left";
            Assert.AreEqual(res2, Direction3.Left);
            res2 = "Right";
            Assert.AreEqual(res2, Direction3.Right);
            res2 = "Forward";
            Assert.AreEqual(res2, Direction3.ForwardLH);
            res2 = "Backward";
            Assert.AreEqual(res2, Direction3.BackwardLH);
            res2 = "ForwardLH";
            Assert.AreEqual(res2, Direction3.ForwardLH);
            res2 = "BackwardLH";
            Assert.AreEqual(res2, Direction3.BackwardLH);
            res2 = "ForwardRH";
            Assert.AreEqual(res2, Direction3.ForwardRH);
            res2 = "BackwardRH";
            Assert.AreEqual(res2, Direction3.BackwardRH);

            res2 = "Nothing parseable";
            Assert.AreEqual(res2, Direction3.ForwardLH);
        }
    }
}