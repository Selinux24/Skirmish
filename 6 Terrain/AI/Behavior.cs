using Engine;

namespace TerrainTest.AI
{
    abstract class Behavior
    {
        public Agent Agent { get; private set; }
        public bool Active { get; protected set; }

        public Behavior(Agent agent)
        {
            this.Agent = agent;
            this.Active = false;
        }

        public abstract void Update(GameTime gameTime);
    }
}
