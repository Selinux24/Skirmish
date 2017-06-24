using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : IUpdatable, IDrawable, ICullable, IDisposable
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
        /// Game class
        /// </summary>
        public virtual Game Game { get { return this.Scene.Game; } }
        /// <summary>
        /// Graphics device
        /// </summary>
        public virtual Graphics Graphics { get { return this.Scene.Game.Graphics; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        public virtual BufferManager BufferManager { get { return this.Scene.BufferManager; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Drawable(Scene scene, SceneObjectDescription description)
        {
            this.Scene = scene;
            this.Description = description;
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public abstract void Update(UpdateContext context);
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public abstract void Draw(DrawContext context);
        /// <summary>
        /// Dispose resources
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public virtual bool Cull(BoundingFrustum frustum)
        {
            return false;
        }
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="box">Box</param>
        public virtual bool Cull(BoundingBox box)
        {
            return false;
        }
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public virtual bool Cull(BoundingSphere sphere)
        {
            return false;
        }
    }
}
