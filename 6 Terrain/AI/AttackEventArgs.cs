using System;

namespace TerrainTest.AI
{
    class AttackEventArgs : EventArgs
    {
        public Agent Target;

        public AttackEventArgs(Agent target)
        {
            this.Target = target;
        }
    }
}
