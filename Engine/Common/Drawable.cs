﻿using System;
using System.Threading.Tasks;

namespace Engine.Common
{
    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable<T> : BaseSceneObject<T>, IUpdatable, IDrawable, ICullable, IDisposable where T : SceneObjectDescription
    {
        /// <summary>
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager { get { return Game.BufferManager; } }

        /// <inheritdoc/>
        public virtual bool Visible { get; set; }
        /// <inheritdoc/>
        public virtual ShadowCastingAlgorihtms CastShadow { get; protected set; }
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
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        protected Drawable(Scene scene, string id, string name) :
            base(scene, id, name)
        {

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
        protected virtual void Dispose(bool disposing)
        {
            Active = Visible = false;
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(T description)
        {
            await base.InitializeAssets(description);

            Visible = description.StartsVisible;
            CastShadow = description.CastShadow;
            DeferredEnabled = description.DeferredEnabled;
            DepthEnabled = description.DepthEnabled;
            BlendMode = description.BlendMode;
            Usage = SceneObjectUsages.None;
            Layer = 0;
            InstanceCount = 1;
        }

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
            return $"{Id}";
        }
    }
}
