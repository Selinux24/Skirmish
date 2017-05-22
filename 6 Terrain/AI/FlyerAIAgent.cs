using Engine;
using Engine.PathFinding;
using SharpDX;

namespace TerrainTest.AI
{
    public class FlyerAIAgent : AIAgent
    {
        public float FlightHeight;

        public FlyerAIAgent(Brain parent, AgentType agentType, Model model, FlyerAIStatusDescription status) :
            base(parent, agentType, model, status)
        {
            this.FlightHeight = status.FlightHeight;
        }

        protected override void SetRouteToPoint(Vector3 point, float velocity)
        {
            var p = point;
            p.Y = this.FlightHeight;

            this.Model.Manipulator.Follow(new[] { this.Model.Manipulator.Position, p });

            this.desiredVelocity = velocity;
        }
    }
}
