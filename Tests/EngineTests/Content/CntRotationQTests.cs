using Engine;
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
    public class CntRotationQTests
    {
        public TestContext TestContext { get; set; }

        static CntRotationQ rotarion0 = CntRotationQ.RotationAxis(CntDirection3.Up, Helper.DegreesToRadians(0));
        static CntRotationQ rotarion33 = CntRotationQ.RotationAxis(CntDirection3.Up, Helper.DegreesToRadians(33));
        static CntRotationQ rotarion90 = CntRotationQ.RotationAxis(CntDirection3.Up, Helper.DegreesToRadians(90));

        static readonly string rotationString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void RotationZeroTest()
        {
            var res = CntRotationQ.Zero;

            Assert.AreEqual(new CntRotationQ(0, 0, 0, 0), res);
        }
        [TestMethod()]
        public void RotationOneTest()
        {
            var res = CntRotationQ.One;

            Assert.AreEqual(new CntRotationQ(1, 1, 1, 1), res);
        }
        [TestMethod()]
        public void RotationIdentityTest()
        {
            var res = CntRotationQ.Identity;

            Assert.AreEqual(new CntRotationQ(0, 0, 0, 1), res);
        }
        [TestMethod()]
        public void RotationValueTest()
        {
            var res = new CntRotationQ(2);

            Assert.AreEqual(new CntRotationQ(2, 2, 2, 2), res);
        }
        [TestMethod()]
        public void RotationArrayTest()
        {
            var res = new CntRotationQ([1, 2, 3, 4]);

            Assert.AreEqual(new CntRotationQ(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void RotationBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new CntRotationQ(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntRotationQ([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntRotationQ([1, 2, 3]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntRotationQ([1, 2, 3, 4, 5]));
        }
        [TestMethod()]
        public void RotationComponentsTest()
        {
            var res = new CntRotationQ(1, 2, 3, 4);

            Assert.AreEqual(new CntRotationQ(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void RotationSettesTest()
        {
            var res = new CntRotationQ
            {
                X = 1,
                Y = 2,
                Z = 3,
                W = 4
            };

            Assert.AreEqual(new CntRotationQ(1, 2, 3, 4), res);
        }

        [TestMethod()]
        public void RotationEqualsTest()
        {
            var res = CntRotationQ.Zero == new CntRotationQ(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void RotationDistinctTest()
        {
            var res = CntRotationQ.Zero != new CntRotationQ(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void RotationAddTest()
        {
            var res = new CntRotationQ(1, 2, 3, 4) + new CntRotationQ(1, 2, 3, 4);
            Assert.AreEqual(new CntRotationQ(2, 4, 6, 8), res);
        }
        [TestMethod()]
        public void RotationSubstracTest()
        {
            var res = new CntRotationQ(1, 2, 3, 4) - new CntRotationQ(1, 2, 3, 4);
            Assert.AreEqual(new CntRotationQ(0, 0, 0, 0), res);
        }
        [TestMethod()]
        public void RotationMultiplyTest()
        {
            CntRotationQ res = new CntRotationQ(1, 2, 3, 4) * new CntRotationQ(1, 2, 3, 4);
            CntRotationQ expected = new Quaternion(1, 2, 3, 4) * new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(expected, res);

            res = new CntRotationQ(1, 2, 3, 4) * 2;
            expected = new Quaternion(1, 2, 3, 4) * 2;
            Assert.AreEqual(expected, res);

            res = new CntRotationQ(1, 2, 3, 4) * 2;
            expected = new Quaternion(1, 2, 3, 4) * 2;
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void RotationNegateTest()
        {
            var res = -new CntRotationQ(1, 2, 3, 4);

            Assert.AreEqual(new CntRotationQ(-1, -2, -3, -4), res);
        }

        [TestMethod()]
        public void RotationToQuaternionTest()
        {
            Quaternion res1 = new CntRotationQ(1, 2, 3, 4);
            Assert.AreEqual(new Quaternion(1, 2, 3, 4), res1);

            CntRotationQ res2 = new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(new CntRotationQ(1, 2, 3, 4), res2);
        }
        [TestMethod()]
        public void RotationToStringTest()
        {
            string res1 = new CntRotationQ(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(rotationString, res1);

            CntRotationQ res2 = rotationString;
            Assert.AreEqual(new CntRotationQ(0.1f, 0.2f, 0.3f, 0.4f), res2);

            res2 = "Identity";
            Assert.AreEqual(CntRotationQ.Identity, res2);
            res2 = "Rot0";
            Assert.AreEqual(rotarion0, res2);
            res2 = "Rot33";
            Assert.AreEqual(rotarion33, res2);
            res2 = "Rot90";
            Assert.AreEqual(rotarion90, res2);

            res2 = "Nothing parseable";
            Assert.AreEqual(CntRotationQ.Identity, res2);
        }
    }
}