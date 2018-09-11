using System;

namespace Engine.Common
{
    /// <summary>
    /// Base scene object class
    /// </summary>
    public abstract class BaseSceneObject : IDisposable
    {
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
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public BaseSceneObject(Scene scene, SceneObjectDescription description)
        {
            this.Scene = scene;
            this.Description = description;
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
    }
}
