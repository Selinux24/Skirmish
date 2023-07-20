using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ColorRgbaTests
    {
        static TestContext _testContext;

        static readonly string colorString3 = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);
        static readonly string colorString4 = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

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
        public void ColorTransparentTest()
        {
            var res = ColorRgba.Transparent;

            Assert.AreEqual(new ColorRgba(0, 0, 0, 0), res);
        }
        [TestMethod()]
        public void ColorWhiteTest()
        {
            var res = ColorRgba.White;

            Assert.AreEqual(new ColorRgba(0, 0, 0, 1), res);
        }
        [TestMethod()]
        public void ColorBlackTest()
        {
            var res = ColorRgba.Black;

            Assert.AreEqual(new ColorRgba(1, 1, 1, 1), res);
        }
        [TestMethod()]
        public void ColorValueTest()
        {
            var res = new ColorRgba(2);

            Assert.AreEqual(new ColorRgba(2, 2, 2, 2), res);
        }
        [TestMethod()]
        public void ColorArrayTest()
        {
            var res = new ColorRgba(new float[] { 1, 2, 3, 4 });

            Assert.AreEqual(new ColorRgba(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void ColorBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ColorRgba(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ColorRgba(Array.Empty<float>()));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ColorRgba(new float[] { 1, 2, 3 }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ColorRgba(new float[] { 1, 2, 3, 4, 5 }));
        }
        [TestMethod()]
        public void ColorComponentsTest()
        {
            var res = new ColorRgba(1, 2, 3, 4);

            Assert.AreEqual(new ColorRgba(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void ColorSettesTest()
        {
            var res = new ColorRgba
            {
                R = 1,
                G = 2,
                B = 3,
                A = 4
            };

            Assert.AreEqual(new ColorRgba(1, 2, 3, 4), res);
        }

        [TestMethod()]
        public void ColorEqualsTest()
        {
            var res = ColorRgba.Transparent == new ColorRgba(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void ColorDistinctTest()
        {
            var res = ColorRgba.Transparent != new ColorRgba(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void ColorToColorTest()
        {
            Color res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Color expected1 = new(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(expected1, res1);

            ColorRgba res2 = Color.Red;
            ColorRgba expected2 = new(1f, 0f, 0f, 1f);
            Assert.AreEqual(expected2, res2);
        }
        [TestMethod()]
        public void ColorToColor3Test()
        {
            Color3 res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color3(0.1f, 0.2f, 0.3f), res1);

            ColorRgba res2 = new Color3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(new ColorRgba(0.1f, 0.2f, 0.3f, 1f), res2);
        }
        [TestMethod()]
        public void ColorToColor4Test()
        {
            Color4 res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 0.4f), res1);

            ColorRgba res2 = new Color4(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f), res2);
        }
        [TestMethod()]
        public void ColorToStringTest()
        {
            string res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(colorString4, res1);

            ColorRgba res2 = colorString3;
            Assert.AreEqual(new ColorRgba(0.1f, 0.2f, 0.3f, 1f), res2);

            ColorRgba res3 = colorString4;
            Assert.AreEqual(new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f), res3);

            ColorRgba res4 = "Transparent";
            Assert.AreEqual(ColorRgba.Transparent, res4);
            res4 = "White";
            Assert.AreEqual(ColorRgba.White, res4);
            res4 = "Black";
            Assert.AreEqual(ColorRgba.Black, res4);

            res4 = "Nothing parseable";
            Assert.AreEqual(ColorRgba.Black, res4);
        }
    }
}