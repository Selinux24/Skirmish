using System;

namespace Engine.Common
{
    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : BaseSceneObject, IUpdatable, IDrawable, ICullable, IDisposable
    {
        /// <summary>
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager { get { return Game.BufferManager; } }

        /// <inheritdoc/>
        public virtual bool Visible { get; set; }
        /// <inheritdoc/>
        public virtual bool CastShadow { get; protected set; }
        /// <inheritdoc/>
        public virtual bool DeferredEnabled { get; protected set; }
        /// <inheritdoc/>
        public virtual bool DepthEnabled { get; protected set; }
        /// <inheritdoc/>
        public virtual BlendModes BlendMode { get; protected set; }
        /// <inheritdoc/>
        public virtual SceneObjectUsages Usage { get; set; }
        /// <inheritdoc/>
        public virtual int Layer { get; set; }
        /// <inheritdoc/>
        public virtual int InstanceCount { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Drawable(string id, string name, Scene scene, SceneDrawableDescription description) :
            base(id, name, scene, description)
        {
            Visible = description.StartsVisible;
            CastShadow = description.CastShadow;
            DeferredEnabled = description.DeferredEnabled;
            DepthEnabled = description.DepthEnabled;
            BlendMode = description.BlendMode;
            Usage = SceneObjectUsages.None;
            Layer = 0;
            InstanceCount = 1;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Drawable()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
        public virtual void EarlyUpdate(UpdateContext context)
        {

        }
        /// <inheritdoc/>
        public virtual void Update(UpdateContext context)
        {

        }
        /// <inheritdoc/>
        public virtual void LateUpdate(UpdateContext context)
        {

        }

        /// <inheritdoc/>
        public virtual void DrawShadows(DrawContextShadows context)
        {

        }
        /// <inheritdoc/>
        public virtual void Draw(DrawContext context)
        {

        }

        /// <inheritdoc/>
        public virtual bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{GetType()}.{Name ?? "NoName"}";
        }
    }
}
