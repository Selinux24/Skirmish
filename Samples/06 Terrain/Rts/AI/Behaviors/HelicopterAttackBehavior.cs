﻿using Engine;
using SharpDX;

namespace Terrain.Rts.AI.Behaviors
{
    /// <summary>
    /// Helicopter attack behavior
    /// </summary>
    public class HelicopterAttackBehavior : AttackBehavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public HelicopterAttackBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Task(GameTime gameTime)
        {
            if (Target != null)
            {
                var model = Agent.SceneObject;
                if (model != null)
                {
                    model.Manipulator.RotateTo(Target.Value, Vector3.Up, Axis.Y, 0.01f);
                }
            }

            base.Task(gameTime);
        }
    }
}
