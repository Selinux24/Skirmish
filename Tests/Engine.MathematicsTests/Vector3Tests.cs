using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class Vector3Tests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod]
        public void Constructors_ComponentIndexAndArray_WorkAsExpected()
        {
            var scalar = new Vector3(2f);
            var components = new Vector3(1f, 2f, 3f);
            var fromVector2 = new Vector3(new Vector2(4f, 5f), 6f);
            var fromArray = new Vector3([7f, 8f, 9f]);

            Assert.AreEqual(new Vector3(2f, 2f, 2f), scalar);
            Assert.AreEqual(1f, components[0]);
            Assert.AreEqual(2f, components[1]);
            Assert.AreEqual(3f, components[2]);
            Assert.AreEqual(new Vector3(4f, 5f, 6f), fromVector2);
            Assert.AreEqual(new Vector3(7f, 8f, 9f), fromArray);

            components[1] = 12f;
            Assert.AreEqual(12f, components.Y);
            Assert.AreSequenceEqual([1f, 12f, 3f], components.ToArray());
        }

        [TestMethod]
        public void Constructor_InvalidArrayValues_ThrowExpectedExceptions()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new Vector3(null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Vector3([1f, 2f]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Vector3([1f, 2f, 3f, 4f]));

            var vector = Vector3.One;
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector[3]);
        }

        [TestMethod]
        public void Length_NormalizeAndLimit_ReturnExpectedValues()
        {
            var vector = new Vector3(3f, 4f, 0f);

            Assert.AreEqual(25f, vector.LengthSquared());
            Assert.AreEqual(5f, vector.Length(), 0.0001f);

            vector.Normalize();

            Assert.AreEqual(new Vector3(0.6f, 0.8f, 0f), vector);
            Assert.AreEqual(1f, vector.Length(), 0.0001f);
            Assert.IsTrue(vector.IsNormalized);

            var limited = new Vector3(10f, 0f, 0f).Limit(3f);
            Assert.AreEqual(new Vector3(3f, 0f, 0f), limited);
            Assert.AreEqual(new Vector3(2f, 0f, 0f).Limit(5f), new Vector3(2f, 0f, 0f));

            var zero = Vector3.Zero;
            zero.Normalize();
            Assert.AreEqual(Vector3.Zero, zero);
        }

        [TestMethod]
        public void AddSubtractMultiplyDivide_ReturnComponentWiseResults()
        {
            var left = new Vector3(8f, 6f, 4f);
            var right = new Vector3(2f, 3f, 4f);

            Assert.AreEqual(new Vector3(10f, 9f, 8f), Vector3.Add(left, right));
            Assert.AreEqual(new Vector3(9f, 7f, 5f), Vector3.Add(left, 1f));
            Assert.AreEqual(new Vector3(6f, 3f, 0f), Vector3.Subtract(left, right));
            Assert.AreEqual(new Vector3(7f, 5f, 3f), Vector3.Subtract(left, 1f));
            Assert.AreEqual(new Vector3(16f, 18f, 16f), Vector3.Multiply(left, right));
            Assert.AreEqual(new Vector3(16f, 12f, 8f), Vector3.Multiply(left, 2f));
            Assert.AreEqual(new Vector3(4f, 2f, 1f), Vector3.Divide(left, right));
            Assert.AreEqual(new Vector3(4f, 3f, 2f), Vector3.Divide(left, 2f));
            Assert.AreEqual(new Vector3(1f, 0.6666667f, 0.5f), Vector3.Divide(2f, right));
        }

        [TestMethod]
        public void Operators_AndUnaryOperations_ReturnExpectedVectors()
        {
            var left = new Vector3(1f, 2f, 3f);
            var right = new Vector3(4f, 5f, 6f);

            Assert.AreEqual(new Vector3(5f, 7f, 9f), left + right);
            Assert.AreEqual(new Vector3(-3f, -3f, -3f), left - right);
            Assert.AreEqual(new Vector3(-1f, -2f, -3f), -left);
            Assert.AreEqual(new Vector3(2f, 4f, 6f), left * 2f);
            Assert.AreEqual(new Vector3(2f, 4f, 6f), 2f * left);
            Assert.AreEqual(new Vector3(0.5f, 1f, 1.5f), left / 2f);
            Assert.AreEqual(new Vector3(2f, 3f, 4f), left + 1f);
            Assert.AreEqual(new Vector3(0f, 1f, 2f), left - 1f);
            Assert.AreEqual(new Vector3(4f, 10f, 18f), left * right);
            Assert.AreEqual(new Vector3(0.25f, 0.4f, 0.5f), left / right);
        }

        [TestMethod]
        public void DotCrossDistanceAndLength_ReturnExpectedGeometry()
        {
            var left = new Vector3(1f, 0f, 0f);
            var right = new Vector3(0f, 1f, 0f);

            Assert.AreEqual(0f, Vector3.Dot(left, right));
            Assert.AreEqual(new Vector3(0f, 0f, 1f), Vector3.Cross(left, right));
            Assert.AreEqual(MathF.Sqrt(2f), Vector3.Distance(left, right), 0.0001f);
            Assert.AreEqual(2f, Vector3.DistanceSquared(left, right));
            Assert.AreEqual(MathUtils.PiOver2, Vector3.Angle(left, right), 0.0001f);
        }

        [TestMethod]
        public void Clamp_MinMaxAbsNegateAndReflect_ReturnExpectedVectors()
        {
            var value = new Vector3(5f, -2f, 3f);
            var min = new Vector3(0f, 0f, 0f);
            var max = new Vector3(4f, 4f, 4f);

            Assert.AreEqual(new Vector3(4f, 0f, 3f), Vector3.Clamp(value, min, max));
            Assert.AreEqual(new Vector3(5f, 0f, 3f), Vector3.Max(value, Vector3.Zero));
            Assert.AreEqual(new Vector3(0f, -2f, 0f), Vector3.Min(value, Vector3.Zero));
            Assert.AreEqual(new Vector3(5f, 2f, 3f), Vector3.Abs(value));
            Assert.AreEqual(new Vector3(-5f, 2f, -3f), Vector3.Negate(value));
            Assert.AreEqual(new Vector3(-1f, 0f, 0f), Vector3.Reflect(new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, 0f)));
        }

        [TestMethod]
        public void LerpAndSmoothStep_ReturnInterpolationBoundaries()
        {
            var start = new Vector3(0f, 10f, -10f);
            var end = new Vector3(10f, 20f, 10f);

            Assert.AreEqual(start, Vector3.Lerp(start, end, 0f));
            Assert.AreEqual(new Vector3(5f, 15f, 0f), Vector3.Lerp(start, end, 0.5f));
            Assert.AreEqual(end, Vector3.Lerp(start, end, 1f));
            Assert.AreEqual(start, Vector3.SmoothStep(start, end, 0f));
            Assert.AreEqual(end, Vector3.SmoothStep(start, end, 1f));

            var componentAmounts = new Vector3(0f, 0.5f, 1f);
            Assert.AreEqual(new Vector3(0f, 15f, 10f), Vector3.Lerp(start, end, componentAmounts));
        }

        [TestMethod]
        public void BarycentricAndHermite_ReturnExpectedInterpolation()
        {
            var v1 = new Vector3(0f, 0f, 0f);
            var v2 = new Vector3(10f, 0f, 0f);
            var v3 = new Vector3(0f, 10f, 0f);

            Assert.AreEqual(v1, Vector3.Barycentric(v1, v2, v3, 0f, 0f));
            Assert.AreEqual(v2, Vector3.Barycentric(v1, v2, v3, 1f, 0f));
            Assert.AreEqual(v3, Vector3.Barycentric(v1, v2, v3, 0f, 1f));

            Assert.AreEqual(v1, Vector3.Hermite(v1, Vector3.Zero, v1, Vector3.Zero, 0f));
            Assert.AreEqual(v1, Vector3.Hermite(v1, Vector3.Zero, v1, Vector3.Zero, 1f));
        }

        [TestMethod]
        public void ProjectionPropertiesAndSwizzles_ReturnExpectedValues()
        {
            var vector = new Vector3(1f, 2f, 3f);

            Assert.AreEqual(new Vector2(1f, 2f), vector.XY);
            Assert.AreEqual(new Vector2(3f, 1f), vector.ZX);
            Assert.AreEqual(new Vector3(1f, 1f, 1f), vector.XXX);
            Assert.AreEqual(new Vector3(3f, 3f, 3f), vector.ZZZ);
            Assert.AreEqual(new Vector3(1f, 0f, 0f), Vector3.Project(new Vector3(2f, 0f, 0f), new Vector3(1f, 2f, 3f)));
            Assert.IsFalse(vector.IsZero);
            Assert.IsFalse(vector.IsNaN);
            Assert.IsFalse(vector.IsInfinity);
        }

        [TestMethod]
        public void EqualityFormattingAndHashing_DistinguishNearAndExactVectors()
        {
            var left = new Vector3(1f, 2f, 3f);
            var equal = new Vector3(1f, 2f, 3f);
            var near = new Vector3(1.00001f, 2f, 3f);
            var different = new Vector3(1f, 2f, 4f);

            Assert.IsTrue(left == equal);
            Assert.IsFalse(left != equal);
            Assert.IsTrue(left.Equals(equal));
            Assert.IsTrue(left.Equals((object)equal));
            Assert.IsTrue(Vector3.NearEqual(left, near, 0.001f));
            Assert.IsFalse(Vector3.NearEqual(left, different));
            Assert.AreEqual(left.GetHashCode(), equal.GetHashCode());
            Assert.AreEqual("X:1 Y:2 Z:3", left.ToString());
            Assert.AreEqual("X:1.00 Y:2.00 Z:3.00", left.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ConstantsAndConversions_ReturnExpectedValues()
        {
            Assert.AreEqual(new Vector3(0f, 0f, 0f), Vector3.Zero);
            Assert.AreEqual(new Vector3(1f, 1f, 1f), Vector3.One);
            Assert.AreEqual(new Vector3(0f, 1f, 0f), Vector3.Up);
            Assert.AreEqual(new Vector3(1f, 0f, 0f), Vector3.UnitX);

            var vector = new Vector3(1f, 2f, 3f);
            Vector2 vector2 = (Vector2)vector;
            Vector4 vector4 = (Vector4)vector;
            System.Numerics.Vector3 numerics = (System.Numerics.Vector3)vector;
            var back = (Vector3)numerics;

            Assert.AreEqual(new Vector2(1f, 2f), vector2);
            Assert.AreEqual(3f, vector4.Z);
            Assert.AreEqual(vector, back);
        }

        [TestMethod]
        public void ArrayOperations_InvalidDestinationAndNullInputs_ThrowExpectedExceptions()
        {
            var zeroVector = Vector3.Zero;
            Assert.ThrowsExactly<ArgumentNullException>(() => Vector3.Add(null, ref zeroVector, null));

            Vector3[] source = [Vector3.One];
            Vector3[] shortDestination = [];
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector3.Add(source, ref zeroVector, shortDestination));

            Vector3[] destination = new Vector3[1];
            var oneVector = Vector3.One;
            Vector3.Add(source, ref oneVector, destination);
            Assert.AreEqual(new Vector3(2f, 2f, 2f), destination[0]);
        }
        [TestMethod]
        public void RefOutOverloads_AreEquivalentToStaticMethods()
        {
            var left = new Vector3(6f, 4f, 2f);
            var right = new Vector3(2f, 2f, 2f);

            Vector3.Add(ref left, ref right, out var add); Assert.AreEqual(new Vector3(8f, 6f, 4f), add);
            Vector3.Add(ref left, 1f, out var addScalar); Assert.AreEqual(new Vector3(7f, 5f, 3f), addScalar);
            Vector3.Subtract(ref left, ref right, out var subtract); Assert.AreEqual(new Vector3(4f, 2f, 0f), subtract);
            Vector3.Subtract(ref left, 2f, out var subtractScalar); Assert.AreEqual(new Vector3(4f, 2f, 0f), subtractScalar);
            Vector3.Subtract(10f, ref right, out var subtractLeft); Assert.AreEqual(new Vector3(8f, 8f, 8f), subtractLeft);

            Vector3.Multiply(ref left, 2f, out var multiplyScalar); Assert.AreEqual(new Vector3(12f, 8f, 4f), multiplyScalar);
            Vector3.Multiply(ref left, ref right, out var multiply); Assert.AreEqual(new Vector3(12f, 8f, 4f), multiply);
            Vector3.Divide(ref left, 2f, out var divideScalar); Assert.AreEqual(new Vector3(3f, 2f, 1f), divideScalar);
            Vector3.Divide(12f, ref right, out var divideLeft); Assert.AreEqual(new Vector3(6f, 6f, 6f), divideLeft);
            Vector3.Divide(ref left, ref right, out var divide); Assert.AreEqual(new Vector3(3f, 2f, 1f), divide);

            Vector3.Distance(ref left, ref right, out float distance); Assert.AreEqual(MathF.Sqrt(20f), distance, 0.0001f);
            Vector3.DistanceSquared(ref left, ref right, out float distanceSquared); Assert.AreEqual(20f, distanceSquared);
            Vector3.Dot(ref left, ref right, out float dot); Assert.AreEqual(24f, dot);
        }

        [TestMethod]
        public void ArrayOverloads_ApplyOperationsToAllVectors()
        {
            Vector3[] source = [new Vector3(2f, 4f, 6f), new Vector3(1f, 3f, 5f)];
            var scalar = new Vector3(1f, 1f, 1f);

            var addDestination = new Vector3[2]; Vector3.Add(source, ref scalar, addDestination);
            Assert.AreSequenceEqual([new(3f, 5f, 7f), new(2f, 4f, 6f)], addDestination);

            var subtractDestination = new Vector3[2]; Vector3.Subtract(source, ref scalar, subtractDestination);
            Assert.AreSequenceEqual([new(1f, 3f, 5f), new(0f, 2f, 4f)], subtractDestination);

            var multiplyDestination = new Vector3[2]; Vector3.Multiply(source, ref scalar, multiplyDestination);
            Assert.AreSequenceEqual([new(2f, 4f, 6f), new(1f, 3f, 5f)], multiplyDestination);

            var divideDestination = new Vector3[2]; Vector3.Divide(source, ref scalar, divideDestination);
            Assert.AreSequenceEqual(source, divideDestination);

            var identity = Matrix4x4.Identity;
            Vector4[] transformed4 = new Vector4[2];
            Vector3.Transform(source, ref identity, transformed4);
            Assert.AreEqual(2f, transformed4[0].X); Assert.AreEqual(4f, transformed4[0].Y); Assert.AreEqual(6f, transformed4[0].Z);

            Vector3[] coordinateDestination = new Vector3[2];
            Vector3.TransformCoordinate(source, ref identity, coordinateDestination);
            Assert.AreSequenceEqual(source, coordinateDestination);

            Vector3[] normalDestination = new Vector3[2];
            Vector3.TransformNormal(source, ref identity, normalDestination);
            Assert.AreSequenceEqual(source, normalDestination);

            Assert.ThrowsExactly<ArgumentNullException>(() => Vector3.Subtract(null, ref scalar, new Vector3[1]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector3.Multiply(source, ref scalar, new Vector3[1]));
        }

        [TestMethod]
        public void NormalizeCrossClampLerpAndMinMax_ReturnExpectedValues()
        {
            var source = new Vector3(3f, 4f, 0f);
            Vector3.Normalize(ref source, out var normalized); Assert.AreEqual(new Vector3(0.6f, 0.8f, 0f), normalized);

            var left = new Vector3(1f, 0f, 0f); var right = new Vector3(0f, 1f, 0f);
            Vector3.Cross(ref left, ref right, out var cross); Assert.AreEqual(Vector3.UnitZ, cross);

            var min = Vector3.Zero; var max = Vector3.One;
            Vector3.Clamp(ref source, ref min, ref max, out var clamp); Assert.AreEqual(new Vector3(1f, 1f, 0f), clamp);
            Vector3.Lerp(ref left, ref right, 0.5f, out var lerp); Assert.AreEqual(new Vector3(0.5f, 0.5f, 0f), lerp);
            Vector3.Lerp(ref left, ref right, new Vector3(0f, 0.5f, 1f), out var lerpVector);
            Assert.AreEqual(new Vector3(1f, 0.5f, 0f), lerpVector);
            Vector3.SmoothStep(ref left, ref right, 0.5f, out var smooth); Assert.AreEqual(new Vector3(0.5f, 0.5f, 0f), smooth);

            Vector3.Min(ref left, ref right, out var minimum); Assert.AreEqual(Vector3.Zero, minimum);
            Vector3.Max(ref left, ref right, out var maximum); Assert.AreEqual(new Vector3(1f, 1f, 0f), maximum);
        }

        [TestMethod]
        public void CatmullRomHermiteAndBarycentric_ReturnExpectedInterpolation()
        {
            var p1 = Vector3.Zero; var p2 = Vector3.UnitY; var p3 = Vector3.UnitX + Vector3.UnitY; var p4 = Vector3.UnitX;
            Vector3.CatmullRom(ref p1, ref p2, ref p3, ref p4, 0.5f, out var catmull);
            Assert.AreEqual(new Vector3(0.5f, 1.125f, 0f), catmull);
            Assert.AreEqual(p2, Vector3.CatmullRom(p1, p2, p3, p4, 0f));

            var tangent1 = Vector3.UnitX; var tangent2 = Vector3.UnitX;
            Vector3.Hermite(ref p1, ref tangent1, ref p1, ref tangent2, 0f, out var hermite); Assert.AreEqual(p1, hermite);
            Assert.AreEqual(p1, Vector3.Hermite(p1, tangent1, p1, tangent2, 0f));

            var v1 = Vector3.Zero; var v2 = Vector3.UnitX; var v3 = Vector3.UnitY;
            Vector3.Barycentric(ref v1, ref v2, ref v3, 0.5f, 0.25f, out var barycentric);
            Assert.AreEqual(new Vector3(0.5f, 0.25f, 0f), barycentric);
        }

        [TestMethod]
        public void OrthogonalizeAndOrthonormalize_ReturnOrthogonalVectors()
        {
            Vector3[] source = [Vector3.UnitX, new Vector3(1f, 1f, 0f), new Vector3(1f, 1f, 1f)];
            var orthogonal = new Vector3[3]; Vector3.Orthogonalize(orthogonal, source);
            Assert.AreEqual(Vector3.UnitX, orthogonal[0]);
            Assert.AreEqual(0f, Vector3.Dot(orthogonal[0], orthogonal[1]), 0.0001f);

            var orthonormal = new Vector3[3]; Vector3.Orthonormalize(orthonormal, source);
            Assert.AreEqual(1f, orthonormal[0].Length(), 0.0001f);
            Assert.AreEqual(0f, Vector3.Dot(orthonormal[0], orthonormal[1]), 0.0001f);

            Assert.ThrowsExactly<ArgumentNullException>(() => Vector3.Orthogonalize(new Vector3[1], null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Vector3.Orthonormalize([], Vector3.UnitX));
        }

        [TestMethod]
        public void TransformMethods_QuaternionMatrixAndCoordinates_WorkAsExpected()
        {
            var vector = Vector3.UnitX; var identityQuaternion = Quaternion.Identity;
            Assert.AreEqual(vector, Vector3.Transform(vector, identityQuaternion));
            Vector3.Transform(ref vector, ref identityQuaternion, out var quaternionResult); Assert.AreEqual(vector, quaternionResult);

            Vector3[] quaternionSource = [Vector3.UnitX, Vector3.UnitY];
            var quaternionDestination = new Vector3[2];
            Vector3.Transform(quaternionSource, ref identityQuaternion, quaternionDestination);
            Assert.AreSequenceEqual(quaternionSource, quaternionDestination);

            var identity = Matrix4x4.Identity; var matrix3 = Matrix3x3.Identity;
            Assert.AreEqual(vector, Vector3.Transform(vector, matrix3));
            Vector3.Transform(ref vector, ref matrix3, out var matrix3Result); Assert.AreEqual(vector, matrix3Result);

            Assert.AreEqual(new Vector4(1f, 0f, 0f, 1f), Vector3.Transform(vector, identity));
            Vector3.Transform(ref vector, ref identity, out Vector4 matrix4Result); Assert.AreEqual(new Vector4(1f, 0f, 0f, 1f), matrix4Result);
            Vector3.Transform(ref vector, ref identity, out Vector3 matrix3Vector); Assert.AreEqual(vector, matrix3Vector);

            var translated = Matrix4x4.Translation(10f, 0f, 0f);
            Assert.AreEqual(new Vector3(11f, 0f, 0f), Vector3.TransformCoordinate(vector, translated));
            Assert.AreEqual(vector, Vector3.TransformNormal(vector, translated));
        }

        [TestMethod]
        public void ProjectUnprojectAndReflection_RoundTripExpectedValues()
        {
            var identity = Matrix4x4.Identity; var point = new Vector3(2f, 3f, 4f);
            var projected = Vector3.Project(point, 10f, 20f, 100f, 50f, 0f, 1f, identity);
            Vector3.Project(ref point, 10f, 20f, 100f, 50f, 0f, 1f, ref identity, out var projectedRef);
            Assert.AreEqual(projected, projectedRef);

            var vectorOnAxis = new Vector3(2f, 0f, 0f); var projectedOntoAxis = Vector3.Project(vectorOnAxis, Vector3.UnitX);
            Assert.AreEqual(Vector3.UnitX, projectedOntoAxis);

            var unprojected = Vector3.Unproject(projected, 10f, 20f, 100f, 50f, 0f, 1f, identity);
            Vector3.Unproject(ref projected, 10f, 20f, 100f, 50f, 0f, 1f, ref identity, out var unprojectedRef);
            Assert.AreEqual(unprojected, unprojectedRef);

            var reflected = Vector3.Reflect(Vector3.UnitX, Vector3.Up);
            var unitX = Vector3.UnitX; var up = Vector3.Up;
            Vector3.Reflect(ref unitX, ref up, out var reflectedRef); Assert.AreEqual(reflected, reflectedRef);
        }

        [TestMethod]
        public void AngleSignedAnglesFromVectorAndVectorFromAngles_ReturnExpectedValues()
        {
            var up = Vector3.Up; var right = Vector3.Right;
            Assert.AreEqual(MathUtils.PiOver2, Vector3.AngleSigned(up, right), 0.0001f);
            Vector3.AngleSigned(ref right, ref up, out float signedRef); Assert.AreEqual(MathUtils.PiOver2, signedRef, 0.0001f);

            Vector3.AnglesFromVector(new Vector3(1f, 0f, 1f), out float yaw, out float pitch);
            Assert.AreEqual(MathUtils.PiOver2, yaw, 0.0001f); Assert.AreEqual(MathUtils.PiOver4, pitch, 0.0001f);

            Vector3.VectorFromAngles(0f, 0f, out var fromAngles); Assert.AreEqual(Vector3.Up, fromAngles);
        }

        [TestMethod]
        public void ToStringEqualsDescriptionAndSwizzles_WorkAsExpected()
        {
            var vector = new Vector3(1.25f, -2.5f, 3.75f);
            Assert.AreEqual("X:1.25 Y:-2.50 Z:3.75", vector.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            Assert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture, "X:{0:0.000}; Y:{1:0.000}; Z:{2:0.000}", 1.25f, -2.5f, 3.75f), vector.GetDescription());
            Assert.AreEqual(new Vector2(1.25f, -2.5f), vector.XY);
            Assert.AreEqual(new Vector2(-2.5f, 1.25f), vector.YX);
            Assert.AreEqual(new Vector2(1.25f, 3.75f), vector.XZ);
            Assert.AreEqual(new Vector2(-2.5f, 3.75f), vector.YZ);
            Assert.AreEqual(new Vector2(3.75f, -2.5f), vector.ZY);

            var equal = new Vector3(1.25f, -2.5f, 3.75f);
            Assert.IsTrue(vector.Equals(ref equal)); Assert.IsTrue(vector.Equals(equal)); Assert.IsTrue(vector.Equals((object)equal));
            Assert.IsFalse(vector.Equals((object)"vector"));
            Assert.IsTrue(Vector3.NearEqual(vector, equal));
            Assert.IsTrue(Vector3.NearEqual(vector, equal, new Vector3(0.001f, 0.001f, 0.001f)));
        }

        [TestMethod]
        public void RemainingPublicOverloadsAndSwizzles_AreCovered()
        {
            var negative = new Vector3(-2f, 3f, -4f);
            Vector3.Negate(ref negative, out var negated); Assert.AreEqual(new Vector3(2f, -3f, 4f), negated);
            Vector3.Abs(ref negative, out var absolute); Assert.AreEqual(new Vector3(2f, 3f, 4f), absolute);

            var direction = new Vector3(2f, 0f, 0f); var point = new Vector3(1f, 2f, 3f);
            Vector3.Project(ref direction, ref point, out var projectedPoint);
            Assert.AreEqual(new Vector3(1f, 0f, 0f), projectedPoint);

            var right = Vector3.Right; var up = Vector3.Up;
            Vector3.AngleSigned(ref right, ref up, ref up, out float signedRef);
            Assert.AreEqual(MathUtils.PiOver2, signedRef, 0.0001f);

            var vector = new Vector3(2f, 3f, 4f);
            Assert.AreEqual(new Vector3(3f, 3f, 3f), vector.YYY);
            Assert.AreEqual(new Vector2(4f, 2f), vector.ZX);
            Assert.AreEqual(vector.ToString(), vector.ToString(System.Globalization.CultureInfo.CurrentCulture));
            Assert.AreEqual(vector.ToString("F2"), vector.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));
            Assert.AreEqual(Vector3.Down, new Vector3(0f, -1f, 0f));
            Assert.AreEqual(Vector3.Left, new Vector3(-1f, 0f, 0f));
            Assert.AreEqual(Vector3.Right, new Vector3(1f, 0f, 0f));
            Assert.AreEqual(Vector3.ForwardRH, new Vector3(0f, 0f, -1f));
            Assert.AreEqual(Vector3.ForwardLH, new Vector3(0f, 0f, 1f));
            Assert.AreEqual(Vector3.BackwardRH, new Vector3(0f, 0f, 1f));
            Assert.AreEqual(Vector3.BackwardLH, new Vector3(0f, 0f, -1f));
            Assert.AreEqual(Vector3.UnitY, new Vector3(0f, 1f, 0f));
            Assert.AreEqual(Vector3.UnitZ, new Vector3(0f, 0f, 1f));
        }
        [TestMethod]
        public void RemainingOperatorsConversionsAndConstants_WorkAsExpected()
        {
            var left = new Vector3(1f, 2f, 3f); var right = new Vector3(2f, 2f, 2f);
            Assert.AreEqual(new Vector3(3f, 4f, 5f), 2f + left);
            Assert.AreEqual(new Vector3(1f, 0f, -1f), 2f - left);
            Assert.AreEqual(left, +left);
            Assert.AreEqual(new Vector3(2f, 1f, 0.6666667f), right / left);

            var numerics = new System.Numerics.Vector3(4f, 5f, 6f);
            var converted = (Vector3)numerics;
            Assert.AreEqual(numerics, (System.Numerics.Vector3)converted);
            Assert.AreEqual(Vector3.SizeInBytes, System.Runtime.InteropServices.Marshal.SizeOf<Vector3>());
            Assert.IsTrue(new Vector3(float.PositiveInfinity, 0f, 0f).IsInfinity);
            Assert.IsTrue(new Vector3(float.NaN, 0f, 0f).IsNaN);
        }

        [TestMethod]
        public void CalculateNormalLH_TriangleInXYPlane_ReturnsPositiveZ()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 0, 0);
            var p3 = new Vector3(0, 1, 0);

            Vector3.CalculateNormalLH(ref p1, ref p2, ref p3, out Vector3 normal);

            Assert.IsTrue(Vector3.NearEqual(normal, Vector3.ForwardLH), $"Expected ({Vector3.ForwardLH}), got ({normal})");
        }
        [TestMethod]
        public void CalculateNormalLH_ArbitraryTriangle_ReturnsCorrectNormal()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 1, 0);
            var p3 = new Vector3(0, 1, 1);

            Vector3.CalculateNormalLH(ref p1, ref p2, ref p3, out Vector3 normal);

            var expected = new Vector3(1, -1, 1);
            expected.Normalize();

            Assert.IsTrue(Vector3.NearEqual(normal, expected), $"Expected ({expected}), got ({normal})");
        }

        [TestMethod]
        public void CalculateNormalRH_TriangleInXYPlane_ReturnsNegativeZ()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 0, 0);
            var p3 = new Vector3(0, 1, 0);

            Vector3.CalculateNormalRH(ref p1, ref p2, ref p3, out Vector3 normal);

            Assert.IsTrue(Vector3.NearEqual(normal, Vector3.BackwardLH), $"Expected ({Vector3.BackwardLH}), got ({normal})");
        }
        [TestMethod]
        public void CalculateNormalRH_ArbitraryTriangle_ReturnsCorrectNormal()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 1, 0);
            var p3 = new Vector3(0, 1, 1);

            Vector3.CalculateNormalRH(ref p1, ref p2, ref p3, out Vector3 normal);

            var expected = new Vector3(-1, 1, -1);
            expected.Normalize();

            Assert.IsTrue(Vector3.NearEqual(normal, expected), $"Expected ({expected}), got ({normal})");
        }

        [TestMethod]
        public void CalculateNormalLH_CollinearPoints_ReturnsNaN()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 1, 1);
            var p3 = new Vector3(2, 2, 2);

            Vector3.CalculateNormalLH(ref p1, ref p2, ref p3, out Vector3 normal);

            // La implementación actual no valida longitud cero antes de normalizar, 
            // por lo que devuelve NaN. Esta prueba documenta dicho comportamiento.
            Assert.IsTrue(float.IsNaN(normal.X) && float.IsNaN(normal.Y) && float.IsNaN(normal.Z),
                "NaN was expected for collinear points due to division by zero during normalization.");
        }

        [TestMethod]
        public void CalculateNormalRH_CollinearPoints_ReturnsNaN()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 1, 1);
            var p3 = new Vector3(2, 2, 2);

            Vector3.CalculateNormalRH(ref p1, ref p2, ref p3, out Vector3 normal);

            Assert.IsTrue(float.IsNaN(normal.X) && float.IsNaN(normal.Y) && float.IsNaN(normal.Z),
                "NaN was expected for collinear points due to division by zero during normalization.");
        }
    }
}
