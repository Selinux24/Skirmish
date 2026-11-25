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
    public sealed class ColorTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod]
        public void Constructors_ValidInputs_AssignExpectedValues()
        {
            // Scalar byte
            Assert.AreEqual(new Color(255, 255, 255, 255), new Color((byte)255));
            Assert.AreEqual(new Color(0, 0, 0, 0), new Color((byte)0));
            Assert.AreEqual(new Color(128, 128, 128, 128), new Color((byte)128));

            // uint rgba
            Assert.AreEqual(new Color(255, 0, 17, 34), new Color(0xFF001122u));

            // Float scalar
            Assert.AreEqual(new Color(0, 0, 0, 0), new Color(0f));
            Assert.AreEqual(new Color(127, 127, 127, 127), new Color(0.5f));
            Assert.AreEqual(new Color(255, 255, 255, 255), new Color(1f));

            // Byte components (3 and 4)
            Assert.AreEqual(new Color(255, 128, 64, 255), new Color((byte)255, (byte)128, (byte)64));
            Assert.AreEqual(new Color(255, 128, 64, 32), new Color((byte)255, (byte)128, (byte)64, (byte)32));

            // Float components (3 and 4)
            Assert.AreEqual(new Color(255, 127, 63, 255), new Color(1f, 0.5f, 0.25f));
            Assert.AreEqual(new Color(255, 127, 63, 31), new Color(1f, 0.5f, 0.25f, 0.125f));

            // Vector3 (2 overloads)
            Assert.AreEqual(new Color(255, 127, 63, 255), new Color(new Vector3(1f, 0.5f, 0.25f)));
            Assert.AreEqual(new Color(255, 127, 63, 31), new Color(new Vector3(1f, 0.5f, 0.25f), 0.125f));

            // Vector4
            Assert.AreEqual(new Color(255, 127, 63, 31), new Color(new Vector4(1f, 0.5f, 0.25f, 0.125f)));

            // byte[]
            Assert.AreEqual(new Color(255, 128, 64, 32), new Color(new byte[] { 255, 128, 64, 32 }));

            // float[]
            Assert.AreEqual(new Color(255, 127, 63, 31), new Color([1f, 0.5f, 0.25f, 0.125f]));
        }

        [TestMethod]
        public void Constructors_IntAndUintClamping_CorrectlyClamped()
        {
            // int constructors - values clamped to [0, 255]
            Assert.AreEqual(new Color(255, 100, 50, 200), new Color(300, 100, 50, 200));
            Assert.AreEqual(new Color(255, 100, 50, 200), new Color(-100, 100, 50, 200));
            Assert.AreEqual(new Color(255, 100, 50, 200), new Color(unchecked((int)uint.MaxValue), 100, 50, 200));

            // uint constructors
            Assert.AreEqual(new Color(255, 100, 50, 200), new Color(300u, 100u, 50u, 200u));

            // int[] constructor
            Assert.AreEqual(new Color(255, 100, 50, 200), new Color([300, 100, 50, 200]));

            // uint[] constructor
            Assert.AreEqual(new Color(255, 100, 50, 200), new Color([300u, 100u, 50u, 200u]));

            // Alpha-only int constructor
            Assert.AreEqual(new Color(255, 100, 50, 255), new Color(300, 100, 50));

            // Alpha-only uint constructor
            Assert.AreEqual(new Color(255, 100, 50, 255), new Color(300u, 100u, 50u));
        }

        [TestMethod]
        public void Constructor_InvalidArrayValues_ThrowExpectedExceptions()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new Color((byte[])null));
            Assert.ThrowsExactly<ArgumentNullException>(() => new Color((int[])null));
            Assert.ThrowsExactly<ArgumentNullException>(() => new Color((uint[])null));
            Assert.ThrowsExactly<ArgumentNullException>(() => new Color((float[])null));

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new byte[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new byte[5]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new int[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new int[5]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new uint[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new uint[5]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new float[3]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Color(new float[5]));
        }

        [TestMethod]
        public void Indexer_ValidAndInvalidAccess()
        {
            var color = new Color(100, 150, 200, 50);

            Assert.AreEqual((byte)100, color[0]);
            Assert.AreEqual((byte)150, color[1]);
            Assert.AreEqual((byte)200, color[2]);
            Assert.AreEqual((byte)50, color[3]);

            color[0] = 200; Assert.AreEqual((byte)200, color.R);
            color[1] = 100; Assert.AreEqual((byte)100, color.G);
            color[2] = 50; Assert.AreEqual((byte)50, color.B);
            color[3] = 200; Assert.AreEqual((byte)200, color.A);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[-1]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[4]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[-1] = 100);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => color[4] = 100);
        }

        [TestMethod]
        public void ArithmeticOperations_AddSubtractModulateScaleNegate_ValueAndRefOut()
        {
            var left = new Color(100, 50, 25, 100);
            var right = new Color(50, 100, 150, 50);

            // Add
            Assert.AreEqual(new Color(150, 150, 175, 150), Color.Add(left, right));
            Color.Add(ref left, ref right, out var add);
            Assert.AreEqual(Color.Add(new Color(100, 50, 25, 100), new Color(50, 100, 150, 50)), add);

            // Subtract
            var subLeft = new Color(100, 100, 100, 100);
            var subRight = new Color(50, 30, 20, 10);
            Assert.AreEqual(new Color(50, 70, 80, 90), Color.Subtract(subLeft, subRight));
            Color.Subtract(ref subLeft, ref subRight, out var subtract);
            Assert.AreEqual(Color.Subtract(new Color(100, 100, 100, 100), new Color(50, 30, 20, 10)), subtract);

            // Modulate
            var modLeft = new Color(255, 128, 64, 255);
            var modRight = new Color(128, 255, 128, 128);
            Assert.AreEqual(new Color(128, 128, 32, 128), Color.Modulate(modLeft, modRight));
            Color.Modulate(ref modLeft, ref modRight, out var modulate);
            Assert.AreEqual(Color.Modulate(new Color(255, 128, 64, 255), new Color(128, 255, 128, 128)), modulate);

            // Scale
            var scaleColor = new Color(100, 150, 200, 50);
            Assert.AreEqual(new Color(200, 44, 144, 100), Color.Scale(scaleColor, 2f));
            Color.Scale(ref scaleColor, 2f, out var scale);
            Assert.AreEqual(Color.Scale(new Color(100, 150, 200, 50), 2f), scale);

            // Negate
            var negateColor = new Color(50, 100, 150, 200);
            Assert.AreEqual(new Color(205, 155, 105, 55), Color.Negate(negateColor));
            Color.Negate(ref negateColor, out var negate);
            Assert.AreEqual(Color.Negate(new Color(50, 100, 150, 200)), negate);
        }

        [TestMethod]
        public void ClampMinMaxAndPremultiply_ExpectedValues()
        {
            // Clamp
            var clampValue = new Color(100, 200, 50, 150);
            var clampMin = new Color(0, 0, 0, 0);
            var clampMax = new Color(128, 128, 128, 128);
            Assert.AreEqual(new Color(100, 128, 50, 128), Color.Clamp(clampValue, clampMin, clampMax));
            Color.Clamp(ref clampValue, ref clampMin, ref clampMax, out var clamp);
            Assert.AreEqual(Color.Clamp(new Color(100, 200, 50, 150), new Color(0), new Color(128)), clamp);

            // Max
            var maxLeft = new Color(100, 50, 200, 150);
            var maxRight = new Color(50, 150, 100, 50);
            Assert.AreEqual(new Color(100, 150, 200, 150), Color.Max(maxLeft, maxRight));
            Color.Max(ref maxLeft, ref maxRight, out var max);
            Assert.AreEqual(Color.Max(new Color(100, 50, 200, 150), new Color(50, 150, 100, 50)), max);

            // Min
            Assert.AreEqual(new Color(50, 50, 100, 50), Color.Min(maxLeft, maxRight));
            Color.Min(ref maxLeft, ref maxRight, out var min);
            Assert.AreEqual(Color.Min(new Color(100, 50, 200, 150), new Color(50, 150, 100, 50)), min);

            // Premultiply
            Assert.AreEqual(new Color(255, 255, 255, 255), Color.Premultiply(new Color(255, 255, 255, 255)));
            Assert.AreEqual(new Color(255, 128, 64, 255), Color.Premultiply(new Color(255, 128, 64, 255)));
            Assert.AreEqual(new Color(127, 127, 127, 128), Color.Premultiply(new Color(255, 255, 255, 128)));
            Assert.AreEqual(new Color(0, 0, 0, 0), Color.Premultiply(new Color(255, 255, 255, 0)));
            Assert.AreEqual(new Color(127, 64, 32, 128), Color.Premultiply(new Color(255, 128, 64, 128)));
        }

        [TestMethod]
        public void LerpAndSmoothStep_ExpectedInterpolation()
        {
            var start = new Color(0, 0, 0, 0);
            var end = new Color(255, 255, 255, 255);

            // Lerp boundaries
            Assert.AreEqual(start, Color.Lerp(start, end, 0f));
            Assert.AreEqual(end, Color.Lerp(start, end, 1f));
            Assert.AreEqual(new Color(127, 127, 127, 127), Color.Lerp(start, end, 0.5f));

            // Lerp ref/out
            var lerpStart = new Color(0, 0, 0, 0);
            var lerpEnd = new Color(255, 255, 255, 255);
            Color.Lerp(ref lerpStart, ref lerpEnd, 0.5f, out var lerp);
            Assert.AreEqual(Color.Lerp(start, end, 0.5f), lerp);

            // SmoothStep boundaries
            Assert.AreEqual(start, Color.SmoothStep(start, end, 0f));
            Assert.AreEqual(end, Color.SmoothStep(start, end, 1f));
            Assert.AreEqual(new Color(127, 127, 127, 127), Color.SmoothStep(start, end, 0.5f));

            // SmoothStep ref/out
            var smoothStart = new Color(0, 0, 0, 0);
            var smoothEnd = new Color(255, 255, 255, 255);
            Color.SmoothStep(ref smoothStart, ref smoothEnd, 0.5f, out var smooth);
            Assert.AreEqual(Color.SmoothStep(start, end, 0.5f), smooth);
        }

        [TestMethod]
        public void AdjustContrastAndSaturation_ExpectedValues()
        {
            // Contrast with factor 1 preserves values
            var contrastColor = new Color(255, 128, 64, 255);
            Assert.AreEqual(contrastColor, Color.AdjustContrast(contrastColor, 1f));

            // Saturation with factor 1 preserves values
            Assert.AreEqual(contrastColor, Color.AdjustSaturation(contrastColor, 1f));

            // Saturation 0 produces grayscale
            var satColor = new Color(255, 128, 64, 255);
            var grayResult = Color.AdjustSaturation(satColor, 0f);
            Assert.AreEqual(grayResult.R, grayResult.G);
            Assert.AreEqual(grayResult.G, grayResult.B);
            Assert.AreEqual(satColor.A, grayResult.A);

            // Grayscale stays grayscale regardless of saturation
            var gray = new Color(128, 128, 128, 255);
            Assert.AreEqual(gray, Color.AdjustSaturation(gray, 2f));
            Assert.AreEqual(gray, Color.AdjustSaturation(gray, 0f));
        }

        [TestMethod]
        public void FactoryMethods_FromBgraAbgrRgba_ReturnExpectedColors()
        {
            // FromBgra: R = (color >> 8) & 255, G = (color >> 16) & 255, B = (color >> 24) & 255, A = color & 255
            Assert.AreEqual(new Color(0x22, 0x11, 0x00, 0x33), Color.FromBgra(0x00112233u));

            // FromAbgr: R = color & 255, G = (color >> 8) & 255, B = (color >> 16) & 255, A = (color >> 24) & 255
            Assert.AreEqual(new Color(0x33, 0x22, 0x11, 0x00), Color.FromAbgr(0x00112233u));

            // FromRgba: R = (color >> 24) & 255, G = (color >> 16) & 255, B = (color >> 8) & 255, A = color & 255
            Assert.AreEqual(new Color(0x33, 0x22, 0x11, 0x00), Color.FromRgba(0x33221100u));
        }

        [TestMethod]
        public void HsbMethods_ReturnExpectedValues()
        {
            // Brightness
            Assert.AreEqual(1f, new Color(255, 255, 255).GetBrightness());
            Assert.AreEqual(0f, new Color(0, 0, 0).GetBrightness());
            Assert.AreEqual(0.5f, new Color(255, 0, 0).GetBrightness());

            // Hue
            Assert.AreEqual(0f, new Color(255, 0, 0).GetHue(), 0.01f);
            Assert.AreEqual(120f, new Color(0, 255, 0).GetHue(), 0.01f);
            Assert.AreEqual(240f, new Color(0, 0, 255).GetHue(), 0.01f);
            Assert.AreEqual(60f, new Color(255, 255, 0).GetHue(), 0.01f);
            Assert.AreEqual(0f, new Color(255, 255, 255).GetHue(), 0.01f);

            // Saturation
            Assert.AreEqual(0f, new Color(255, 255, 255).GetSaturation());
            Assert.AreEqual(0f, new Color(0, 0, 0).GetSaturation());
            Assert.AreEqual(1f, new Color(255, 0, 0).GetSaturation());
            Assert.AreEqual(1f, new Color(0, 255, 0).GetSaturation());
            Assert.AreEqual(1f, new Color(0, 0, 255).GetSaturation());
        }

        [TestMethod]
        public void Operators_AllFamilies_ReturnExpectedResults()
        {
            // Binary +
            Assert.AreEqual(new Color(150, 150, 175, 150), new Color(100, 50, 25, 100) + new Color(50, 100, 150, 50));

            // Binary -
            Assert.AreEqual(new Color(50, 70, 80, 90), new Color(100, 100, 100, 100) - new Color(50, 30, 20, 10));

            // Unary -
            Assert.AreEqual(new Color(155, 205, 230, 155), -new Color(100, 50, 25, 100));

            // Unary +
            var color = new Color(100, 50, 25, 100);
            Assert.AreEqual(color, +color);

            // Scale: * (float, Color)
            Assert.AreEqual(new Color(200, 100, 50, 200), 2f * color);

            // Scale: * (Color, float)
            Assert.AreEqual(new Color(200, 100, 50, 200), color * 2f);

            // Modulate: * (Color, Color)
            Assert.AreEqual(new Color(128, 128, 32, 128), new Color(255, 128, 64, 255) * new Color(128, 255, 128, 128));

            // ==
            Assert.IsTrue(color == new Color(100, 50, 25, 100));
            Assert.IsFalse(color == new Color(100, 50, 25, 101));

            // !=
            Assert.IsFalse(color != new Color(100, 50, 25, 100));
            Assert.IsTrue(color != new Color(100, 50, 25, 101));
        }

        [TestMethod]
        public void ConversionMethods_ToVector3Color3Vector4Color4ArrayBgraRgbaAbgr()
        {
            var color = new Color(255, 128, 64, 32);

            // ToVector3
            var vector3 = color.ToVector3();
            Assert.AreEqual(1f, vector3.X);
            Assert.AreEqual(128f / 255f, vector3.Y, 0.0001f);
            Assert.AreEqual(64f / 255f, vector3.Z, 0.0001f);

            // ToColor3
            var color3 = color.ToColor3();
            Assert.AreEqual(1f, color3.Red);
            Assert.AreEqual(128f / 255f, color3.Green, 0.0001f);
            Assert.AreEqual(64f / 255f, color3.Blue, 0.0001f);

            // ToVector4
            var vector4 = color.ToVector4();
            Assert.AreEqual(1f, vector4.X);
            Assert.AreEqual(128f / 255f, vector4.Y, 0.0001f);
            Assert.AreEqual(64f / 255f, vector4.Z, 0.0001f);
            Assert.AreEqual(32f / 255f, vector4.W, 0.0001f);

            // ToColor4
            var color4 = color.ToColor4();
            Assert.AreEqual(1f, color4.Red);
            Assert.AreEqual(128f / 255f, color4.Green, 0.0001f);
            Assert.AreEqual(64f / 255f, color4.Blue, 0.0001f);
            Assert.AreEqual(32f / 255f, color4.Alpha, 0.0001f);

            // ToArray
            Assert.AreSequenceEqual(new byte[] { 255, 128, 64, 32 }, color.ToArray());
            var arr = color.ToArray();
            arr[0] = 0; // Should not affect original
            Assert.AreEqual((byte)255, color.R);

            // ToBgra
            Assert.AreEqual((uint)0x4080FF20, color.ToBgra());

            // ToRgba
            Assert.AreEqual((uint)0xFF804020, color.ToRgba());

            // ToAbgr
            Assert.AreEqual((uint)0x204080FF, color.ToAbgr());
        }

        [TestMethod]
        public void TypeConversions_ExplicitAndImplicit_ExpectedValues()
        {
            var color = new Color(255, 128, 64, 32);

            // explicit Vector3
            Assert.AreEqual(color.ToVector3(), (Vector3)color);

            // explicit Color from Vector3
            Assert.AreEqual(new Color(255, 127, 63, 255), (Color)new Vector3(1f, 0.5f, 0.25f));

            // implicit Color3
            Assert.AreEqual(color.ToColor3(), (Color3)color);

            // explicit Color from Color3
            Assert.AreEqual(new Color(255, 127, 63, 255), (Color)new Color3(1f, 0.5f, 0.25f));

            // explicit Vector4
            Assert.AreEqual(color.ToVector4(), (Vector4)color);

            // explicit Color from Vector4
            Assert.AreEqual(new Color(255, 127, 63, 31), (Color)new Vector4(1f, 0.5f, 0.25f, 0.125f));

            // implicit Color4
            Assert.AreEqual(color.ToColor4(), (Color4)color);

            // explicit Color from Color4
            Assert.AreEqual(new Color(255, 127, 63, 31), (Color)new Color4(1f, 0.5f, 0.25f, 0.125f));

            // implicit System.Drawing.Color
            var drawingColor = (System.Drawing.Color)color;
            Assert.AreEqual(32, drawingColor.A);
            Assert.AreEqual(255, drawingColor.R);
            Assert.AreEqual(128, drawingColor.G);
            Assert.AreEqual(64, drawingColor.B);

            // implicit System.Drawing.Color to Color
            Assert.AreEqual(new Color(255, 128, 64, 32), (Color)System.Drawing.Color.FromArgb(32, 255, 128, 64));
            Assert.AreEqual(new Color(255, 0, 0, 255), (Color)System.Drawing.Color.Red);
        }

        [TestMethod]
        public void ValueSemantics_EqualsAndHashCode()
        {
            var left = new Color(100, 50, 25, 100);
            var equal = new Color(100, 50, 25, 100);
            var different = new Color(100, 50, 25, 101);

            // == and !=
            Assert.IsTrue(left == equal);
            Assert.IsFalse(left != equal);
            Assert.IsFalse(left == different);
            Assert.IsTrue(left != different);

            // Equals(ref)
            Assert.IsTrue(left.Equals(ref equal));
            Assert.IsFalse(left.Equals(ref different));

            // Equals(Color)
            Assert.IsTrue(left.Equals(equal));
            Assert.IsFalse(left.Equals(different));

            // Equals(object)
            Assert.IsTrue(left.Equals((object)equal));
            Assert.IsFalse(left.Equals((object)different));
            Assert.IsFalse(left.Equals((object)"not a color"));
            Assert.IsFalse(left.Equals(null));

            // GetHashCode
            Assert.AreEqual(left.GetHashCode(), equal.GetHashCode());
        }

        [TestMethod]
        public void Formatting_AllToStringOverloads_ReturnExpectedFormat()
        {
            var color = new Color(255, 128, 64, 32);

            // ToString()
            Assert.AreEqual("A:32 R:255 G:128 B:64", color.ToString());
            Assert.AreEqual("A:32 R:255 G:128 B:64", color.ToString(CultureInfo.CurrentCulture));

            // ToString(format)
            Assert.AreEqual("A:32.00 R:255.00 G:128.00 B:64.00", color.ToString("F2", CultureInfo.InvariantCulture));

            // ToString(format, formatProvider)
            Assert.AreEqual("A:32.00 R:255.00 G:128.00 B:64.00", color.ToString("F2", CultureInfo.InvariantCulture));

            // null format
            Assert.AreEqual("A:32 R:255 G:128 B:64", color.ToString(null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void RefOutOverloads_AreEquivalentToStaticMethods()
        {
            // All ref/out operations should produce the same result as their static counterparts

            var left = new Color(100, 50, 25, 100);
            var right = new Color(50, 100, 150, 50);

            Color.Add(ref left, ref right, out var add);
            Assert.AreEqual(Color.Add(new Color(100, 50, 25, 100), new Color(50, 100, 150, 50)), add);

            var subLeft = new Color(100, 100, 100, 100);
            var subRight = new Color(50, 30, 20, 10);
            Color.Subtract(ref subLeft, ref subRight, out var subtract);
            Assert.AreEqual(Color.Subtract(new Color(100, 100, 100, 100), new Color(50, 30, 20, 10)), subtract);

            var modLeft = new Color(255, 128, 64, 255);
            var modRight = new Color(128, 255, 128, 128);
            Color.Modulate(ref modLeft, ref modRight, out var modulate);
            Assert.AreEqual(Color.Modulate(new Color(255, 128, 64, 255), new Color(128, 255, 128, 128)), modulate);

            var scaleColor = new Color(100, 150, 200, 50);
            Color.Scale(ref scaleColor, 2f, out var scale);
            Assert.AreEqual(Color.Scale(new Color(100, 150, 200, 50), 2f), scale);

            var negateColor = new Color(50, 100, 150, 200);
            Color.Negate(ref negateColor, out var negate);
            Assert.AreEqual(Color.Negate(new Color(50, 100, 150, 200)), negate);

            var clampValue = new Color(100, 200, 50, 150);
            var clampMin = new Color(0, 0, 0, 0);
            var clampMax = new Color(128, 128, 128, 128);
            Color.Clamp(ref clampValue, ref clampMin, ref clampMax, out var clamp);
            Assert.AreEqual(Color.Clamp(new Color(100, 200, 50, 150), new Color(0), new Color(128)), clamp);

            var lerpStart = new Color(0, 0, 0, 0);
            var lerpEnd = new Color(255, 255, 255, 255);
            Color.Lerp(ref lerpStart, ref lerpEnd, 0.5f, out var lerp);
            Assert.AreEqual(Color.Lerp(new Color(0, 0, 0, 0), new Color(255, 255, 255, 255), 0.5f), lerp);

            Color.SmoothStep(ref lerpStart, ref lerpEnd, 0.5f, out var smooth);
            Assert.AreEqual(Color.SmoothStep(new Color(0, 0, 0, 0), new Color(255, 255, 255, 255), 0.5f), smooth);

            Color.Max(ref left, ref right, out var max);
            Assert.AreEqual(Color.Max(new Color(100, 50, 25, 100), new Color(50, 100, 150, 50)), max);

            Color.Min(ref left, ref right, out var min);
            Assert.AreEqual(Color.Min(new Color(100, 50, 25, 100), new Color(50, 100, 150, 50)), min);

            var contrastColor = new Color(255, 128, 64, 255);
            Color.AdjustContrast(ref contrastColor, 1.5f, out var contrast);
            Assert.AreEqual(Color.AdjustContrast(new Color(255, 128, 64, 255), 1.5f), contrast);

            var satColor = new Color(255, 128, 64, 255);
            Color.AdjustSaturation(ref satColor, 0.5f, out var sat);
            Assert.AreEqual(Color.AdjustSaturation(new Color(255, 128, 64, 255), 0.5f), sat);

            var premulColor = new Color(255, 128, 64, 255);
            Color.Premultiply(ref premulColor, out var premul);
            Assert.AreEqual(Color.Premultiply(new Color(255, 128, 64, 255)), premul);
        }
    }
}
