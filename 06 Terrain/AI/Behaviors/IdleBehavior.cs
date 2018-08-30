using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    public class IdleBehavior : Behavior
    {
        public override Vector3? Target
        {
            get
            {
                return null;
            }
        }

        public IdleBehavior(AIAgent agent) : base(agent)
        {

        }

        public override bool Test(GameTime gameTime)
        {
            return true;
        }

        public override void Task(GameTime gameTime)
        {
            //Do nothing
        }
    }
}
