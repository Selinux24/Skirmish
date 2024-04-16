using System;

namespace TerrainSamples.SceneModularDungeon
{
    /// <summary>
    /// Agent actions
    /// </summary>
    [Flags]
    enum AgentActionTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Walk
        /// </summary>
        Walk = 1,
        /// <summary>
        /// Jump
        /// </summary>
        Jump = 2,
        /// <summary>
        /// All actions
        /// </summary>
        All = Walk | Jump,
    }
}
