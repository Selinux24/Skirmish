using System;

namespace TerrainSamples.SceneRts.AI
{
    /// <summary>
    /// Base behavior events args
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="active">Active agent</param>
    /// <param name="passive">Passive agent</param>
    public class BehaviorEventArgs(AIAgent active, AIAgent passive) : EventArgs
    {
        /// <summary>
        /// Active agent
        /// </summary>
        public AIAgent Active { get; set; } = active;
        /// <summary>
        /// Passive agent
        /// </summary>
        public AIAgent Passive { get; set; } = passive;
    }
}
