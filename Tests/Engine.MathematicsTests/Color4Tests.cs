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
    public sealed class Color4Tests
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
            Assert.AreEqual(new Color4(0f, 0f, 0f, 1f), Color4.Black);
            Assert.AreEqual(new Color4(1f, 1f, 1f, 1f), Color4.White);
        }

        [TestMethod]
        public void Constructors_ComponentIndexAndArray_WorkAsExpected()
        {
            var scalar = new Color4(0.5f);
            var components = new Color4(0.2f, 0.4f, 0.6f, 0.8f);
            var fromVector3 = new Color4(new Vector3(0.1f, 0.3f, 0.5f));
            var fromVector3_2 = new Color4(new Vector3(0.1f, 0.3f, 0.5f), 0.7f);
            var fromVector4 = new Color4(new Vector4(0.1f, 0.3f, 0.5f, 0.7f));
            var fromColor3 = new Color4(new Color3(0.1f, 0.3f, 0.5f));
            var fromColor3_2 = new Color4(new Color3(0.1f, 0.3f, 0.5f), 0.7f);

            Assert.AreEqual(0.5f, scalar.Red, ZeroTolerance);
            Assert.AreEqual(0.5f, scalar.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, scalar.Blue, ZeroTolerance);
            Assert.AreEqual(0.5f, scalar.Alpha, ZeroTolerance);
            Assert.AreEqual(0.2f, components.Red, ZeroTolerance);
            Assert.AreEqual(0.4f, components.Green, ZeroTolerance);
            Assert.AreEqual(0.6f, components.Blue, ZeroTolerance);
            Assert.AreEqual(0.8f, components.Alpha, ZeroTolerance);
            Assert.AreEqual(0.1f, fromVector3.Red, ZeroTolerance);
            Assert.AreEqual(0.3f, fromVector3.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, fromVector3.Blue, ZeroTolerance);
            Assert.AreEqual(1.0f, fromVector3.Alpha, ZeroTolerance);
            Assert.AreEqual(0.1f, fromVector3_2.Red, ZeroTolerance);
            Assert.AreEqual(0.3f, fromVector3_2.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, fromVector3_2.Blue, ZeroTolerance);
            Assert.AreEqual(0.7f, fromVector3_2.Alpha, ZeroTolerance);
            Assert.AreEqual(0.1f, fromVector4.Red, ZeroTolerance);
            Assert.AreEqual(0.3f, fromVector4.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, fromVector4.Blue, ZeroTolerance);
            Assert.AreEqual(0.7f, fromVector4.Alpha, ZeroTolerance);
            Assert.AreEqual(0.1f, fromColor3.Red, ZeroTolerance);
            Assert.AreEqual(0.3f, fromColor3.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, fromColor3.Blue, ZeroTolerance);
            Assert.AreEqual(1.0f, fromColor3.Alpha, ZeroTolerance);
            Assert.AreEqual(0.1f, fromColor3_2.Red, ZeroTolerance);
            Assert.AreEqual(0.3f, fromColor3_2.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, fromColor3_2.Blue, ZeroTolerance);
            Assert.AreEqual(0.7f, fromColor3_2.Alpha, ZeroTolerance);

            components[0] = 0.9f;
            Assert.AreEqual(0.9f, components.Red, ZeroTolerance);
            Assert.AreEqual(0.4f, components.Green, ZeroTolerance);
            Assert.AreEqual(0.6f, components.Blue, ZeroTolerance);
            Assert.AreEqual(0.8f, components.Alpha, ZeroTolerance);
            Assert.AreEqual(0.9f, components[0], ZeroTolerance);
            Assert.AreEqual(0.4f, components[1], ZeroTolerance);
            Assert.AreEqual(0.6f, components[2], ZeroTolerance);
            Assert.AreEqual(0.8f, components[3], ZeroTolerance);
            Assert.AreSequenceEqual([0.9f, 0.4f, 0.6f, 0.8f], components.ToArray());
        }

        [TestMethod]
        public void Constructor_FromBytePacked_ExpectedValues()
        {
            // R=0x00, G=0x00, B=0x00, A=0xFF => Black
            Assert.AreEqual(Color4.Black, new Color4((uint)0x000000FF));

            // R=0xFF, G=0xFF, B=0xFF, A=0xFF => White
            Assert.AreEqual(Color4.White, new Color4((uint)0xFFFFFFFF));

            // R=0x00, G=0x00, B=0x00, A=0x00 => Transparent
            Assert.AreEqual(Color4.Transparent, new Color4((uint)0x00000000));

            // R=0xFF, G=0x00, B=0x00, A=0xFF => Pure red
            var red = new Color4(0xFF0000FF);
            Assert.AreEqual(1f, red.Red, ZeroTolerance);
            Assert.AreEqual(0f, red.Green, ZeroTolerance);
            Assert.AreEqual(0f, red.Blue, ZeroTolerance);
            Assert.AreEqual(1f, red.Alpha, ZeroTolerance);

            // R=0x00, G=0xFF, B=0x00, A=0xFF => Pure green
            var green = new Color4(0x00FF00FF);
            Assert.AreEqual(0f, green.Red, ZeroTolerance);
            Assert.AreEqual(1f, green.Green, ZeroTolerance);
            Assert.AreEqual(0f, green.Blue, ZeroTolerance);
            Assert.AreEqual(1f, green.Alpha, ZeroTolerance);

            // R=0x00, G=0x00, B=0xFF, A=0xFF => Pure blue
            var blue = new Color4(0x0000FFFF);
            Assert.AreEqual(0f, blue.Red, ZeroTolerance);
            Assert.AreEqual(0f, blue.Green, ZeroTolerance);
            Assert.AreEqual(1f, blue.Blue, ZeroTolerance);
            Assert.AreEqual(1f, blue.Alpha, ZeroTolerance);

            // Mixed
            var mixed = new Color4(0x12345678);
            Assert.AreEqual(0x12 / 255f, mixed.Red, ZeroTolerance);
            Assert.AreEqual(0x34 / 255f, mixed.Green, ZeroTolerance);
            Assert.AreEqual(0x56 / 255f, mixed.Blue, ZeroTolerance);
            Assert.AreEqual(0x78 / 255f, mixed.Alpha, ZeroTolerance);
        }

        [TestMethod]
        public void Constructor_FromIntAndUintPacked_ExpectedValues()
        {
            Assert.AreEqual((uint)0x0000FFFF, new Color4(0x0000FFFF).ToRgba());
            Assert.AreEqual((uint)0xFFFFFFFF, new Color4(0xFFFFFFFF).ToRgba());
            Assert.AreEqual((uint)0x12345678, new Color4(0x12345678).ToRgba());
        }

        [TestMethod]
        public void Constructor_FromBytes_ExpectedValues()
        {
            byte r = 255;
            byte g = 128;
            byte b = 64;
            byte a = 48;
            var color = new Color4(r, g, b, a);
            Assert.AreEqual(r / 255f, color.Red, ZeroTolerance);
            Assert.AreEqual(g / 255f, color.Green, ZeroTolerance);
            Assert.AreEqual(b / 255f, color.Blue, ZeroTolerance);
            Assert.AreEqual(a / 255f, color.Alpha, ZeroTolerance);

            Assert.AreEqual(Color4.Black, new Color4((byte)0, (byte)0, (byte)0, (byte)255));
            Assert.AreEqual(Color4.White, new Color4((byte)255, (byte)255, (byte)255, (byte)255));
            Assert.AreEqual(Color4.Transparent, new Color4((byte)0, (byte)0, (byte)0, (byte)0));
        }

        [TestMethod]
        public void Constructor_FromInts_ExpectedValues()
        {
            int r = 255;
            int g = 128;
            int b = 64;
            int a = 48;
            var color = new Color4(r, g, b, a);
            Assert.AreEqual(r / 255f, color.Red, ZeroTolerance);
            Assert.AreEqual(g / 255f, color.Green, ZeroTolerance);
            Assert.AreEqual(b / 255f, color.Blue, ZeroTolerance);
            Assert.AreEqual(a / 255f, color.Alpha, ZeroTolerance);

            Assert.AreEqual(Color4.Black, new Color4(0, 0, 0, 255));
            Assert.AreEqual(Color4.White, new Color4(255, 255, 255, 255));
            Assert.AreEqual(Color4.Transparent, new Color4(0, 0, 0, 0));
        }

        [TestMethod]
        public void Constructor_FromUInts_ExpectedValues()
        {
            uint r = 255;
            uint g = 128;
            uint b = 64;
            uint a = 48;
            var color = new Color4(r, g, b, a);
            Assert.AreEqual(r / 255f, color.Red, ZeroTolerance);
            Assert.AreEqual(g / 255f, color.Green, ZeroTolerance);
            Assert.AreEqual(b / 255f, color.Blue, ZeroTolerance);
            Assert.AreEqual(a / 255f, color.Alpha, ZeroTolerance);

            Assert.AreEqual(Color4.Black, new Color4(0u, 0u, 0u, 255u));
            Assert.AreEqual(Color4.White, new Color4(255u, 255u, 255u, 255u));
            Assert.AreEqual(Color4.Transparent, new Color4(0u, 0u, 0u, 0u));
        }

        [TestMethod]
        public void Constructor_InvalidArrayValues_ThrowExpectedExceptions()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new Color4((byte[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new byte[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new byte[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new byte[5]));

            Assert.ThrowsExactly<ArgumentNullException>(() => new Color4((int[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new int[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new int[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new int[5]));

            Assert.ThrowsExactly<ArgumentNullException>(() => new Color4((uint[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new uint[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new uint[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new uint[5]));

            Assert.ThrowsExactly<ArgumentNullException>(() => new Color4((float[])null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new float[2]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new float[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color4(new float[5]));

            var color = Color4.Black;
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[4]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[-1]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[4] = 1f);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[-1] = 1f);
        }

        [TestMethod]
        public void AddSubtractModulate_ReturnComponentWiseResults()
        {
            var left = new Color4(0.1f, 0.2f, 0.3f, 0.4f);
            var right = new Color4(0.2f, 0.3f, 0.4f, 0.5f);

            Assert.AreEqual(new Color4(0.3f, 0.5f, 0.7f, 0.9f), Color4.Add(left, right));
            Assert.AreEqual(new Color4(-0.1f, -0.1f, -0.1f, -0.1f), Color4.Subtract(left, right));
            Assert.AreEqual(new Color4(0.02f, 0.06f, 0.12f, 0.2f), Color4.Modulate(left, right));
        }

        [TestMethod]
        public void Clamp_MinMax_ReturnExpectedColors()
        {
            var value = new Color4(1.5f, -0.5f, 0.5f, 1f);
            var min = new Color4(0f, 0f, 0f, 1f);
            var max = new Color4(1f, 1f, 1f, 1f);

            Assert.AreEqual(new Color4(1f, 0f, 0.5f, 1f), Color4.Clamp(value, min, max));

            // Clamp within range
            var inside = new Color4(0.5f, 0.5f, 0.5f, 1f);
            Assert.AreEqual(inside, Color4.Clamp(inside, min, max));

            // Clamp with non-zero min/max
            var min2 = new Color4(0.2f, 0.2f, 0.2f, 1f);
            var max2 = new Color4(0.8f, 0.8f, 0.8f, 1f);
            var value2 = new Color4(0.1f, 0.5f, 0.9f, 1f);
            Assert.AreEqual(new Color4(0.2f, 0.5f, 0.8f, 1f), Color4.Clamp(value2, min2, max2));
        }

        [TestMethod]
        public void Lerp_ReturnInterpolationBoundaries()
        {
            var start = new Color4(0f, 0f, 0f, 1f);
            var end = new Color4(1f, 1f, 1f, 1f);

            Assert.AreEqual(start, Color4.Lerp(start, end, 0f));
            Assert.AreEqual(Color4.White, Color4.Lerp(start, end, 1f));
            Assert.AreEqual(new Color4(0.5f, 0.5f, 0.5f, 1f), Color4.Lerp(start, end, 0.5f));

            // Non-uniform interpolation
            var start2 = new Color4(0f, 0.5f, 1f, 1f);
            var end2 = new Color4(1f, 0f, 0f, 1f);
            Assert.AreEqual(start2, Color4.Lerp(start2, end2, 0f));
            Assert.AreEqual(end2, Color4.Lerp(start2, end2, 1f));
            Assert.AreEqual(new Color4(0.5f, 0.25f, 0.5f, 1f), Color4.Lerp(start2, end2, 0.5f));
        }

        [TestMethod]
        public void SmoothStep_ReturnInterpolationBoundaries()
        {
            var start = new Color4(0f, 0f, 0f, 1f);
            var end = new Color4(1f, 1f, 1f, 1f);

            Assert.AreEqual(start, Color4.SmoothStep(start, end, 0f));
            Assert.AreEqual(end, Color4.SmoothStep(start, end, 1f));
            // SmoothStep(0.5) = 0.5 (symmetric point)
            Assert.AreEqual(new Color4(0.5f, 0.5f, 0.5f, 1f), Color4.SmoothStep(start, end, 0.5f));
        }

        [TestMethod]
        public void MaxMin_ReturnComponentWiseExtremes()
        {
            var left = new Color4(0.3f, 0.7f, 0.1f, 1f);
            var right = new Color4(0.6f, 0.4f, 0.8f, 1f);

            Assert.AreEqual(new Color4(0.6f, 0.7f, 0.8f, 1f), Color4.Max(left, right));
            Assert.AreEqual(new Color4(0.3f, 0.4f, 0.1f, 1f), Color4.Min(left, right));

            // Same colors
            Assert.AreEqual(left, Color4.Max(left, left));
            Assert.AreEqual(left, Color4.Min(left, left));
        }

        [TestMethod]
        public void AdjustContrast_ReturnExpectedColors()
        {
            // Contrast = 1.0 returns the original color
            var color = new Color4(0.5f, 0.3f, 0.7f, 1f);
            var adjustedContrast = Color4.AdjustContrast(color, 1f);
            Assert.AreEqual(color.Red, adjustedContrast.Red, ZeroTolerance);
            Assert.AreEqual(color.Green, adjustedContrast.Green, ZeroTolerance);
            Assert.AreEqual(color.Blue, adjustedContrast.Blue, ZeroTolerance);

            // Contrast = 0.0 returns middle gray
            var gray = Color4.AdjustContrast(color, 0f);
            Assert.AreEqual(0.5f, gray.Red, ZeroTolerance);
            Assert.AreEqual(0.5f, gray.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, gray.Blue, ZeroTolerance);
        }

        [TestMethod]
        public void AdjustContrast_DetailedCalculations()
        {
            var color = new Color4(0.8f, 0.2f, 0.5f, 1f);

            // Contrast = 2.0: result = 0.5 + 2*(value - 0.5)
            var increased = Color4.AdjustContrast(color, 2f);
            Assert.AreEqual(0.5f + 2f * (0.8f - 0.5f), increased.Red, ZeroTolerance);  // 1.1
            Assert.AreEqual(0.5f + 2f * (0.2f - 0.5f), increased.Green, ZeroTolerance); // -0.1
            Assert.AreEqual(0.5f + 2f * (0.5f - 0.5f), increased.Blue, ZeroTolerance);  // 0.0

            // Contrast = 0.5 (decreased)
            var decreased = Color4.AdjustContrast(color, 0.5f);
            Assert.AreEqual(0.5f + 0.5f * (0.8f - 0.5f), decreased.Red, ZeroTolerance);  // 0.65
            Assert.AreEqual(0.5f + 0.5f * (0.2f - 0.5f), decreased.Green, ZeroTolerance); // 0.35
            Assert.AreEqual(0.5f + 0.5f * (0.5f - 0.5f), decreased.Blue, ZeroTolerance);  // 0.5
        }

        [TestMethod]
        public void AdjustSaturation_ReturnExpectedColors()
        {
            var color = new Color4(0.8f, 0.2f, 0.5f, 1f);

            // Saturation = 1.0 returns the original color
            var adjustedSaturation = Color4.AdjustSaturation(color, 1f);
            Assert.AreEqual(color.Red, adjustedSaturation.Red, ZeroTolerance);
            Assert.AreEqual(color.Green, adjustedSaturation.Green, ZeroTolerance);
            Assert.AreEqual(color.Blue, adjustedSaturation.Blue, ZeroTolerance);

            // Saturation = 0.0 returns grayscale (using ITU-R BT.709 weights)
            float grey = 0.2125f * 0.8f + 0.7154f * 0.2f + 0.0721f * 0.5f;
            var gray = Color4.AdjustSaturation(color, 0f);
            Assert.AreEqual(grey, gray.Red, ZeroTolerance);
            Assert.AreEqual(grey, gray.Green, ZeroTolerance);
            Assert.AreEqual(grey, gray.Blue, ZeroTolerance);

            // Saturation = 2.0 doubles the saturation
            var saturated = Color4.AdjustSaturation(color, 2f);
            Assert.AreEqual(grey + 2f * (0.8f - grey), saturated.Red, ZeroTolerance);
            Assert.AreEqual(grey + 2f * (0.2f - grey), saturated.Green, ZeroTolerance);
            Assert.AreEqual(grey + 2f * (0.5f - grey), saturated.Blue, ZeroTolerance);
        }

        [TestMethod]
        public void Premultiply_ReturnExpectedColors()
        {
            var color = new Color4(0.5f, 0.8f, 0.3f, 0.6f);
            var premultiplied = Color4.Premultiply(color);
            Assert.AreEqual(0.5f * 0.6f, premultiplied.Red, ZeroTolerance);
            Assert.AreEqual(0.8f * 0.6f, premultiplied.Green, ZeroTolerance);
            Assert.AreEqual(0.3f * 0.6f, premultiplied.Blue, ZeroTolerance);

            // Alpha = 1.0 returns the original color
            color = new Color4(0.5f, 0.8f, 0.3f, 1f);
            var premultipliedFull = Color4.Premultiply(color);
            Assert.AreEqual(color.Red, premultipliedFull.Red, ZeroTolerance);
            Assert.AreEqual(color.Green, premultipliedFull.Green, ZeroTolerance);
            Assert.AreEqual(color.Blue, premultipliedFull.Blue, ZeroTolerance);
            // Alpha = 0.0 returns black
            color = new Color4(0.5f, 0.8f, 0.3f, 0f);
            Assert.AreEqual(Color4.Transparent, Color4.Premultiply(color));
        }

        [TestMethod]
        public void Operators_AddSubtractMultiply_ReturnExpectedColors()
        {
            var left = new Color4(0.2f, 0.4f, 0.6f, 1f);
            var right = new Color4(0.1f, 0.3f, 0.5f, 1f);

            Assert.AreEqual(new Color4(0.3f, 0.7f, 1.1f, 2f), left + right);
            Assert.AreEqual(new Color4(0.1f, 0.1f, 0.1f, 0f), left - right);
            Assert.AreEqual(new Color4(0.02f, 0.12f, 0.3f, 1f), left * right);
            Assert.AreEqual(new Color4(0.2f, 0.4f, 0.6f, 1f), left * Color4.White);
        }

        [TestMethod]
        public void Operators_UnaryAndScale_ReturnExpectedColors()
        {
            var color = new Color4(0.2f, 0.4f, 0.6f, 1f);

            Assert.AreEqual(+color, color);
            Assert.AreEqual(-color, Color4.Negate(color));
            Assert.AreEqual(new Color4(0.4f, 0.8f, 1.2f, 2f), color * 2f);
            Assert.AreEqual(new Color4(0.4f, 0.8f, 1.2f, 2f), 2f * color);
        }

        [TestMethod]
        public void Operators_Equality_ReturnExpectedResults()
        {
            var left = new Color4(0.5f, 0.5f, 0.5f, 1f);
            var right = new Color4(0.5f, 0.5f, 0.5f, 1f);
            var different = new Color4(0.6f, 0.4f, 0.5f, 1f);

            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
            Assert.IsFalse(left == different);
            Assert.IsTrue(left != different);
        }

        [TestMethod]
        public void Conversions_ToColor4_Vector3AndArray_ReturnExpectedValues()
        {
            var color4 = new Color4(0.2f, 0.5f, 0.8f, 1f);
            var color3 = new Color3(0.2f, 0.5f, 0.8f);

            var color = (Color3)color4;
            Assert.AreEqual(0.2f, color.Red, ZeroTolerance);
            Assert.AreEqual(0.5f, color.Green, ZeroTolerance);
            Assert.AreEqual(0.8f, color.Blue, ZeroTolerance);
            Assert.AreEqual(color4.ToColor3(), color);

            var vector3 = (Vector3)color3;
            Assert.AreEqual(0.2f, vector3.X, ZeroTolerance);
            Assert.AreEqual(0.5f, vector3.Y, ZeroTolerance);
            Assert.AreEqual(0.8f, vector3.Z, ZeroTolerance);
            Assert.AreEqual(color4.ToVector3(), vector3);

            var vector4 = (Vector4)color4;
            Assert.AreEqual(0.2f, vector4.X, ZeroTolerance);
            Assert.AreEqual(0.5f, vector4.Y, ZeroTolerance);
            Assert.AreEqual(0.8f, vector4.Z, ZeroTolerance);
            Assert.AreEqual(color4.ToVector4(), vector4);

            var fromVector = (Color4)vector4;
            Assert.AreEqual(color4, fromVector);

            var array = color4.ToArray();
            Assert.AreEqual(0.2f, array[0], ZeroTolerance);
            Assert.AreEqual(0.5f, array[1], ZeroTolerance);
            Assert.AreEqual(0.8f, array[2], ZeroTolerance);

            // ToArray returns a new array (mutation safety)
            var arr = color4.ToArray();
            arr[0] = 99f;
            Assert.AreEqual(0.2f, color4.Red, ZeroTolerance);
        }

        [TestMethod()]
        [DataRow((uint)0xFFFFFFFF, 1f, 1f, 1f, 1f)]
        [DataRow((uint)0x00FFFFFF, 0f, 1f, 1f, 1f)]
        [DataRow((uint)0x0000FFFF, 0f, 0f, 1f, 1f)]
        [DataRow((uint)0x000000FF, 0f, 0f, 0f, 1f)]
        [DataRow((uint)0xFF000000, 1f, 0f, 0f, 0f)]
        [DataRow((uint)0x00FF0000, 0f, 1f, 0f, 0f)]
        [DataRow((uint)0x0000FF00, 0f, 0f, 1f, 0f)]
        public void ColorValueTest(uint intCol, float red, float green, float blue, float alpha)
        {
            Assert.AreEqual(new Color4(red, green, blue, alpha), new Color4(intCol));
        }

        [TestMethod]
        public void Conversions_ToPackedIntegers_ReturnExpectedValues()
        {
            var white = new Color4(1f, 1f, 1f, 1f);
            Assert.AreEqual((uint)0xFFFFFFFF, white.ToRgba());

            var black = new Color4(0f, 0f, 0f, 1f);
            Assert.AreEqual((uint)0x000000FF, black.ToRgba());
            Assert.AreEqual((uint)0x000000FF, black.ToBgra());

            var red = new Color4(1f, 0f, 0f, 1f);
            Assert.AreEqual((uint)0xFF0000FF, red.ToRgba());
            Assert.AreEqual((uint)0x0000FFFF, red.ToBgra());

            var blue = new Color4(0f, 0f, 1f, 1f);
            Assert.AreEqual((uint)0x0000FFFF, blue.ToRgba());
            Assert.AreEqual((uint)0xFF0000FF, blue.ToBgra());

            var green = new Color4(0f, 1f, 0f, 1f);
            Assert.AreEqual((uint)0x00FF00FF, green.ToRgba());
            Assert.AreEqual((uint)0x00FF00FF, green.ToBgra());
        }

        [TestMethod]
        public void Conversions_ToDrawingColor_ReturnExpectedValues()
        {
            var white = new Color4(1f, 1f, 1f, 1f);
            var drawingWhite = (System.Drawing.Color)white;
            Assert.AreEqual(255, drawingWhite.R);
            Assert.AreEqual(255, drawingWhite.G);
            Assert.AreEqual(255, drawingWhite.B);
            Assert.AreEqual(255, drawingWhite.A);

            var black = new Color4(0f, 0f, 0f, 1f);
            var drawingBlack = (System.Drawing.Color)black;
            Assert.AreEqual(0, drawingBlack.R);
            Assert.AreEqual(0, drawingBlack.G);
            Assert.AreEqual(0, drawingBlack.B);

            var red = new Color4(1f, 0f, 0f, 1f);
            var drawingRed = (System.Drawing.Color)red;
            Assert.AreEqual(255, drawingRed.R);
            Assert.AreEqual(0, drawingRed.G);
            Assert.AreEqual(0, drawingRed.B);

            // Round-trip
            var original = System.Drawing.Color.FromArgb(255, 100, 150, 200);
            var color3 = (Color4)original;
            var back = (System.Drawing.Color)color3;
            Assert.AreEqual(original.R, back.R);
            Assert.AreEqual(original.G, back.G);
            Assert.AreEqual(original.B, back.B);
        }

        [TestMethod]
        public void ValueSemantics_AndFormatting_AllOverloads_WorkAsExpected()
        {
            var color = new Color4(0.25f, 0.5f, 0.75f, 1f);
            var equal = new Color4(0.25f, 0.5f, 0.75f, 1f);
            var near = new Color4(0.2501f, 0.5f, 0.75f, 1f);
            var different = new Color4(0.25f, 0.6f, 0.75f, 1f);

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
            var expectedDefault = string.Format(CultureInfo.CurrentCulture, "Alpha:{0} Red:{1} Green:{2} Blue:{3}", color.Alpha, color.Red, color.Green, color.Blue);
            Assert.AreEqual(expectedDefault, color.ToString());
            Assert.AreEqual(expectedDefault, color.ToString(CultureInfo.CurrentCulture));

            // ToString(format)
            var expectedFormatted = string.Format(CultureInfo.CurrentCulture, "Alpha:{0} Red:{1} Green:{2} Blue:{3}",
                color.Alpha.ToString("F2", CultureInfo.CurrentCulture),
                color.Red.ToString("F2", CultureInfo.CurrentCulture),
                color.Green.ToString("F2", CultureInfo.CurrentCulture),
                color.Blue.ToString("F2", CultureInfo.CurrentCulture));
            Assert.AreEqual(expectedFormatted, color.ToString("F2"));
            Assert.AreEqual(expectedFormatted, color.ToString("F2", CultureInfo.CurrentCulture));

            // ToString with different culture
            var expectedInvariant = string.Format(CultureInfo.InvariantCulture, "Alpha:{0} Red:{1} Green:{2} Blue:{3}",
                color.Alpha.ToString("F2", CultureInfo.InvariantCulture),
                color.Red.ToString("F2", CultureInfo.InvariantCulture),
                color.Green.ToString("F2", CultureInfo.InvariantCulture),
                color.Blue.ToString("F2", CultureInfo.InvariantCulture));
            Assert.AreEqual(expectedInvariant, color.ToString("F2", CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void RefOutOverloads_AreEquivalentToStaticMethods()
        {
            var left = new Color4(0.2f, 0.4f, 0.6f, 0.5f);
            var right = new Color4(0.1f, 0.3f, 0.5f, 0.5f);

            Color4.Add(ref left, ref right, out var add); Assert.AreEqual(Color4.Add(left, right), add);
            Color4.Subtract(ref left, ref right, out var subtract); Assert.AreEqual(Color4.Subtract(left, right), subtract);
            Color4.Modulate(ref left, ref right, out var modulate); Assert.AreEqual(Color4.Modulate(left, right), modulate);
            Color4.Scale(ref left, 2f, out var scale); Assert.AreEqual(Color4.Scale(left, 2f), scale);
            Color4.Negate(ref left, out var negate); Assert.AreEqual(Color4.Negate(left), negate);

            var min = new Color4(0f, 0f, 0f, 1f);
            var max = new Color4(1f, 1f, 1f, 1f);
            Color4.Clamp(ref left, ref min, ref max, out var clamp); Assert.AreEqual(Color4.Clamp(left, min, max), clamp);

            Color4.Lerp(ref left, ref right, 0.5f, out var lerp); Assert.AreEqual(Color4.Lerp(left, right, 0.5f), lerp);
            Color4.SmoothStep(ref left, ref right, 0.5f, out var smooth); Assert.AreEqual(Color4.SmoothStep(left, right, 0.5f), smooth);
            Color4.Max(ref left, ref right, out var maximum); Assert.AreEqual(Color4.Max(left, right), maximum);
            Color4.Min(ref left, ref right, out var minimum); Assert.AreEqual(Color4.Min(left, right), minimum);

            Color4.AdjustContrast(ref left, 1.5f, out var contrast); Assert.AreEqual(Color4.AdjustContrast(left, 1.5f), contrast);
            Color4.AdjustSaturation(ref left, 1.5f, out var saturation); Assert.AreEqual(Color4.AdjustSaturation(left, 1.5f), saturation);
            Color4.Premultiply(ref left, out var premultiply); Assert.AreEqual(Color4.Premultiply(left), premultiply);
        }

        [TestMethod]
        public void RefOverloads_ModifyRefCorrectly()
        {
            var color = new Color4(0.5f, 0.5f, 0.5f, 1f);
            var min = new Color4(0.2f, 0.2f, 0.2f, 1f);
            var max = new Color4(0.8f, 0.8f, 0.8f, 1f);

            // Clamp ref should not modify the input ref, result is out
            Color4.Clamp(ref color, ref min, ref max, out var result);
            Assert.AreEqual(new Color4(0.5f, 0.5f, 0.5f, 1f), result);
            // Input ref should be unchanged
            Assert.AreEqual(0.5f, color.Red, ZeroTolerance);

            // Lerp ref
            var start = new Color4(0f, 0f, 0f, 1f);
            var end = new Color4(1f, 1f, 1f, 1f);
            Color4.Lerp(ref start, ref end, 0.5f, out var lerpResult);
            Assert.AreEqual(new Color4(0.5f, 0.5f, 0.5f, 1f), lerpResult);
        }

        [TestMethod]
        public void EdgeCases_BoundaryValues()
        {
            // Values at exact boundaries
            var boundary = new Color4(0f, 1f, 0.5f, 1f);
            Assert.AreEqual(0f, boundary.Red, ZeroTolerance);
            Assert.AreEqual(1f, boundary.Green, ZeroTolerance);
            Assert.AreEqual(0.5f, boundary.Blue, ZeroTolerance);

            // Clamp at exact boundaries
            var min = new Color4(0f, 0f, 0f, 1f);
            var max = new Color4(1f, 1f, 1f, 1f);
            var outsideMin = new Color4(-1f, -1f, -1f, 1f);
            var outsideMax = new Color4(2f, 2f, 2f, 1f);
            Assert.AreEqual(min, Color4.Clamp(outsideMin, min, max));
            Assert.AreEqual(max, Color4.Clamp(outsideMax, min, max));
        }


        [TestMethod]
        [DataRow(0, 255, (uint)0xC0C0C0FF)]
        [DataRow(1, 255, (uint)0x81C0C0FF)]
        [DataRow(2, 255, (uint)0xC081C0FF)]
        [DataRow(3, 255, (uint)0x8181C0FF)]
        [DataRow(4, 255, (uint)0xC0C081FF)]
        [DataRow(5, 255, (uint)0x81C081FF)]
        [DataRow(6, 255, (uint)0xC08181FF)]
        [DataRow(7, 255, (uint)0x818181FF)]
        [DataRow(8, 255, (uint)0x42C0C0FF)]
        [DataRow(9, 255, (uint)0x03C0C0FF)]
        [DataRow(99, 255, (uint)0x818142FF)]
        [DataRow(99, 127, (uint)0x8181427F)]
        [DataRow(99, 0, (uint)0x81814200)]
        public void IntToCol_DistributeValues(int idx, int alpha, uint color)
        {
            var expected = new Color4(color);
            var result = Color4.IntToCol(idx, alpha);
            Assert.AreEqual(expected, result, $"Color {result} <> {expected}");
        }
    }
}
