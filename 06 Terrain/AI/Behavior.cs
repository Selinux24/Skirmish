using Engine;
using SharpDX;

namespace Terrain.AI
{
    public abstract class Behavior
    {
        public AIAgent Agent { get; private set; }

        public abstract Vector3? Target { get; }

        public Behavior(AIAgent agent)
        {
            this.Agent = agent;
        }

        public abstract bool Test(GameTime gameTime);

        public abstract void Task(GameTime gameTime);
    }
}
