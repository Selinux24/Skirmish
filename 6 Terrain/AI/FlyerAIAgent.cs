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
            this.Controller = new HeliManipulatorController();
        }

        protected override void SetRouteToPoint(Vector3 point, float velocity)
        {
            var p = point;

            if (this.Status.Life > 0)
            {
                p.Y = this.FlightHeight;
            }

            this.Controller.Follow(new SegmentPath(new[] { this.Model.Manipulator.Position, p }));

            this.Model.Manipulator.LinearVelocity = velocity;
        }

        protected override void FireDestroyed(AIAgent active, AIAgent passive)
        {
            //Find nearest ground position
            Vector3 p;
            Triangle t;
            float d;
            if (this.Parent.Ground.FindNearestGroundPosition(this.Manipulator.Position, out p, out t, out d))
            {
                this.SetRouteToPoint(p, 15f);
                this.Model.AnimationController.Stop();
            }
            else
            {
                this.Model.Visible = false;
            }

            base.FireDestroyed(active, passive);
        }
    }
}
