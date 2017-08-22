using SharpDX;
using System;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Circle obstacle
    /// </summary>
    public struct ObstacleCircle : IObstacle
    {
        private const float EPS = 0.0001f;

        private static bool SweepCircleCircle(Vector3 center0, float radius0, Vector3 v, Vector3 center1, float radius1, out float tmin, out float tmax)
        {
            tmin = float.MinValue;
            tmax = float.MaxValue;

            Vector3 s = center1 - center0;
            float r = radius0 + radius1;
            float c = Helper.Dot2D(ref s, ref s) - r * r;
            float a = Helper.Dot2D(ref v, ref v);
            if (a < EPS)
            {
                //not moving
                return false;
            }

            //overlap, calculate time to exit
            float b = Helper.Dot2D(ref v, ref s);
            float d = b * b - a * c;
            if (d < 0.0f)
            {
                //no intersection
                return false;
            }

            a = 1.0f / a;
            float rd = (float)Math.Sqrt(d);
            tmin = (b - rd) * a;
            tmax = (b + rd) * a;

            return true;
        }

        /// <summary>
        /// The position of the obstacle
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The velocity of the obstacle
        /// </summary>
        public Vector3 Vel;
        /// <summary>
        /// The desired velocity of the obstacle
        /// </summary>
        public Vector3 DesiredVel;
        /// <summary>
        /// The radius of the obstacle
        /// </summary>
        public float Radius;
        /// <summary>
        /// Used for side selection during sampling
        /// </summary>
        public Vector3 Dp;
        /// <summary>
        /// Used for side selection during sampling
        /// </summary>
        public Vector3 Np;

        /// <summary>
        /// Updates the obstacle against a position at a specified desired velocity
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="desiredVelocity">Desired velocity</param>
        public void Update(Vector3 position, Vector3 desiredVelocity)
        {
            Vector3 pa = position;
            Vector3 pb = this.Position;

            Vector3 orig = new Vector3(0, 0, 0);
            this.Dp = pb - pa;
            this.Dp.Normalize();
            Vector3 dv = this.DesiredVel - desiredVelocity;

            float a;
            Helper.Area2D(ref orig, ref this.Dp, ref dv, out a);
            if (a < 0.01f)
            {
                this.Np.X = -this.Dp.Z;
                this.Np.Z = this.Dp.X;
            }
            else
            {
                this.Np.X = this.Dp.Z;
                this.Np.Z = -this.Dp.X;
            }
        }
        /// <summary>
        /// Process sample
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="vcand">Candidate velocity</param>
        /// <param name="vel">Velocity</param>
        /// <param name="tmin">Distance to intersection with obstacle</param>
        /// <param name="side">Intersected side</param>
        /// <returns>Returns true if velocity change needed</returns>
        public bool ProcessSample(Vector3 position, float radius, Vector3 vcand, Vector3 vel, out float tmin, out float side)
        {
            tmin = float.MaxValue;
            side = 0;

            //RVO
            Vector3 vab = vcand * 2;
            vab = vab - vel;
            vab = vab - this.Vel;

            //side
            float numSide = 0;
            side += MathUtil.Clamp(Math.Min(Helper.Dot2D(ref this.Dp, ref vab) * 0.5f + 0.5f, Helper.Dot2D(ref this.Np, ref vab) * 2.0f), 0.0f, 1.0f);
            numSide++;

            float htmin;
            float htmax;
            if (SweepCircleCircle(position, radius, vab, this.Position, this.Radius, out htmin, out htmax))
            {
                //handle overlapping obstacles
                if (htmin < 0.0f && htmax > 0.0f)
                {
                    //avoid more when overlapped
                    htmin = -htmin * 0.5f;
                }

                if (htmin >= 0.0f)
                {
                    tmin = htmin;

                    //normalize side bias
                    if (numSide != 0)
                    {
                        side /= numSide;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
