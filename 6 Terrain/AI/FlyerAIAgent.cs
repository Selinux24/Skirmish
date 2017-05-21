using Engine;
using Engine.PathFinding;
using SharpDX;

namespace TerrainTest.AI
{
    public class FlyerAIAgent : AIAgent
    {
        public float FlightHeight;

        public FlyerAIAgent(Brain parent, AgentType agentType, Model model, WeaponDescription primaryWeapon, WeaponDescription secondaryWeapon, float life, float flightHeight) :
            base(parent, agentType, model, primaryWeapon, secondaryWeapon, life)
        {
            this.FlightHeight = flightHeight;
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
