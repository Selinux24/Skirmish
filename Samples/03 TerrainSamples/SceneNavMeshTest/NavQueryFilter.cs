using Engine.PathFinding;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Navigation query filter
    /// </summary>
    class NavQueryFilter : GraphQueryFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NavQueryFilter() : base((int)NavAreaTypes.Soil)
        {

        }

        /// <inheritdoc/>
        public override int EvaluateArea(int area)
        {
            return area switch
            {
                (int)NavAreaTypes.Soil or (int)NavAreaTypes.Grass or (int)NavAreaTypes.Rock => (int)(AgentAcionTypes.Walk | AgentAcionTypes.Jump),
                _ => (int)AgentAcionTypes.None
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
}
