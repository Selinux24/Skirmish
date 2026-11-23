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
    public class BoundingBoxITests
    {
        public TestContext TestContext { get; set; }

        BoundingBoxI box;

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
            box = new BoundingBoxI(new Vector3I(-3, 2, -5), new Vector3I(4, 9, 1));
        }

        [TestMethod]
        public void ConstructorDefaultAndDimensions_UseInclusiveBoundDifferences()
        {
            BoundingBoxI empty = default;
            Assert.AreEqual(Vector3I.Zero, empty.Minimum);
            Assert.AreEqual(Vector3I.Zero, empty.Maximum);
            Assert.AreEqual(0, empty.Width);
            Assert.AreEqual(0, empty.Height);
            Assert.AreEqual(0, empty.Depth);
            Assert.AreEqual(Vector3I.Zero, empty.Size);

            Assert.AreEqual(new Vector3I(-3, 2, -5), box.Minimum);
            Assert.AreEqual(new Vector3I(4, 9, 1), box.Maximum);
            Assert.AreEqual(7, box.Width);
            Assert.AreEqual(7, box.Height);
            Assert.AreEqual(6, box.Depth);
            Assert.AreEqual(new Vector3I(7, 7, 6), box.Size);
            box.Minimum = new Vector3I(-8, -4, -2);
            box.Maximum = new Vector3I(3, -4, 10);
            Assert.AreEqual(11, box.Width);
            Assert.AreEqual(0, box.Height);
            Assert.AreEqual(12, box.Depth);
            Assert.AreEqual(new Vector3I(11, 0, 12), box.Size);
        }

        [TestMethod]
        public void CornersAndVertices_ReturnEightExpectedCoordinates()
        {
            Vector3I[] expectedCorners = [
                new(-3, 9, 1), new(4, 9, 1), new(4, 2, 1), new(-3, 2, 1),
                new(-3, 9, -5), new(4, 9, -5), new(4, 2, -5), new(-3, 2, -5)];
            Vector3I[] expectedVertices = [
                new(4, 9, 1), new(4, 9, -5), new(-3, 9, -5), new(-3, 9, 1),
                new(4, 2, 1), new(4, 2, -5), new(-3, 2, -5), new(-3, 2, 1)];
            Assert.AreSequenceEqual(expectedCorners, box.GetCorners());
            var destination = new Vector3I[8];
            box.GetCorners(destination);
            Assert.AreSequenceEqual(expectedCorners, destination);
            Assert.AreSequenceEqual(expectedVertices, box.GetVertices());
            for (int i = 0; i < expectedVertices.Length; i++)
            {
                Assert.AreEqual(expectedVertices[i], box.GetVertex(i));
                Assert.AreEqual(expectedVertices[i], box.GetVertex((BoxVertices)i));
            }
            var oversized = new Vector3I[9];
            Assert.ThrowsExactly<ArgumentException>(() => box.GetCorners(oversized));
            Assert.ThrowsExactly<ArgumentException>(() => box.GetCorners(new Vector3I[7]));
            Assert.ThrowsExactly<ArgumentNullException>(() => box.GetCorners(null));
            Assert.ThrowsExactly<ArgumentException>(() => box.GetVertex(8));
            Assert.ThrowsExactly<IndexOutOfRangeException>(() => box.GetVertex(-1));
            Assert.ThrowsExactly<ArgumentException>(() => box.GetVertex((BoxVertices)8));
        }

        [TestMethod]
        public void Rectangles_ProjectTheCorrectAxesAndPreserveZeroDimensions()
        {
            var xy = box.GetRectangleXY();
            Assert.AreEqual(-3, xy.Left);
            Assert.AreEqual(2, xy.Top);
            Assert.AreEqual(4, xy.Right);
            Assert.AreEqual(9, xy.Bottom);
            Assert.AreEqual(7, xy.Width);
            Assert.AreEqual(7, xy.Height);
            var xz = box.GetRectangleXZ();
            Assert.AreEqual(-3, xz.Left);
            Assert.AreEqual(-5, xz.Top);
            Assert.AreEqual(4, xz.Right);
            Assert.AreEqual(1, xz.Bottom);
            Assert.AreEqual(7, xz.Width);
            Assert.AreEqual(6, xz.Height);
            var flat = new BoundingBoxI(new Vector3I(-2, 5, 7), new Vector3I(-2, 5, 7));
            Assert.AreEqual(0, flat.GetRectangleXY().Width);
            Assert.AreEqual(0, flat.GetRectangleXZ().Height);
            Assert.AreEqual(-2, flat.GetRectangleXY().Left);
            Assert.AreEqual(7, flat.GetRectangleXZ().Top);
        }

        [TestMethod]
        public void FromPoints_ComputesComponentBoundsAndHandlesNullEmptyAndSinglePoint()
        {
            Vector3I[] points = [new(5, -4, 6), new(-8, 9, -7), new(2, 3, 1), new(-8, 9, -7)];
            BoundingBoxI.FromPoints(points, out var result);
            Assert.AreEqual(new Vector3I(-8, -4, -7), result.Minimum);
            Assert.AreEqual(new Vector3I(5, 9, 6), result.Maximum);
            BoundingBoxI.FromPoints([new Vector3I(4, -2, 11)], out result);
            Assert.AreEqual(new Vector3I(4, -2, 11), result.Minimum);
            Assert.AreEqual(new Vector3I(4, -2, 11), result.Maximum);
            BoundingBoxI.FromPoints([], out result);
            Assert.AreEqual(new Vector3I(int.MaxValue), result.Minimum);
            Assert.AreEqual(new Vector3I(int.MinValue), result.Maximum);
            Assert.ThrowsExactly<ArgumentNullException>(() => BoundingBoxI.FromPoints(null, out _));
        }

        [TestMethod]
        public void MergeOverloads_ReturnComponentWiseUnionAndPreserveInputs()
        {
            var first = new BoundingBoxI(new Vector3I(-6, -2, -5), new Vector3I(2, 4, 8));
            var second = new BoundingBoxI(new Vector3I(-3, -7, 1), new Vector3I(9, 3, 6));
            var expected = new BoundingBoxI(new Vector3I(-6, -7, -5), new Vector3I(9, 4, 8));
            var copyFirst = first;
            var copySecond = second;
            BoundingBoxI.Merge(ref first, ref second, out var byRef);
            var byValue = BoundingBoxI.Merge(first, second);
            Assert.AreEqual(expected, byRef);
            Assert.AreEqual(expected, byValue);
            Assert.AreEqual(copyFirst, first);
            Assert.AreEqual(copySecond, second);
            Assert.AreEqual(expected, BoundingBoxI.Merge([second, first]));
            Assert.AreEqual(first, BoundingBoxI.Merge([first]));
            Assert.AreEqual(default, BoundingBoxI.Merge([]));
            Assert.ThrowsExactly<ArgumentNullException>(() => BoundingBoxI.Merge(null));
        }

        [TestMethod]
        public void EqualityHashAndFormatting_CoverAllPublicOverloads()
        {
            var first = new BoundingBoxI(new Vector3I(2, 3, 4), new Vector3I(6, 7, 8));
            var same = new BoundingBoxI(new Vector3I(2, 3, 4), new Vector3I(6, 7, 8));
            var otherMinimum = new BoundingBoxI(new Vector3I(1, 3, 4), new Vector3I(6, 7, 8));
            var otherMaximum = new BoundingBoxI(new Vector3I(2, 3, 4), new Vector3I(6, 7, 9));
            Assert.IsTrue(first == same);
            Assert.IsFalse(first != same);
            Assert.IsTrue(first.Equals(same));
            Assert.IsTrue(first.Equals((object)same));
            Assert.IsTrue(first.Equals(ref same));
            Assert.IsFalse(first.Equals(otherMinimum));
            Assert.IsFalse(first.Equals(otherMaximum));
            Assert.IsFalse(first.Equals((object)null));
            Assert.IsFalse(first.Equals("not a BoundingBoxI"));
            Assert.AreEqual(first.GetHashCode(), same.GetHashCode());
            Assert.IsTrue(first != otherMinimum);
            Assert.IsTrue(first != otherMaximum);

            var invariant = CultureInfo.InvariantCulture;
            var plain = first.ToString();
            Assert.AreEqual(plain, first.ToString(invariant));
            Assert.AreEqual("Minimum:X:002 Y:003 Z:004 Maximum:X:006 Y:007 Z:008", first.ToString("D3", invariant));
            Assert.IsTrue(first.ToString("D3").Contains("Minimum:", StringComparison.Ordinal));
            Assert.IsTrue(first.ToString("D3", invariant).Contains("Maximum:X:006", StringComparison.Ordinal));
        }
    }
}
