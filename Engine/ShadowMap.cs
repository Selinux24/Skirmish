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
        /// Depth map
        /// </summary>
        public DepthStencilView DepthMap { get; protected set; }
        /// <summary>
        /// Deph map texture
        /// </summary>
        public ShaderResourceView Texture { get; protected set; }
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
                this.DepthMap = new DepthStencilView(
                    game.Graphics.Device,
                    depthMap,
                    new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        Dimension = DepthStencilViewDimension.Texture2D,
                        Texture2D = new DepthStencilViewDescription.Texture2DResource()
                        {
                            MipSlice = 0,
                        },
                    });

                this.Texture = new ShaderResourceView(
                    game.Graphics.Device,
                    depthMap,
                    new ShaderResourceViewDescription
                    {
                        Format = Format.R24_UNorm_X8_Typeless,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                        {
                            MipLevels = 1,
                            MostDetailedMip = 0
                        },
                    });
            }
        }
        /// <summary>
        /// Updates shadow map generation parameters
        /// </summary>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="sceneVolume">Scene volume</param>
        public void Update(SceneLightDirectional light, BoundingSphere sceneVolume)
        {
            // Calc light position outside the scene volume
            this.LightPosition = light.GetPosition(2.0f * sceneVolume.Radius);
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
            if (this.DepthMap != null)
            {
                this.DepthMap.Dispose();
                this.DepthMap = null;
            }

            if (this.Texture != null)
            {
                this.Texture.Dispose();
                this.Texture = null;
            }
        }
    }
}
