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
        public virtual Scene Scene { get; private set; }
        /// <summary>
        /// Object description
        /// </summary>
        public virtual SceneObjectDescription Description { get; private set; }

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
        /// Dispose resources
        /// </summary>
        public abstract void Dispose();
    }
}
