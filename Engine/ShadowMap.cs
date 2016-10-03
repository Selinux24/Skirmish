using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;
using DepthStencilViewDescription = SharpDX.Direct3D11.DepthStencilViewDescription;
using DepthStencilViewDimension = SharpDX.Direct3D11.DepthStencilViewDimension;
using DepthStencilViewFlags = SharpDX.Direct3D11.DepthStencilViewFlags;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using ShaderResourceViewDescription = SharpDX.Direct3D11.ShaderResourceViewDescription;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Shadow map
    /// </summary>
    public class ShadowMap : IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get; private set; }
        /// <summary>
        /// Map width
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// Map height
        /// </summary>
        public readonly int Height;
        /// <summary>
        /// Viewport
        /// </summary>
        public readonly Viewport Viewport;
        /// <summary>
        /// Static depth map
        /// </summary>
        public DepthStencilView DepthMapStatic { get; protected set; }
        /// <summary>
        /// Dynamic depth map
        /// </summary>
        public DepthStencilView DepthMapDynamic { get; protected set; }
        /// <summary>
        /// Shadow map flags
        /// </summary>
        public ShadowMapFlags Flags { get; set; }
        /// <summary>
        /// Static deph map texture
        /// </summary>
        public ShaderResourceView TextureStatic { get; protected set; }
        /// <summary>
        /// Dynamic deph map texture
        /// </summary>
        public ShaderResourceView TextureDynamic { get; protected set; }
        /// <summary>
        /// View * Projection matrix
        /// </summary>
        public Matrix ViewProjection { get; protected set; }

        /// <summary>
        /// Generate internal resources
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="width">Buffer width</param>
        /// <param name="height">Buffer height</param>
        /// <param name="dsv">Depth stencil view to be created</param>
        /// <param name="srv">Texture to be created</param>
        private static void GenerateResources(Game game, int width, int height, out DepthStencilView dsv, out ShaderResourceView srv)
        {
            Texture2D depthMap = new Texture2D(
                game.Graphics.Device,
                new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0,
                    },
                };

                var rvDescription = new ShaderResourceViewDescription
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };

                dsv = new DepthStencilView(game.Graphics.Device, depthMap, dsDescription);
                srv = new ShaderResourceView(game.Graphics.Device, depthMap, rvDescription);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        public ShadowMap(Game game, int width, int height)
        {
            this.Game = game;

            this.Width = width;
            this.Height = height;

            this.Viewport = new Viewport(0, 0, width, height, 0, 1.0f);

            DepthStencilView dsvStatic;
            ShaderResourceView srvStatic;
            GenerateResources(game, width, height, out dsvStatic, out srvStatic);
            this.DepthMapStatic = dsvStatic;
            this.TextureStatic = srvStatic;

            DepthStencilView dsvDynamic;
            ShaderResourceView srvDynamic;
            GenerateResources(game, width, height, out dsvDynamic, out srvDynamic);
            this.DepthMapDynamic = dsvDynamic;
            this.TextureDynamic = srvDynamic;
        }
        /// <summary>
        /// Updates drawing context with shadow map generation parameters
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="center">Scene center</param>
        /// <param name="radius">Scene radius</param>
        /// <param name="context">Drawing context to update</param>
        public void Update(SceneLightDirectional light, Vector3 center, float radius, ref DrawContext context)
        {
            // Calc light position outside the scene volume
            var lightPosition = light.GetPosition(radius);
            var lightDirection = light.Direction;

            // View from light to scene center position
            var view = Matrix.LookAtLH(lightPosition, center, Vector3.Up);

            // Transform bounding sphere to light space.
            Vector3 sphereCenterLS = Vector3.TransformCoordinate(center, view);

            // Ortho frustum in light space encloses scene.
            float xleft = sphereCenterLS.X - radius;
            float xright = sphereCenterLS.X + radius;
            float ybottom = sphereCenterLS.Y - radius;
            float ytop = sphereCenterLS.Y + radius;
            float znear = sphereCenterLS.Z - radius;
            float zfar = sphereCenterLS.Z + radius;

            // Orthogonal projection from center
            var projection = Matrix.OrthoOffCenterLH(xleft, xright, ybottom, ytop, znear, zfar);

            this.ViewProjection = view * projection;

            context.View = view;
            context.Projection = projection;
            context.ViewProjection = this.ViewProjection;
            context.Frustum = new BoundingFrustum(this.ViewProjection);
            context.EyePosition = lightPosition;
            context.EyeTarget = lightDirection;
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public void Dispose()
        {
            if (this.DepthMapStatic != null)
            {
                this.DepthMapStatic.Dispose();
                this.DepthMapStatic = null;
            }
            if (this.DepthMapDynamic != null)
            {
                this.DepthMapDynamic.Dispose();
                this.DepthMapDynamic = null;
            }

            if (this.TextureStatic != null)
            {
                this.TextureStatic.Dispose();
                this.TextureStatic = null;
            }
            if (this.TextureDynamic != null)
            {
                this.TextureDynamic.Dispose();
                this.TextureDynamic = null;
            }
        }
    }

    /// <summary>
    /// Flags
    /// </summary>
    [Flags]
    public enum ShadowMapFlags : int
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Static shadow map
        /// </summary>
        Static = 1,
        /// <summary>
        /// Dynamix shadow map
        /// </summary>
        Dynamic = 2,
    }
}
