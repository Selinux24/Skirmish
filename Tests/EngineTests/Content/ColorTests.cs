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
    public class ColorTests
    {
        public TestContext TestContext { get; set; }

        static readonly string colorString3 = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 0.1f, 0.2f, 0.3f);
        static readonly string colorString4 = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 0.1f, 0.2f, 0.3f, 0.4f);

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void ColorTransparentTest()
        {
            var res = Color4.Transparent;

            Assert.AreEqual(new Color4(0f, 0f, 0f, 0f), res);
        }
        [TestMethod()]
        public void ColorWhiteTest()
        {
            var res = Color4.White;

            Assert.AreEqual(new Color4(1f, 1f, 1f, 1f), res);
        }
        [TestMethod()]
        public void ColorBlackTest()
        {
            var res = Color4.Black;

            Assert.AreEqual(new Color4(0f, 0f, 0f, 1f), res);
        }
        [TestMethod()]
        [DataRow(4294967295, 1f, 1f, 1f, 1f)]
        [DataRow(4294967040, 0f, 1f, 1f, 1f)]
        [DataRow(4294901760, 0f, 0f, 1f, 1f)]
        [DataRow(4278190080, 0f, 0f, 0f, 1f)]
        [DataRow((uint)255, 1f, 0f, 0f, 0f)]
        [DataRow((uint)65280, 0f, 1f, 0f, 0f)]
        [DataRow((uint)16711680, 0f, 0f, 1f, 0f)]
        public void ColorValueTest(uint intCol, float red, float green, float blue, float alpha)
        {
            Assert.AreEqual(new Color4(red, green, blue, alpha), new Color4(intCol));
        }
        [TestMethod()]
        public void ColorByteArrayTest()
        {
            byte r = 32;
            byte g = 64;
            byte b = 128;
            byte a = 255;

            var res = new Color4([r, g, b, a]);

            Assert.AreEqual(new Color4(r, g, b, a), res);
        }
        [TestMethod()]
        public void ColorIntArrayTest()
        {
            var res = new Color4([1, 2, 3, 4]);

            Assert.AreEqual(new Color4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void ColorFloatArrayTest()
        {
            var res = new Color4([1f, 2f, 3f, 4f]);

            Assert.AreEqual(new Color4(1f, 2f, 3f, 4f), res);
        }
        [TestMethod()]
        public void ColorBadArrayTest()
        {
            byte[] bytes = null;
            Assert.Throws<ArgumentNullException>(() => new Color4(bytes));
            bytes = [];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(bytes));
            bytes = [1, 2, 3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(bytes));
            bytes = [1, 2, 3, 4, 5];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(bytes));

            int[] ints = null;
            Assert.Throws<ArgumentNullException>(() => new Color4(ints));
            ints = [];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(ints));
            ints = [1, 2, 3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(ints));
            bytes = [1, 2, 3, 4, 5];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(bytes));

            float[] floats = null;
            Assert.Throws<ArgumentNullException>(() => new Color4(floats));
            floats = [];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(floats));
            floats = [1f, 2f, 3f];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(floats));
            bytes = [1, 2, 3, 4, 5];
            Assert.Throws<ArgumentOutOfRangeException>(() => new Color4(bytes));
        }
        [TestMethod()]
        public void ColorComponentsTest()
        {
            var res = new Color4(1, 2, 3, 4);

            Assert.AreEqual(new Color4(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void ColorSettesTest()
        {
            var res = new Color4
            {
                Red = 32 / 255f,
                Green = 64 / 255f,
                Blue = 128 / 255f,
                Alpha = 255 / 255f
            };

            Assert.AreEqual(new Color4(32, 64, 128, 255), res);
        }

        [TestMethod()]
        public void ColorEqualsTest()
        {
            var res = Color4.Transparent == new Color4(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void ColorDistinctTest()
        {
            var res = Color4.Transparent != new Color4(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void ColorToColor3Test()
        {
            Color3 res1 = (Color3)new Color4(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color3(0.1f, 0.2f, 0.3f), res1);

            Color4 res2 = (Color4)new Color3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 1f), res2);
        }
        [TestMethod()]
        public void ColorToColor4Test()
        {
            Color4 res1 = new(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 0.4f), res1);

            Color4 res2 = new(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 0.4f), res2);
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