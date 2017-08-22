using SharpDX;

namespace Engine.PathFinding.NavMesh.Crowds
{
    using Engine.Common;

    /// <summary>
    /// Segment obstacle
    /// </summary>
    public struct ObstacleSegment : IObstacle
    {
        private const float EPS = 0.0001f;

        /// <summary>
        /// First endpoint of the obstacle segment
        /// </summary>
        public Vector3 P;
        /// <summary>
        /// Second endpoint of the obstacle segment
        /// </summary>
        public Vector3 Q;
        /// <summary>
        /// Gets if the obstacle is touched after updated against position
        /// </summary>
        public bool Touched;

        /// <summary>
        /// Updates the obstacle state against a position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="desiredVelocity">Desired velocity</param>
        public void Update(Vector3 position, Vector3 desiredVelocity)
        {
            float t;
            float distance = Intersection.PointToSegment2DSquared(
                position,
                this.P,
                this.Q,
                out t);

            this.Touched = distance < EPS;
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

            float htmin;
            if (this.Touched)
            {
                //special case when the agent is very close to the segment
                Vector3 sdir = this.Q - this.P;
                Vector3 snorm = new Vector3(-sdir.Z, 0, sdir.X);

                //if the velocity is pointing towards the segment, no collision
                if (Helper.Dot2D(ref snorm, ref vcand) < 0.0f)
                {
                    return false;
                }

                //else immediate collision
                htmin = 0.0f;
            }
            else
            {
                Ray ray = new Ray(position, vcand);
                if (!Intersection.RayIntersectsSegment(ref ray, ref this.P, ref this.Q, out htmin))
                {
                    return false;
                }
            }

            //avoid less when facing walls
            htmin *= 2.0f;

            tmin = htmin;

            return true;
        }
    }
}
