﻿using Engine;

namespace Deferred
{
    /// <summary>
    /// Game agent state
    /// </summary>
    public class GameAgentState : ISceneObjectState
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Controller
        /// </summary>
        public IGameState Controller { get; set; }
    }
}