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
        None,
        /// <summary>
        /// Walk
        /// </summary>
        Walk,
        /// <summary>
        /// Jump
        /// </summary>
        Jump,
        /// <summary>
        /// All actions
        /// </summary>
        All = Walk | Jump,
    }
}
