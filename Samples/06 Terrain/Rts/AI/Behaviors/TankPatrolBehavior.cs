using Engine;
using SharpDX;

namespace Terrain.Rts.AI.Behaviors
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

        /// <summary>
        /// Attack task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <remarks>Rotate turret towards target</remarks>
        public override void Task(GameTime gameTime)
        {
            if (NextCheckPoint.HasValue)
            {
                var model = Agent.SceneObject;
                if (model?.ModelPartCount > 0)
                {
                    Vector3 viewTarget;

                    if (IsWaitingInCheckPoint())
                    {
                        if (!randomTarget.HasValue)
                        {
                            randomTarget = Agent.Parent.Scene.GetRandomPoint(Helper.RandomGenerator, Vector3.One * 5f);
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
                }
            }

            base.Task(gameTime);
        }
    }
}
