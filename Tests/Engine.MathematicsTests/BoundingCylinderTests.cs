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
    public sealed class BoundingCylinderTests
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
            var cylinder = new BoundingCylinder(new Vector3(2.25f, -3.5f, 4.75f), 1.25f, 6f);

            AssertVector(new Vector3(2.25f, -3.5f, 4.75f), cylinder.Center);
            Assert.AreEqual(1.25f, cylinder.Radius, Tolerance);
            Assert.AreEqual(6f, cylinder.Height, Tolerance);
            AssertVector(new Vector3(2.25f, -6.5f, 4.75f), cylinder.BasePosition);
            AssertVector(new Vector3(2.25f, -0.5f, 4.75f), cylinder.CapPosition);
        }

        [TestMethod]
        public void Constructor_SmallAndZeroDimensions_RemainsDeterministic()
        {
            var small = new BoundingCylinder(new Vector3(-1f, 0.5f, 2f), 0.001f, 0.002f);
            var zero = new BoundingCylinder(Vector3.Zero, 0f, 0f);

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
            var cylinder = new BoundingCylinder(new Vector3(-8f, -3f, -2f), 2f, 4f);

            AssertVector(new Vector3(-8f, -5f, -2f), cylinder.BasePosition);
            AssertVector(new Vector3(-8f, -1f, -2f), cylinder.CapPosition);
            AssertVector(new Vector3(-8f, -3f, -2f), cylinder.Center);
        }

        [TestMethod]
        public void Contains_InteriorAxisAndSideBoundary_ReturnsExpectedContainment()
        {
            var cylinder = new BoundingCylinder(Vector3.Zero, 2f, 2f);

            Assert.AreEqual(ContainmentType.Contains, cylinder.Contains(new Vector3(0f, 0f, 0f)));
            Assert.AreEqual(ContainmentType.Contains, cylinder.Contains(new Vector3(1f, 0f, 0f)));
            Assert.AreEqual(ContainmentType.Intersects, cylinder.Contains(new Vector3(2f, 0f, 0f)));
        }

        [TestMethod]
        public void Contains_CapsAndExteriorPoints_UsesRadiusAndAxisLimits()
        {
            var cylinder = new BoundingCylinder(Vector3.Zero, 2f, 2f);

            Assert.AreEqual(ContainmentType.Intersects, cylinder.Contains(new Vector3(0f, -1f, 0f)));
            Assert.AreEqual(ContainmentType.Contains, cylinder.Contains(new Vector3(0f, 1f, 0f)));
            Assert.AreEqual(ContainmentType.Disjoint, cylinder.Contains(new Vector3(2.1f, 0f, 0f)));
            Assert.AreEqual(ContainmentType.Disjoint, cylinder.Contains(new Vector3(0f, 4f, 0f)));
        }

        [TestMethod]
        public void Contains_RefAndValueOverloads_AgreeForInsideBoundaryAndOutsidePoints()
        {
            var cylinder = new BoundingCylinder(new Vector3(1f, -2f, 3f), 0.75f, 1.5f);
            var points = new[]
            {
                new Vector3(1.5f, -2.5f, 3f),
                new Vector3(1.75f, -2.5f, 3f),
                new Vector3(1f, -5f, 3f)
            };
            var expected = new[]
            {
                ContainmentType.Contains,
                ContainmentType.Intersects,
                ContainmentType.Disjoint
            };

            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                Assert.AreEqual(expected[i], cylinder.Contains(point));
                Assert.AreEqual(cylinder.Contains(point), cylinder.Contains(ref point));
            }
        }

        [TestMethod]
        public void FromPoints_ThreeDimensionalSet_ComputesAxisAlignedBoundsAndOutResult()
        {
            var points = new[]
            {
                new Vector3(-4f, 2f, -2f),
                new Vector3(6f, -3f, 4f),
                new Vector3(2f, 8f, -6f)
            };

            var cylinder = BoundingCylinder.FromPoints(points);
            BoundingCylinder.FromPoints(points, out var outCylinder);

            Assert.AreEqual(cylinder, outCylinder);
            AssertVector(new Vector3(1f, -3f, -1f), cylinder.Center);
            Assert.AreEqual(11f, cylinder.Height, Tolerance);
            Assert.AreEqual(7.071068f, cylinder.Radius, Tolerance);
            AssertVector(new Vector3(1f, -8.5f, -1f), cylinder.BasePosition);
            AssertVector(new Vector3(1f, 2.5f, -1f), cylinder.CapPosition);
        }

        [TestMethod]
        public void FromPoints_CollinearSet_UsesXZDiagonalAndYSpan()
        {
            var points = new[]
            {
                new Vector3(-4f, 1f, -2f),
                new Vector3(0f, 5f, 2f),
                new Vector3(4f, 9f, 6f)
            };

            var cylinder = BoundingCylinder.FromPoints(points);

            AssertVector(new Vector3(0f, 1f, 2f), cylinder.Center);
            Assert.AreEqual(8f, cylinder.Height, Tolerance);
            Assert.AreEqual(5.656854f, cylinder.Radius, Tolerance);
            AssertVector(new Vector3(0f, -3f, 2f), cylinder.BasePosition);
            AssertVector(new Vector3(0f, 5f, 2f), cylinder.CapPosition);
        }

        [TestMethod]
        public void FromPoints_IdenticalAndSinglePointSets_ProduceZeroDimensions()
        {
            var identicalPoints = new[] { new Vector3(3f, -4f, 5f), new Vector3(3f, -4f, 5f) };
            var singlePoint = new[] { new Vector3(-2f, 7f, 1f) };

            var identical = BoundingCylinder.FromPoints(identicalPoints);
            var single = BoundingCylinder.FromPoints(singlePoint);

            AssertVector(new Vector3(3f, -4f, 5f), identical.Center);
            Assert.AreEqual(0f, identical.Height, Tolerance);
            Assert.AreEqual(0f, identical.Radius, Tolerance);
            AssertVector(new Vector3(-2f, 7f, 1f), single.Center);
            Assert.AreEqual(0f, single.Height, Tolerance);
            Assert.AreEqual(0f, single.Radius, Tolerance);
            AssertVector(single.Center, single.BasePosition);
        }

        [TestMethod]
        public void FromPoints_EmptyArray_UsesSentinelsWithoutThrowing()
        {
            var cylinder = BoundingCylinder.FromPoints([]);
            BoundingCylinder.FromPoints([], out var outCylinder);

            Assert.AreEqual(cylinder.Radius, outCylinder.Radius);
            Assert.AreEqual(cylinder.Height, outCylinder.Height);
            Assert.IsTrue(float.IsPositiveInfinity(cylinder.Radius));
            Assert.IsTrue(float.IsNegativeInfinity(cylinder.Height));
            Assert.AreEqual(0f, cylinder.Center.X, Tolerance);
            Assert.IsTrue(float.IsPositiveInfinity(cylinder.Center.Y));
            Assert.AreEqual(0f, cylinder.Center.Z, Tolerance);
            Assert.IsTrue(float.IsPositiveInfinity(cylinder.BasePosition.Y));
            Assert.IsTrue(float.IsNaN(cylinder.CapPosition.Y));
        }

        [TestMethod]
        public void FromPoints_NullArray_ThrowsArgumentNullExceptionExactly()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => BoundingCylinder.FromPoints(null));
            Assert.ThrowsExactly<ArgumentNullException>(() => BoundingCylinder.FromPoints(null, out _));
        }

        [TestMethod]
        public void Equality_IdenticalAndDifferentCylinders_ComparesAcrossOverloads()
        {
            var left = new BoundingCylinder(new Vector3(1f, 2f, 3f), 4f, 5f);
            var same = new BoundingCylinder(new Vector3(1f, 2f, 3f), 4f, 5f);
            var differentRadius = new BoundingCylinder(new Vector3(1f, 2f, 3f), 4.5f, 5f);
            var differentHeight = new BoundingCylinder(new Vector3(1f, 2f, 3f), 4f, 5.5f);
            var differentCenter = new BoundingCylinder(new Vector3(1.5f, 2f, 3f), 4f, 5f);

            Assert.IsTrue(left.Equals(ref same));
            Assert.IsTrue(left.Equals(same));
            Assert.IsTrue(left.Equals((object)same));
            Assert.IsTrue(left == same);
            Assert.IsFalse(left != same);
            Assert.IsFalse(left.Equals((object)null));
            Assert.IsFalse(left.Equals((object)"cylinder"));
            Assert.IsFalse(left == differentRadius);
            Assert.IsTrue(left != differentRadius);
            Assert.IsFalse(left == differentHeight);
            Assert.IsFalse(left == differentCenter);
        }

        [TestMethod]
        public void GetHashCode_EqualCylinders_ProduceEqualHashes()
        {
            var left = new BoundingCylinder(new Vector3(-2f, 1f, 3f), 0.75f, 4.25f);
            var right = new BoundingCylinder(new Vector3(-2f, 1f, 3f), 0.75f, 4.25f);

            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [TestMethod]
        public void NearEqual_WithinAndBeyondTolerance_FollowsEpsilonContract()
        {
            var original = new BoundingCylinder(new Vector3(1f, 2f, 3f), 4f, 5f);
            var close = new BoundingCylinder(new Vector3(1.005f, 2f, 3f), 4.005f, 5.005f);
            var far = new BoundingCylinder(new Vector3(1.02f, 2f, 3f), 4.02f, 5.02f);

            Assert.IsTrue(BoundingCylinder.NearEqual(original, close, 0.006f));
            Assert.IsFalse(BoundingCylinder.NearEqual(original, far, 0.006f));
            Assert.IsTrue(BoundingCylinder.NearEqual(original, original, 0f));
            Assert.IsFalse(BoundingCylinder.NearEqual(original, close, -0.001f));
            Assert.IsFalse(BoundingCylinder.NearEqual(original, close, 0f));
        }

        [TestMethod]
        public void NearEqual_CenterRadiusAndHeightDifferences_AffectResult()
        {
            var original = new BoundingCylinder(Vector3.Zero, 1f, 2f);
            var shiftedCenter = new BoundingCylinder(new Vector3(0.01f, 0f, 0f), 1f, 2f);
            var changedRadius = new BoundingCylinder(Vector3.Zero, 1.01f, 2f);
            var changedHeight = new BoundingCylinder(Vector3.Zero, 1f, 2.01f);

            Assert.IsTrue(BoundingCylinder.NearEqual(original, shiftedCenter, 0.02f));
            Assert.IsTrue(BoundingCylinder.NearEqual(original, changedRadius, 0.02f));
            Assert.IsTrue(BoundingCylinder.NearEqual(original, changedHeight, 0.02f));
            Assert.IsFalse(BoundingCylinder.NearEqual(original, shiftedCenter, 0.005f));
            Assert.IsFalse(BoundingCylinder.NearEqual(original, changedRadius, 0.005f));
            Assert.IsFalse(BoundingCylinder.NearEqual(original, changedHeight, 0.005f));
        }

        [TestMethod]
        public void ToString_DefaultAndFormatOverloads_RenderBothEndpointsAndRadius()
        {
            var cylinder = new BoundingCylinder(new Vector3(1.25f, -2.5f, 3.75f), 0.5f, 4f);

            var defaultText = cylinder.ToString();
            var formattedText = cylinder.ToString("F1", CultureInfo.InvariantCulture);

            Assert.Contains("Base:", defaultText);
            Assert.Contains("Cap:", defaultText);
            Assert.Contains("Radius:", defaultText);
            Assert.Contains("Base:X:1.2 Y:-4.5 Z:3.8", formattedText);
            Assert.Contains("Cap:X:1.2 Y:-0.5 Z:3.8", formattedText);
            Assert.Contains("Radius:0.5", formattedText);
        }

        [TestMethod]
        public void ToString_ProviderOverloads_HonorCultureAndNullFormatFallback()
        {
            var cylinder = new BoundingCylinder(new Vector3(1.25f, -2.5f, 3.75f), 0.5f, 4f);
            var en = CultureInfo.GetCultureInfo("en-US");
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            var enText = cylinder.ToString("F1", en);
            var frText = cylinder.ToString("F1", fr);

            Assert.AreNotEqual(enText, frText);
            Assert.Contains("Radius:0,5", frText);
            Assert.IsNotNull(cylinder.ToString(en));
            Assert.IsNotNull(cylinder.ToString("F2", en));
            Assert.AreEqual(cylinder.ToString(), cylinder.ToString((string)null));
            Assert.IsNotNull(cylinder.ToString((IFormatProvider)null));
            Assert.IsNotNull(cylinder.ToString((string)null, (IFormatProvider)null));
        }

        [TestMethod]
        public void DefaultValue_HasDeterministicZeroState()
        {
            BoundingCylinder cylinder = default;

            AssertVector(Vector3.Zero, cylinder.Center);
            Assert.AreEqual(0f, cylinder.Radius, Tolerance);
            Assert.AreEqual(0f, cylinder.Height, Tolerance);
            AssertVector(Vector3.Zero, cylinder.BasePosition);
            AssertVector(Vector3.Zero, cylinder.CapPosition);
            Assert.AreEqual(ContainmentType.Intersects, cylinder.Contains(Vector3.Zero));
        }
    }
}


