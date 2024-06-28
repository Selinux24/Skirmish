using Engine.PathFinding;
using System;

namespace TerrainSamples.SceneCrowds
{
    /// <summary>
    /// Crowd query filter
    /// </summary>
    [Serializable]
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
                (int)TerrainAreaTypes.Mud => (int)AgentAcionTypes.SlowMove,
                (int)TerrainAreaTypes.Dirt or (int)TerrainAreaTypes.Grass => (int)(AgentAcionTypes.SlowMove | AgentAcionTypes.Move),
                (int)TerrainAreaTypes.Road => (int)(AgentAcionTypes.SlowMove | AgentAcionTypes.Move | AgentAcionTypes.FastMove),
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
    [Flags]
    enum AgentAcionTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Normal move
        /// </summary>
        Move = 1,
        /// <summary>
        /// Slow move
        /// </summary>
        SlowMove = 2,
        /// <summary>
        /// Fast move
        /// </summary>
        FastMove = 4,
    }
}
