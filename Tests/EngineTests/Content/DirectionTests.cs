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
    public class DirectionTests
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
            var res = Vector3.Zero;

            Assert.AreEqual(new Vector3(0, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionUpTest()
        {
            var res = Vector3.Up;

            Assert.AreEqual(new Vector3(0, 1, 0), res);
        }
        [TestMethod()]
        public void DirectionDownTest()
        {
            var res = Vector3.Down;

            Assert.AreEqual(new Vector3(0, -1, 0), res);
        }
        [TestMethod()]
        public void DirectionLeftTest()
        {
            var res = Vector3.Left;

            Assert.AreEqual(new Vector3(-1, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionRightTest()
        {
            var res = Vector3.Right;

            Assert.AreEqual(new Vector3(1, 0, 0), res);
        }
        [TestMethod()]
        public void DirectionForwardLHTest()
        {
            var res = Vector3.ForwardLH;

            Assert.AreEqual(new Vector3(0, 0, 1), res);
        }
        [TestMethod()]
        public void DirectionBackwardLHTest()
        {
            var res = Vector3.BackwardLH;

            Assert.AreEqual(new Vector3(0, 0, -1), res);
        }
        [TestMethod()]
        public void DirectionForwardRHTest()
        {
            var res = Vector3.ForwardRH;

            Assert.AreEqual(new Vector3(0, 0, -1), res);
        }
        [TestMethod()]
        public void DirectionBackwardRHTest()
        {
            var res = Vector3.BackwardRH;

            Assert.AreEqual(new Vector3(0, 0, 1), res);
        }

        [TestMethod()]
        public void DirectionValueTest()
        {
            var res = new Vector3(2);

            Assert.AreEqual(new Vector3(2, 2, 2), res);
        }
        [TestMethod()]
        public void DirectionArrayTest()
        {
            var res = new Vector3([1, 2, 3]);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Vector3(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3([1, 2, 3, 4]));
        }
        [TestMethod()]
        public void DirectionComponentsTest()
        {
            var res = new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionSettesTest()
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
        public void DirectionEqualsTest()
        {
            var res = Vector3.Zero == new Vector3(0, 0, 0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void DirectionDistinctTest()
        {
            var res = Vector3.Zero != Vector3.Up;

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void DirectionAddTest()
        {
            var res = new Vector3(1, 2, 3) + new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 4, 6), res);

            res = new Vector3(1, 2, 3) + 1;
            Assert.AreEqual(new Vector3(2, 3, 4), res);

            res = 1 + new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 3, 4), res);
        }
        [TestMethod()]
        public void DirectionSubstracTest()
        {
            var res = new Vector3(1, 2, 3) - new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(0, 0, 0), res);

            res = new Vector3(1, 2, 3) - 1;
            Assert.AreEqual(new Vector3(0, 1, 2), res);

            res = 1 - new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(0, -1, -2), res);
        }
        [TestMethod()]
        public void DirectionMultiplyTest()
        {
            var res = new Vector3(1, 2, 3) * new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 4, 9), res);

            res = new Vector3(1, 2, 3) * 2;
            Assert.AreEqual(new Vector3(2, 4, 6), res);

            res = 2 * new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 4, 6), res);
        }
        [TestMethod()]
        public void DirectionDivideTest()
        {
            var res = new Vector3(1, 2, 3) / new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 1, 1), res);

            res = new Vector3(1, 2, 3) / 2;
            Assert.AreEqual(new Vector3(0.5f, 1, 1.5f), res);

            res = 2 / new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(2, 1, 2f / 3f), res);
        }
        [TestMethod()]
        public void DirectionPositiveTest()
        {
            var res = +new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(1, 2, 3), res);
        }
        [TestMethod()]
        public void DirectionNegateTest()
        {
            var res = -new Vector3(1, 2, 3);

            Assert.AreEqual(new Vector3(-1, -2, -3), res);
        }

        [TestMethod()]
        public void DirectionToVector3Test()
        {
            Vector3 res1 = new(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res1);

            Vector3 res2 = new(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), res2);
        }
        [TestMethod()]
        public void DirectionToStringTest()
        {
            string res1 = ContentHelper.WriteDirection(new Vector3(0.1f, 0.2f, 0.3f));
            Assert.AreEqual(directionString, res1);

            Vector3? res2 = ContentHelper.ReadDirection(directionString);
            Assert.AreEqual(new Vector3(0.1f, 0.2f, 0.3f), res2);

            res2 = ContentHelper.ReadDirection("Up");
            Assert.AreEqual(Vector3.Up, res2);
            res2 = ContentHelper.ReadDirection("Down");
            Assert.AreEqual(Vector3.Down, res2);
            res2 = ContentHelper.ReadDirection("Left");
            Assert.AreEqual(Vector3.Left, res2);
            res2 = ContentHelper.ReadDirection("Right");
            Assert.AreEqual(Vector3.Right, res2);
            res2 = ContentHelper.ReadDirection("Forward");
            Assert.AreEqual(Vector3.ForwardLH, res2);
            res2 = ContentHelper.ReadDirection("Backward");
            Assert.AreEqual(Vector3.BackwardLH, res2);
            res2 = ContentHelper.ReadDirection("ForwardLH");
            Assert.AreEqual(Vector3.ForwardLH, res2);
            res2 = ContentHelper.ReadDirection("BackwardLH");
            Assert.AreEqual(Vector3.BackwardLH, res2);
            res2 = ContentHelper.ReadDirection("ForwardRH");
            Assert.AreEqual(Vector3.ForwardRH, res2);
            res2 = ContentHelper.ReadDirection("BackwardRH");
            Assert.AreEqual(Vector3.BackwardRH, res2);

            res2 = ContentHelper.ReadDirection("Nothing parseable");
            Assert.AreEqual(Vector3.ForwardLH, res2);
        }
    }
}