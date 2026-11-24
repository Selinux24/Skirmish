using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public sealed class BoundingRectangleITests
    {
        [TestMethod]
        public void VectorConstructor_PositiveCoordinates_PreservesBoundsExactly()
        {
            var minimum = new Vector2I(-12, 3);
            var maximum = new Vector2I(25, 48);

            var rectangle = new BoundingRectangleI(minimum, maximum);

            Assert.AreEqual(minimum, rectangle.Minimum);
            Assert.AreEqual(maximum, rectangle.Maximum);
        }

        [TestMethod]
        public void ScalarConstructor_NegativeAndZeroCoordinates_PreservesBoundsExactly()
        {
            var rectangle = new BoundingRectangleI(-7, 0, 0, 19);

            Assert.AreEqual(new Vector2I(-7, 0), rectangle.Minimum);
            Assert.AreEqual(new Vector2I(0, 19), rectangle.Maximum);
        }

        [TestMethod]
        public void BothConstructors_SameCoordinates_CreateEqualRectangles()
        {
            var fromVectors = new BoundingRectangleI(new Vector2I(-4, 11), new Vector2I(17, -2));
            var fromScalars = new BoundingRectangleI(-4, 11, 17, -2);

            Assert.AreEqual(fromVectors.Minimum, fromScalars.Minimum);
            Assert.AreEqual(fromVectors.Maximum, fromScalars.Maximum);
            Assert.IsTrue(fromVectors.Equals(fromScalars));
        }

        [TestMethod]
        public void DefaultValue_UsesZeroMinimumAndMaximum()
        {
            BoundingRectangleI rectangle = default;

            Assert.AreEqual(Vector2I.Zero, rectangle.Minimum);
            Assert.AreEqual(Vector2I.Zero, rectangle.Maximum);
        }

        [TestMethod]
        public void MinimumAndMaximum_ReversedCoordinateValues_RemainDistinct()
        {
            var rectangle = new BoundingRectangleI(20, -9, -3, 14);

            Assert.AreEqual(new Vector2I(20, -9), rectangle.Minimum);
            Assert.AreEqual(new Vector2I(-3, 14), rectangle.Maximum);
            Assert.AreNotEqual(rectangle.Minimum, rectangle.Maximum);
        }

        [TestMethod]
        public void RefEquals_EqualAndDifferentRectangles_ReturnsExpectedResults()
        {
            var first = new BoundingRectangleI(2, 3, 6, 7);
            var same = new BoundingRectangleI(2, 3, 6, 7);
            var differentMinimum = new BoundingRectangleI(1, 3, 6, 7);
            var differentMaximum = new BoundingRectangleI(2, 3, 6, 8);

            Assert.IsTrue(first.Equals(ref same));
            Assert.IsFalse(first.Equals(ref differentMinimum));
            Assert.IsFalse(first.Equals(ref differentMaximum));
        }

        [TestMethod]
        public void ValueEquals_EqualAndDifferentRectangles_ReturnsExpectedResults()
        {
            var first = new BoundingRectangleI(2, 3, 6, 7);
            var same = new BoundingRectangleI(2, 3, 6, 7);
            var differentMinimum = new BoundingRectangleI(1, 3, 6, 7);
            var differentMaximum = new BoundingRectangleI(2, 3, 6, 8);

            Assert.IsTrue(first.Equals(same));
            Assert.IsFalse(first.Equals(differentMinimum));
            Assert.IsFalse(first.Equals(differentMaximum));
        }

        [TestMethod]
        public void ObjectEquals_RectangleOnly_WhenValuesMatch()
        {
            var first = new BoundingRectangleI(2, 3, 6, 7);
            var same = new BoundingRectangleI(2, 3, 6, 7);

            Assert.IsTrue(first.Equals((object)same));
            Assert.IsFalse(first.Equals((object)null));
            Assert.IsFalse(first.Equals("not a BoundingRectangleI"));
        }

        [TestMethod]
        public void EqualityOperators_EqualAndUnequalRectangles_ReturnInverseResults()
        {
            var first = new BoundingRectangleI(-5, 4, 10, 12);
            var same = new BoundingRectangleI(-5, 4, 10, 12);
            var other = new BoundingRectangleI(-5, 4, 10, 13);

            Assert.IsTrue(first == same);
            Assert.IsFalse(first != same);
            Assert.IsFalse(first == other);
            Assert.IsTrue(first != other);
        }

        [TestMethod]
        public void GetHashCode_EqualRectangles_HaveEqualHashes()
        {
            var first = new BoundingRectangleI(2, 3, 6, 7);
            var same = new BoundingRectangleI(2, 3, 6, 7);

            Assert.AreEqual(first.GetHashCode(), same.GetHashCode());
        }

        [TestMethod]
        public void ToString_DefaultAndProviderOverloads_IncludeBothBounds()
        {
            var rectangle = new BoundingRectangleI(2, 3, 6, 7);
            var invariant = CultureInfo.InvariantCulture;

            var plain = rectangle.ToString();
            Assert.AreEqual("Min:X:2 Y:3 Max:X:6 Y:7", plain);
            Assert.AreEqual(plain, rectangle.ToString(invariant));
        }

        [TestMethod]
        public void ToString_FormatOverload_WithNullFormat_FallsBackToDefault()
        {
            var rectangle = new BoundingRectangleI(-2, 5, 9, 11);

            Assert.AreEqual(rectangle.ToString(), rectangle.ToString((string)null));
            Assert.AreEqual(rectangle.ToString(), rectangle.ToString(null, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ToString_FormatAndProviderOverloads_HonorD3InvariantFormatting()
        {
            var rectangle = new BoundingRectangleI(2, 3, 6, 7);
            var invariant = CultureInfo.InvariantCulture;

            Assert.AreEqual("Min:X:002 Y:003 Max:X:006 Y:007", rectangle.ToString("D3", invariant));
            Assert.AreEqual("Min:X:002 Y:003 Max:X:006 Y:007", rectangle.ToString("D3"));
        }
    }
}
