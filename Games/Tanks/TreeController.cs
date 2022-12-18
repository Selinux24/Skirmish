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
        private static readonly List<IController> trees = new List<IController>();
        /// <summary>
        /// Broken trees
        /// </summary>
        private static readonly List<ModelInstance> brokenTrees = new List<ModelInstance>();

        /// <summary>
        /// Adds a tree to the controller
        /// </summary>
        /// <param name="tree">Tree</param>
        /// <param name="collision">Collision vector</param>
        public static void AddFallingTree(ModelInstance tree, Vector3 collision)
        {
            if (brokenTrees.Any(t => t == tree))
            {
                return;
            }

            brokenTrees.Add(tree);

            trees.Add(new FallingTreeController() { Tree = tree, Collision = collision, Active = true });
        }
        /// <summary>
        /// Adds a tree to the controller
        /// </summary>
        /// <param name="tree">Tree</param>
        /// <param name="durationSeconds">Seconds to disapear</param>
        private static void AddBrokenTree(ModelInstance tree, float durationSeconds)
        {
            trees.Add(new BrokenTreeController() { Tree = tree, DurationSeconds = durationSeconds, Active = true });
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            if (!trees.Any())
            {
                return;
            }

            trees.RemoveAll(controller => !controller.Active);

            if (!trees.Any())
            {
                return;
            }

            trees.ForEach(controller => controller.UpdateController(gameTime));
        }

        /// <summary>
        /// Gets whether the tree is broken or not
        /// </summary>
        /// <param name="tree">Tree</param>
        public static bool IsBroken(ModelInstance tree)
        {
            return brokenTrees.Any(t => t == tree);
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
            void UpdateController(GameTime gameTime);
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
            /// Collision
            /// </summary>
            public Vector3 Collision { get; set; }
            /// <summary>
            /// Active
            /// </summary>
            public bool Active { get; set; }

            /// <summary>
            /// Updates the controller
            /// </summary>
            /// <param name="gameTime">Game time</param>
            public void UpdateController(GameTime gameTime)
            {
                if (!Active)
                {
                    return;
                }

                var target = Tree.Manipulator.Position + (Collision * 10f);

                Tree.Manipulator.RotateTo(target, Axis.None, 0.1f);

                if (MathUtil.NearEqual(1f, Vector3.Dot(Collision, Tree.Manipulator.Forward)))
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
            /// Disapearing duration
            /// </summary>
            public float DurationSeconds { get; set; }
            /// <summary>
            /// Active
            /// </summary>
            public bool Active { get; set; }

            /// <summary>
            /// Updates the controller
            /// </summary>
            /// <param name="gameTime">Game time</param>
            public void UpdateController(GameTime gameTime)
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
