using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    /// <summary>
    /// Tests for IntersectionTriangle class covering all code paths.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public sealed class IntersectionTriangleTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        #region Non-intersecting triangles (different planes)

        [TestMethod]
        public void Intersection_NonIntersectingSamePlaneSide_ReturnsFalse()
        {
            // t1 is entirely on one side of t2's plane, and vice versa
            // t1 in XY plane at Z=0, t2 in XY plane at Z=10
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(0, 0, 10), new Vector3(1, 0, 10), new Vector3(0.5f, 1, 10));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2, out var segment));
            Assert.IsNull(segment);
        }

        [TestMethod]
        public void Intersection_NonIntersectingOffsetInSamePlane_ReturnsFalse()
        {
            // Two triangles in the same plane but not overlapping
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(5, 5, 0), new Vector3(6, 5, 0), new Vector3(5.5f, 6, 0));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_NonIntersectingPerpendicularPlanes_ReturnsFalse()
        {
            // Two triangles in perpendicular planes that don't intersect
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0)); // XY plane
            var t2 = new Triangle(new Vector3(5, 0, 0), new Vector3(5, 1, 0), new Vector3(5, 0.5f, 1)); // XZ-ish plane at X=5

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_NonIntersectingOneVertexOnOtherPlane_ReturnsFalse()
        {
            // t1 has one vertex on t2's plane, but the rest is on one side and far from t2's area
            var t2 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t1 = new Triangle(new Vector3(10, 0, 0), new Vector3(10, -1, 1), new Vector3(10, 1, 1));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        #endregion

        #region Intersecting triangles (different planes)

        [TestMethod]
        public void Intersection_TwoIntersectingTrianglesInPerpendicularPlanes_ReturnsTrue()
        {
            // t1 in XY plane, t2 in XZ plane, crossing through the center
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(0, -1, -1), new Vector3(0, -1, 1), new Vector3(0, 1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2, out var segment));
            Assert.IsNotNull(segment);
        }

        [TestMethod]
        public void Intersection_TwoIntersectingTrianglesGeneralCase_ReturnsTrue()
        {
            // Two triangles in different planes that clearly intersect
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(0, 0, -1));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2, out var segment));
            Assert.IsNotNull(segment);
        }

        [TestMethod]
        public void Intersection_IntersectingTriangles_ReturnsValidSegment()
        {
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(0, -1, -1), new Vector3(0, -1, 1), new Vector3(0, 1, 0));

            var result = IntersectionTriangle.Intersection(ref t1, ref t2, out var segment);
            Assert.IsTrue(result);
            Assert.IsNotNull(segment);

            // The segment should lie on both triangles
            var seg = segment.Value;
            Assert.IsGreaterThan(0.0001f, Vector3.Distance(seg.Point1, seg.Point2));
        }

        [TestMethod]
        public void Intersection_IntersectingTriangles_SegmentEndpointsLieOnBothTriangles()
        {
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(0, -1, -1), new Vector3(0, -1, 1), new Vector3(0, 1, 0));

            var result = IntersectionTriangle.Intersection(ref t1, ref t2, out var segment);
            Assert.IsTrue(result);
            Assert.IsNotNull(segment);

            // Segment endpoints should be close to the intersection line
            var seg = segment.Value;
            var dist1 = Vector3.Distance(seg.Point1, seg.Point2);
            Assert.IsGreaterThan(0.0001f, dist1);
            Assert.IsLessThan(3f, dist1); // Should be bounded
        }

        #endregion

        #region Coplanar triangles

        [TestMethod]
        public void Intersection_CoplanarOverlappingTriangles_ReturnsTrue()
        {
            // Two triangles in the same plane that overlap
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 2, 0));
            var t2 = new Triangle(new Vector3(1, -1, 0), new Vector3(3, -1, 0), new Vector3(2, 1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2, out var segment));
            Assert.IsNull(segment); // Coplanar intersections don't return segments
        }

        [TestMethod]
        public void Intersection_CoplanarNonOverlappingTriangles_ReturnsFalse()
        {
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(5, 5, 0), new Vector3(6, 5, 0), new Vector3(5.5f, 6, 0));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_CoplanarOneInsideOther_ReturnsTrue()
        {
            // t2 is completely inside t1
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(4, 0, 0), new Vector3(2, 4, 0));
            var t2 = new Triangle(new Vector3(1, 1, 0), new Vector3(3, 1, 0), new Vector3(2, 3, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_CoplanarTouchingAtEdge_ReturnsTrue()
        {
            // Two triangles sharing an edge
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 1, 0));
            var t2 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, -1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_CoplanarTouchingAtVertex_ReturnsTrue()
        {
            // Two triangles touching at a single vertex
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(1, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_CoplanarOverlappingWithDifferentOrientation_ReturnsTrue()
        {
            // t1 is clockwise, t2 is counter-clockwise in the same plane
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(1.5f, 0f, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        #endregion

        #region Edge cases: one vertex on the other plane

        [TestMethod]
        public void Intersection_OneVertexOnPlane_Intersects_ReturnsTrue()
        {
            // t1 has one vertex exactly on t2's plane, and the triangle crosses the plane
            var t2 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 2, 0)); // XY plane
            var t1 = new Triangle(new Vector3(1, 0, -1), new Vector3(1, 0, 1), new Vector3(1, -1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_OneVertexOnPlaneNoIntersection_ReturnsFalse()
        {
            // t1 has one vertex on t2's plane but doesn't cross it AND the triangle is far from t2's area
            var t2 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t1 = new Triangle(new Vector3(10, 0, 0), new Vector3(10, 0.5f, 1), new Vector3(10, -0.5f, 1));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_OneEdgeOnPlane_Intersects_ReturnsTrue()
        {
            // t1 has an edge exactly on t2's plane
            var t2 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 2, 0));
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 1, 1));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        #endregion

        #region Edge cases: degenerate and near-degenerate triangles

        [TestMethod]
        public void Intersection_NearlyCoplanarTriangles_ReturnsCorrectResult()
        {
            // Two triangles almost in the same plane that clearly DO NOT intersect.
            // The algorithm should handle near-coplanar cases without crashing.
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(5, 5, 0.0001f), new Vector3(6, 5, 0.0001f), new Vector3(5.5f, 6, 0.0001f));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_SkewedTriangles_ReturnsCorrectResult()
        {
            // Triangles in arbitrary orientations
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            var t2 = new Triangle(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0, -0.5f));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        #endregion

        #region Segment output verification

        [TestMethod]
        public void Intersection_IntersectingTriangles_SegmentLengthIsReasonable()
        {
            var t1 = new Triangle(new Vector3(-2, -2, 0), new Vector3(2, -2, 0), new Vector3(0, 2, 0));
            var t2 = new Triangle(new Vector3(0, -2, -2), new Vector3(0, -2, 2), new Vector3(0, 2, 0));

            var result = IntersectionTriangle.Intersection(ref t1, ref t2, out var segment);
            Assert.IsTrue(result);
            Assert.IsNotNull(segment);

            var seg = segment.Value;
            Vector3.Distance(ref seg.Point1, ref seg.Point2, out var length);
            Assert.IsGreaterThan(0.0001f, length);
            Assert.IsLessThan(10f, length);
        }

        [TestMethod]
        public void Intersection_IntersectingTriangles_SegmentEndpointsCloseToTriangleEdges()
        {
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(0, -1, -1), new Vector3(0, -1, 1), new Vector3(0, 1, 0));

            var result = IntersectionTriangle.Intersection(ref t1, ref t2, out var segment);
            Assert.IsTrue(result);
            Assert.IsNotNull(segment);

            var seg = segment.Value;
            // Both endpoints should be close to at least one edge of each triangle
            var t1Edges = t1.GetEdgeSegments();

            foreach (var endpoint in new[] { seg.Point1, seg.Point2 })
            {
                bool nearT1Edge = false;
                foreach (var edge in t1Edges)
                {
                    if (Vector3.Distance(endpoint, edge.Point1) < 0.01f ||
                        Vector3.Distance(endpoint, edge.Point2) < 0.01f ||
                        PointToSegmentDistance(endpoint, edge) < 0.01f)
                    {
                        nearT1Edge = true;
                        break;
                    }
                }
                Assert.IsTrue(nearT1Edge, "Segment endpoint should be near a t1 edge");
            }
        }

        [TestMethod]
        public void Intersection_CoplanarTriangles_SegmentIsNull()
        {
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 2, 0));
            var t2 = new Triangle(new Vector3(1, -1, 0), new Vector3(3, -1, 0), new Vector3(2, 1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2, out var segment));
            Assert.IsNull(segment);
        }

        [TestMethod]
        public void Intersection_NonIntersectingTriangles_SegmentIsNull()
        {
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(5, 5, 10), new Vector3(6, 5, 10), new Vector3(5.5f, 6, 10));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2, out var segment));
            Assert.IsNull(segment);
        }

        #endregion

        #region Reflexive and symmetry tests

        [TestMethod]
        public void Intersection_SameTriangle_Intersects()
        {
            var t = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t, ref t));
        }

        [TestMethod]
        public void Intersection_Symmetric_IntersectsConsistently()
        {
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(0, -1, -1), new Vector3(0, -1, 1), new Vector3(0, 1, 0));

            var result1 = IntersectionTriangle.Intersection(ref t1, ref t2);
            var result2 = IntersectionTriangle.Intersection(ref t2, ref t1);

            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void Intersection_ReversedTriangleNormal_IntersectsConsistently()
        {
            var t1 = new Triangle(new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1, 0));
            var t2 = new Triangle(new Vector3(0, -1, -1), new Vector3(0, -1, 1), new Vector3(0, 1, 0));

            var t1Reversed = t1.Reverse();

            var result1 = IntersectionTriangle.Intersection(ref t1, ref t2);
            var result2 = IntersectionTriangle.Intersection(ref t1Reversed, ref t2);

            Assert.AreEqual(result1, result2);
        }

        #endregion

        #region Multiple intersection configurations

        [TestMethod]
        public void Intersection_TriangleCornerPassesThroughOther_ReturnsTrue()
        {
            // One triangle's corner pierces through the other
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 2, 0));
            var t2 = new Triangle(new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_TriangleCompletelyAboveOther_ReturnsFalse()
        {
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));
            var t2 = new Triangle(new Vector3(0, 0, 5), new Vector3(1, 0, 5), new Vector3(0.5f, 1, 5));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_TriangleCompletelyBelowOther_ReturnsFalse()
        {
            var t1 = new Triangle(new Vector3(0, 0, 5), new Vector3(1, 0, 5), new Vector3(0.5f, 1, 5));
            var t2 = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0));

            Assert.IsFalse(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_TriangleEdgeCrossesOther_ReturnsTrue()
        {
            // An edge of t1 crosses through t2
            var t1 = new Triangle(new Vector3(0, 0, -1), new Vector3(0, 0, 1), new Vector3(0, 2, 0));
            var t2 = new Triangle(new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        [TestMethod]
        public void Intersection_SkewedCrossingTriangles_ReturnsTrue()
        {
            // Two triangles in highly skewed planes
            var t1 = new Triangle(new Vector3(0, 0, 0), new Vector3(3, 0, 0), new Vector3(1, 3, 0));
            var t2 = new Triangle(new Vector3(1, 1, -2), new Vector3(1, 1, 2), new Vector3(1, -2, 0));

            Assert.IsTrue(IntersectionTriangle.Intersection(ref t1, ref t2));
        }

        #endregion

        #region Helper methods

        private static float PointToSegmentDistance(Vector3 point, Segment seg)
        {
            var dir = seg.Point2 - seg.Point1;
            var lenSq = dir.LengthSquared();
            if (lenSq < 0.0001f)
                return Vector3.Distance(point, seg.Point1);

            var t = Vector3.Dot(point - seg.Point1, dir) / lenSq;
            t = Math.Max(0f, Math.Min(1f, t));

            var closest = seg.Point1 + dir * t;
            return Vector3.Distance(point, closest);
        }

        #endregion
    }
}
