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
    public class StringFormatingTests
    {
        public TestContext TestContext { get; set; }

        static readonly float scale1by5 = 1f / 5f;
        static readonly float scale1by4 = 1f / 4f;
        static readonly float scale1by3 = 1f / 3f;
        static readonly float scale1by2 = 1f / 2f;
        static Quaternion rotarion0 = Quaternion.RotationAxis(Vector3.Up, MathUtils.DegreesToRadians(0));
        static Quaternion rotarion33 = Quaternion.RotationAxis(Vector3.Up, MathUtils.DegreesToRadians(33));
        static Quaternion rotarion90 = Quaternion.RotationAxis(Vector3.Up, MathUtils.DegreesToRadians(90));

        static readonly string position3String = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);
        static readonly string position4String = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);
        static readonly string scale3String = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);
        static readonly string rotationQString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);
        static readonly string direction3String = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);
        static readonly string colorString3 = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);
        static readonly string colorString4 = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void Position3ToStringTest()
        {
            string res1 = ContentHelper.WritePosition3(new Vector3(0.1f, 0.2f, 0.3f));
            Assert.AreEqual(position3String, res1);

            Vector3? res2 = ContentHelper.ReadPosition3(position3String);
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

        [TestMethod()]
        public void Position4ToStringTest()
        {
            string res1 = ContentHelper.WritePosition4(new Vector4(0.1f, 0.2f, 0.3f, 0.4f));
            Assert.AreEqual(position4String, res1);

            Vector4? res2 = ContentHelper.ReadPosition4(position4String);
            Assert.AreEqual(new Vector4(0.1f, 0.2f, 0.3f, 0.4f), res2);

            res2 = ContentHelper.ReadPosition4("Zero");
            Assert.AreEqual(new Vector4(0), res2);
            res2 = ContentHelper.ReadPosition4("Max");
            Assert.AreEqual(new Vector4(float.MaxValue), res2);
            res2 = ContentHelper.ReadPosition4("Min");
            Assert.AreEqual(new Vector4(float.MinValue), res2);

            res2 = ContentHelper.ReadPosition4("Nothing parseable");
            Assert.AreEqual(new Vector4(0), res2);
        }

        [TestMethod()]
        public void Direction3ToStringTest()
        {
            string res1 = ContentHelper.WriteDirection(new Vector3(0.1f, 0.2f, 0.3f));
            Assert.AreEqual(direction3String, res1);

            Vector3? res2 = ContentHelper.ReadDirection(direction3String);
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

        [TestMethod()]
        public void Scale3ToStringTest()
        {
            string res1 = ContentHelper.WriteScale(new Vector3(0.1f, 0.2f, 0.3f));
            Assert.AreEqual(scale3String, res1);

            Vector3? res2 = ContentHelper.ReadScale(scale3String);
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

        [TestMethod()]
        public void QuaternionToStringTest()
        {
            string res1 = ContentHelper.WriteRotation(new Quaternion(0.1f, 0.2f, 0.3f, 0.4f));
            Assert.AreEqual(rotationQString, res1);

            Quaternion? res2 = ContentHelper.ReadRotation(rotationQString);
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

        [TestMethod()]
        public void ColorToStringTest()
        {
            string res1 = ContentHelper.WriteColor(new Color4(0.1f, 0.2f, 0.3f, 0.4f));
            Assert.AreEqual(colorString4, res1);

            Color4? res2 = ContentHelper.ReadColor(colorString3);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 1f), res2);

            Color4? res3 = ContentHelper.ReadColor(colorString4);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 0.4f), res3);

            Color4 res4 = ContentHelper.ReadColor("Transparent");
            Assert.AreEqual(Color4.Transparent, res4);
            res4 = ContentHelper.ReadColor("White");
            Assert.AreEqual(Color4.White, res4);
            res4 = ContentHelper.ReadColor("Black");
            Assert.AreEqual(Color4.Black, res4);

            res4 = ContentHelper.ReadColor("Nothing parseable");
            Assert.AreEqual(Color4.Black, res4);
        }
    }
}