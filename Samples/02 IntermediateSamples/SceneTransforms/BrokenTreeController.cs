using System.Numerics;

namespace IntermediateSamples.SceneTransforms
{
    using Engine;
    using Engine.Mathematics;

    /// <summary>
    /// Broken tree controller
    /// </summary>
    struct BrokenTreeController : IController
    {
        /// <summary>
        /// Tree
        /// </summary>
        public ITransformable3D Tree { get; set; }
        /// <summary>
        /// Disappearing duration
        /// </summary>
        public float DurationSeconds { get; set; }
        /// <inheritdoc/>
        public bool Active { get; set; }

        /// <inheritdoc/>
        public void UpdateController(IGameTime gameTime)
        {
            if (!Active)
            {
                return;
            }

            float time = gameTime.ElapsedSeconds;
            DurationSeconds -= time;
            Tree.Manipulator.SetPosition(Tree.Manipulator.Position + (Vector3Extensions.Down * time));

            if (DurationSeconds <= 0)
            {
                Tree.Manipulator.SetPosition(new Vector3(float.MaxValue));

                Active = false;
            }
        }
    }
}
