﻿using Engine.Common;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Tween
{
    /// <summary>
    /// Tweener
    /// </summary>
    public class Tweener : IUpdatable
    {
        /// <summary>
        /// Task list
        /// </summary>
        private readonly ConcurrentBag<ITweenCollection> tweens = new();

        /// <inheritdoc/>
        public string Id { get; private set; }
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public bool Active { get; set; } = true;
        /// <inheritdoc/>
        public Scene Scene { get; private set; }
        /// <inheritdoc/>
        public SceneObjectUsages Usage { get; set; }
        /// <inheritdoc/>
        public int Layer { get; set; }
        /// <inheritdoc/>
        public bool HasOwner { get; private set; }
        /// <inheritdoc/>
        public ISceneObject Owner { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Tweener(Scene scene, string id, string name)
        {
            Scene = scene;
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Adds a new tween collection to the tween manager
        /// </summary>
        /// <param name="tweenCollection">Tween collection</param>
        public void AddTweenCollection(ITweenCollection tweenCollection)
        {
            tweens.Add(tweenCollection);
        }
        /// <summary>
        /// Clears the tween manager
        /// </summary>
        public void Clear()
        {
            while (!tweens.IsEmpty)
            {
                if (tweens.TryTake(out var tween))
                {
                    tween.Clear();
                }
            }
        }

        /// <inheritdoc/>
        public void EarlyUpdate(UpdateContext context)
        {
            if (!tweens.Any())
            {
                return;
            }

            Parallel.ForEach(tweens.ToArray(), t =>
            {
                t?.Update(context.GameTime);
            });
        }
        /// <inheritdoc/>
        public void Update(UpdateContext context)
        {
            //Not applicable
        }
        /// <inheritdoc/>
        public void LateUpdate(UpdateContext context)
        {
            //Not applicable
        }
    }
}