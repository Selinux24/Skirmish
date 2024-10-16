﻿using Engine.PathFinding;
using System;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Navigation query filter
    /// </summary>
    [Serializable]
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
                (int)NavAreaTypes.Soil or (int)NavAreaTypes.Grass or (int)NavAreaTypes.Rock => (int)(AgentActionTypes.Walk | AgentActionTypes.Jump),
                _ => (int)AgentActionTypes.None
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
