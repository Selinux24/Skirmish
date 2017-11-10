using System;

namespace Terrain.AI
{
    public class BehaviorEventArgs : EventArgs
    {
        public AIAgent Active;
        public AIAgent Passive;

        public BehaviorEventArgs(AIAgent active, AIAgent passive)
        {
            this.Active = active;
            this.Passive = passive;
        }
    }
}
