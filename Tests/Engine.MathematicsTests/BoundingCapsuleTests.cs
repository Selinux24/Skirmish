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
    public sealed class BoundingCapsuleTests
    {
        private const float Tolerance = 1e-6f;

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.X, actual.X, Tolerance);
            Assert.AreEqual(expected.Y, actual.Y, Tolerance);
            Assert.AreEqual(expected.Z, actual.Z, Tolerance);
        }

        [TestMethod]
        public void Constructor_PositiveDimensions_ExposesStateAndEndpoints()
        {
            var capsule = new BoundingCapsule(new Vector3(2.25f, -3.5f, 4.75f), 1.25f, 6f);

            AssertVector(new Vector3(2.25f, -3.5f, 4.75f), capsule.Center);
            Assert.AreEqual(1.25f, capsule.Radius, Tolerance);
            Assert.AreEqual(6f, capsule.Height, Tolerance);
            AssertVector(new Vector3(2.25f, -6.5f, 4.75f), capsule.BasePosition);
            AssertVector(new Vector3(2.25f, -0.5f, 4.75f), capsule.CapPosition);
        }

        [TestMethod]
        public void Constructor_SmallAndZeroDimensions_RemainsDeterministic()
        {
            var small = new BoundingCapsule(new Vector3(-1f, 0.5f, 2f), 0.001f, 0.002f);
            var zero = new BoundingCapsule(Vector3.Zero, 0f, 0f);

            Assert.AreEqual(0.001f, small.Radius, Tolerance);
            Assert.AreEqual(0.002f, small.Height, Tolerance);
            AssertVector(new Vector3(-1f, 0.499f, 2f), small.BasePosition);
            AssertVector(new Vector3(-1f, 0.501f, 2f), small.CapPosition);
            Assert.AreEqual(0f, zero.Radius, Tolerance);
            Assert.AreEqual(0f, zero.Height, Tolerance);
            AssertVector(Vector3.Zero, zero.BasePosition);
            AssertVector(Vector3.Zero, zero.CapPosition);
        }

        [TestMethod]
        public void Constructor_NegativeCenter_PreservesSignedCoordinates()
        {
            var capsule = new BoundingCapsule(new Vector3(-8f, -3f, -2f), 2f, 4f);

            AssertVector(new Vector3(-8f, -5f, -2f), capsule.BasePosition);
            AssertVector(new Vector3(-8f, -1f, -2f), capsule.CapPosition);
            AssertVector(new Vector3(-8f, -3f, -2f), capsule.Center);
        }

        [TestMethod]
        public void Contains_InteriorSegmentAndRadiusBoundary_ReturnsExpectedContainment()
        {
            var capsule = new BoundingCapsule(Vector3.Zero, 0.5f, 2f);
            var interior = new Vector3(0f, -1f, 0f);
            var boundary = new Vector3(0.5f, 0f, 0f);
            var outsideRadius = new Vector3(0f, -2f, 0f);

            Assert.AreEqual(ContainmentType.Contains, capsule.Contains(interior));
            Assert.AreEqual(ContainmentType.Intersects, capsule.Contains(boundary));
            Assert.AreEqual(ContainmentType.Disjoint, capsule.Contains(outsideRadius));
        }

        [TestMethod]
        public void Contains_CapsAndOutsideEndpoints_AreClassifiedFromSegmentDistance()
        {
            var capsule = new BoundingCapsule(Vector3.Zero, 0.5f, 2f);
            var baseCenter = new Vector3(0f, -1f, 0f);
            var capCenter = new Vector3(0f, 1f, 0f);
            var beyondEndpoint = new Vector3(0f, 2f, 0f);

            Assert.AreEqual(ContainmentType.Contains, capsule.Contains(baseCenter));
            Assert.AreEqual(ContainmentType.Disjoint, capsule.Contains(capCenter));
            Assert.AreEqual(ContainmentType.Disjoint, capsule.Contains(beyondEndpoint));
        }

        [TestMethod]
        public void Contains_RefAndValueOverloads_Agree()
        {
            var capsule = new BoundingCapsule(new Vector3(1f, -2f, 3f), 0.75f, 4f);
            var point = new Vector3(1.5f, -2f, 3f);

            var refPoint = point;
            Assert.AreEqual(capsule.Contains(ref refPoint), capsule.Contains(point));
            Assert.AreEqual(ContainmentType.Contains, capsule.Contains(point));

            var outside = new Vector3(1f, -5f, 3f);
            var refOutside = outside;
            Assert.AreEqual(capsule.Contains(ref refOutside), capsule.Contains(outside));
            Assert.AreEqual(ContainmentType.Disjoint, capsule.Contains(outside));
        }

        [TestMethod]
        public void FromPoints_ThreeDimensionalSet_ComputesBoundsAndMatchingOutResult()
        {
            var points = new[]
            {
                new Vector3(-5f, 2f, 0f),
                new Vector3(7f, -3f, 4f),
                new Vector3(1f, 9f, -8f)
            };

            var capsule = BoundingCapsule.FromPoints(points);
            BoundingCapsule.FromPoints(points, out var outCapsule);

            Assert.AreEqual(capsule, outCapsule);
            AssertVector(new Vector3(1f, -3f, -2f), capsule.Center);
            Assert.AreEqual(12f, capsule.Height, Tolerance);
            Assert.AreEqual(8.485281f, capsule.Radius, 1e-5f);
            AssertVector(new Vector3(1f, -9f, -2f), capsule.BasePosition);
            AssertVector(new Vector3(1f, 3f, -2f), capsule.CapPosition);
        }

        [TestMethod]
        public void FromPoints_CollinearSet_UsesXZRadiusAndYSpan()
        {
            var points = new[]
            {
                new Vector3(-4f, 1f, -2f),
                new Vector3(0f, 5f, 2f),
                new Vector3(4f, 9f, 6f)
            };

            var capsule = BoundingCapsule.FromPoints(points);

            AssertVector(new Vector3(0f, 1f, 2f), capsule.Center);
            Assert.AreEqual(8f, capsule.Height, Tolerance);
            Assert.AreEqual(5.656854f, capsule.Radius, 1e-5f);
        }

        [TestMethod]
        public void FromPoints_IdenticalAndDegenerateSets_RemainDeterministic()
        {
            var identical = new[] { new Vector3(3f, -4f, 5f), new Vector3(3f, -4f, 5f) };
            var degenerate = new[] { new Vector3(-2f, 7f, 1f) };

            var identicalCapsule = BoundingCapsule.FromPoints(identical);
            var degenerateCapsule = BoundingCapsule.FromPoints(degenerate);

            AssertVector(new Vector3(3f, -4f, 5f), identicalCapsule.Center);
            Assert.AreEqual(0f, identicalCapsule.Height, Tolerance);
            Assert.AreEqual(0f, identicalCapsule.Radius, Tolerance);
            AssertVector(new Vector3(-2f, 7f, 1f), degenerateCapsule.Center);
            Assert.AreEqual(0f, degenerateCapsule.Height, Tolerance);
            Assert.AreEqual(0f, degenerateCapsule.Radius, Tolerance);
        }

        [TestMethod]
        public void FromPoints_EmptyArray_FollowsSentinelImplementationWithoutThrowing()
        {
            var capsule = BoundingCapsule.FromPoints([]);
            BoundingCapsule.FromPoints([], out var outCapsule);

            Assert.AreEqual(capsule.Radius, outCapsule.Radius, Tolerance);
            Assert.AreEqual(capsule.Height, outCapsule.Height, Tolerance);
            Assert.AreEqual(float.PositiveInfinity, capsule.Radius);
            Assert.AreEqual(float.NegativeInfinity, capsule.Height);
            Assert.AreEqual(0f, capsule.Center.X, Tolerance);
            Assert.AreEqual(float.PositiveInfinity, capsule.Center.Y);
            Assert.AreEqual(0f, capsule.Center.Z, Tolerance);
            Assert.AreEqual(float.PositiveInfinity, capsule.BasePosition.Y);
            Assert.IsTrue(float.IsNaN(capsule.CapPosition.Y));
        }

        [TestMethod]
        public void FromPoints_NullArray_ThrowsArgumentNullExceptionExactly()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => BoundingCapsule.FromPoints(null));
            Assert.ThrowsExactly<ArgumentNullException>(() => BoundingCapsule.FromPoints(null, out _));
        }

        [TestMethod]
        public void Equality_IdenticalAndDifferentCapsules_ComparesAcrossOverloads()
        {
            var left = new BoundingCapsule(new Vector3(1f, 2f, 3f), 4f, 5f);
            var same = new BoundingCapsule(new Vector3(1f, 2f, 3f), 4f, 5f);
            var differentRadius = new BoundingCapsule(new Vector3(1f, 2f, 3f), 4.5f, 5f);
            var differentHeight = new BoundingCapsule(new Vector3(1f, 2f, 3f), 4f, 5.5f);
            var differentCenter = new BoundingCapsule(new Vector3(1.5f, 2f, 3f), 4f, 5f);

            Assert.IsTrue(left.Equals(ref same));
            Assert.IsTrue(left.Equals(same));
            Assert.IsTrue(left.Equals((object)same));
            Assert.IsTrue(left == same);
            Assert.IsFalse(left != same);
            Assert.IsFalse(left.Equals((object)null));
            Assert.IsFalse(left.Equals((object)"capsule"));
            Assert.IsFalse(left == differentRadius);
            Assert.IsFalse(left == differentHeight);
            Assert.IsFalse(left == differentCenter);
        }

        [TestMethod]
        public void GetHashCode_EqualCapsules_ProduceEqualHashes()
        {
            var left = new BoundingCapsule(new Vector3(-2f, 1f, 3f), 0.75f, 4.25f);
            var right = new BoundingCapsule(new Vector3(-2f, 1f, 3f), 0.75f, 4.25f);

            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [TestMethod]
        public void NearEqual_WithinAndBeyondTolerance_FollowsEpsilonContract()
        {
            var original = new BoundingCapsule(new Vector3(1f, 2f, 3f), 4f, 5f);
            var close = new BoundingCapsule(new Vector3(1.005f, 2f, 3f), 4.005f, 5.005f);
            var far = new BoundingCapsule(new Vector3(1.02f, 2f, 3f), 4.02f, 5.02f);

            Assert.IsTrue(BoundingCapsule.NearEqual(original, close, 0.006f));
            Assert.IsFalse(BoundingCapsule.NearEqual(original, far, 0.006f));
            Assert.IsTrue(BoundingCapsule.NearEqual(original, original, 0f));
            Assert.IsFalse(BoundingCapsule.NearEqual(original, close, -0.001f));
            Assert.IsFalse(BoundingCapsule.NearEqual(original, close, 0f));
        }

        [TestMethod]
        public void NearEqual_CenterDifferenceAndRadiusDifference_AffectResult()
        {
            var original = new BoundingCapsule(new Vector3(0f, 0f, 0f), 1f, 2f);
            var shiftedCenter = new BoundingCapsule(new Vector3(0.01f, 0f, 0f), 1f, 2f);
            var changedRadius = new BoundingCapsule(Vector3.Zero, 1.01f, 2f);

            Assert.IsTrue(BoundingCapsule.NearEqual(original, shiftedCenter, 0.02f));
            Assert.IsTrue(BoundingCapsule.NearEqual(original, changedRadius, 0.02f));
            Assert.IsFalse(BoundingCapsule.NearEqual(original, shiftedCenter, 0.005f));
            Assert.IsFalse(BoundingCapsule.NearEqual(original, changedRadius, 0.005f));
        }

        [TestMethod]
        public void ToString_DefaultAndFormatOverloads_RenderState()
        {
            var capsule = new BoundingCapsule(new Vector3(1.25f, -2.5f, 3.75f), 0.5f, 4f);

            var defaultText = capsule.ToString();
            var formatText = capsule.ToString("F1");

            Assert.Contains("Base:", defaultText);
            Assert.Contains("Cap:", defaultText);
            Assert.Contains("Radius:", defaultText);
            Assert.Contains("Base:", formatText);
            Assert.Contains("Cap:", formatText);
            Assert.Contains("Radius:", formatText);
        }

        [TestMethod]
        public void ToString_ProviderOverloads_HonorCultureAndNullArguments()
        {
            var capsule = new BoundingCapsule(new Vector3(1.25f, -2.5f, 3.75f), 0.5f, 4f);
            var en = CultureInfo.GetCultureInfo("en-US");
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            var enText = capsule.ToString("F1", en);
            var frText = capsule.ToString("F1", fr);

            Assert.AreNotEqual(enText, frText);
            Assert.IsNotNull(capsule.ToString(en));
            Assert.IsNotNull(capsule.ToString("F2", en));
            Assert.AreEqual(capsule.ToString(), capsule.ToString((string)null));
            Assert.IsNotNull(capsule.ToString((IFormatProvider)null));
            Assert.IsNotNull(capsule.ToString((string)null, (IFormatProvider)null));
        }

        [TestMethod]
        public void DefaultValue_HasDeterministicZeroState()
        {
            BoundingCapsule capsule = default;

            AssertVector(Vector3.Zero, capsule.Center);
            Assert.AreEqual(0f, capsule.Radius, Tolerance);
            Assert.AreEqual(0f, capsule.Height, Tolerance);
            AssertVector(Vector3.Zero, capsule.BasePosition);
            AssertVector(Vector3.Zero, capsule.CapPosition);
        }
    }
}
