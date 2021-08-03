using System;

namespace Engine.Common
{
    /// <summary>
    /// Base scene object class
    /// </summary>
    public abstract class BaseSceneObject : ISceneObject
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
        public virtual SceneDrawableDescription Description { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected BaseSceneObject(string id, string name, Scene scene, SceneDrawableDescription description)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), $"An id must be specified.");
            }

            Id = id;
            Name = name;
            Active = description.StartsActive;
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            Description = description ?? throw new ArgumentNullException(nameof(description));

            Game = scene?.Game;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Id: {Id}; Name: {Name ?? base.ToString()}";
        }
    }
}
