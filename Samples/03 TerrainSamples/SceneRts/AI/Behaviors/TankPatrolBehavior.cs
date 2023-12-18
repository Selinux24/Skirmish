using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Tank patrol behavior
    /// </summary>
    public class TankPatrolBehavior : PatrolBehavior
    {
        /// <summary>
        /// Random target when waiting
        /// </summary>
        private Vector3? randomTarget = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public TankPatrolBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <inheritdoc/>
        public override void Task(IGameTime gameTime)
        {
            if (!NextCheckPoint.HasValue)
            {
                return;
            }

            var model = Agent.SceneObject;
            if (model == null || model.ModelPartCount <= 0)
            {
                return;
            }

            Vector3 viewTarget;

            if (IsWaitingInCheckPoint())
            {
                if (!randomTarget.HasValue)
                {
                    Agent.Parent.Scene.GetRandomPoint(Helper.RandomGenerator, Vector3.One * 5f, out var p);

                    randomTarget = p;
                }

                viewTarget = randomTarget.Value;
            }
            else
            {
                randomTarget = null;

                viewTarget = NextCheckPoint.Value;
            }

            model.GetModelPartByName("Turret-mesh").Manipulator.RotateTo(viewTarget, Vector3.Up, Axis.Y, 0.01f);
            model.GetModelPartByName("Barrel-mesh").Manipulator.RotateTo(viewTarget + (model.GetBoundingBox().Height * 0.5f), Vector3.Up, Axis.X, 0.01f);

            base.Task(gameTime);
        }
    }
}
