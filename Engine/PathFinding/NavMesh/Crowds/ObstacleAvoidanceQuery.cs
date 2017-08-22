using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Obstacle avoidance query
    /// </summary>
    public abstract class ObstacleAvoidanceQuery : IObstacleAvoidanceQuery
    {
        protected const int MaxPatternDivs = 32;
        protected const int MaxPatternRings = 4;

        private int maxCircles;
        private int numCircles;

        private int maxSegments;
        private int numSegments;

        private List<IObstacle> obstacles = new List<IObstacle>();

        protected ObstacleAvoidanceParams Parameters;
        protected float InverseHorizontalTime;
        protected float VMax;
        protected float InverseVMax;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCircles">Maximum number of circles</param>
        /// <param name="maxSegments">Maximum number of segments</param>
        public ObstacleAvoidanceQuery(int maxCircles, int maxSegments)
        {
            this.maxCircles = maxCircles;
            this.numCircles = 0;

            this.maxSegments = maxSegments;
            this.numSegments = 0;

            //initialize obstacle query params
            this.Parameters = new ObstacleAvoidanceParams()
            {
                VelBias = 0.4f,
                WeightDesVel = 2.0f,
                WeightCurVel = 0.75f,
                WeightSide = 0.75f,
                WeightToi = 2.5f,
                HorizTime = 2.5f,
                GridSize = 33,
                AdaptiveDivs = 7,
                AdaptiveRings = 2,
                AdaptiveDepth = 5,
            };
        }

        /// <summary>
        /// Adds a new obstacle to the query
        /// </summary>
        /// <param name="obstacle">Obstacle</param>
        public void AddObstacle(IObstacle obstacle)
        {
            if (obstacle is ObstacleCircle)
            {
                if (this.numCircles <= this.maxCircles)
                {
                    this.obstacles.Add(obstacle);

                    this.numCircles++;
                }
            }
            else if (obstacle is ObstacleSegment)
            {
                if (this.numSegments <= this.maxSegments)
                {
                    this.obstacles.Add(obstacle);

                    this.numSegments++;
                }
            }
        }
        /// <summary>
        /// Resets the ObstacleAvoidanceQuery's internal data
        /// </summary>
        public void Reset()
        {
            this.numCircles = 0;
            this.numSegments = 0;
            this.obstacles.Clear();
        }

        /// <summary>
        /// Samples the new velocity
        /// </summary>
        /// <param name="position">Agent position</param>
        /// <param name="radius">Agent radius</param>
        /// <param name="maximumVelocity">Agent maximum velocity</param>
        /// <param name="velocity">Agent current velocity</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        /// <param name="newVelocity">Returns the new agent velocity to avoid the query obstacles</param>
        /// <returns>Returns the number of samples used in the query</returns>
        public int SampleVelocity(Vector3 position, float radius, float maximumVelocity, Vector3 velocity, Vector3 desiredVelocity, out Vector3 newVelocity)
        {
            this.Prepare(position, desiredVelocity);

            return this.Sample(position, radius, maximumVelocity, velocity, desiredVelocity, out newVelocity);
        }
        /// <summary>
        /// Prepare the obstacles for further calculations
        /// </summary>
        /// <param name="position">Current agent position</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        protected void Prepare(Vector3 position, Vector3 desiredVelocity)
        {
            //prepare obstacles
            for (int i = 0; i < this.obstacles.Count; i++)
            {
                this.obstacles[i].Update(position, desiredVelocity);
            }
        }
        /// <summary>
        /// Samples velocity
        /// </summary>
        /// <param name="position">Agent position</param>
        /// <param name="radius">Agent radius</param>
        /// <param name="maximumVelocity">Agent maximum velocity</param>
        /// <param name="velocity">Agent current velocity</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        /// <param name="newVelocity">Agent new velocity</param>
        /// <returns>Returns the number of samples used in the query</returns>
        protected abstract int Sample(Vector3 position, float radius, float maximumVelocity, Vector3 velocity, Vector3 desiredVelocity, out Vector3 newVelocity);
        /// <summary>
        /// Process sample
        /// </summary>
        /// <param name="vcand"></param>
        /// <param name="cs"></param>
        /// <param name="position">Agent position</param>
        /// <param name="radius">Agent radius</param>
        /// <param name="velocity">Agent velocity</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        /// <returns></returns>
        protected float ProcessSample(Vector3 vcand, float cs, Vector3 position, float radius, Vector3 velocity, Vector3 desiredVelocity)
        {
            //find min time of impact and exit amongst all obstacles
            float tmin = this.Parameters.HorizTime;
            float side = 0;

            for (int i = 0; i < this.obstacles.Count; i++)
            {
                var obstacle = this.obstacles[i];

                float htmin;
                if (obstacle.ProcessSample(position, radius, vcand, velocity, out htmin, out side))
                {
                    //the closest obstacle is sometime ahead of us, keep track of nearest obstacle
                    if (htmin < tmin)
                    {
                        tmin = htmin;
                    }
                }
            }

            float vpen = this.Parameters.WeightDesVel * (Helper.Distance2D(vcand, desiredVelocity) * InverseVMax);
            float vcpen = this.Parameters.WeightCurVel * (Helper.Distance2D(vcand, velocity) * InverseVMax);
            float spen = this.Parameters.WeightSide * side;
            float tpen = this.Parameters.WeightToi * (1.0f / (0.1f + tmin * InverseHorizontalTime));

            return vpen + vcpen + spen + tpen;
        }
    }
}
