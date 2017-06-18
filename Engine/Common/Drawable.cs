using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : IUpdatable, IDrawable, ICull, IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        public virtual Game Game { get; private set; }
        /// <summary>
        /// Graphics device
        /// </summary>
        public virtual Graphics Graphics { get { return this.Game.Graphics; } }
        /// <summary>
        /// Graphics context
        /// </summary>
        public virtual DeviceContext DeviceContext { get { return this.Game.Graphics.DeviceContext; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        public virtual BufferManager BufferManager { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        public Drawable(Game game, BufferManager bufferManager)
        {
            this.Game = game;
            this.BufferManager = bufferManager;
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
