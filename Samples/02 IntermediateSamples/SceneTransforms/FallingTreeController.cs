using Engine;
using SharpDX;

namespace IntermediateSamples.SceneTransforms
{
    /// <summary>
    /// Falling tree controller
    /// </summary>
    struct FallingTreeController : IController
    {
        /// <summary>
        /// Tree
        /// </summary>
        public ITransformable3D Tree { get; set; }
        /// <summary>
        /// Collision vector
        /// </summary>
        public Vector3 CollisionVector { get; set; }
        /// <inheritdoc/>
        public bool Active { get; set; }

        /// <inheritdoc/>
        public void UpdateController(IGameTime gameTime)
        {
            if (!Active)
            {
                return;
            }

            Tree.Manipulator.SetNormal(CollisionVector, 0.1f);

            if (MathUtil.NearEqual(1f, Vector3.Dot(CollisionVector, Tree.Manipulator.Up)))
            {
                Active = false;

                TreeController.AddBrokenTree(Tree, 5f);
            }
        }
    }
}
