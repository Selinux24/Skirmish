using SharpDX;
using System;

namespace Engine.PathFinding.NavMesh.Crowds
{
    using Engine.Common;

    //TODO: Convert this class into an interface.
    //TODO: Implement the two avoidance modes in two separate classes
    //TODO: Set the crowd property to the interface type, and initialize it using some new parametrization

    /// <summary>
    /// Obstacle avoidance query
    /// </summary>
    public class ObstacleAvoidanceQuery
    {
        private const int MaxPatternDivs = 32;
        private const int MaxPatternRings = 4;
        private const float EPS = 0.0001f;

        /// <summary>
        /// Obstacle interface
        /// </summary>
        interface IObstable
        {
            /// <summary>
            /// Updates the obstacle against a position at a specified desired velocity
            /// </summary>
            /// <param name="position">Position</param>
            /// <param name="desiredVelocity">Desired velocity</param>
            void Update(Vector3 position, Vector3 desiredVelocity);
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
            bool ProcessSample(Vector3 position, float radius, Vector3 vcand, Vector3 vel, out float tmin, out float side);
        }

        /// <summary>
        /// Circle obstacle
        /// </summary>
        struct ObstacleCircle : IObstable
        {
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

        /// <summary>
        /// Segment obstacle
        /// </summary>
        struct ObstacleSegment : IObstable
        {
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
                return false; //not moving
            }

            //overlap, calculate time to exit
            float b = Helper.Dot2D(ref v, ref s);
            float d = b * b - a * c;
            if (d < 0.0f)
                return false; //no intersection
            a = 1.0f / a;
            float rd = (float)Math.Sqrt(d);
            tmin = (b - rd) * a;
            tmax = (b + rd) * a;
            return true;
        }

        private ObstacleAvoidanceParams parameters;
        private float invHorizTime;
        private float vmax;
        private float invVmax;

        private int maxCircles;
        private ObstacleCircle[] circles;
        private int numCircles;

        private int maxSegments;
        private ObstacleSegment[] segments;
        private int numSegments;

