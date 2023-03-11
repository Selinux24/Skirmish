using SharpDX;
using System;

namespace Engine.Physics.SepAxis
{
    /// <summary>
    /// Separating axis helper
    /// </summary>
    public static class SeparatingAxis
    {
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

            if (!DetectBestAxis(one, two, out var toCentre, out var pen, out var best, out var bestSingleAxis))
            {
                return false;
            }

            // We have collision, and we have the axis of collision with less penetration
            if (best < 3)
            {
                // There is a vertex of box two on a face of box one.
                var contactPointFace = FillPointFaceBoxBox(one, two, toCentre, best, pen);

                position = contactPointFace.position;
                normal = contactPointFace.normal;
                penetration = contactPointFace.penetration;

                return true;
            }

            if (best < 6)
            {
                // There is a vertex of box one on a face of box two.

                // Swap bodies
                (one, two) = (two, one);
                var contactPointFace = FillPointFaceBoxBox(one, two, toCentre * -1f, best - 3, pen);

                position = contactPointFace.position;
                normal = contactPointFace.normal;
                penetration = contactPointFace.penetration;

                return true;
            }

            // Edge-to-edge contact.
            var contactEdgeEdge = FillEdgeEdgeBoxBox(one, two, toCentre, best - 6, bestSingleAxis, pen);

            position = contactEdgeEdge.position;
            normal = contactEdgeEdge.normal;
            penetration = contactEdgeEdge.penetration;

            return true;
        }

