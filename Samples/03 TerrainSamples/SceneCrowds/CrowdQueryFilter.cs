using Engine.PathFinding;

namespace TerrainSamples.SceneCrowds
{
    /// <summary>
    /// Crowd query filter
    /// </summary>
    class CrowdQueryFilter : GraphQueryFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CrowdQueryFilter() : base((int)TerrainAreaTypes.Dirt)
        {

        }

        /// <inheritdoc/>
        public override int EvaluateArea(int area)
        {
            return area switch
            {
                (int)TerrainAreaTypes.Dirt or (int)TerrainAreaTypes.Grass => (int)AgentAcionTypes.Move,
                (int)TerrainAreaTypes.Mud => (int)AgentAcionTypes.SlowMove,
                (int)TerrainAreaTypes.Road => (int)AgentAcionTypes.FastMove,
                _ => (int)AgentAcionTypes.None,
            };
        }
        /// <inheritdoc/>
        public override TAction EvaluateArea<TArea, TAction>(TArea area)
        {
            int ar = (int)(object)area;
            int ac = EvaluateArea(ar);
            return (TAction)(object)ac;
        }
    }

    /// <summary>
    /// Terrain area types
    /// </summary>
    enum TerrainAreaTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Dirt
        /// </summary>
        Dirt,
        /// <summary>
        /// Road
        /// </summary>
        Road,
        /// <summary>
        /// Grass
        /// </summary>
        Grass,
        /// <summary>
        /// Mud
        /// </summary>
        Mud,
    }
    /// <summary>
    /// Agent action types
    /// </summary>
    enum AgentAcionTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Normal move
        /// </summary>
        Move,
        /// <summary>
        /// Slow move
        /// </summary>
        SlowMove,
        /// <summary>
        /// Fast move
        /// </summary>
        FastMove,
    }
}
