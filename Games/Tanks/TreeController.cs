using Engine;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Tanks
{
    /// <summary>
    /// Tree controller
    /// </summary>
    static class TreeController
    {
        /// <summary>
        /// Controller list
        /// </summary>
        private static readonly List<IController> trees = [];
        /// <summary>
        /// Broken trees
        /// </summary>
        private static readonly List<ModelInstance> brokenTrees = [];

        /// <summary>
        /// Adds a tree to the controller
        /// </summary>
        /// <param name="tree">Tree</param>
        /// <param name="collision">Collision vector</param>
        public static void AddFallingTree(ModelInstance tree, Vector3 collisionVector)
        {
            if (brokenTrees.Exists(t => t == tree))
            {
                return;
            }

            brokenTrees.Add(tree);

            trees.Add(new FallingTreeController() { Tree = tree, CollisionVector = collisionVector, Active = true });
        }
        /// <summary>
        /// Adds a tree to the controller
        /// </summary>
        /// <param name="tree">Tree</param>
        /// <param name="durationSeconds">Seconds to disappear</param>
        public static void AddBrokenTree(ModelInstance tree, float durationSeconds)
        {
            trees.Add(new BrokenTreeController() { Tree = tree, DurationSeconds = durationSeconds, Active = true });
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        public static void Update(IGameTime gameTime)
        {
            if (trees.Count == 0)
            {
                return;
            }

            trees.RemoveAll(controller => !controller.Active);

            if (trees.Count == 0)
            {
                return;
            }

            var activeTrees = trees.Where(controller => controller.Active).ToArray();
            foreach (var tree in activeTrees)
            {
                tree.UpdateController(gameTime);
            }
        }

        /// <summary>
        /// Gets whether the tree is broken or not
        /// </summary>
        /// <param name="tree">Tree</param>
        public static bool IsBroken(ModelInstance tree)
        {
            return brokenTrees.Exists(t => t == tree);
        }

        /// <summary>
        /// Controller interface
        /// </summary>
        interface IController
        {
            /// <summary>
            /// Active
            /// </summary>
            bool Active { get; }

            /// <summary>
            /// Updates the controller
            /// </summary>
            /// <param name="gameTime">Game time</param>
            void UpdateController(IGameTime gameTime);
        }

        /// <summary>
        /// Falling tree controller
        /// </summary>
        struct FallingTreeController : IController
        {
            /// <summary>
            /// Tree
            /// </summary>
            public ModelInstance Tree { get; set; }
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

                    AddBrokenTree(Tree, 5f);
                }
            }
        }

        /// <summary>
        /// Broken tree controller
        /// </summary>
        struct BrokenTreeController : IController
        {
            /// <summary>
            /// Tree
            /// </summary>
            public ModelInstance Tree { get; set; }
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
                Tree.Manipulator.SetPosition(Tree.Manipulator.Position + (Vector3.Down * time));

                if (DurationSeconds <= 0)
                {
                    Tree.Manipulator.SetPosition(new Vector3(float.MaxValue));

                    Active = false;
                }
            }
        }
    }
}
