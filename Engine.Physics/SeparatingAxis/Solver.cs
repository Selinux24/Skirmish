using SharpDX;
using System;

namespace Engine.Physics.SeparatingAxis
{
    /// <summary>
    /// Separating axis helper
    /// </summary>
    public static class Solver
    {
        /// <summary>
        /// Distance tolerance
        /// </summary>
        public const float Tolerance = 0.0001f;

        /// <summary>
        /// Gets whether the boxes collided or not.
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="position">Contact position</param>
        /// <param name="normal">Contact normal</param>
        /// <param name="penetration">Penetration</param>
        /// <returns>Returns true if collision found</returns>
        public static bool BoxAndBox(CollisionBox one, CollisionBox two, out Vector3 position, out Vector3 normal, out float penetration)
        {
            position = Vector3.Zero;
            normal = Vector3.Zero;
            penetration = 0;

            var oneObb = one.OrientedBoundingBox;
            var twoObb = two.OrientedBoundingBox;
            Vector3 toCentre = two.RigidBody.Position - one.RigidBody.Position;

            if (!DetectBestAxis(oneObb, twoObb, toCentre, out var pen, out var best, out var bestSingleAxis))
            {
                return false;
            }

            // We have collision, and we have the axis of collision with less penetration
            if (best < 3)
            {
                // There is a vertex of box two on a face of box one.
                var contactPointFace = FillPointFaceBoxBox(oneObb, twoObb, toCentre, best, pen);

                position = contactPointFace.position;
                normal = contactPointFace.normal;
                penetration = contactPointFace.penetration;

                return true;
            }

            if (best < 6)
            {
                // Swap bodies
                (oneObb, twoObb) = (twoObb, oneObb);

                // There is a vertex of box one on a face of box two.
                var contactPointFace = FillPointFaceBoxBox(oneObb, twoObb, toCentre * -1f, best - 3, pen);

                position = contactPointFace.position;
                normal = contactPointFace.normal;
                penetration = contactPointFace.penetration;

                return true;
            }

            // Edge-to-edge contact.
            var contactEdgeEdge = FillEdgeEdgeBoxBox(oneObb, twoObb, toCentre, best - 6, bestSingleAxis, pen);

            position = contactEdgeEdge.position;
            normal = contactEdgeEdge.normal;
            penetration = contactEdgeEdge.penetration;

            return true;
        }

