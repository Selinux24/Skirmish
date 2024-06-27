using Engine;
using Engine.PathFinding;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Agents
{
    using Engine.BuiltIn.Components.Models;
    using TerrainSamples.SceneRts.AI.Behaviors;
    using TerrainSamples.SceneRts.Controllers;

    /// <summary>
    /// Helicopter agent
    /// </summary>
    public class HelicopterAIAgent : AIAgent
    {
        /// <summary>
        /// Flight height
        /// </summary>
        public float FlightHeight { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Brain</param>
        /// <param name="agentType">Agent type</param>
        /// <param name="sceneObject">Scene object</param>
        /// <param name="stats">Agent stats</param>
        public HelicopterAIAgent(Brain parent, AgentType agentType, Model sceneObject, HelicopterAIStatsDescription stats) :
            base(parent, agentType, sceneObject, stats)
        {
            FlightHeight = stats.FlightHeight;
            Controller = new HeliManipulatorController();
            AttackBehavior = new HelicopterAttackBehavior(this);
        }

        /// <summary>
        /// Calculates a route to a point
        /// </summary>
        /// <param name="point">Targer point</param>
        /// <param name="speed">Speed</param>
        /// <param name="refine">Refine</param>
        public override void SetRouteToPoint(Vector3 point, float speed, bool refine)
        {
            var p = point;

            if (Stats.Life > 0)
            {
                p.Y = FlightHeight;
            }

            Controller.Follow(new SegmentPath([Manipulator.Position, p]));
            Controller.MaximumSpeed = speed;
        }

        /// <summary>
        /// Fires the damaged action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected override void FireDamaged(AIAgent active, AIAgent passive)
        {
            base.FireDamaged(active, passive);

            var model = SceneObject;
            if (model != null)
            {
                if (Stats.Damage > 0.9f)
                {
                    model.TextureIndex = 2;
                }
                else if (Stats.Damage > 0.2f)
                {
                    model.TextureIndex = 1;
                }
                else
                {
                    model.TextureIndex = 0;
                }
            }
        }
        /// <summary>
        /// Fires the destroyed action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected override void FireDestroyed(AIAgent active, AIAgent passive)
        {
            var model = SceneObject;

            //Find nearest ground position
            if (Parent.Scene.FindNearestGroundPosition<Triangle>(Manipulator.Position, out var r))
            {
                SetRouteToPoint(r.Position, 15f, false);
                model?.AnimationController.Stop();
            }
            else
            {
                SceneObject.Visible = false;
            }

            if (model != null)
            {
                model.TextureIndex = 2;
            }

            base.FireDestroyed(active, passive);
        }
    }
}
