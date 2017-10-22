using SharpDX;
using SharpDX.DXGI;
using System;

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
        public EngineDepthStencilView DepthMap { get; protected set; }
        /// <summary>
        /// Deph map texture
        /// </summary>
        public EngineShaderResourceView Texture { get; protected set; }

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

            EngineDepthStencilView dsv;
            EngineShaderResourceView srv;
            game.Graphics.CreateShadowMapTextures(Format.R24G8_Typeless, width, height, out dsv, out srv);
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
