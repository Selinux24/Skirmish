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
    public class RotationTests
    {
        public TestContext TestContext { get; set; }

        static Quaternion rotarion0 = Quaternion.RotationAxis(Vector3.Up, Helper.DegreesToRadians(0));
        static Quaternion rotarion33 = Quaternion.RotationAxis(Vector3.Up, Helper.DegreesToRadians(33));
        static Quaternion rotarion90 = Quaternion.RotationAxis(Vector3.Up, Helper.DegreesToRadians(90));

        static readonly string rotationString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void RotationZeroTest()
        {
            var res = Quaternion.Zero;

            Assert.AreEqual(Quaternion.Zero, res);
        }
        [TestMethod()]
        public void RotationOneTest()
        {
            var res = Quaternion.One;

            Assert.AreEqual(new Quaternion(1, 1, 1, 1), res);
        }
        [TestMethod()]
        public void RotationIdentityTest()
        {
            var res = Quaternion.Identity;

            Assert.AreEqual(new Quaternion(0, 0, 0, 1), res);
        }
        [TestMethod()]
        public void RotationValueTest()
        {
            var res = new Quaternion(2, 2, 2, 2);

            Assert.AreEqual(new Quaternion(2, 2, 2, 2), res);
        }
        [TestMethod()]
        public void RotationArrayTest()
        {
            var res = new Quaternion([1, 2, 3, 4]);

            Assert.AreEqual(new Quaternion(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void RotationBadArrayTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Quaternion(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Quaternion([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Quaternion([1, 2, 3]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Quaternion([1, 2, 3, 4, 5]));
        }
        [TestMethod()]
        public void RotationComponentsTest()
        {
            var res = new Quaternion(1, 2, 3, 4);

            Assert.AreEqual(new Quaternion(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void RotationSettesTest()
        {
            var res = new Quaternion
            {
                X = 1,
                Y = 2,
                Z = 3,
                W = 4
            };

            Assert.AreEqual(new Quaternion(1, 2, 3, 4), res);
        }

        [TestMethod()]
        public void RotationEqualsTest()
        {
            var res = Quaternion.Zero == new Quaternion(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void RotationDistinctTest()
        {
            var res = Quaternion.Zero != new Quaternion(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void RotationAddTest()
        {
            var res = new Quaternion(1, 2, 3, 4) + new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(new Quaternion(2, 4, 6, 8), res);
        }
        [TestMethod()]
        public void RotationSubstracTest()
        {
            var res = new Quaternion(1, 2, 3, 4) - new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(new Quaternion(0, 0, 0, 0), res);
        }
        [TestMethod()]
        public void RotationMultiplyTest()
        {
            Quaternion res = new Quaternion(1, 2, 3, 4) * new Quaternion(1, 2, 3, 4);
            Quaternion expected = new Quaternion(1, 2, 3, 4) * new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(expected, res);

            res = new Quaternion(1, 2, 3, 4) * 2;
            expected = new Quaternion(1, 2, 3, 4) * 2;
            Assert.AreEqual(expected, res);

            res = new Quaternion(1, 2, 3, 4) * 2;
            expected = new Quaternion(1, 2, 3, 4) * 2;
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void RotationNegateTest()
        {
            var res = -new Quaternion(1, 2, 3, 4);

            Assert.AreEqual(new Quaternion(-1, -2, -3, -4), res);
        }

        [TestMethod()]
        public void RotationToQuaternionTest()
        {
            Quaternion res1 = new(1, 2, 3, 4);
            Assert.AreEqual(new Quaternion(1, 2, 3, 4), res1);

            Quaternion res2 = new(1, 2, 3, 4);
            Assert.AreEqual(new Quaternion(1, 2, 3, 4), res2);
        }
        [TestMethod()]
        public void RotationToStringTest()
        {
            string res1 = ContentHelper.WriteRotation(new Quaternion(0.1f, 0.2f, 0.3f, 0.4f));
            Assert.AreEqual(rotationString, res1);

            Quaternion? res2 = ContentHelper.ReadRotation(rotationString);
            Assert.AreEqual(new Quaternion(0.1f, 0.2f, 0.3f, 0.4f), res2);

            res2 = ContentHelper.ReadRotation("Identity");
            Assert.AreEqual(Quaternion.Identity, res2);
            res2 = ContentHelper.ReadRotation("Rot0");
            Assert.AreEqual(rotarion0, res2);
            res2 = ContentHelper.ReadRotation("Rot33");
            Assert.AreEqual(rotarion33, res2);
            res2 = ContentHelper.ReadRotation("Rot90");
            Assert.AreEqual(rotarion90, res2);

            res2 = ContentHelper.ReadRotation("Nothing parseable");
            Assert.AreEqual(Quaternion.Identity, res2);
        }
    }
}