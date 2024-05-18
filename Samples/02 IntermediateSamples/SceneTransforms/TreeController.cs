using Engine;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace IntermediateSamples.SceneTransforms
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
        private static readonly List<(ITransformable3D Obj, Vector3 Position, Vector3 Normal, float Time)> brokenTrees = [];

        /// <summary>
        /// Adds a tree to the controller
        /// </summary>
        /// <param name="tree">Tree</param>
        /// <param name="collisionVector">Collision vector</param>
        /// <param name="time">Total time seconds</param>
        public static void AddFallingTree(ITransformable3D tree, Vector3 collisionVector, float time)
        {
            if (brokenTrees.Exists(t => t.Obj == tree))
            {
                return;
            }

            brokenTrees.Add((tree, tree.Manipulator.Position, tree.Manipulator.Up, time));

            trees.Add(new FallingTreeController() { Tree = tree, CollisionVector = collisionVector, Active = true });
        }
        /// <summary>
        /// Adds a tree to the controller
        /// </summary>
        /// <param name="tree">Tree</param>
        /// <param name="durationSeconds">Seconds to disappear</param>
        public static void AddBrokenTree(ITransformable3D tree, float durationSeconds)
        {
            trees.Add(new BrokenTreeController() { Tree = tree, DurationSeconds = durationSeconds, Active = true });
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public static void Update(IGameTime gameTime)
        {
            UpdateTrees(gameTime);

            UpdateBrokenTrees(gameTime);
        }
        private static void UpdateTrees(IGameTime gameTime)
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
        private static void UpdateBrokenTrees(IGameTime gameTime)
        {
            if (brokenTrees.Count == 0)
            {
                return;
            }

            brokenTrees
                .Where(t => gameTime.TotalSeconds - t.Time > 10f)
                .ToList()
                .ForEach(t =>
                {
                    t.Obj.Manipulator.SetNormal(t.Normal);
                    t.Obj.Manipulator.SetPosition(t.Position);
                });

            brokenTrees.RemoveAll(t => gameTime.TotalSeconds - t.Time > 10f);
        }
    }
}
