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
    public sealed class Vector2Tests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod]
        public void ConstantsAndState_ValidValuesAndPredicates()
        {
            Assert.AreEqual(8, Vector2.SizeInBytes);
            Assert.AreEqual(Vector2.Zero, new Vector2(0f, 0f));
            Assert.AreEqual(Vector2.One, new Vector2(1f, 1f));
            Assert.AreEqual(Vector2.UnitX, new Vector2(1f, 0f));
            Assert.AreEqual(Vector2.UnitY, new Vector2(0f, 1f));
            var vector = new Vector2(0.6f, 0.8f);
            Assert.IsTrue(vector.IsNormalized); Assert.IsFalse(vector.IsZero); Assert.IsFalse(vector.IsInfinity); Assert.IsFalse(vector.IsNaN);
            Assert.IsTrue(Vector2.Zero.IsZero);
            Assert.IsTrue(new Vector2(float.PositiveInfinity, 0f).IsInfinity);
            Assert.IsTrue(new Vector2(float.NaN, 0f).IsNaN);
            vector[0] = 1f; vector[1] = 0f;
            Assert.AreEqual(1f, vector.X); Assert.AreEqual(0f, vector.Y);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector[-1]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector[2]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector[-1] = 1f);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector[2] = 1f);
        }

        [TestMethod]
        public void Constructors_ValidAndInvalidInputs()
        {
            Assert.AreEqual(new Vector2(2f, 2f), new Vector2(2f));
            Assert.AreEqual(new Vector2(1f, 2f), new Vector2(1f, 2f));
            Assert.AreEqual(new Vector2(3f, 4f), new Vector2([3f, 4f]));
            Assert.ThrowsExactly<ArgumentNullException>(() => new Vector2(null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Vector2([1f]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Vector2([1f, 2f, 3f]));
        }

        [TestMethod]
        public void LengthNormalizeAndToArray_StateAndValues()
        {
            var vector = new Vector2(3f, 4f);
            Assert.AreEqual(25f, vector.LengthSquared());
            Assert.AreEqual(5f, vector.Length(), 0.0001f);
            Assert.AreSequenceEqual([3f, 4f], vector.ToArray());
            vector.Normalize();
            Assert.AreEqual(0.6f, vector.X, 0.0001f); Assert.AreEqual(0.8f, vector.Y, 0.0001f);
            Assert.IsTrue(vector.IsNormalized);
            var unit = Vector2.UnitX; unit.Normalize(); Assert.AreEqual(Vector2.UnitX, unit);
            var zero = Vector2.Zero; zero.Normalize(); Assert.AreEqual(Vector2.Zero, zero);
            var array = vector.ToArray(); array[0] = 99f; Assert.AreEqual(0.6f, vector.X, 0.0001f);
        }

        [TestMethod]
        public void ArithmeticFamilies_ValueAndRefOut_ExpectedParity()
        {
            var left = new Vector2(8f, 6f); var right = new Vector2(-2f, 3f);
            Assert.AreEqual(new Vector2(6f, 9f), Vector2.Add(left, right));
            Assert.AreEqual(new Vector2(9f, 7f), Vector2.Add(left, 1f));
            Assert.AreEqual(new Vector2(10f, 3f), Vector2.Subtract(left, right));
            Assert.AreEqual(new Vector2(7f, 5f), Vector2.Subtract(left, 1f));
            Assert.AreEqual(new Vector2(0f, -5f), Vector2.Subtract(-2f, right));
            Assert.AreEqual(new Vector2(-16f, 18f), Vector2.Multiply(left, right));
            Assert.AreEqual(new Vector2(16f, 12f), Vector2.Multiply(left, 2f));
            Assert.AreEqual(new Vector2(-4f, 2f), Vector2.Divide(left, right));
            Assert.AreEqual(new Vector2(4f, 3f), Vector2.Divide(left, 2f));
            Assert.AreEqual(new Vector2(-8f, -6f), Vector2.Negate(left));
            Assert.AreEqual(new Vector2(8f, 6f), Vector2.Abs(new Vector2(-8f, 6f)));

            Vector2.Add(ref left, ref right, out var add); Assert.AreEqual(Vector2.Add(left, right), add);
            Vector2.Add(ref left, 1f, out var addScalar); Assert.AreEqual(Vector2.Add(left, 1f), addScalar);
            Vector2.Subtract(ref left, ref right, out var subtract); Assert.AreEqual(Vector2.Subtract(left, right), subtract);
            Vector2.Subtract(ref left, 1f, out var subtractVector); Assert.AreEqual(Vector2.Subtract(left, 1f), subtractVector);
            Vector2.Subtract(10f, ref right, out var subtractScalar); Assert.AreEqual(Vector2.Subtract(10f, right), subtractScalar);
            Vector2.Multiply(ref left, 2f, out var multiplyScalar); Assert.AreEqual(Vector2.Multiply(left, 2f), multiplyScalar);
            Vector2.Multiply(ref left, ref right, out var multiply); Assert.AreEqual(Vector2.Multiply(left, right), multiply);
            Vector2.Divide(ref left, 2f, out var divideScalar); Assert.AreEqual(Vector2.Divide(left, 2f), divideScalar);
            Vector2.Divide(12f, ref right, out var divideLeft); Assert.AreEqual(Vector2.Divide(12f, right), divideLeft);
            Vector2.Divide(ref left, ref right, out var divide); Assert.AreEqual(Vector2.Divide(left, right), divide);
            Vector2.Negate(ref left, out var negate); Assert.AreEqual(Vector2.Negate(left), negate);
            Vector2.Abs(ref left, out var absolute); Assert.AreEqual(Vector2.Abs(left), absolute);
        }

        [TestMethod]
        public void GeometryAndInterpolation_Families_ExpectedValues()
        {
            var v1 = Vector2.Zero; var v2 = Vector2.UnitX; var v3 = Vector2.UnitY;
            Assert.AreEqual(v1, Vector2.Barycentric(v1, v2, v3, 0f, 0f));
            Assert.AreEqual(v2, Vector2.Barycentric(v1, v2, v3, 1f, 0f));
            Assert.AreEqual(v3, Vector2.Barycentric(v1, v2, v3, 0f, 1f));
            Assert.AreEqual(Vector2.One, Vector2.Clamp(new Vector2(5f, 5f), Vector2.Zero, Vector2.One));
            Assert.AreEqual(MathF.Sqrt(2f), Vector2.Distance(Vector2.Zero, Vector2.One), 0.0001f);
            Assert.AreEqual(2f, Vector2.DistanceSquared(Vector2.Zero, Vector2.One));
            Assert.AreEqual(0f, Vector2.Dot(v2, v3));
            Assert.AreEqual(1f, Vector2.Cross2D(v2, v3));
            Assert.AreEqual(MathUtils.PiOver2, Vector2.Angle(v2, v3), 0.0001f);
            Assert.AreEqual(MathUtils.PiOver2, Vector2.AngleSigned(v2, v3), 0.0001f);
            Assert.AreEqual(v1, Vector2.Lerp(v1, v2, 0f));
            Assert.AreEqual(new Vector2(0.5f, 0f), Vector2.Lerp(v1, v2, 0.5f));
            Assert.AreEqual(v2, Vector2.Lerp(v1, v2, 1f));
            Assert.AreEqual(new Vector2(0f, 1f), Vector2.Lerp(v1, Vector2.One, new Vector2(0f, 1f)));
            Assert.AreEqual(v1, Vector2.SmoothStep(v1, v2, 0f));
            Assert.AreEqual(v2, Vector2.SmoothStep(v1, v2, 1f));
            Assert.AreEqual(Vector2.One, Vector2.Max(new Vector2(0f, 1f), Vector2.One));
            Assert.AreEqual(Vector2.Zero, Vector2.Min(new Vector2(0f, 1f), Vector2.Zero));

            Vector2.Normalize(ref v2, out var normalized); Assert.AreEqual(Vector2.UnitX, normalized);
            Vector2.Distance(ref v1, ref v2, out float distance); Assert.AreEqual(1f, distance);
            Vector2.DistanceSquared(ref v1, ref v2, out float squared); Assert.AreEqual(1f, squared);
            Vector2.Dot(ref v1, ref v2, out float dot); Assert.AreEqual(0f, dot);
            Vector2.Cross2D(ref v2, ref v3, out float cross); Assert.AreEqual(1f, cross);
            Vector2.Lerp(ref v1, ref v2, 0.5f, out var lerp); Assert.AreEqual(new Vector2(0.5f, 0f), lerp);
            Vector2.SmoothStep(ref v1, ref v2, 0.5f, out var smooth); Assert.AreEqual(new Vector2(0.5f, 0f), smooth);
        }

        [TestMethod]
        public void HermiteCatmullAndRefOverloads_ExpectedInterpolation()
        {
            var p1 = Vector2.Zero; var p2 = Vector2.UnitX; var p3 = Vector2.UnitY; var p4 = Vector2.One;
            Assert.AreEqual(p1, Vector2.Hermite(p1, Vector2.Zero, p1, Vector2.Zero, 0f));
            Assert.AreEqual(p1, Vector2.Hermite(p1, Vector2.Zero, p1, Vector2.Zero, 1f));
            Assert.AreEqual(p2, Vector2.CatmullRom(p1, p2, p3, p4, 0f));
            Vector2.CatmullRom(ref p1, ref p2, ref p3, ref p4, 0.5f, out var catmull);
            Assert.AreEqual(Vector2.CatmullRom(p1, p2, p3, p4, 0.5f), catmull);
            Vector2.Hermite(ref p1, ref p2, ref p3, ref p4, 0.5f, out var hermite);
            Assert.AreEqual(Vector2.Hermite(p1, p2, p3, p4, 0.5f), hermite);
            Vector2.Barycentric(ref p1, ref p2, ref p3, 0.5f, 0.25f, out var bary);
            Assert.AreEqual(Vector2.Barycentric(p1, p2, p3, 0.5f, 0.25f), bary);
        }

        [TestMethod]
        public void ReflectAndOrthogonalize_ValidAndEdgeCases()
        {
            Assert.AreEqual(new Vector2(-1f, 0f), Vector2.Reflect(Vector2.UnitX, Vector2.UnitX));
            var incoming = Vector2.UnitX; var normal = Vector2.UnitX;
            Vector2.Reflect(ref incoming, ref normal, out var reflected); Assert.AreEqual(new Vector2(-1f, 0f), reflected);
            Assert.AreEqual(Vector2.Zero, Vector2.Reflect(Vector2.Zero, Vector2.UnitY));

            Vector2[] source = [Vector2.UnitX, new Vector2(1f, 1f), Vector2.UnitX];
            var orthogonal = new Vector2[3]; Vector2.Orthogonalize(orthogonal, source);
            Assert.AreEqual(Vector2.UnitX, orthogonal[0]);
            Assert.AreEqual(0f, Vector2.Dot(orthogonal[0], orthogonal[1]), 0.0001f);
            var orthonormal = new Vector2[3]; Vector2.Orthonormalize(orthonormal, source);
            Assert.AreEqual(1f, orthonormal[0].Length(), 0.0001f);
            Assert.AreEqual(0f, Vector2.Dot(orthonormal[0], orthonormal[1]), 0.0001f);
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.Orthogonalize(new Vector2[1], null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector2.Orthonormalize([], Vector2.UnitX));
        }

        [TestMethod]
        public void QuaternionTransforms_SingleAndArray_ExpectedParity()
        {
            var vector = Vector2.UnitX; var identity = Quaternion.Identity;
            Assert.AreEqual(vector, Vector2.Transform(vector, identity));
            Vector2.Transform(ref vector, ref identity, out var transformed); Assert.AreEqual(vector, transformed);
            Vector2[] source = [Vector2.UnitX, Vector2.UnitY]; var destination = new Vector2[2];
            Vector2.Transform(source, ref identity, destination); Assert.AreSequenceEqual(source, destination);
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.Transform(null, ref identity, destination));
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.Transform(source, ref identity, null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector2.Transform(source, ref identity, new Vector2[1]));
        }

        [TestMethod]
        public void Matrix4x4Vector4Transform_SingleRefAndArray_ExpectedParity()
        {
            var vector = new Vector2(2f, 3f); var matrix = Matrix4x4.Translation(10f, 20f, 30f);
            Assert.AreEqual(new Vector4(12f, 23f, 30f, 1f), Vector2.Transform(vector, matrix));
            Vector2.Transform(ref vector, ref matrix, out Vector4 result); Assert.AreEqual(new Vector4(12f, 23f, 30f, 1f), result);
            Vector2[] source = [Vector2.UnitX, Vector2.UnitY]; var destination = new Vector4[2];
            Vector2.Transform(source, ref matrix, destination);
            Assert.AreEqual(new Vector4(11f, 20f, 30f, 1f), destination[0]);
            Assert.AreEqual(new Vector4(10f, 21f, 30f, 1f), destination[1]);
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.Transform(null, ref matrix, destination));
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.Transform(source, ref matrix, null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector2.Transform(source, ref matrix, new Vector4[1]));
        }

        [TestMethod]
        public void TransformCoordinate_SingleRefAndArray_ExpectedParity()
        {
            var point = new Vector2(2f, 3f); var identity = Matrix4x4.Identity;
            Assert.AreEqual(point, Vector2.TransformCoordinate(point, identity));
            Vector2.TransformCoordinate(ref point, ref identity, out var result); Assert.AreEqual(point, result);
            var translated = Matrix4x4.Translation(10f, 20f, 30f);
            Assert.AreEqual(new Vector2(12f, 23f), Vector2.TransformCoordinate(point, translated));
            Vector2[] source = [Vector2.UnitX, Vector2.UnitY]; var destination = new Vector2[2];
            Vector2.TransformCoordinate(source, ref translated, destination);
            Assert.AreSequenceEqual([new Vector2(11f, 20f), new Vector2(10f, 21f)], destination);
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.TransformCoordinate(null, ref translated, destination));
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.TransformCoordinate(source, ref translated, null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector2.TransformCoordinate(source, ref translated, new Vector2[1]));
            var wZero = Matrix4x4.Identity; wZero.M44 = 0f;
            var wResult = Vector2.TransformCoordinate(Vector2.One, wZero);
            Assert.IsTrue(float.IsInfinity(wResult.X) || float.IsInfinity(wResult.Y));
        }

        [TestMethod]
        public void TransformNormal_SingleRefAndArray_ExpectedParity()
        {
            var normal = Vector2.UnitX; var scaled = Matrix4x4.Scaling(2f, 3f, 4f);
            Assert.AreEqual(new Vector2(2f, 0f), Vector2.TransformNormal(normal, scaled));
            Vector2.TransformNormal(ref normal, ref scaled, out var result); Assert.AreEqual(new Vector2(2f, 0f), result);
            Vector2[] source = [Vector2.UnitX, Vector2.UnitY]; var destination = new Vector2[2];
            Vector2.TransformNormal(source, ref scaled, destination);
            Assert.AreSequenceEqual([new Vector2(2f, 0f), new Vector2(0f, 3f)], destination);
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.TransformNormal(null, ref scaled, destination));
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector2.TransformNormal(source, ref scaled, null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector2.TransformNormal(source, ref scaled, new Vector2[1]));
        }

        [TestMethod]
        public void Operators_UnaryAndBinary_AllFamilies_ExpectedParity()
        {
            var left = new Vector2(1f, 2f); var right = new Vector2(4f, 5f);
            Assert.AreEqual(new Vector2(5f, 7f), left + right);
            Assert.AreEqual(new Vector2(-3f, -3f), left - right);
            Assert.AreEqual(new Vector2(4f, 10f), left * right);
            Assert.AreEqual(new Vector2(0.25f, 0.4f), left / right);
            Assert.AreEqual(+left, left); Assert.AreEqual(-left, new Vector2(-1f, -2f));
            Assert.AreEqual(new Vector2(2f, 4f), left * 2f); Assert.AreEqual(new Vector2(2f, 4f), 2f * left);
            Assert.AreEqual(new Vector2(0.5f, 1f), left / 2f); Assert.AreEqual(new Vector2(0.5f, 0.4f), 2f / right);
            Assert.AreEqual(new Vector2(2f, 3f), left + 1f); Assert.AreEqual(new Vector2(2f, 3f), 1f + left);
            Assert.AreEqual(new Vector2(0f, 1f), left - 1f); Assert.AreEqual(new Vector2(0f, -1f), 1f - left);
            Assert.IsTrue(left == new Vector2(1f, 2f)); Assert.IsFalse(left != new Vector2(1f, 2f)); Assert.IsFalse(left == right);
        }

        [TestMethod]
        public void Conversions_ExplicitVectorAndNumericsTypes_ExpectedValues()
        {
            var vector = new Vector2(1f, 2f);
            Assert.AreEqual(new Vector3(1f, 2f, 0f), (Vector3)vector);
            Assert.AreEqual(new Vector4(1f, 2f, 0f, 0f), (Vector4)vector);
            var numerics = new System.Numerics.Vector2(3f, 4f);
            var converted = (Vector2)numerics;
            Assert.AreEqual(numerics, (System.Numerics.Vector2)converted);
            Assert.AreEqual(new Vector2(3f, 4f), converted);
        }

        [TestMethod]
        public void ValueSemanticsAndFormatting_AllOverloads_ExpectedBehavior()
        {
            var vector = new Vector2(1.25f, -2.5f); var equal = new Vector2(1.25f, -2.5f);
            var near = new Vector2(1.2504f, -2.5f); var different = new Vector2(1.25f, -2.4f);
            Assert.AreEqual(equal.GetHashCode(), vector.GetHashCode());
            Assert.IsTrue(vector.Equals(ref equal)); Assert.IsTrue(vector.Equals(equal)); Assert.IsTrue(vector.Equals((object)equal));
            Assert.IsFalse(vector.Equals((object)"vector")); Assert.IsFalse(vector.Equals(different));
            Assert.IsTrue(Vector2.NearEqual(vector, near, 0.001f)); Assert.IsFalse(Vector2.NearEqual(vector, different, 0.001f));
            Assert.AreEqual("X:1.25 Y:-2.50", vector.ToString("F2", CultureInfo.InvariantCulture));
            Assert.AreEqual(vector.ToString(), vector.ToString(CultureInfo.CurrentCulture));
            Assert.AreEqual(vector.ToString("F2"), vector.ToString("F2", CultureInfo.CurrentCulture));
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "X:{0:0.000}; Y:{1:0.000}", 1.25f, -2.5f), vector.GetDescription());
        }
    }
}