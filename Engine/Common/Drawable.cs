using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : ICull, IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        public virtual Game Game { get; private set; }
        /// <summary>
        /// Graphics device
        /// </summary>
        public virtual Device Device { get { return this.Game.Graphics.Device; } }
        /// <summary>
        /// Graphics context
        /// </summary>
        public virtual DeviceContext DeviceContext { get { return this.Game.Graphics.DeviceContext; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        public virtual BufferManager BufferManager { get; protected set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Maximum instance count
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets or sets whether the object is static
        /// </summary>
        public bool Static { get; set; }
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        public bool DeferredEnabled { get; set; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        public bool DepthEnabled { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool AlphaEnabled { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Description</param>
        public Drawable(Game game, BufferManager bufferManager, DrawableDescription description)
        {
            this.Game = game;
            this.BufferManager = bufferManager;
            this.Active = true;
            this.Visible = true;
            this.Order = 0;

            this.Name = description.Name;
            this.Static = description.Static;
            this.CastShadow = description.CastShadow;
            this.DeferredEnabled = description.DeferredEnabled;
            this.DepthEnabled = description.DepthEnabled;
            this.AlphaEnabled = description.AlphaEnabled;
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

        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Type: {0}; Name: {1}; Order: {2}", this.GetType(), this.Name, this.Order);
        }
    }
}
