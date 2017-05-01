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
        public int Width { get; private set; }
        /// <summary>
        /// Map height
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Viewport
        /// </summary>
        public Viewport Viewport { get; private set; }
        /// <summary>
        /// Depth map
        /// </summary>
        public DepthStencilView DepthMap { get; protected set; }
        /// <summary>
        /// Deph map texture
        /// </summary>
        public ShaderResourceView Texture { get; protected set; }

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
        /// Sets shadow light
        /// </summary>
        /// <param name="light">Light</param>
        public static void SetLight(SceneLightDirectional light, float lightDistance, out Vector3 lightPosition, out Vector3 lightDirection)
        {
            // Calc light position outside the scene volume
            lightPosition = light.GetPosition(lightDistance);
            lightDirection = light.Direction;
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

            DepthStencilView dsv;
            ShaderResourceView srv;
            GenerateResources(game, width, height, out dsv, out srv);
            this.DepthMap = dsv;
            this.Texture = srv;
        }
        /// <summary>
        /// Gets from light view * projection matrix
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="shadowDistance">Shadows visible distance</param>
        public Matrix GetFromLightViewProjection(Vector3 lightPosition, Vector3 eyePosition, float shadowDistance)
        {
            // View from light to scene center position
            var view = Matrix.LookAtLH(lightPosition, eyePosition, Vector3.Up);

            // Transform bounding sphere to light space.
            Vector3 sphereCenterLS = Vector3.TransformCoordinate(eyePosition, view);

            // Ortho frustum in light space encloses scene.
            float xleft = sphereCenterLS.X - shadowDistance;
            float xright = sphereCenterLS.X + shadowDistance;
            float ybottom = sphereCenterLS.Y - shadowDistance;
            float ytop = sphereCenterLS.Y + shadowDistance;
            float znear = sphereCenterLS.Z - shadowDistance;
            float zfar = sphereCenterLS.Z + shadowDistance;

            // Orthogonal projection from center
            var projection = Matrix.OrthoOffCenterLH(xleft, xright, ybottom, ytop, znear, zfar);

            return view * projection;
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.DepthMap);
            Helper.Dispose(this.Texture);
        }
    }
}
