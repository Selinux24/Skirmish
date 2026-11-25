using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public sealed class Color3Tests
    {
        const float ZeroTolerance = 0.0001f;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod]
        public void Constants_ValidValues()
        {
            Assert.AreEqual(new Color3(0f, 0f, 0f), Color3.Black);
            Assert.AreEqual(new Color3(1f, 1f, 1f), Color3.White);
        }

        [TestMethod]
        public void Constructors_ComponentIndexAndArray_WorkAsExpected()
        {
            var scalar = new Color3(0.5f);
            var components = new Color3(0.2f, 0.4f, 0.6f);
            var fromVector3 = new Color3(new Vector3(0.1f, 0.3f, 0.5f));

            Assert.AreEqual(0.5f, scalar.Red, ZeroTolerance);
            Assert.AreEqual(0.5f, scalar.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, scalar.Blue, ZeroTolerance);
            Assert.AreEqual(0.2f, components.Red, ZeroTolerance);
            Assert.AreEqual(0.4f, components.Green, ZeroTolerance);
            Assert.AreEqual(0.6f, components.Blue, ZeroTolerance);
            Assert.AreEqual(0.1f, fromVector3.Red, ZeroTolerance);
            Assert.AreEqual(0.3f, fromVector3.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, fromVector3.Blue, ZeroTolerance);

            components[0] = 0.9f;
            Assert.AreEqual(0.9f, components.Red, ZeroTolerance);
            Assert.AreEqual(0.4f, components.Green, ZeroTolerance);
            Assert.AreEqual(0.6f, components.Blue, ZeroTolerance);
            Assert.AreEqual(0.9f, components[0], ZeroTolerance);
            Assert.AreEqual(0.4f, components[1], ZeroTolerance);
            Assert.AreEqual(0.6f, components[2], ZeroTolerance);
            Assert.AreSequenceEqual([0.9f, 0.4f, 0.6f], components.ToArray());
        }

        [TestMethod]
        public void Constructor_FromBytePacked_ExpectedValues()
        {
            // rgb packed: R=0x00, G=0x00, B=0x00 => Black
            Assert.AreEqual(Color3.Black, new Color3((int)0x000000));
            Assert.AreEqual(Color3.Black, new Color3((uint)0x000000));

            // R=0xFF, G=0xFF, B=0xFF => White
            Assert.AreEqual(Color3.White, new Color3((int)0xFFFFFF));
            Assert.AreEqual(Color3.White, new Color3((uint)0xFFFFFF));

            // R=0xFF, G=0x00, B=0x00 => Pure red
            var red = new Color3(0xFF0000);
            Assert.AreEqual(1f, red.Red, ZeroTolerance);
            Assert.AreEqual(0f, red.Green, ZeroTolerance);
            Assert.AreEqual(0f, red.Blue, ZeroTolerance);

            // R=0x00, G=0xFF, B=0x00 => Pure green
            var green = new Color3(0x00FF00);
            Assert.AreEqual(0f, green.Red, ZeroTolerance);
            Assert.AreEqual(1f, green.Green, ZeroTolerance);
            Assert.AreEqual(0f, green.Blue, ZeroTolerance);

            // R=0x00, G=0x00, B=0xFF => Pure blue
            var blue = new Color3(0x0000FF);
            Assert.AreEqual(0f, blue.Red, ZeroTolerance);
            Assert.AreEqual(0f, blue.Green, ZeroTolerance);
            Assert.AreEqual(1f, blue.Blue, ZeroTolerance);

            // Mixed
            var mixed = new Color3(0x123456);
            Assert.AreEqual(0x12 / 255f, mixed.Red, ZeroTolerance);
            Assert.AreEqual(0x34 / 255f, mixed.Green, ZeroTolerance);
            Assert.AreEqual(0x56 / 255f, mixed.Blue, ZeroTolerance);
        }

        [TestMethod]
        public void Constructor_FromIntAndUintPacked_ExpectedValues()
        {
            Assert.AreEqual((uint)0x0000FF, new Color3(0x0000FF).ToRgb());
            Assert.AreEqual((uint)0xFFFFFF, new Color3(0xFFFFFF).ToRgb());
            Assert.AreEqual((uint)0x123456, new Color3(0x123456).ToRgb());
        }

        [TestMethod]
        public void Constructor_FromBytes_ExpectedValues()
        {
            byte r = 255;
            byte g = 128;
            byte b = 64;
            var color = new Color3(r, g, b);
            Assert.AreEqual(r / 255f, color.Red, ZeroTolerance);
            Assert.AreEqual(g / 255f, color.Green, ZeroTolerance);
            Assert.AreEqual(b / 255f, color.Blue, ZeroTolerance);

            Assert.AreEqual(Color3.Black, new Color3((byte)0, (byte)0, (byte)0));
            Assert.AreEqual(Color3.White, new Color3((byte)255, (byte)255, (byte)255));
        }

        [TestMethod]
        public void Constructor_FromInts_ExpectedValues()
        {
            int r = 255;
            int g = 128;
            int b = 64;
            var color = new Color3(r, g, b);
            Assert.AreEqual(r / 255f, color.Red, ZeroTolerance);
            Assert.AreEqual(g / 255f, color.Green, ZeroTolerance);
            Assert.AreEqual(b / 255f, color.Blue, ZeroTolerance);

            Assert.AreEqual(Color3.Black, new Color3(0, 0, 0));
            Assert.AreEqual(Color3.White, new Color3(255, 255, 255));
        }

        [TestMethod]
        public void Constructor_FromUInts_ExpectedValues()
        {
            uint r = 255;
            uint g = 128;
            uint b = 64;
            var color = new Color3(r, g, b);
            Assert.AreEqual(r / 255f, color.Red, ZeroTolerance);
            Assert.AreEqual(g / 255f, color.Green, ZeroTolerance);
            Assert.AreEqual(b / 255f, color.Blue, ZeroTolerance);

            Assert.AreEqual(Color3.Black, new Color3(0, 0, 0));
            Assert.AreEqual(Color3.White, new Color3(255, 255, 255));
        }

        [TestMethod]
        public void Constructor_InvalidArrayValues_ThrowExpectedExceptions()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new Color3((byte[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new byte[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new byte[4]));

            Assert.ThrowsExactly<ArgumentNullException>(() => new Color3((int[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new int[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new int[4]));

            Assert.ThrowsExactly<ArgumentNullException>(() => new Color3((uint[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new uint[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new uint[4]));

            Assert.ThrowsExactly<ArgumentNullException>(() => new Color3((float[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new float[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color3(new float[4]));

            var color = Color3.Black;
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[3]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[-1]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[3] = 1f);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[-1] = 1f);
        }

        [TestMethod]
        public void AddSubtractModulate_ReturnComponentWiseResults()
        {
            var left = new Color3(0.1f, 0.2f, 0.3f);
            var right = new Color3(0.4f, 0.5f, 0.6f);

            Assert.AreEqual(new Color3(0.5f, 0.7f, 0.9f), Color3.Add(left, right));
            Assert.AreEqual(new Color3(-0.3f, -0.3f, -0.3f), Color3.Subtract(left, right));
            Assert.AreEqual(new Color3(0.04f, 0.1f, 0.18f), Color3.Modulate(left, right));
        }

        [TestMethod]
        public void Clamp_MinMax_ReturnExpectedColors()
        {
            var value = new Color3(1.5f, -0.5f, 0.5f);
            var min = new Color3(0f, 0f, 0f);
            var max = new Color3(1f, 1f, 1f);

            Assert.AreEqual(new Color3(1f, 0f, 0.5f), Color3.Clamp(value, min, max));

            // Clamp within range
            var inside = new Color3(0.5f, 0.5f, 0.5f);
            Assert.AreEqual(inside, Color3.Clamp(inside, min, max));

            // Clamp with non-zero min/max
            var min2 = new Color3(0.2f, 0.2f, 0.2f);
            var max2 = new Color3(0.8f, 0.8f, 0.8f);
            var value2 = new Color3(0.1f, 0.5f, 0.9f);
            Assert.AreEqual(new Color3(0.2f, 0.5f, 0.8f), Color3.Clamp(value2, min2, max2));
        }

        [TestMethod]
        public void Lerp_ReturnInterpolationBoundaries()
        {
            var start = new Color3(0f, 0f, 0f);
            var end = new Color3(1f, 1f, 1f);

            Assert.AreEqual(start, Color3.Lerp(start, end, 0f));
            Assert.AreEqual(Color3.White, Color3.Lerp(start, end, 1f));
            Assert.AreEqual(new Color3(0.5f, 0.5f, 0.5f), Color3.Lerp(start, end, 0.5f));

            // Non-uniform interpolation
            var start2 = new Color3(0f, 0.5f, 1f);
            var end2 = new Color3(1f, 0f, 0f);
            Assert.AreEqual(start2, Color3.Lerp(start2, end2, 0f));
            Assert.AreEqual(end2, Color3.Lerp(start2, end2, 1f));
            Assert.AreEqual(new Color3(0.5f, 0.25f, 0.5f), Color3.Lerp(start2, end2, 0.5f));
        }

        [TestMethod]
        public void SmoothStep_ReturnInterpolationBoundaries()
        {
            var start = new Color3(0f, 0f, 0f);
            var end = new Color3(1f, 1f, 1f);

            Assert.AreEqual(start, Color3.SmoothStep(start, end, 0f));
            Assert.AreEqual(end, Color3.SmoothStep(start, end, 1f));
            // SmoothStep(0.5) = 0.5 (symmetric point)
            Assert.AreEqual(new Color3(0.5f, 0.5f, 0.5f), Color3.SmoothStep(start, end, 0.5f));
        }

        [TestMethod]
        public void MaxMin_ReturnComponentWiseExtremes()
        {
            var left = new Color3(0.3f, 0.7f, 0.1f);
            var right = new Color3(0.6f, 0.4f, 0.8f);

            Assert.AreEqual(new Color3(0.6f, 0.7f, 0.8f), Color3.Max(left, right));
            Assert.AreEqual(new Color3(0.3f, 0.4f, 0.1f), Color3.Min(left, right));

            // Same colors
            Assert.AreEqual(left, Color3.Max(left, left));
            Assert.AreEqual(left, Color3.Min(left, left));
        }

        [TestMethod]
        public void AdjustContrast_ReturnExpectedColors()
        {
            // Contrast = 1.0 returns the original color
            var color = new Color3(0.5f, 0.3f, 0.7f);
            var adjustedContrast = Color3.AdjustContrast(color, 1f);
            Assert.AreEqual(color.Red, adjustedContrast.Red, ZeroTolerance);
            Assert.AreEqual(color.Green, adjustedContrast.Green, ZeroTolerance);
            Assert.AreEqual(color.Blue, adjustedContrast.Blue, ZeroTolerance);

            // Contrast = 0.0 returns middle gray
            var gray = Color3.AdjustContrast(color, 0f);
            Assert.AreEqual(0.5f, gray.Red, ZeroTolerance);
            Assert.AreEqual(0.5f, gray.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, gray.Blue, ZeroTolerance);
        }

        [TestMethod]
        public void AdjustContrast_DetailedCalculations()
        {
            var color = new Color3(0.8f, 0.2f, 0.5f);

            // Contrast = 2.0: result = 0.5 + 2*(value - 0.5)
            var increased = Color3.AdjustContrast(color, 2f);
            Assert.AreEqual(0.5f + 2f * (0.8f - 0.5f), increased.Red, ZeroTolerance);  // 1.1
            Assert.AreEqual(0.5f + 2f * (0.2f - 0.5f), increased.Green, ZeroTolerance); // -0.1
            Assert.AreEqual(0.5f + 2f * (0.5f - 0.5f), increased.Blue, ZeroTolerance);  // 0.0

            // Contrast = 0.5 (decreased)
            var decreased = Color3.AdjustContrast(color, 0.5f);
            Assert.AreEqual(0.5f + 0.5f * (0.8f - 0.5f), decreased.Red, ZeroTolerance);  // 0.65
            Assert.AreEqual(0.5f + 0.5f * (0.2f - 0.5f), decreased.Green, ZeroTolerance); // 0.35
            Assert.AreEqual(0.5f + 0.5f * (0.5f - 0.5f), decreased.Blue, ZeroTolerance);  // 0.5
        }

        [TestMethod]
        public void AdjustSaturation_ReturnExpectedColors()
        {
            var color = new Color3(0.8f, 0.2f, 0.5f);

            // Saturation = 1.0 returns the original color
            var adjustedSaturation = Color3.AdjustSaturation(color, 1f);
            Assert.AreEqual(color.Red, adjustedSaturation.Red, ZeroTolerance);
            Assert.AreEqual(color.Green, adjustedSaturation.Green, ZeroTolerance);
            Assert.AreEqual(color.Blue, adjustedSaturation.Blue, ZeroTolerance);

            // Saturation = 0.0 returns grayscale (using ITU-R BT.709 weights)
            float grey = 0.2125f * 0.8f + 0.7154f * 0.2f + 0.0721f * 0.5f;
            var gray = Color3.AdjustSaturation(color, 0f);
            Assert.AreEqual(grey, gray.Red, ZeroTolerance);
            Assert.AreEqual(grey, gray.Green, ZeroTolerance);
            Assert.AreEqual(grey, gray.Blue, ZeroTolerance);

            // Saturation = 2.0 doubles the saturation
            var saturated = Color3.AdjustSaturation(color, 2f);
            Assert.AreEqual(grey + 2f * (0.8f - grey), saturated.Red, ZeroTolerance);
            Assert.AreEqual(grey + 2f * (0.2f - grey), saturated.Green, ZeroTolerance);
            Assert.AreEqual(grey + 2f * (0.5f - grey), saturated.Blue, ZeroTolerance);
        }

        [TestMethod]
        public void Premultiply_ReturnExpectedColors()
        {
            var color = new Color3(0.5f, 0.8f, 0.3f);
            var premultiplied = Color3.Premultiply(color, 0.6f);

            Assert.AreEqual(0.5f * 0.6f, premultiplied.Red, ZeroTolerance);
            Assert.AreEqual(0.8f * 0.6f, premultiplied.Green, ZeroTolerance);
            Assert.AreEqual(0.3f * 0.6f, premultiplied.Blue, ZeroTolerance);

            // Alpha = 1.0 returns the original color
            var premultipliedFull = Color3.Premultiply(color, 1f);
            Assert.AreEqual(color.Red, premultipliedFull.Red, ZeroTolerance);
            Assert.AreEqual(color.Green, premultipliedFull.Green, ZeroTolerance);
            Assert.AreEqual(color.Blue, premultipliedFull.Blue, ZeroTolerance);
            // Alpha = 0.0 returns black
            Assert.AreEqual(Color3.Black, Color3.Premultiply(color, 0f));
        }

        [TestMethod]
        public void Operators_AddSubtractMultiply_ReturnExpectedColors()
        {
            var left = new Color3(0.2f, 0.4f, 0.6f);
            var right = new Color3(0.1f, 0.3f, 0.5f);

            Assert.AreEqual(new Color3(0.3f, 0.7f, 1.1f), left + right);
            Assert.AreEqual(new Color3(0.1f, 0.1f, 0.1f), left - right);
            Assert.AreEqual(new Color3(0.02f, 0.12f, 0.3f), left * right);
            Assert.AreEqual(new Color3(0.2f, 0.4f, 0.6f), left * Color3.White);
        }

        [TestMethod]
        public void Operators_UnaryAndScale_ReturnExpectedColors()
        {
            var color = new Color3(0.2f, 0.4f, 0.6f);

            Assert.AreEqual(+color, color);
            Assert.AreEqual(-color, Color3.Negate(color));
            Assert.AreEqual(new Color3(0.4f, 0.8f, 1.2f), color * 2f);
            Assert.AreEqual(new Color3(0.4f, 0.8f, 1.2f), 2f * color);
        }

        [TestMethod]
        public void Operators_Equality_ReturnExpectedResults()
        {
            var left = new Color3(0.5f, 0.5f, 0.5f);
            var right = new Color3(0.5f, 0.5f, 0.5f);
            var different = new Color3(0.6f, 0.4f, 0.5f);

            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
            Assert.IsFalse(left == different);
            Assert.IsTrue(left != different);
        }

        [TestMethod]
        public void Conversions_ToColor4_Vector3AndArray_ReturnExpectedValues()
        {
            var color = new Color3(0.2f, 0.5f, 0.8f);

            var color4 = (Color4)color;
            Assert.AreEqual(0.2f, color4.Red, ZeroTolerance);
            Assert.AreEqual(0.5f, color4.Green, ZeroTolerance);
            Assert.AreEqual(0.8f, color4.Blue, ZeroTolerance);
            Assert.AreEqual(1f, color4.Alpha, ZeroTolerance);

            Assert.AreEqual(color.ToColor4(), color4);

            var vector = (Vector3)color;
            Assert.AreEqual(0.2f, vector.X, ZeroTolerance);
            Assert.AreEqual(0.5f, vector.Y, ZeroTolerance);
            Assert.AreEqual(0.8f, vector.Z, ZeroTolerance);
            Assert.AreEqual(color.ToVector3(), vector);

            var fromVector = (Color3)vector;
            Assert.AreEqual(color, fromVector);

            var array = color.ToArray();
            Assert.AreEqual(0.2f, array[0], ZeroTolerance);
            Assert.AreEqual(0.5f, array[1], ZeroTolerance);
            Assert.AreEqual(0.8f, array[2], ZeroTolerance);

            // ToArray returns a new array (mutation safety)
            var arr = color.ToArray();
            arr[0] = 99f;
            Assert.AreEqual(0.2f, color.Red, ZeroTolerance);
        }

        [TestMethod()]
        [DataRow((uint)0xFFFFFF, 1f, 1f, 1f)]
        [DataRow((uint)0x00FFFF, 0f, 1f, 1f)]
        [DataRow((uint)0x0000FF, 0f, 0f, 1f)]
        [DataRow((uint)0x000000, 0f, 0f, 0f)]
        [DataRow((uint)0xFF0000, 1f, 0f, 0f)]
        [DataRow((uint)0x00FF00, 0f, 1f, 0f)]
        public void ColorValueTest(uint intCol, float red, float green, float blue)
        {
            Assert.AreEqual(new Color3(red, green, blue), new Color3(intCol));
        }

        [TestMethod]
        public void Conversions_ToPackedIntegers_ReturnExpectedValues()
        {
            var white = new Color3(1f, 1f, 1f);
            Assert.AreEqual((uint)0xFFFFFFFF, white.ToRgba());
            Assert.AreEqual((uint)0xFFFFFF, white.ToRgb());

            var black = new Color3(0f, 0f, 0f);
            Assert.AreEqual((uint)0x000000FF, black.ToRgba());
            Assert.AreEqual((uint)0x000000, black.ToRgb());
            Assert.AreEqual((uint)0x000000FF, black.ToBgra());
            Assert.AreEqual((uint)0x000000, black.ToBgr());

            var red = new Color3(1f, 0f, 0f);
            Assert.AreEqual((uint)0xFF0000FF, red.ToRgba());
            Assert.AreEqual((uint)0xFF0000, red.ToRgb());
            Assert.AreEqual((uint)0x0000FFFF, red.ToBgra());
            Assert.AreEqual((uint)0x0000FF, red.ToBgr());

            var blue = new Color3(0f, 0f, 1f);
            Assert.AreEqual((uint)0x0000FFFF, blue.ToRgba());
            Assert.AreEqual((uint)0x0000FF, blue.ToRgb());
            Assert.AreEqual((uint)0xFF0000FF, blue.ToBgra());
            Assert.AreEqual((uint)0xFF0000, blue.ToBgr());

            var green = new Color3(0f, 1f, 0f);
            Assert.AreEqual((uint)0x00FF00FF, green.ToRgba());
            Assert.AreEqual((uint)0x00FF00, green.ToRgb());
            Assert.AreEqual((uint)0x00FF00FF, green.ToBgra());
            Assert.AreEqual((uint)0x00FF00, green.ToBgr());
        }

        [TestMethod]
        public void Conversions_ToDrawingColor_ReturnExpectedValues()
        {
            var white = new Color3(1f, 1f, 1f);
            var drawingWhite = (System.Drawing.Color)white;
            Assert.AreEqual(255, drawingWhite.R);
            Assert.AreEqual(255, drawingWhite.G);
            Assert.AreEqual(255, drawingWhite.B);
            Assert.AreEqual(255, drawingWhite.A);

            var black = new Color3(0f, 0f, 0f);
            var drawingBlack = (System.Drawing.Color)black;
            Assert.AreEqual(0, drawingBlack.R);
            Assert.AreEqual(0, drawingBlack.G);
            Assert.AreEqual(0, drawingBlack.B);

            var red = new Color3(1f, 0f, 0f);
            var drawingRed = (System.Drawing.Color)red;
            Assert.AreEqual(255, drawingRed.R);
            Assert.AreEqual(0, drawingRed.G);
            Assert.AreEqual(0, drawingRed.B);

            // Round-trip
            var original = System.Drawing.Color.FromArgb(255, 100, 150, 200);
            var color3 = (Color3)original;
            var back = (System.Drawing.Color)color3;
            Assert.AreEqual(original.R, back.R);
            Assert.AreEqual(original.G, back.G);
            Assert.AreEqual(original.B, back.B);
        }

        [TestMethod]
        public void ValueSemantics_AndFormatting_AllOverloads_WorkAsExpected()
        {
            var color = new Color3(0.25f, 0.5f, 0.75f);
            var equal = new Color3(0.25f, 0.5f, 0.75f);
            var near = new Color3(0.2501f, 0.5f, 0.75f);
            var different = new Color3(0.25f, 0.6f, 0.75f);

            // GetHashCode
            Assert.AreEqual(color.GetHashCode(), equal.GetHashCode());

            // Equals (ref)
            Assert.IsTrue(color.Equals(ref equal));
            Assert.IsFalse(color.Equals(ref different));

            // Equals (value)
            Assert.IsTrue(color.Equals(equal));
            Assert.IsFalse(color.Equals(different));

            // Equals (object)
            Assert.IsTrue(color.Equals((object)equal));
            Assert.IsFalse(color.Equals((object)different));
            Assert.IsFalse(color.Equals((object)"color"));
            Assert.IsFalse(color.Equals(null));

            // NearEqual via MathUtils.NearEqual (used internally)
            Assert.AreEqual(color.Red, near.Red, 0.0001f);
            Assert.AreEqual(color.Green, near.Green, 0.0001f);
            Assert.AreEqual(color.Blue, near.Blue, 0.0001f);

            // ToString()
            var expectedDefault = string.Format(CultureInfo.CurrentCulture, "Red:{0} Green:{1} Blue:{2}", color.Red, color.Green, color.Blue);
            Assert.AreEqual(expectedDefault, color.ToString());
            Assert.AreEqual(expectedDefault, color.ToString(CultureInfo.CurrentCulture));

            // ToString(format)
            var expectedFormatted = string.Format(CultureInfo.CurrentCulture, "Red:{0} Green:{1} Blue:{2}",
                color.Red.ToString("F2", CultureInfo.CurrentCulture),
                color.Green.ToString("F2", CultureInfo.CurrentCulture),
                color.Blue.ToString("F2", CultureInfo.CurrentCulture));
            Assert.AreEqual(expectedFormatted, color.ToString("F2"));
            Assert.AreEqual(expectedFormatted, color.ToString("F2", CultureInfo.CurrentCulture));

            // ToString with different culture
            var expectedInvariant = string.Format(CultureInfo.InvariantCulture, "Red:{0} Green:{1} Blue:{2}",
                color.Red.ToString("F2", CultureInfo.InvariantCulture),
                color.Green.ToString("F2", CultureInfo.InvariantCulture),
                color.Blue.ToString("F2", CultureInfo.InvariantCulture));
            Assert.AreEqual(expectedInvariant, color.ToString("F2", CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void RefOutOverloads_AreEquivalentToStaticMethods()
        {
            var left = new Color3(0.2f, 0.4f, 0.6f);
            var right = new Color3(0.1f, 0.3f, 0.5f);

            Color3.Add(ref left, ref right, out var add); Assert.AreEqual(Color3.Add(left, right), add);
            Color3.Subtract(ref left, ref right, out var subtract); Assert.AreEqual(Color3.Subtract(left, right), subtract);
            Color3.Modulate(ref left, ref right, out var modulate); Assert.AreEqual(Color3.Modulate(left, right), modulate);
            Color3.Scale(ref left, 2f, out var scale); Assert.AreEqual(Color3.Scale(left, 2f), scale);
            Color3.Negate(ref left, out var negate); Assert.AreEqual(Color3.Negate(left), negate);

            var min = new Color3(0f, 0f, 0f);
            var max = new Color3(1f, 1f, 1f);
            Color3.Clamp(ref left, ref min, ref max, out var clamp); Assert.AreEqual(Color3.Clamp(left, min, max), clamp);

            Color3.Lerp(ref left, ref right, 0.5f, out var lerp); Assert.AreEqual(Color3.Lerp(left, right, 0.5f), lerp);
            Color3.SmoothStep(ref left, ref right, 0.5f, out var smooth); Assert.AreEqual(Color3.SmoothStep(left, right, 0.5f), smooth);
            Color3.Max(ref left, ref right, out var maximum); Assert.AreEqual(Color3.Max(left, right), maximum);
            Color3.Min(ref left, ref right, out var minimum); Assert.AreEqual(Color3.Min(left, right), minimum);

            Color3.AdjustContrast(ref left, 1.5f, out var contrast); Assert.AreEqual(Color3.AdjustContrast(left, 1.5f), contrast);
            Color3.AdjustSaturation(ref left, 1.5f, out var saturation); Assert.AreEqual(Color3.AdjustSaturation(left, 1.5f), saturation);
            Color3.Premultiply(ref left, 0.5f, out var premultiply); Assert.AreEqual(Color3.Premultiply(left, 0.5f), premultiply);
        }

        [TestMethod]
        public void RefOverloads_ModifyRefCorrectly()
        {
            var color = new Color3(0.5f, 0.5f, 0.5f);
            var min = new Color3(0.2f, 0.2f, 0.2f);
            var max = new Color3(0.8f, 0.8f, 0.8f);

            // Clamp ref should not modify the input ref, result is out
            Color3.Clamp(ref color, ref min, ref max, out var result);
            Assert.AreEqual(new Color3(0.5f, 0.5f, 0.5f), result);
            // Input ref should be unchanged
            Assert.AreEqual(0.5f, color.Red, ZeroTolerance);

            // Lerp ref
            var start = new Color3(0f, 0f, 0f);
            var end = new Color3(1f, 1f, 1f);
            Color3.Lerp(ref start, ref end, 0.5f, out var lerpResult);
            Assert.AreEqual(new Color3(0.5f, 0.5f, 0.5f), lerpResult);
        }

        [TestMethod]
        public void EdgeCases_BoundaryValues()
        {
            // Values at exact boundaries
            var boundary = new Color3(0f, 1f, 0.5f);
            Assert.AreEqual(0f, boundary.Red, ZeroTolerance);
            Assert.AreEqual(1f, boundary.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, boundary.Blue, ZeroTolerance);

            // Clamp at exact boundaries
            var min = new Color3(0f, 0f, 0f);
            var max = new Color3(1f, 1f, 1f);
            var outsideMin = new Color3(-1f, -1f, -1f);
            var outsideMax = new Color3(2f, 2f, 2f);
            Assert.AreEqual(min, Color3.Clamp(outsideMin, min, max));
            Assert.AreEqual(max, Color3.Clamp(outsideMax, min, max));
        }

        [TestMethod]
        [DataRow(0, (uint)0xC0C0C0)]
        [DataRow(1, (uint)0x81C0C0)]
        [DataRow(2, (uint)0xC081C0)]
        [DataRow(3, (uint)0x8181C0)]
        [DataRow(4, (uint)0xC0C081)]
        [DataRow(5, (uint)0x81C081)]
        [DataRow(6, (uint)0xC08181)]
        [DataRow(7, (uint)0x818181)]
        [DataRow(8, (uint)0x42C0C0)]
        [DataRow(9, (uint)0x03C0C0)]
        [DataRow(99, (uint)0x818142)]
        public void IntToCol_DistributeValues(int idx, uint color)
        {
            var expected = new Color3(color);
            var result = Color3.IntToCol(idx);
            Assert.AreEqual(expected, result, $"Color {result} <> {expected}");
        }
    }
}
