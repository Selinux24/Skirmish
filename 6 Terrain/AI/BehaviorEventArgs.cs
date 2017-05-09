using System;

namespace TerrainTest.AI
{
    class BehaviorEventArgs : EventArgs
    {
        public Agent Active;
        public Agent Passive;

        public BehaviorEventArgs(Agent active, Agent passive)
        {
            this.Active = active;
            this.Passive = passive;
        }
    }
}
