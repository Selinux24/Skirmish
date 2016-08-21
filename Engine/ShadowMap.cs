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
        /// Static deph map texture
        /// </summary>
        public ShaderResourceView TextureStatic { get; protected set; }
        /// <summary>
        /// Dynamic deph map texture
        /// </summary>
        public ShaderResourceView TextureDynamic { get; protected set; }
        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix View { get; protected set; }
        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection { get; protected set; }
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 LightPosition { get; protected set; }
        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 LightDirection { get; protected set; }
        /// <summary>
        /// Shadow transform
        /// </summary>
        public Matrix Transform { get; protected set; }

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
        /// Updates shadow map generation parameters
        /// </summary>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="sceneVolume">Scene volume</param>
        public void Update(SceneLightDirectional light, BoundingSphere sceneVolume)
        {
            // Calc light position outside the scene volume
            this.LightPosition = light.GetPosition(sceneVolume.Radius);
            this.LightDirection = light.Direction;

            // View from light to scene center position
            this.View = Matrix.LookAtLH(this.LightPosition, sceneVolume.Center, Vector3.Up);

            // Transform bounding sphere to light space.
            Vector3 sphereCenterLS = Vector3.TransformCoordinate(sceneVolume.Center, this.View);

            // Ortho frustum in light space encloses scene.
            float xleft = sphereCenterLS.X - sceneVolume.Radius;
            float xright = sphereCenterLS.X + sceneVolume.Radius;
            float ybottom = sphereCenterLS.Y - sceneVolume.Radius;
            float ytop = sphereCenterLS.Y + sceneVolume.Radius;
            float znear = sphereCenterLS.Z - sceneVolume.Radius;
            float zfar = sphereCenterLS.Z + sceneVolume.Radius;

            // Orthogonal projection from center
            this.Projection = Matrix.OrthoOffCenterLH(xleft, xright, ybottom, ytop, znear, zfar);

            // Normal device coordinates transformation
            this.Transform = Helper.NormalDeviceCoordinatesTransform(this.View, this.Projection);
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
}
