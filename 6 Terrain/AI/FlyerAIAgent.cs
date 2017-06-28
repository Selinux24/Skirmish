using Engine;
using Engine.PathFinding;
using SharpDX;

namespace TerrainTest.AI
{
    public class FlyerAIAgent : AIAgent
    {
        public float FlightHeight;

        public FlyerAIAgent(Brain parent, AgentType agentType, SceneObject sceneObject, FlyerAIStatusDescription status) :
            base(parent, agentType, sceneObject, status)
        {
            this.FlightHeight = status.FlightHeight;
            this.Controller = new HeliManipulatorController(sceneObject.Get<ITransformable3D>().Manipulator);
        }

        public override void SetRouteToPoint(Vector3 point, float speed, bool fine)
        {
            var p = point;

            if (this.Status.Life > 0)
            {
                p.Y = this.FlightHeight;
            }

            this.Controller.Follow(new SegmentPath(new[] { this.Manipulator.Position, p }));
            this.Controller.MaximumSpeed = speed;
        }

        protected override void FireDamaged(AIAgent active, AIAgent passive)
        {
            base.FireDamaged(active, passive);

            var model = this.SceneObject.Get<Model>();
            if (model != null)
            {
                if (this.Status.Damage > 0.9f)
                {
                    model.TextureIndex = 2;
                }
                else if (this.Status.Damage > 0.2f)
                {
                    model.TextureIndex = 1;
                }
                else
                {
                    model.TextureIndex = 0;
                }
            }
        }
        protected override void FireDestroyed(AIAgent active, AIAgent passive)
        {
            var model = this.SceneObject.Get<Model>();

            //Find nearest ground position
            Vector3 p;
            Triangle t;
            float d;
            if (this.Parent.Scene.FindNearestGroundPosition(this.Manipulator.Position, out p, out t, out d))
            {
                this.SetRouteToPoint(p, 15f, false);
                if (model != null)
                {
                    model.AnimationController.Stop();
                }
            }
            else
            {
                this.SceneObject.Visible = false;
            }

            if (model != null)
            {
                model.TextureIndex = 2;
            }

            base.FireDestroyed(active, passive);
        }
    }
}
