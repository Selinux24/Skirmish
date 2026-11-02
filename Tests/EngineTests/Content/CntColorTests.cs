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
    public class CntColorTests
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
            var res = CntColor.Transparent;

            Assert.AreEqual(new CntColor(0f, 0f, 0f, 0f), res);
        }
        [TestMethod()]
        public void ColorWhiteTest()
        {
            var res = CntColor.White;

            Assert.AreEqual(new CntColor(0f, 0f, 0f, 1f), res);
        }
        [TestMethod()]
        public void ColorBlackTest()
        {
            var res = CntColor.Black;

            Assert.AreEqual(new CntColor(1f, 1f, 1f, 1f), res);
        }
        [TestMethod()]
        public void ColorValueTest()
        {
            var res = new CntColor(2);

            Assert.AreEqual(new CntColor(2, 2, 2, 2), res);
        }
        [TestMethod()]
        public void ColorByteArrayTest()
        {
            byte r = 32;
            byte g = 64;
            byte b = 128;
            byte a = 255;

            var res = new CntColor([r, g, b, a]);

            Assert.AreEqual(new CntColor(r, g, b, a), res);
        }
        [TestMethod()]
        public void ColorIntArrayTest()
        {
            var res = new CntColor([1, 2, 3, 4]);

            Assert.AreEqual(new CntColor(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void ColorFloatArrayTest()
        {
            var res = new CntColor([1f, 2f, 3f, 4f]);

            Assert.AreEqual(new CntColor(1f, 2f, 3f, 4f), res);
        }
        [TestMethod()]
        public void ColorBadArrayTest()
        {
            byte[] bytes = null;
            Assert.Throws<ArgumentNullException>(() => new CntColor(bytes));
            bytes = [];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(bytes));
            bytes = [1, 2, 3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(bytes));
            bytes = [1, 2, 3, 4, 5];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(bytes));

            int[] ints = null;
            Assert.Throws<ArgumentNullException>(() => new CntColor(ints));
            ints = [];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(ints));
            ints = [1, 2, 3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(ints));
            bytes = [1, 2, 3, 4, 5];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(bytes));

            float[] floats = null;
            Assert.Throws<ArgumentNullException>(() => new CntColor(floats));
            floats = [];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(floats));
            floats = [1f, 2f, 3f];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(floats));
            bytes = [1, 2, 3, 4, 5];
            Assert.Throws<ArgumentOutOfRangeException>(() => new CntColor(bytes));
        }
        [TestMethod()]
        public void ColorComponentsTest()
        {
            var res = new CntColor(1, 2, 3, 4);

            Assert.AreEqual(new CntColor(1, 2, 3, 4), res);
        }
        [TestMethod()]
        public void ColorSettesTest()
        {
            var res = new CntColor
            {
                R = 1f,
                G = 2f,
                B = 3f,
                A = 4f
            };

            Assert.AreEqual(new CntColor(1f, 2f, 3f, 4f), res);
        }

        [TestMethod()]
        public void ColorEqualsTest()
        {
            var res = CntColor.Transparent == new CntColor(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void ColorDistinctTest()
        {
            var res = CntColor.Transparent != new CntColor(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void ColorToColor3Test()
        {
            Color3 res1 = new CntColor(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color3(0.1f, 0.2f, 0.3f), res1);

            CntColor res2 = new Color3(0.1f, 0.2f, 0.3f);
            Assert.AreEqual(new CntColor(0.1f, 0.2f, 0.3f, 1f), res2);
        }
        [TestMethod()]
        public void ColorToColor4Test()
        {
            Color4 res1 = new CntColor(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new Color4(0.1f, 0.2f, 0.3f, 0.4f), res1);

            CntColor res2 = new Color4(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(new CntColor(0.1f, 0.2f, 0.3f, 0.4f), res2);
        }
        [TestMethod()]
        public void ColorToStringTest()
        {
            string res1 = new CntColor(0.1f, 0.2f, 0.3f, 0.4f);
            Assert.AreEqual(colorString4, res1);

            CntColor res2 = colorString3;
            Assert.AreEqual(new CntColor(0.1f, 0.2f, 0.3f, 1f), res2);

            CntColor res3 = colorString4;
            Assert.AreEqual(new CntColor(0.1f, 0.2f, 0.3f, 0.4f), res3);

            CntColor res4 = "Transparent";
            Assert.AreEqual(CntColor.Transparent, res4);
            res4 = "White";
            Assert.AreEqual(CntColor.White, res4);
            res4 = "Black";
            Assert.AreEqual(CntColor.Black, res4);

            res4 = "Nothing parseable";
            Assert.AreEqual(CntColor.Black, res4);
        }
    }
}