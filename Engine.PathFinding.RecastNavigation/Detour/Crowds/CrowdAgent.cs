using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Represents an agent managed by a Crowd object.
    /// </summary>
    public class CrowdAgent
    {
        /// <summary>
        /// True if the agent is active, false if the agent is in an unused slot in the agent pool.
        /// </summary>
        public bool Active { get; set; } = false;
        /// <summary>
        /// The type of mesh polygon the agent is traversing.
        /// </summary>
        public CrowdAgentState State { get; set; } = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
        /// <summary>
        /// True if the agent has valid path (targetState == DT_CROWDAGENT_TARGET_VALID) and the path does not lead to the requested position, else false.
        /// </summary>
        public bool Partial { get; set; }
        /// <summary>
        /// The path corridor the agent is using.
        /// </summary>
        public PathCorridor Corridor { get; set; } = new PathCorridor();
        /// <summary>
        /// The local boundary data for the agent.
        /// </summary>
        public LocalBoundary Boundary { get; set; } = new LocalBoundary();
        /// <summary>
        /// Time since the agent's path corridor was optimized.
        /// </summary>
        public float TopologyOptTime { get; set; }
        /// <summary>
        /// The known neighbors of the agent.
        /// </summary>
        public CrowdNeighbour[] Neis { get; set; } = new CrowdNeighbour[Crowd.DT_CROWDAGENT_MAX_NEIGHBOURS];
        /// <summary>
        /// The number of neighbors.
        /// </summary>
        public int NNeis { get; set; }
        /// <summary>
        /// The desired speed.
        /// </summary>
        public float DesiredSpeed { get; set; }

        /// <summary>
        /// The current agent position. [(x, y, z)]
        /// </summary>
        public Vector3 NPos { get; set; }
        /// <summary>
        /// A temporary value used to accumulate agent displacement during iterative collision resolution. [(x, y, z)]
        /// </summary>
        public Vector3 Disp { get; set; }
        /// <summary>
        /// The desired velocity of the agent. Based on the current path, calculated from scratch each frame. [(x, y, z)]
        /// </summary>
        public Vector3 DVel { get; set; }
        /// <summary>
        /// The desired velocity adjusted by obstacle avoidance, calculated from scratch each frame. [(x, y, z)]
        /// </summary>
        public Vector3 NVel { get; set; }
        /// <summary>
        /// The actual velocity of the agent. The change from nvel -> vel is constrained by max acceleration. [(x, y, z)]
        /// </summary>
        public Vector3 Vel { get; set; }

        /// <summary>
        /// The agent's configuration parameters.
        /// </summary>
        public CrowdAgentParams Params { get; set; } = new CrowdAgentParams();

        /// <summary>
        /// The local path corridor corners for the agent. (Staight path.) [(x, y, z) * #ncorners]
        /// </summary>
        public Vector3[] CornerVerts { get; set; } = new Vector3[Crowd.DT_CROWDAGENT_MAX_CORNERS];

        /// <summary>
        /// The local path corridor corner flags. (See: #dtStraightPathFlags) [(flags) * #ncorners]
        /// </summary>
        public StraightPathFlagTypes[] CornerFlags { get; set; } = new StraightPathFlagTypes[Crowd.DT_CROWDAGENT_MAX_CORNERS];

        /// <summary>
        /// The reference id of the polygon being entered at the corner. [(polyRef) * #ncorners]
        /// </summary>
        public int[] CornerPolys { get; set; } = new int[Crowd.DT_CROWDAGENT_MAX_CORNERS];

        /// <summary>
        /// The number of corners.
        /// </summary>
        public int NCorners { get; set; }

        /// <summary>
        /// State of the movement request.
        /// </summary>
        public MoveRequestState TargetState { get; set; } = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;
        /// <summary>
        /// Target polyref of the movement request.
        /// </summary>
        public int TargetRef { get; set; }
        /// <summary>
        /// Target position of the movement request (or velocity in case of DT_CROWDAGENT_TARGET_VELOCITY).
        /// </summary>
        public Vector3 TargetPos { get; set; }
        /// <summary>
        /// Path finder ref.
        /// </summary>
        public int TargetPathqRef { get; set; }
        /// <summary>
        /// Flag indicating that the current path is being replanned.
        /// </summary>
        public bool TargetReplan { get; set; }
        /// <summary>
        /// Time since the agent's target was replanned.
        /// </summary>
        public float TargetReplanTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CrowdAgent()
        {

        }
    }
}
