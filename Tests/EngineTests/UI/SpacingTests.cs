using Engine.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.UI
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SpacingTests
    {
        [TestMethod]
        public void TestSpacingZero()
        {
            Spacing spacing0 = Spacing.Zero;

            Assert.AreEqual(spacing0.Horizontal, 0);
            Assert.AreEqual(spacing0.Vertical, 0);
        }

        [TestMethod]
        public void TestSpacingConstructor()
        {
            Spacing spacing1 = new(1);
            Spacing spacing23 = new(2, 3);

            Assert.AreEqual(spacing1.Horizontal, 1);
            Assert.AreEqual(spacing1.Vertical, 1);

            Assert.AreEqual(spacing23.Horizontal, 2);
            Assert.AreEqual(spacing23.Vertical, 3);
        }

        [TestMethod]
        public void TestSpacingEquality()
        {
            Spacing spacing1 = new(10, 20);
            Spacing spacing2 = new(10, 20);
            Spacing spacing3 = new(5, 10);
            object obj2 = new Spacing(10, 20);
            object obj3 = new Spacing(5, 10);

            Assert.AreEqual(spacing1, spacing2);
            Assert.AreNotEqual(spacing1, spacing3);
            Assert.IsTrue(spacing1 == spacing2);
            Assert.IsTrue(spacing1 != spacing3);
            Assert.IsTrue(spacing1.Equals(spacing2));
            Assert.IsTrue(!spacing1.Equals(spacing3));
            Assert.IsTrue(spacing1.Equals(obj2));
            Assert.IsTrue(!spacing1.Equals(obj3));
        }

        [TestMethod]
        public void TestSpacingGetHashCode()
        {
            Spacing spacing1 = new(10, 20);
            Spacing spacing2 = new(10, 20);

            int hc1 = spacing1.GetHashCode();
            int hc2 = spacing2.GetHashCode();

            Assert.AreEqual(hc1, hc2);
        }

        [TestMethod]
        public void TestSpacingToString()
        {
            Spacing spacing = new(10, 20);

            string expected = "Horizontal: 10; Vertical: 20;";
            string actual = spacing.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSpacingImplicitIntegerConversion()
        {
            Spacing spacing1 = 10;
            Spacing spacing2 = new int[] { 10, 20 };
            Spacing spacing3 = 5;

            Assert.AreEqual(new Spacing(10, 10), spacing1);
            Assert.AreEqual(new Spacing(10, 20), spacing2);
            Assert.AreEqual(new Spacing(5, 5), spacing3);
        }

        [TestMethod]
        public void TestSpacingImplicitFloatConversion()
        {
            Spacing spacing1 = 10f;
            Spacing spacing2 = new float[] { 10f, 20f };
            Spacing spacing3 = 5f;

            Assert.AreEqual(new Spacing(10f, 10f), spacing1);
            Assert.AreEqual(new Spacing(10f, 20f), spacing2);
            Assert.AreEqual(new Spacing(5f, 5f), spacing3);
        }
    }
}