        public bool Adaptive = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCircles">Maximum number of circles</param>
        /// <param name="maxSegments">Maximum number of segments</param>
        public ObstacleAvoidanceQuery(int maxCircles, int maxSegments)
        {
            this.maxCircles = maxCircles;
            this.numCircles = 0;
            this.circles = new ObstacleCircle[this.maxCircles];

            this.maxSegments = maxSegments;
            this.numSegments = 0;
            this.segments = new ObstacleSegment[this.maxSegments];

            //initialize obstacle query params
            this.parameters = new ObstacleAvoidanceParams()
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
        /// Add a new circle to the array
        /// </summary>
        /// <param name="pos">The position</param>
        /// <param name="rad">The radius</param>
        /// <param name="vel">The velocity</param>
        /// <param name="dvel">The desired velocity</param>
        public void AddCircle(Vector3 pos, float rad, Vector3 vel, Vector3 dvel)
        {
            if (this.numCircles <= this.maxCircles)
            {
                this.circles[this.numCircles].Position = pos;
                this.circles[this.numCircles].Radius = rad;
                this.circles[this.numCircles].Vel = vel;
                this.circles[this.numCircles].DesiredVel = dvel;
                this.numCircles++;
            }
        }
        /// <summary>
        /// Add a segment to the array
        /// </summary>
        /// <param name="p">One endpoint</param>
        /// <param name="q">The other endpoint</param>
        public void AddSegment(Vector3 p, Vector3 q)
        {
            if (this.numSegments <= this.maxSegments)
            {
                this.segments[this.numSegments].P = p;
                this.segments[this.numSegments].Q = q;
                this.numSegments++;
            }
        }
        /// <summary>
        /// Resets the ObstacleAvoidanceQuery's internal data
        /// </summary>
        public void Reset()
        {
            this.numCircles = 0;
            this.numSegments = 0;
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

            if (this.Adaptive)
            {
                return this.SampleVelocityAdaptive(position, radius, maximumVelocity, velocity, desiredVelocity, out newVelocity);
            }
            else
            {
                return this.SampleVelocityGrid(position, radius, maximumVelocity, velocity, desiredVelocity, out newVelocity);
            }
        }
        /// <summary>
        /// Prepare the obstacles for further calculations
        /// </summary>
        /// <param name="position">Current agent position</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        private void Prepare(Vector3 position, Vector3 desiredVelocity)
        {
            //prepare obstacles
            for (int i = 0; i < this.numCircles; i++)
            {
                //side
                this.circles[i].Update(position, desiredVelocity);
            }

            for (int i = 0; i < this.numSegments; i++)
            {
                //precalculate if the agent is close to the segment
                this.segments[i].Update(position, desiredVelocity);
            }
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
        private int SampleVelocityGrid(Vector3 position, float radius, float maximumVelocity, Vector3 velocity, Vector3 desiredVelocity, out Vector3 newVelocity)
        {
            this.invHorizTime = 1.0f / this.parameters.HorizTime;
            this.vmax = maximumVelocity;
            this.invVmax = 1.0f / maximumVelocity;

            newVelocity = Vector3.Zero;

            float cvx = desiredVelocity.X * this.parameters.VelBias;
            float cvz = desiredVelocity.Z * this.parameters.VelBias;
            float cs = maximumVelocity * 2 * (1 - this.parameters.VelBias) / (float)(this.parameters.GridSize - 1);
            float half = (this.parameters.GridSize - 1) * cs * 0.5f;

            float minPenalty = float.MaxValue;
            int numSamples = 0;

            for (int y = 0; y < this.parameters.GridSize; y++)
            {
                for (int x = 0; x < this.parameters.GridSize; x++)
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
        private int SampleVelocityAdaptive(Vector3 position, float radius, float maximumVelocity, Vector3 velocity, Vector3 desiredVelocity, out Vector3 newVelocity)
        {
            this.invHorizTime = 1.0f / parameters.HorizTime;
            this.vmax = maximumVelocity;
            this.invVmax = 1.0f / maximumVelocity;

            newVelocity = new Vector3(0, 0, 0);

            //build sampling pattern aligned to desired velocity
            float[] pattern = new float[(MaxPatternDivs * MaxPatternRings + 1) * 2];
            int numPatterns = 0;

            int numDivs = parameters.AdaptiveDivs;
            int numRings = parameters.AdaptiveRings;
            int depth = parameters.AdaptiveDepth;

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
            float cr = maximumVelocity * (1.0f - parameters.VelBias);
            Vector3 res = new Vector3(desiredVelocity.X * parameters.VelBias, 0, desiredVelocity.Z * parameters.VelBias);
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
        private float ProcessSample(Vector3 vcand, float cs, Vector3 position, float radius, Vector3 velocity, Vector3 desiredVelocity)
        {
            //find min time of impact and exit amongst all obstacles
            float tmin = this.parameters.HorizTime;
            float side = 0;

            for (int i = 0; i < this.numCircles; i++)
            {
                var circle = this.circles[i];

                float htmin;
                if (circle.ProcessSample(position, radius, vcand, velocity, out htmin, out side))
                {
                    //the closest obstacle is sometime ahead of us, keep track of nearest obstacle
                    if (htmin < tmin)
                    {
                        tmin = htmin;
                    }
                }
            }

            for (int i = 0; i < numSegments; i++)
            {
                var segment = segments[i];

                float htmin;
                if (segment.ProcessSample(position, radius, vcand, velocity, out htmin, out side))
                {
                    //the closest obstacle is sometime ahead of us, keep track of nearest obstacle
                    if (htmin < tmin)
                    {
                        tmin = htmin;
                    }
                }
            }

            float vpen = this.parameters.WeightDesVel * (Helper.Distance2D(vcand, desiredVelocity) * invVmax);
            float vcpen = this.parameters.WeightCurVel * (Helper.Distance2D(vcand, velocity) * invVmax);
            float spen = this.parameters.WeightSide * side;
            float tpen = this.parameters.WeightToi * (1.0f / (0.1f + tmin * invHorizTime));

            return vpen + vcpen + spen + tpen;
        }
    }
}
