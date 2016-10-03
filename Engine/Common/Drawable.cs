using System;
using SharpDX;
using SharpDX.Direct3D11;

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
        /// Culling test flag
        /// </summary>
        /// <remarks>True if passes culling test</remarks>
        public bool Cull { get; protected set; }

        /// <summary>
        /// Gets or sets whether the object is static
        /// </summary>
        public bool Static { get; set; }
        /// <summary>
        /// Always visible
        /// </summary>
        public bool AlwaysVisible { get; set; }
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        public bool DeferredEnabled { get; set; }
        /// <summary>
        /// Enables z-buffer writting
        /// </summary>
        public bool EnableDepthStencil { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool EnableAlphaBlending { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public Drawable(Game game)
        {
            this.Game = game;
            this.Active = true;
            this.Visible = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.Order = 0;
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
        public virtual void Culling(BoundingFrustum frustum)
        {
            this.Cull = false;
        }
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public virtual void Culling(BoundingSphere sphere)
        {
            this.Cull = false;
        }
        /// <summary>
        /// Sets cull value
        /// </summary>
        /// <param name="value">New value</param>
        public virtual void SetCulling(bool value)
        {
            this.Cull = value;
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
    /// Updating context
    /// </summary>
    public class UpdateContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name = "";
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime;
        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix World;
        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix View;
        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection;
        /// <summary>
        /// Bounding frustum
        /// </summary>
        public BoundingFrustum Frustum;
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition;
        /// <summary>
        /// Eye target
        /// </summary>
        public Vector3 EyeTarget;
    }

    /// <summary>
    /// Drawing context
    /// </summary>
    public class DrawContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name = "";
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime;
        /// <summary>
        /// Drawer mode
        /// </summary>
        public DrawerModesEnum DrawerMode = DrawerModesEnum.Forward;
        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix World;
        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix View;
        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection;
        /// <summary>
        /// Bounding frustum
        /// </summary>
        public BoundingFrustum Frustum;
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition;
        /// <summary>
        /// Eye target
        /// </summary>
        public Vector3 EyeTarget;
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLights Lights;
        /// <summary>
        /// View * projection from light matrix
        /// </summary>
        public Matrix FromLightViewProjection;
        /// <summary>
        /// Shadow maps
        /// </summary>
        public int ShadowMaps;
        /// <summary>
        /// Static shadow map
        /// </summary>
        public ShaderResourceView ShadowMapStatic;
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        public ShaderResourceView ShadowMapDynamic;
    }

    /// <summary>
    /// Drawer modes
    /// </summary>
    public enum DrawerModesEnum
    {
        /// <summary>
        /// Forward rendering (default)
        /// </summary>
        Forward,
        /// <summary>
        /// Deferred rendering
        /// </summary>
        Deferred,
        /// <summary>
        /// Shadow map
        /// </summary>
        ShadowMap,
    }
}
