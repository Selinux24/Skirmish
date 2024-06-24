using Engine.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.UI
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class PaddingTests
    {
        [TestMethod]
        public void TestPaddingZero()
        {
            Padding padding0 = Padding.Zero;

            Assert.AreEqual(0, padding0.Left);
            Assert.AreEqual(0, padding0.Top);
            Assert.AreEqual(0, padding0.Bottom);
            Assert.AreEqual(0, padding0.Right);
        }

        [TestMethod]
        public void TestPaddingConstructor()
        {
            Padding spacing1 = new(1);
            Padding spacing23 = new(2, 3);
            Padding spacing4567 = new(4, 5, 6, 7);

            Assert.AreEqual(1, spacing1.Left);
            Assert.AreEqual(1, spacing1.Top);
            Assert.AreEqual(1, spacing1.Bottom);
            Assert.AreEqual(1, spacing1.Right);

            Assert.AreEqual(2, spacing23.Left);
            Assert.AreEqual(3, spacing23.Top);
            Assert.AreEqual(3, spacing23.Bottom);
            Assert.AreEqual(2, spacing23.Right);

            Assert.AreEqual(4, spacing4567.Left);
            Assert.AreEqual(5, spacing4567.Top);
            Assert.AreEqual(6, spacing4567.Bottom);
            Assert.AreEqual(7, spacing4567.Right);
        }

        [TestMethod]
        public void Apply_ShouldApplyPaddingToRectangle()
        {
            // Arrange
            var padding = new Padding(10, 20, 30, 40);
            var rectangle = new RectangleF(0, 0, 100, 100);

            // Act
            var result = padding.Apply(rectangle);

            // Assert
            Assert.AreEqual(10, result.Left);
            Assert.AreEqual(60, result.Right);
            Assert.AreEqual(20, result.Top);
            Assert.AreEqual(70, result.Bottom);
            Assert.AreEqual(50, result.Width);
            Assert.AreEqual(50, result.Height);
        }

        [TestMethod]
        public void Equals_ShouldReturnTrueForEqualPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(10, 20, 30, 40);

            // Act
            var result = padding1.Equals(padding2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseForDifferentPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(50, 60, 70, 80);

            // Act
            var result = padding1.Equals(padding2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetHashCode_ShouldReturnSameHashCodeForEqualPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(10, 20, 30, 40);

            // Act
            var hashCode1 = padding1.GetHashCode();
            var hashCode2 = padding2.GetHashCode();

            // Assert
            Assert.AreEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void GetHashCode_ShouldReturnDifferentHashCodeForDifferentPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(50, 60, 70, 80);

            // Act
            var hashCode1 = padding1.GetHashCode();
            var hashCode2 = padding2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var padding = new Padding(10, 20, 30, 40);

            // Act
            var result = padding.ToString();

            // Assert
            Assert.AreEqual("Left: 10; Top: 20; Bottom: 30; Right: 40;", result);
        }

        [TestMethod]
        public void OperatorEqual_ShouldReturnTrueForEqualPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(10, 20, 30, 40);

            // Act
            var result = padding1 == padding2;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void OperatorEqual_ShouldReturnFalseForDifferentPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(50, 60, 70, 80);

            // Act
            var result = padding1 == padding2;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void OperatorNotEqual_ShouldReturnTrueForDifferentPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(50, 60, 70, 80);

            // Act
            var result = padding1 != padding2;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void OperatorNotEqual_ShouldReturnFalseForEqualPadding()
        {
            // Arrange
            var padding1 = new Padding(10, 20, 30, 40);
            var padding2 = new Padding(10, 20, 30, 40);

            // Act
            var result = padding1 != padding2;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ImplicitOperatorInt_ShouldReturnPaddingWithEqualValues()
        {
            // Arrange
            int value = 10;

            // Act
            Padding padding = value;

            // Assert
            Assert.AreEqual(10, padding.Left);
            Assert.AreEqual(10, padding.Top);
            Assert.AreEqual(10, padding.Right);
            Assert.AreEqual(10, padding.Bottom);
        }

        [TestMethod]
        public void ImplicitOperatorIntArray_ShouldReturnPaddingWithEqualValues()
        {
            // Arrange
            int[] value1 = [10, 20];
            int[] value2 = [10, 20, 30, 40];

            // Act
            Padding padding1 = value1;
            Padding padding2 = value2;

            // Assert
            Assert.AreEqual(10, padding1.Left);
            Assert.AreEqual(20, padding1.Top);
            Assert.AreEqual(20, padding1.Bottom);
            Assert.AreEqual(10, padding1.Right);
            Assert.AreEqual(10, padding2.Left);
            Assert.AreEqual(20, padding2.Top);
            Assert.AreEqual(30, padding2.Bottom);
            Assert.AreEqual(40, padding2.Right);
        }

        [TestMethod]
        public void ImplicitOperatorFloat_ShouldReturnPaddingWithEqualValues()
        {
            // Arrange
            float value = 10.5f;

            // Act
            Padding padding = value;

            // Assert
            Assert.AreEqual(10.5f, padding.Left);
            Assert.AreEqual(10.5f, padding.Top);
            Assert.AreEqual(10.5f, padding.Right);
            Assert.AreEqual(10.5f, padding.Bottom);
        }

        [TestMethod]
        public void ImplicitOperatorFloatArray_ShouldReturnPaddingWithEqualValues()
        {
            // Arrange
            float[] value1 = [10.5f, 20.5f];
            float[] value2 = [10.5f, 20.5f, 30.5f, 40.5f];

            // Act
            Padding padding1 = value1;
            Padding padding2 = value2;

            // Assert
            Assert.AreEqual(10.5f, padding1.Left);
            Assert.AreEqual(20.5f, padding1.Top);
            Assert.AreEqual(20.5f, padding1.Bottom);
            Assert.AreEqual(10.5f, padding1.Right);
            Assert.AreEqual(10.5f, padding2.Left);
            Assert.AreEqual(20.5f, padding2.Top);
            Assert.AreEqual(30.5f, padding2.Bottom);
            Assert.AreEqual(40.5f, padding2.Right);
        }
    }
}
