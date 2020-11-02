using System;

namespace Engine.Common
{
    /// <summary>
    /// Base scene object class
    /// </summary>
    public abstract class BaseSceneObject : IDisposable
    {
        /// <summary>
        /// Object name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Game class
        /// </summary>
        public Scene Scene { get; private set; }
        /// <summary>
        /// Object description
        /// </summary>
        public SceneObjectDescription Description { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected BaseSceneObject(string name, Scene scene, SceneObjectDescription description)
        {
            Name = name;
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseSceneObject()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected abstract void Dispose(bool disposing);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name ?? base.ToString()}";
        }
    }
}
