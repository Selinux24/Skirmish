using SharpDX;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// A crowd agent is a unit that moves across the navigation mesh
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// The maximum number of corners a crowd agent will look ahead in the path
        /// </summary>
        private const int AgentMaxCorners = 4;

        public const int AgentMaxNeighbors = 6;

        public float topologyOptTime { get; set; }
        public float DesiredSpeed { get; set; }

        public Vector3 Disp { get; set; }
        public Vector3 DesiredVel { get; set; }
        public Vector3 NVel { get; set; }
        public Vector3 Vel { get; set; }

        public AgentParams Parameters { get; set; }
        public StraightPath Corners { get; set; }
        public PolyId TargetRef { get; set; }
        public int TargetPathQueryIndex { get; set; }
        public bool TargetReplan { get; set; }
        public float TargetReplanTime { get; set; }

        public bool IsActive { get; set; }
        public bool IsPartial { get; set; }
        public AgentState State { get; set; }
        public Vector3 Position { get; set; }
        internal LocalBoundary Boundary { get; private set; }
        internal PathCorridor Corridor { get; private set; }
        public CrowdNeighbor[] Neighbors { get; private set; }
        public int NeighborCount { get; set; }
        public TargetState TargetState { get; set; }
        public Vector3 TargetPosition { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Agent()
        {
            this.IsActive = false;
            this.Corridor = new PathCorridor();
            this.Boundary = new LocalBoundary();
            this.Neighbors = new CrowdNeighbor[AgentMaxNeighbors];
            this.Corners = new StraightPath();
        }

        /// <summary>
        /// Update the position after a certain time 'dt'
        /// </summary>
        /// <param name="dt">Time that passed</param>
        public void Integrate(float dt)
        {
            //fake dyanmic constraint
            float maxDelta = this.Parameters.MaxAcceleration * dt;
            Vector3 dv = this.NVel - this.Vel;
            float ds = dv.Length();
            if (ds > maxDelta)
            {
                dv = dv * (maxDelta / ds);
            }
            this.Vel += dv;

            //integrate
            if (this.Vel.Length() > 0.0001f)
            {
                this.Position += (this.Vel * dt);
            }
            else
            {
                this.Vel = Vector3.Zero;
            }
        }

        public void Reset(PolyId reference, Vector3 nearest)
        {
            this.Corridor.Reset(reference, nearest);
            this.Boundary.Reset();
            this.IsPartial = false;

            this.topologyOptTime = 0;
            this.TargetReplanTime = 0;
            this.NeighborCount = 0;

            this.DesiredVel = Vector3.Zero;
            this.NVel = Vector3.Zero;
            this.Vel = Vector3.Zero;
            this.Position = nearest;

            this.DesiredSpeed = 0;

            if (reference != PolyId.Null)
            {
                this.State = AgentState.Walking;
            }
            else
            {
                this.State = AgentState.Invalid;
            }

            this.TargetState = TargetState.None;
        }
        /// <summary>
        /// Change the move target
        /// </summary>
        /// <param name="reference">The polygon reference</param>
        /// <param name="pos">The target's coordinates</param>
        public void RequestMoveTargetReplan(PolyId reference, Vector3 pos)
        {
            //initialize request
            this.TargetRef = reference;
            this.TargetPosition = pos;
            this.TargetPathQueryIndex = PathQueue.Invalid;
            this.TargetReplan = true;
            if (this.TargetRef != PolyId.Null)
            {
                this.TargetState = TargetState.Requesting;
            }
            else
            {
                this.TargetState = TargetState.Failed;
            }
        }
        /// <summary>
        /// Request a new move target
        /// </summary>
        /// <param name="reference">The polygon reference</param>
        /// <param name="pos">The target's coordinates</param>
        /// <returns>True if request met, false if not</returns>
        public bool RequestMoveTarget(PolyId reference, Vector3 pos)
        {
            if (reference == PolyId.Null)
            {
                return false;
            }

            //initialize request
            this.TargetRef = reference;
            this.TargetPosition = pos;
            this.TargetPathQueryIndex = PathQueue.Invalid;
            this.TargetReplan = false;
            if (this.TargetRef != PolyId.Null)
            {
                this.TargetState = TargetState.Requesting;
            }
            else
            {
                this.TargetState = TargetState.Failed;
            }

            return true;
        }
        /// <summary>
        /// Request a new move velocity
        /// </summary>
        /// <param name="vel">The agent's velocity</param>
        public void RequestMoveVelocity(Vector3 vel)
        {
            //initialize request
            this.TargetRef = PolyId.Null;
            this.TargetPosition = vel;
            this.TargetPathQueryIndex = PathQueue.Invalid;
            this.TargetReplan = false;
            this.TargetState = TargetState.Velocity;
        }
        /// <summary>
        /// Reset the move target of an agent
        /// </summary>
        public void ResetMoveTarget()
        {
            //initialize request
            this.TargetRef = PolyId.Null;
            this.TargetPosition = Vector3.Zero;
            this.TargetPathQueryIndex = PathQueue.Invalid;
            this.TargetReplan = false;
            this.TargetState = TargetState.None;
        }
    }
}