        /// <summary>
        /// Detects the best collision axis in a box to box collision
        /// </summary>
        /// <param name="oneObb">First box</param>
        /// <param name="twoObb">Second box</param>
        /// <param name="toCenter">To center position</param>
        /// <param name="pen">Penetration value</param>
        /// <param name="best">Best axis</param>
        /// <param name="bestSingleAxis">Best single axis</param>
        /// <returns>Returns true if best axis detected</returns>
        private static bool DetectBestAxis(OrientedBoundingBox oneObb, OrientedBoundingBox twoObb, Vector3 toCenter, out float pen, out uint best, out uint bestSingleAxis)
        {
            pen = float.MaxValue;
            best = uint.MaxValue;
            bestSingleAxis = uint.MaxValue;

            var oneTrn = oneObb.Transformation;

            // Check each axis, storing penetration and the best axis
            var oneLeft = oneTrn.Left;
            if (!TryAxis(oneObb, twoObb, toCenter, oneLeft, 0, ref pen, ref best)) return false;
            var oneUp = oneTrn.Up;
            if (!TryAxis(oneObb, twoObb, toCenter, oneUp, 1, ref pen, ref best)) return false;
            var oneBackward = oneTrn.Backward;
            if (!TryAxis(oneObb, twoObb, toCenter, oneBackward, 2, ref pen, ref best)) return false;

            var twoTrn = twoObb.Transformation;

            var twoLeft = twoTrn.Left;
            if (!TryAxis(oneObb, twoObb, toCenter, twoLeft, 3, ref pen, ref best)) return false;
            var twoUp = twoTrn.Up;
            if (!TryAxis(oneObb, twoObb, toCenter, twoUp, 4, ref pen, ref best)) return false;
            var twoBackward = twoTrn.Backward;
            if (!TryAxis(oneObb, twoObb, toCenter, twoBackward, 5, ref pen, ref best)) return false;

            // Store the best axis so far, in case of being in a parallel axis collision later.
            bestSingleAxis = best;

            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneLeft, twoLeft), 6, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneLeft, twoUp), 7, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneLeft, twoBackward), 8, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneUp, twoLeft), 9, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneUp, twoUp), 10, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneUp, twoBackward), 11, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneBackward, twoLeft), 12, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneBackward, twoUp), 13, ref pen, ref best)) return false;
            if (!TryAxis(oneObb, twoObb, toCenter, Vector3.Cross(oneBackward, twoBackward), 14, ref pen, ref best)) return false;

            // Making sure we have a result.
            if (best == uint.MaxValue)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets whether there is penetration between the projections of the boxes in the specified axis
        /// </summary>
        /// <param name="oneObb">First box</param>
        /// <param name="twoObb">Second box</param>
        /// <param name="toCenter">Distance to center</param>
        /// <param name="axis">Axis</param>
        /// <param name="index">Index</param>
        /// <param name="smallestPenetration">Smallest penetration</param>
        /// <param name="smallestCase">Smallest test case</param>
        /// <returns>Returns true if there has been a penetration</returns>
        private static bool TryAxis(OrientedBoundingBox oneObb, OrientedBoundingBox twoObb, Vector3 toCenter, Vector3 axis, uint index, ref float smallestPenetration, ref uint smallestCase)
        {
            if (axis.LengthSquared() < Tolerance)
            {
                return true;
            }

            axis.Normalize();

            float penetration = PenetrationOnAxis(oneObb, twoObb, axis, toCenter);
            if (penetration < 0)
            {
                return false;
            }

            if (penetration < smallestPenetration)
            {
                smallestPenetration = penetration;
                smallestCase = index;
            }

            return true;
        }
        /// <summary>
        /// Gets the penetration of the projections of the boxes in the specified axis
        /// </summary>
        /// <param name="oneObb">First box</param>
        /// <param name="twoObb">Second box</param>
        /// <param name="axis">Axis</param>
        /// <param name="toCenter">Distance to center</param>
        /// <returns>Returns true if there has been a penetration</returns>
        private static float PenetrationOnAxis(OrientedBoundingBox oneObb, OrientedBoundingBox twoObb, Vector3 axis, Vector3 toCenter)
        {
            // Project the extensions of each box onto the axis
            float oneProject = oneObb.ProjectToVector(axis);
            float twoProject = twoObb.ProjectToVector(axis);

            // Obtain the distance between centers of the boxes on the axis
            float distance = Math.Abs(Vector3.Dot(toCenter, axis));

            // Positive indicates overlap, negative separation
            return oneProject + twoProject - distance;
        }
        /// <summary>
        /// Fills the collision information between two boxes, once it is known that there is vertex-face contact
        /// </summary>
        /// <param name="oneObb">First box</param>
        /// <param name="twoObb">Second box</param>
        /// <param name="toCenter">Distance to center</param>
        /// <param name="best">Best penetration axis</param>
        /// <param name="pen">Minor penetration axis</param>
        private static (Vector3 position, Vector3 normal, float penetration) FillPointFaceBoxBox(OrientedBoundingBox oneObb, OrientedBoundingBox twoObb, Vector3 toCenter, uint best, float pen)
        {
            // We know which is the axis of the collision, but we have to know which face we have to work with
            var normal = GetAxis(oneObb.Transformation, best);
            if (Vector3.Dot(normal, toCenter) > 0f)
            {
                normal *= -1f;
            }

            Vector3 vertex = twoObb.Extents;
            if (Vector3.Dot(twoObb.Transformation.Left, normal) < 0f) vertex.X = -vertex.X;
            if (Vector3.Dot(twoObb.Transformation.Up, normal) < 0f) vertex.Y = -vertex.Y;
            if (Vector3.Dot(twoObb.Transformation.Backward, normal) < 0f) vertex.Z = -vertex.Z;

            var position = Vector3.TransformCoordinate(vertex, twoObb.Transformation);

            return (position, normal, pen);
        }
        /// <summary>
        /// Fills the collision information between two boxes, once it is known that there is edge-edge contact
        /// </summary>
        /// <param name="oneObb">First box</param>
        /// <param name="twoObb">Second box</param>
        /// <param name="toCenter">Distance to center</param>
        /// <param name="best">Best penetration axis</param>
        /// <param name="bestSingleAxis">Best single axis</param>
        /// <param name="pen">Minor penetration axis</param>
        private static (Vector3 position, Vector3 normal, float penetration) FillEdgeEdgeBoxBox(OrientedBoundingBox oneObb, OrientedBoundingBox twoObb, Vector3 toCenter, uint best, uint bestSingleAxis, float pen)
        {
            // Get the common axis.
            uint oneAxisIndex = best / 3;
            uint twoAxisIndex = best % 3;
            Vector3 oneAxis = GetAxis(oneObb.Transformation, oneAxisIndex);
            Vector3 twoAxis = GetAxis(twoObb.Transformation, twoAxisIndex);
            Vector3 axis = Vector3.Cross(oneAxis, twoAxis);
            axis.Normalize();

            // The axis should point from box one to box two.
            if (Vector3.Dot(axis, toCenter) > 0f)
            {
                axis *= -1.0f;
            }

            // We have the axes, but not the edges.

            // Each axis has 4 edges parallel to it, we have to find the 4 of each box.
            // We will look for the point in the center of the edge.
            // We know that its component on the collision axis is 0 and we determine which endpoint on each of the other axes is closest.
            Vector3 vOne = oneObb.Extents;
            Vector3 vTwo = twoObb.Extents;
            float[] ptOnOneEdge = new float[] { vOne.X, vOne.Y, vOne.Z };
            float[] ptOnTwoEdge = new float[] { vTwo.X, vTwo.Y, vTwo.Z };
            for (uint i = 0; i < 3; i++)
            {
                if (i == oneAxisIndex)
                {
                    ptOnOneEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(oneObb.Transformation, i), axis) > 0f)
                {
                    ptOnOneEdge[i] = -ptOnOneEdge[i];
                }

                if (i == twoAxisIndex)
                {
                    ptOnTwoEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(twoObb.Transformation, i), axis) < 0f)
                {
                    ptOnTwoEdge[i] = -ptOnTwoEdge[i];
                }
            }

            vOne.X = ptOnOneEdge[0];
            vOne.Y = ptOnOneEdge[1];
            vOne.Z = ptOnOneEdge[2];

            vTwo.X = ptOnTwoEdge[0];
            vTwo.Y = ptOnTwoEdge[1];
            vTwo.Z = ptOnTwoEdge[2];

            // Go to world coordinates
            vOne = Vector3.TransformCoordinate(vOne, oneObb.Transformation);
            vTwo = Vector3.TransformCoordinate(vTwo, twoObb.Transformation);

            // We have a point and a direction for the colliding edges.
            // We need to find the closest point of the two segments.
            float[] vOneAxis = new float[] { oneObb.Extents.X, oneObb.Extents.Y, oneObb.Extents.Z };
            float[] vTwoAxis = new float[] { twoObb.Extents.X, twoObb.Extents.Y, twoObb.Extents.Z };
            Vector3 vertex = ContactPoint(
                vOne, oneAxis, vOneAxis[oneAxisIndex],
                vTwo, twoAxis, vTwoAxis[twoAxisIndex],
                bestSingleAxis > 2);

            // Fill in the contact.
            return (vertex, axis, pen);
        }
        /// <summary>
        /// Gets the closest point to the segments involved in an edge-to-edge or face-to-edge, or face-to-face collision
        /// </summary>
        /// <param name="pOne"></param>
        /// <param name="dOne"></param>
        /// <param name="oneSize"></param>
        /// <param name="pTwo"></param>
        /// <param name="dTwo"></param>
        /// <param name="twoSize"></param>
        /// <param name="useOne">If true, and the contact point is off edge (face-to-edge collision), only box one will be used, otherwise box two.</param>
        /// <returns>Returns the closest point to the two segments involved in an edge-to-edge collision</returns>
        private static Vector3 ContactPoint(Vector3 pOne, Vector3 dOne, float oneSize, Vector3 pTwo, Vector3 dTwo, float twoSize, bool useOne)
        {
            Vector3 toSt, cOne, cTwo;
            float dpStaOne, dpStaTwo, dpOneTwo, smOne, smTwo;
            float denom, mua, mub;

            smOne = dOne.LengthSquared();
            smTwo = dTwo.LengthSquared();
            dpOneTwo = Vector3.Dot(dTwo, dOne);

            toSt = pOne - pTwo;
            dpStaOne = Vector3.Dot(dOne, toSt);
            dpStaTwo = Vector3.Dot(dTwo, toSt);

            denom = smOne * smTwo - dpOneTwo * dpOneTwo;

            // Zero denominator indicates parallel lines
            if (Math.Abs(denom) < 0.0001f)
            {
                return useOne ? pOne : pTwo;
            }

            mua = (dpOneTwo * dpStaTwo - smTwo * dpStaOne) / denom;
            mub = (smOne * dpStaTwo - dpOneTwo * dpStaOne) / denom;

            // If any of the edges has the closest point out of bounds, the edges are not closed, and we have an edge-to-face collision..
            // The point is in the edge, which we know from the useOne parameter.
            if (mua > oneSize ||
                mua < -oneSize ||
                mub > twoSize ||
                mub < -twoSize)
            {
                return useOne ? pOne : pTwo;
            }
            else
            {
                cOne = pOne + dOne * mua;
                cTwo = pTwo + dTwo * mub;

                return cOne * 0.5f + cTwo * 0.5f;
            }
        }
        /// <summary>
        /// Gets the axis vector value from the specified transform
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        /// <param name="axis">Axis value</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Vector3 GetAxis(Matrix transform, uint axis)
        {
            if (axis == 0) return transform.Left;

            if (axis == 1) return transform.Up;

            if (axis == 2) return transform.Backward;

            throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Axis value must be between 0 and 2");
        }
    }
}
