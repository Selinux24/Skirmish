using SharpDX;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Grid obstacle avoidance query
    /// </summary>
    public class ObstacleAvoidanceQueryGrid : ObstacleAvoidanceQuery
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCircles">Maximum number of circles</param>
        /// <param name="maxSegments">Maximum number of segments</param>
        public ObstacleAvoidanceQueryGrid(int maxCircles, int maxSegments) : base(maxCircles, maxSegments)
        {

        }

        /// <summary>
        /// Samples velocity using a grid
        /// </summary>
        /// <param name="position">Agent position</param>
        /// <param name="radius">Agent radius</param>
        /// <param name="maximumVelocity">Agent maximum velocity</param>
        /// <param name="velocity">Agent current velocity</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        /// <param name="newVelocity">Agent new velocity</param>
        /// <returns>Returns the number of samples used in the query</returns>
        protected override int Sample(Vector3 position, float radius, float maximumVelocity, Vector3 velocity, Vector3 desiredVelocity, out Vector3 newVelocity)
        {
            this.InverseHorizontalTime = 1.0f / this.Parameters.HorizTime;
            this.VMax = maximumVelocity;
            this.InverseVMax = 1.0f / maximumVelocity;

            newVelocity = Vector3.Zero;

            float cvx = desiredVelocity.X * this.Parameters.VelBias;
            float cvz = desiredVelocity.Z * this.Parameters.VelBias;
            float cs = maximumVelocity * 2 * (1 - this.Parameters.VelBias) / (float)(this.Parameters.GridSize - 1);
            float half = (this.Parameters.GridSize - 1) * cs * 0.5f;

            float minPenalty = float.MaxValue;
            int numSamples = 0;

            for (int y = 0; y < this.Parameters.GridSize; y++)
            {
                for (int x = 0; x < this.Parameters.GridSize; x++)
                {
                    Vector3 vcand = new Vector3(0, 0, 0);
                    vcand.X = cvx + x * cs - half;
                    vcand.Y = 0;
                    vcand.Z = cvz + y * cs - half;

                    if (vcand.X * vcand.X + vcand.Z * vcand.Z > (maximumVelocity + cs / 2) * (maximumVelocity + cs / 2))
                    {
                        continue;
                    }

                    float penalty = this.ProcessSample(vcand, cs, position, radius, velocity, desiredVelocity);
                    numSamples++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        newVelocity = vcand;
                    }
                }
            }

            return numSamples;
        }
    }
}
