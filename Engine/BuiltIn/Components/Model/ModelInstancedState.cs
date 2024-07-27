﻿using Engine.Common;
using System.Collections.Generic;

namespace Engine.BuiltIn.Components.Models
{
    /// <summary>
    /// Instanced model state
    /// </summary>
    public class ModelInstancedState : BaseSceneObjectState
    {
        /// <summary>
        /// Maximum count
        /// </summary>
        public int MaximumCount { get; set; }
        /// <summary>
        /// Instance list
        /// </summary>
        public IEnumerable<IGameState> Instances { get; set; } = [];
    }
}