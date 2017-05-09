using System;

namespace TerrainTest.AI
{
    class BehaviorChangingEventArgs : EventArgs
    {
        public Agent Active;
        public Behavior Previous;
        public Behavior Next;

        public BehaviorChangingEventArgs(Agent active, Behavior previous, Behavior next)
        {
            this.Active = active;
            this.Previous = previous;
            this.Next = next;
        }
    }
}
