using System;
using System.Threading.Tasks;

namespace Engine.Common
{
    /// <summary>
    /// Base scene object class
    /// </summary>
    /// <typeparam name="T">Description type</typeparam>
    public abstract class BaseSceneObject<T> : ISceneObject where T : SceneObjectDescription
    {
        /// <summary>
        /// Game instance
        /// </summary>
        protected Game Game { get; private set; }

        /// <inheritdoc/>
        public virtual string Id { get; private set; }
        /// <inheritdoc/>
        public virtual string Name { get; set; }
        /// <inheritdoc/>
        public virtual bool Active { get; set; }
        /// <inheritdoc/>
        public virtual Scene Scene { get; protected set; }
        /// <inheritdoc/>
        public virtual bool HasOwner { get { return Owner != null; } }
        /// <inheritdoc/>
        public virtual ISceneObject Owner { get; set; } = null;
        /// <summary>
        /// Object description
        /// </summary>
        public virtual T Description { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected BaseSceneObject(Scene scene, string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "An id must be specified.");
            }

            Id = id;
            Name = name ?? $"_noname_{id}";
            Scene = scene ?? throw new ArgumentNullException(nameof(scene), "The scene must be specified");
            Game = scene.Game;
            Active = false;
        }

        /// <summary>
        /// Initializes internal assets
        /// </summary>
        /// <param name="description">Scene object description</param>
        public virtual async Task InitializeAssets(T description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description), "The description must be specified");
            Active = description.StartsActive;

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Id: {Id}; {Name ?? base.ToString()}";
        }
    }
}
