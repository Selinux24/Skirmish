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

            Assert.AreEqual(res, new ColorRgba(0, 0, 0, 0));
        }
        [TestMethod()]
        public void ColorWhiteTest()
        {
            var res = ColorRgba.White;

            Assert.AreEqual(res, new ColorRgba(0, 0, 0, 1));
        }
        [TestMethod()]
        public void ColorBlackTest()
        {
            var res = ColorRgba.Black;

            Assert.AreEqual(res, new ColorRgba(1, 1, 1, 1));
        }
        [TestMethod()]
        public void ColorValueTest()
        {
            var res = new ColorRgba(2);

            Assert.AreEqual(res, new ColorRgba(2, 2, 2, 2));
        }
        [TestMethod()]
        public void ColorArrayTest()
        {
            var res = new ColorRgba(new float[] { 1, 2, 3, 4 });

            Assert.AreEqual(res, new ColorRgba(1, 2, 3, 4));
        }
        [TestMethod()]
        public void ColorBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ColorRgba(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ColorRgba(new float[] { }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ColorRgba(new float[] { 1, 2, 3 }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ColorRgba(new float[] { 1, 2, 3, 4, 5 }));
        }
        [TestMethod()]
        public void ColorComponentsTest()
        {
            var res = new ColorRgba(1, 2, 3, 4);

            Assert.AreEqual(res, new ColorRgba(1, 2, 3, 4));
        }
        [TestMethod()]
        public void ColorSettesTest()
        {
            var res = new ColorRgba();
            res.R = 1;
            res.G = 2;
            res.B = 3;
            res.A = 4;

            Assert.AreEqual(res, new ColorRgba(1, 2, 3, 4));
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
            Color expected1 = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(expected1, res1);

            ColorRgba res2 = Color.Red;
            ColorRgba expected2 = new ColorRgba(1f, 0f, 0f, 1f);
            Assert.AreEqual(expected2, res2);
        }
        [TestMethod()]
        public void ColorToColor3Test()
        {
            Color3 res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(res1, new Color3(0.1f, 0.2f, 0.3f));

            ColorRgba res2 = new Color3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(res2, new ColorRgba(0.1f, 0.2f, 0.3f, 1f));
        }
        [TestMethod()]
        public void ColorToColor4Test()
        {
            Color4 res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(res1, new Color4(0.1f, 0.2f, 0.3f, 0.4f));

            ColorRgba res2 = new Color4(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(res2, new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f));
        }
        [TestMethod()]
        public void ColorToStringTest()
        {
            string res1 = new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(res1, colorString4);

            ColorRgba res2 = colorString3;
            Assert.AreEqual(res2, new ColorRgba(0.1f, 0.2f, 0.3f, 1f));

            ColorRgba res3 = colorString4;
            Assert.AreEqual(res3, new ColorRgba(0.1f, 0.2f, 0.3f, 0.4f));

            ColorRgba res4 = "Transparent";
            Assert.AreEqual(res4, ColorRgba.Transparent);
            res4 = "White";
            Assert.AreEqual(res4, ColorRgba.White);
            res4 = "Black";
            Assert.AreEqual(res4, ColorRgba.Black);

            res4 = "Nothing parseable";
            Assert.AreEqual(res4, ColorRgba.Black);
        }
    }
}