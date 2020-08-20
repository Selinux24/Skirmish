using System;

namespace Terrain.Rts.AI
{
    /// <summary>
    /// Base behavior events args
    /// </summary>
    public class BehaviorEventArgs : EventArgs
    {
        /// <summary>
        /// Active agent
        /// </summary>
        public AIAgent Active { get; set; }
        /// <summary>
        /// Passive agent
        /// </summary>
        public AIAgent Passive { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="active">Active agent</param>
        /// <param name="passive">Passive agent</param>
        public BehaviorEventArgs(AIAgent active, AIAgent passive)
        {
            this.Active = active;
            this.Passive = passive;
        }
    }
}
