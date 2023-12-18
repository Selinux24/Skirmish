﻿using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Tank attack behavior
    /// </summary>
    public class TankAttackBehavior : AttackBehavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public TankAttackBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <inheritdoc/>
        public override void Task(IGameTime gameTime)
        {
            if (Target.HasValue)
            {
                var model = Agent.SceneObject;
                if (model?.ModelPartCount > 0)
                {
                    model.GetModelPartByName("Turret-mesh").Manipulator.RotateTo(Target.Value, Vector3.Up, Axis.Y, 0.01f);
                    model.GetModelPartByName("Barrel-mesh").Manipulator.RotateTo(Target.Value, Vector3.Up, Axis.X, 0.01f);
                }
            }

            base.Task(gameTime);
        }
    }
}
