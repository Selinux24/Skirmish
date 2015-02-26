using System;
using SharpDX.Direct3D11;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : IDisposable
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
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public Drawable(Game game)
        {
            this.Game = game;
            this.Active = true;
            this.Visible = true;
            this.Order = 0;
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public abstract void Update(GameTime gameTime, Context context);
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public abstract void Draw(GameTime gameTime, Context context);
        /// <summary>
        /// Dispose resources
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Performs frustum culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if component passes frustum culling test</returns>
        public virtual void FrustumCulling(BoundingFrustum frustum)
        {
            
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

    /// <summary>
    /// Drawable context
    /// </summary>
    public class Context
    {
        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix World;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection;
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition;
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLight Lights;
    }
}
