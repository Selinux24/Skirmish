using Engine.Common;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Engine.Tween
{
    /// <summary>
    /// Tweener
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class Tweener(Scene scene, string id, string name) : IUpdatable
    {
        /// <summary>
        /// Task list
        /// </summary>
        private readonly ConcurrentBag<ITweenCollection> tweens = [];

        /// <inheritdoc/>
        public string Id { get; private set; } = id;
        /// <inheritdoc/>
        public string Name { get; set; } = name;
        /// <inheritdoc/>
        public bool Active { get; set; } = true;
        /// <inheritdoc/>
        public Scene Scene { get; private set; } = scene;
        /// <inheritdoc/>
        public SceneObjectUsages Usage { get; set; }
        /// <inheritdoc/>
        public int Layer { get; set; }
        /// <inheritdoc/>
        public bool HasOwner
        {
            get
            {
                return Owner != null;
            }
        }
        /// <inheritdoc/>
        public ISceneObject Owner { get; set; }

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
            if (tweens.IsEmpty)
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
