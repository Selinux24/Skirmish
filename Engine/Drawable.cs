using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : Updatable, IDrawable, ICullable
    {
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
        public Drawable(Scene scene, SceneObjectDescription description) : base(scene, description)
        {

        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public abstract void Draw(DrawContext context);

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
