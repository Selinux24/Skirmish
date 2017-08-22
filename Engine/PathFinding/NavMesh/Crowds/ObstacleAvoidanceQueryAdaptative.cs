using SharpDX;
using System;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Adaptative obstacle avoidance query
    /// </summary>
    public class ObstacleAvoidanceQueryAdaptative : ObstacleAvoidanceQuery
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCircles">Maximum number of circles</param>
        /// <param name="maxSegments">Maximum number of segments</param>
        public ObstacleAvoidanceQueryAdaptative(int maxCircles, int maxSegments) : base(maxCircles, maxSegments)
        {

        }

        /// <summary>
        /// Samples velocity adaptative
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
            this.InverseHorizontalTime = 1.0f / Parameters.HorizTime;
            this.VMax = maximumVelocity;
            this.InverseVMax = 1.0f / maximumVelocity;

            newVelocity = new Vector3(0, 0, 0);

            //build sampling pattern aligned to desired velocity
            float[] pattern = new float[(MaxPatternDivs * MaxPatternRings + 1) * 2];
            int numPatterns = 0;

            int numDivs = Parameters.AdaptiveDivs;
            int numRings = Parameters.AdaptiveRings;
            int depth = Parameters.AdaptiveDepth;

            int newNumDivs = MathUtil.Clamp(numDivs, 1, MaxPatternDivs);
            int newNumRings = MathUtil.Clamp(numRings, 1, MaxPatternRings);
            float da = (1.0f / newNumDivs) * (float)Math.PI * 2;
            float dang = (float)Math.Atan2(desiredVelocity.Z, desiredVelocity.X);

            //always add sample at zero
            pattern[numPatterns * 2 + 0] = 0;
            pattern[numPatterns * 2 + 1] = 0;
            numPatterns++;

            for (int j = 0; j < newNumRings; j++)
            {
                float r = (float)(newNumRings - j) / (float)newNumRings;
                float a = dang + (j & 1) * 0.5f * da;
                for (int i = 0; i < newNumDivs; i++)
                {
                    pattern[numPatterns * 2 + 0] = (float)Math.Cos(a) * r;
                    pattern[numPatterns * 2 + 1] = (float)Math.Sin(a) * r;
                    numPatterns++;
                    a += da;
                }
            }

            //start sampling
            float cr = maximumVelocity * (1.0f - Parameters.VelBias);
            Vector3 res = new Vector3(desiredVelocity.X * Parameters.VelBias, 0, desiredVelocity.Z * Parameters.VelBias);
            int ns = 0;

            for (int k = 0; k < depth; k++)
            {
                float minPenalty = float.MaxValue;
                Vector3 bvel = new Vector3(0, 0, 0);

                for (int i = 0; i < numPatterns; i++)
                {
                    Vector3 vcand = new Vector3();
                    vcand.X = res.X + pattern[i * 2 + 0] * cr;
                    vcand.Y = 0;
                    vcand.Z = res.Z + pattern[i * 2 + 1] * cr;

                    if (vcand.X * vcand.X + vcand.Z * vcand.Z > (maximumVelocity + 0.001f) * (maximumVelocity + 0.001f))
                    {
                        continue;
                    }

                    float penalty = this.ProcessSample(vcand, cr / 10, position, radius, velocity, desiredVelocity);
                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        bvel = vcand;
                    }
                }

                res = bvel;

                cr *= 0.5f;
            }

            newVelocity = res;

            return ns;
        }
    }
}
