using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Scale3Tests
    {
        static TestContext _testContext;

        static readonly float scale1by5 = 1f / 5f;
        static readonly float scale1by4 = 1f / 4f;
        static readonly float scale1by3 = 1f / 3f;
        static readonly float scale1by2 = 1f / 2f;

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
        public void ScaleZeroTest()
        {
            var res = Scale3.Zero;

            Assert.AreEqual(res, new Scale3(0, 0, 0));
        }
        [TestMethod()]
        public void ScaleOneTest()
        {
            var res = Scale3.One;

            Assert.AreEqual(res, new Scale3(1, 1, 1));
        }
        [TestMethod()]
        public void ScaleValueTest()
        {
            var res = new Scale3(2);

            Assert.AreEqual(res, new Scale3(2, 2, 2));
        }
        [TestMethod()]
        public void ScaleArrayTest()
        {
            var res = new Scale3(new float[] { 1, 2, 3 });

            Assert.AreEqual(res, new Scale3(1, 2, 3));
        }
        [TestMethod()]
        public void ScaleBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Scale3(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Scale3(new float[] { }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Scale3(new float[] { 1, 2, 3, 4 }));
        }
        [TestMethod()]
        public void ScaleComponentsTest()
        {
            var res = new Scale3(1, 2, 3);

            Assert.AreEqual(res, new Scale3(1, 2, 3));
        }
        [TestMethod()]
        public void ScaleSettesTest()
        {
            var res = new Scale3();
            res.X = 1;
            res.Y = 2;
            res.Z = 3;

            Assert.AreEqual(res, new Scale3(1, 2, 3));
        }

        [TestMethod()]
        public void ScaleEqualsTest()
        {
            var res = Scale3.Zero == new Scale3(0, 0, 0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void ScaleDistinctTest()
        {
            var res = Scale3.Zero != Scale3.One;

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void ScaleAddTest()
        {
            var res = new Scale3(1, 2, 3) + new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(2, 4, 6));

            res = new Scale3(1, 2, 3) + 1;
            Assert.AreEqual(res, new Scale3(2, 3, 4));

            res = 1 + new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(2, 3, 4));
        }
        [TestMethod()]
        public void ScaleSubstracTest()
        {
            var res = new Scale3(1, 2, 3) - new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(0, 0, 0));

            res = new Scale3(1, 2, 3) - 1;
            Assert.AreEqual(res, new Scale3(0, 1, 2));

            res = 1 - new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(0, -1, -2));
        }
        [TestMethod()]
        public void ScaleMultiplyTest()
        {
            var res = new Scale3(1, 2, 3) * new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(1, 4, 9));

            res = new Scale3(1, 2, 3) * 2;
            Assert.AreEqual(res, new Scale3(2, 4, 6));

            res = 2 * new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(2, 4, 6));
        }
        [TestMethod()]
        public void ScaleDivideTest()
        {
            var res = new Scale3(1, 2, 3) / new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(1, 1, 1));

            res = new Scale3(1, 2, 3) / 2;
            Assert.AreEqual(res, new Scale3(0.5f, 1, 1.5f));

            res = 2 / new Scale3(1, 2, 3);
            Assert.AreEqual(res, new Scale3(2, 1, 2f / 3f));
        }
        [TestMethod()]
        public void ScalePositiveTest()
        {
            var res = +new Scale3(1, 2, 3);

            Assert.AreEqual(res, new Scale3(1, 2, 3));
        }
        [TestMethod()]
        public void ScaleNegateTest()
        {
            var res = -new Scale3(1, 2, 3);

            Assert.AreEqual(res, new Scale3(-1, -2, -3));
        }

        [TestMethod()]
        public void ScaleToVector3Test()
        {
            Vector3 res1 = new Scale3(1, 2, 3);
            Assert.AreEqual(res1, new Vector3(1, 2, 3));

            Scale3 res2 = new Vector3(1, 2, 3);
            Assert.AreEqual(res2, new Scale3(1, 2, 3));
        }
        [TestMethod()]
        public void ScaleToStringTest()
        {
            string res1 = new Scale3(1, 2, 3);
            Assert.AreEqual(res1, "1 2 3");

            Scale3 res2 = "1 2 3";
            Assert.AreEqual(res2, new Scale3(1, 2, 3));

            res2 = "One";
            Assert.AreEqual(res2, new Scale3(1));
            res2 = "Two";
            Assert.AreEqual(res2, new Scale3(2));

            res2 = "1/5";
            Assert.AreEqual(res2, new Scale3(scale1by5));
            res2 = "1/4";
            Assert.AreEqual(res2, new Scale3(scale1by4));
            res2 = "1/3";
            Assert.AreEqual(res2, new Scale3(scale1by3));
            res2 = "1/2";
            Assert.AreEqual(res2, new Scale3(scale1by2));

            res2 = "Nothing parseable";
            Assert.AreEqual(res2, new Scale3(1));
        }
    }
}