        /// <summary>
        /// Detects the best collision axis in a box to box collision
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="toCenter">To center position</param>
        /// <param name="pen">Penetration value</param>
        /// <param name="best">Best axis</param>
        /// <param name="bestSingleAxis">Best single axis</param>
        /// <returns>Returns true if best axis detected</returns>
        private static bool DetectBestAxis(CollisionBox one, CollisionBox two, out Vector3 toCenter, out float pen, out uint best, out uint bestSingleAxis)
        {
            toCenter = two.RigidBody.Position - one.RigidBody.Position;
            pen = float.MaxValue;
            best = uint.MaxValue;
            bestSingleAxis = uint.MaxValue;

            var oneTrn = one.RigidBody.Transform;
            var twoTrn = two.RigidBody.Transform;

            // Check each axis, storing penetration and the best axis
            if (!TryAxis(one, two, oneTrn.Left, toCenter, 0, ref pen, ref best)) return false;
            if (!TryAxis(one, two, oneTrn.Up, toCenter, 1, ref pen, ref best)) return false;
            if (!TryAxis(one, two, oneTrn.Backward, toCenter, 2, ref pen, ref best)) return false;

            if (!TryAxis(one, two, twoTrn.Left, toCenter, 3, ref pen, ref best)) return false;
            if (!TryAxis(one, two, twoTrn.Up, toCenter, 4, ref pen, ref best)) return false;
            if (!TryAxis(one, two, twoTrn.Backward, toCenter, 5, ref pen, ref best)) return false;

            // Store the best axis so far, in case of being in a parallel axis collision later.
            bestSingleAxis = best;

            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Left, twoTrn.Left), toCenter, 6, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Left, twoTrn.Up), toCenter, 7, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Left, twoTrn.Backward), toCenter, 8, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Up, twoTrn.Left), toCenter, 9, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Up, twoTrn.Up), toCenter, 10, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Up, twoTrn.Backward), toCenter, 11, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Backward, twoTrn.Left), toCenter, 12, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Backward, twoTrn.Up), toCenter, 13, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Backward, twoTrn.Backward), toCenter, 14, ref pen, ref best)) return false;

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
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="axis">Axis</param>
        /// <param name="toCenter">Distance to center</param>
        /// <param name="index">Index</param>
        /// <param name="smallestPenetration">Smallest penetration</param>
        /// <param name="smallestCase">Smallest test case</param>
        /// <returns>Returns true if there has been a penetration</returns>
        private static bool TryAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCenter, uint index, ref float smallestPenetration, ref uint smallestCase)
        {
            if (axis.LengthSquared() < 0.0001)
            {
                return true;
            }

            axis.Normalize();

            float penetration = PenetrationOnAxis(one, two, axis, toCenter);
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
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="axis">Axis</param>
        /// <param name="toCenter">Distance to center</param>
        /// <returns>Returns true if there has been a penetration</returns>
        private static float PenetrationOnAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCenter)
        {
            // Project the extensions of each box onto the axis
            float oneProject = one.OrientedBoundingBox.ProjectToVector(axis);
            float twoProject = two.OrientedBoundingBox.ProjectToVector(axis);

            // Obtain the distance between centers of the boxes on the axis
            float distance = Math.Abs(Vector3.Dot(toCenter, axis));

            // Positive indicates overlap, negative separation
            return oneProject + twoProject - distance;
        }
        /// <summary>
        /// Fills the collision information between two boxes, once it is known that there is vertex-face contact
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="toCenter">Distance to center</param>
        /// <param name="best">Best penetration axis</param>
        /// <param name="pen">Minor penetration axis</param>
        private static (Vector3 position, Vector3 normal, float penetration) FillPointFaceBoxBox(CollisionBox one, CollisionBox two, Vector3 toCenter, uint best, float pen)
        {
            // We know which is the axis of the collision, but we have to know which face we have to work with
            var normal = GetAxis(one.RigidBody.Transform, best);
            if (Vector3.Dot(normal, toCenter) > 0f)
            {
                normal *= -1f;
            }

            Vector3 vertex = two.Extents;
            if (Vector3.Dot(two.RigidBody.Transform.Left, normal) < 0f) vertex.X = -vertex.X;
            if (Vector3.Dot(two.RigidBody.Transform.Up, normal) < 0f) vertex.Y = -vertex.Y;
            if (Vector3.Dot(two.RigidBody.Transform.Backward, normal) < 0f) vertex.Z = -vertex.Z;

            var position = Vector3.TransformCoordinate(vertex, two.RigidBody.Transform);

            return (position, normal, pen);
        }
        /// <summary>
        /// Fills the collision information between two boxes, once it is known that there is edge-edge contact
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="toCenter">Distance to center</param>
        /// <param name="best">Best penetration axis</param>
        /// <param name="bestSingleAxis">Best single axis</param>
        /// <param name="pen">Minor penetration axis</param>
        private static (Vector3 position, Vector3 normal, float penetration) FillEdgeEdgeBoxBox(CollisionBox one, CollisionBox two, Vector3 toCenter, uint best, uint bestSingleAxis, float pen)
        {
            // Get the common axis.
            uint oneAxisIndex = best / 3;
            uint twoAxisIndex = best % 3;
            Vector3 oneAxis = GetAxis(one.RigidBody.Transform, oneAxisIndex);
            Vector3 twoAxis = GetAxis(two.RigidBody.Transform, twoAxisIndex);
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
            Vector3 vOne = one.Extents;
            Vector3 vTwo = two.Extents;
            float[] ptOnOneEdge = new float[] { vOne.X, vOne.Y, vOne.Z };
            float[] ptOnTwoEdge = new float[] { vTwo.X, vTwo.Y, vTwo.Z };
            for (uint i = 0; i < 3; i++)
            {
                if (i == oneAxisIndex)
                {
                    ptOnOneEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(one.RigidBody.Transform, i), axis) > 0f)
                {
                    ptOnOneEdge[i] = -ptOnOneEdge[i];
                }

                if (i == twoAxisIndex)
                {
                    ptOnTwoEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(two.RigidBody.Transform, i), axis) < 0f)
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
            vOne = Vector3.TransformCoordinate(vOne, one.RigidBody.Transform);
            vTwo = Vector3.TransformCoordinate(vTwo, two.RigidBody.Transform);

            // We have a point and a direction for the colliding edges.
            // We need to find the closest point of the two segments.
            float[] vOneAxis = new float[] { one.Extents.X, one.Extents.Y, one.Extents.Z };
            float[] vTwoAxis = new float[] { two.Extents.X, two.Extents.Y, two.Extents.Z };
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
