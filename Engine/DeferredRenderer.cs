using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Deferred renderer class
    /// </summary>
    public class DeferredRenderer : IDisposable, IScreenFitted
    {
        /// <summary>
        /// Light geometry
        /// </summary>
        class LightGeometry : IDisposable
        {
            /// <summary>
            /// Window vertex buffer
            /// </summary>
            public Buffer VertexBuffer;
            /// <summary>
            /// Vertex buffer binding
            /// </summary>
            public VertexBufferBinding VertexBufferBinding;
            /// <summary>
            /// Window index buffer
            /// </summary>
            public Buffer IndexBuffer;
            /// <summary>
            /// Index count
            /// </summary>
            public int IndexCount;

            /// <summary>
            /// Dispose objects
            /// </summary>
            public void Dispose()
            {
                if (this.VertexBuffer != null)
                {
                    this.VertexBuffer.Dispose();
                    this.VertexBuffer = null;
                }

                if (this.IndexBuffer != null)
                {
                    this.IndexBuffer.Dispose();
                    this.IndexBuffer = null;
                }
            }
        }

        /// <summary>
        /// Geometry buffer
        /// </summary>
        private GBuffer geometryBuffer = null;
        /// <summary>
        /// Light buffer
        /// </summary>
        private LightBuffer lightBuffer = null;
        /// <summary>
        /// Light geometry collection
        /// </summary>
        private LightGeometry[] lightGeometry = null;

        /// <summary>
        /// Game
        /// </summary>
        protected Game Game;
        /// <summary>
        /// Renderer width
        /// </summary>
        protected int Width;
        /// <summary>
        /// Renderer height
        /// </summary>
        protected int Height;
        /// <summary>
        /// View * OrthoProjection Matrix
        /// </summary>
        protected Matrix ViewProjection;

        /// <summary>
        /// Viewport
        /// </summary>
        public Viewport Viewport;
        /// <summary>
        /// Geometry Buffer
        /// </summary>
        public GBuffer GeometryBuffer
        {
            get
            {
                if (this.geometryBuffer == null)
                {
                    this.geometryBuffer = new GBuffer(this.Game);
                }

                return this.geometryBuffer;
            }
        }
        /// <summary>
        /// Light Buffer
        /// </summary>
        public LightBuffer LightBuffer
        {
            get
            {
                if (this.lightBuffer == null)
                {
                    this.lightBuffer = new LightBuffer(this.Game);
                }

                return this.lightBuffer;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public DeferredRenderer(Game game)
        {
            this.Game = game;

            this.UpdateRectangleAndView();
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public virtual void Dispose()
        {
            if (this.geometryBuffer != null)
            {
                this.geometryBuffer.Dispose();
                this.geometryBuffer = null;
            }

            if (this.lightBuffer != null)
            {
                this.lightBuffer.Dispose();
                this.lightBuffer = null;
            }

            if (this.lightGeometry != null && this.lightGeometry.Length > 0)
            {
                for (int i = 0; i < this.lightGeometry.Length; i++)
                {
                    this.lightGeometry[i].Dispose();
                }
            }

            this.lightGeometry = null;
        }
        /// <summary>
        /// Resizes buffers
        /// </summary>
        public virtual void Resize()
        {
            this.UpdateRectangleAndView();

            if (this.geometryBuffer != null)
            {
                this.geometryBuffer.Resize();
            }

            if (this.lightBuffer != null)
            {
                this.lightBuffer.Resize();
            }
        }
        /// <summary>
        /// Draw lights
        /// </summary>
        /// <param name="context">Drawing context</param>
        public void DrawLights(Context context)
        {
            var deviceContext = this.Game.Graphics.DeviceContext;
            var effect = DrawerPool.EffectDeferred;

            this.Game.Graphics.SetBlendAdditive();
#if DEBUG
            Stopwatch swDirectional = Stopwatch.StartNew();
#endif
            #region Directional Lights

            SceneLightDirectional[] directionalLights = Array.FindAll(context.Lights.DirectionalLights, l => l.Enabled);
            if (directionalLights != null && directionalLights.Length > 0)
            {
                var effectTechnique = effect.DeferredDirectionalLight;
                var geometry = this.lightGeometry[0];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                effect.World = Matrix.Identity;
                effect.WorldViewProjection = this.ViewProjection;
                effect.EyePositionWorld = context.EyePosition;
                effect.ColorMap = context.GeometryMap[0];
                effect.NormalMap = context.GeometryMap[1];
                effect.DepthMap = context.GeometryMap[2];

                for (int i = 0; i < directionalLights.Length; i++)
                {
                    var light = directionalLights[i];

                    effect.DirectionalLight = new BufferDirectionalLight(light);

                    for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                    {
                        effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                        deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }

            #endregion
#if DEBUG
            swDirectional.Stop();
#endif
#if DEBUG
            Stopwatch swPoint = Stopwatch.StartNew();
#endif
            #region Point Lights

            SceneLightPoint[] pointLights = Array.FindAll(context.Lights.PointLights, l => l.Enabled);
            if (pointLights != null && pointLights.Length > 0)
            {
                var effectTechnique = effect.DeferredPointLight;
                var geometry = this.lightGeometry[1];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                effect.EyePositionWorld = context.EyePosition;
                effect.ColorMap = context.GeometryMap[0];
                effect.NormalMap = context.GeometryMap[1];
                effect.DepthMap = context.GeometryMap[2];

                for (int i = 0; i < pointLights.Length; i++)
                {
                    var light = pointLights[i];

                    if (context.Frustum.Contains(new BoundingSphere(light.Position, light.Range)) != ContainmentType.Disjoint)
                    {
                        float cameraToCenter = Vector3.Distance(context.EyePosition, light.Position);
                        if (cameraToCenter < light.Range)
                        {
                            this.Game.Graphics.SetRasterizerDefault();
                        }
                        else
                        {
                            this.Game.Graphics.SetRasterizerCullFrontFace();
                        }

                        Matrix world = Matrix.Scaling(light.Range) * Matrix.Translation(light.Position);

                        effect.PointLight = new BufferPointLight(light);
                        effect.World = world;
                        effect.WorldViewProjection = world * context.ViewProjection;

                        for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                        {
                            effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                            deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                            Counters.DrawCallsPerFrame++;
                            Counters.InstancesPerFrame++;
                        }
                    }
                }
            }

            #endregion
#if DEBUG
            swPoint.Stop();

            Stopwatch swSpot = Stopwatch.StartNew();
#endif
            #region Spot Lights

            SceneLightSpot[] spotLights = Array.FindAll(context.Lights.SpotLights, l => l.Enabled);
            if (spotLights != null && spotLights.Length > 0)
            {
                var effectTechnique = effect.DeferredSpotLight;
                var geometry = this.lightGeometry[2];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                effect.EyePositionWorld = context.EyePosition;
                effect.ColorMap = context.GeometryMap[0];
                effect.NormalMap = context.GeometryMap[1];
                effect.DepthMap = context.GeometryMap[2];

                for (int i = 0; i < spotLights.Length; i++)
                {
                    var light = spotLights[i];

                    if (context.Frustum.Contains(new BoundingSphere(light.Position, light.Range)) != ContainmentType.Disjoint)
                    {
                        float cameraToCenter = Vector3.Distance(context.EyePosition, light.Position);
                        if (cameraToCenter < light.Range)
                        {
                            this.Game.Graphics.SetRasterizerDefault();
                        }
                        else
                        {
                            this.Game.Graphics.SetRasterizerCullFrontFace();
                        }

                        Matrix world = Matrix.Scaling(light.Range) * Matrix.Translation(light.Position);

                        effect.SpotLight = new BufferSpotLight(light);
                        effect.World = world;
                        effect.WorldViewProjection = world * context.ViewProjection;

                        for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                        {
                            effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                            deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                            Counters.DrawCallsPerFrame++;
                            Counters.InstancesPerFrame++;
                        }
                    }
                }
            }

            #endregion
#if DEBUG
            swSpot.Stop();
#endif
            this.Game.Graphics.SetBlendAlphaToCoverage();

            this.Game.Graphics.SetRasterizerCullFrontFace();
#if DEBUG
            context.Tag = new[]
            {
                swDirectional.ElapsedTicks,
                swPoint.ElapsedTicks,
                swSpot.ElapsedTicks,
            };
#endif
        }
        /// <summary>
        /// Draw result
        /// </summary>
        /// <param name="context">Drawing context</param>
        public void DrawResult(Context context)
        {
            if (context.GeometryMap != null && context.LightMap != null)
            {
                var effect = DrawerPool.EffectDeferred;
                var effectTechnique = effect.DeferredCombineLights;

                effect.World = Matrix.Identity;
                effect.WorldViewProjection = this.ViewProjection;
                effect.EyePositionWorld = context.EyePosition;
                effect.FogStart = context.Lights.FogStart;
                effect.FogRange = context.Lights.FogRange;
                effect.FogColor = context.Lights.FogColor;
                effect.ColorMap = context.GeometryMap[0];
                effect.NormalMap = context.GeometryMap[1];
                effect.DepthMap = context.GeometryMap[2];
                effect.LightMap = context.LightMap;

                var deviceContext = this.Game.Graphics.DeviceContext;
                var geometry = this.lightGeometry[0];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                {
                    effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                    deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
            }
        }
        /// <summary>
        /// Updates renderer parameters
        /// </summary>
        private void UpdateRectangleAndView()
        {
            this.Width = this.Game.Form.RenderWidth;
            this.Height = this.Game.Form.RenderHeight;

            this.Viewport = new Viewport(0, 0, this.Width, this.Height, 0, 1.0f);

            this.ViewProjection = Sprite.CreateViewOrthoProjection(this.Width, this.Height);

            if (this.lightGeometry == null)
            {
                this.lightGeometry = new[]
                {
                    new LightGeometry(),
                    new LightGeometry(),
                    new LightGeometry(),
                };
            }

            this.UpdateDirectionalLightGeometry(ref this.lightGeometry[0]);
            this.UpdatePointLightGeometry(ref this.lightGeometry[1]);
            this.UpdateSpotLightGeometry(ref this.lightGeometry[2]);
        }
        /// <summary>
        /// Update directional light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdateDirectionalLightGeometry(ref LightGeometry geometry)
        {
            VertexData[] cv;
            uint[] ci;
            VertexData.CreateScreen(
                Game.Form,
                out cv,
                out ci);

            List<VertexPositionTexture> vertList = new List<VertexPositionTexture>();

            Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPositionTexture(v)); });

            if (geometry.VertexBuffer == null)
            {
                geometry.VertexBuffer = Game.Graphics.Device.CreateVertexBufferWrite(vertList.ToArray());
                geometry.VertexBufferBinding = new VertexBufferBinding(geometry.VertexBuffer, vertList[0].Stride, 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteBuffer(geometry.VertexBuffer, vertList.ToArray());
            }

            if (geometry.IndexBuffer == null)
            {
                geometry.IndexBuffer = Game.Graphics.Device.CreateIndexBufferImmutable(ci);
            }

            geometry.IndexCount = ci.Length;
        }
        /// <summary>
        /// Update point light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdatePointLightGeometry(ref LightGeometry geometry)
        {
            VertexData[] cv;
            uint[] ci;
            VertexData.CreateSphere(
                1, 12, 12,
                out cv,
                out ci);

            List<VertexPosition> vertList = new List<VertexPosition>();

            Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPosition(v)); });

            if (geometry.VertexBuffer == null)
            {
                geometry.VertexBuffer = Game.Graphics.Device.CreateVertexBufferWrite(vertList.ToArray());
                geometry.VertexBufferBinding = new VertexBufferBinding(geometry.VertexBuffer, vertList[0].Stride, 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteBuffer(geometry.VertexBuffer, vertList.ToArray());
            }

            if (geometry.IndexBuffer == null)
            {
                geometry.IndexBuffer = Game.Graphics.Device.CreateIndexBufferImmutable(ci);
            }

            geometry.IndexCount = ci.Length;
        }
        /// <summary>
        /// Update spot light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdateSpotLightGeometry(ref LightGeometry geometry)
        {
            VertexData[] cv;
            uint[] ci;
            VertexData.CreateCone(
                1, 12, 12,
                out cv,
                out ci);

            List<VertexPosition> vertList = new List<VertexPosition>();

            Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPosition(v)); });

            if (geometry.VertexBuffer == null)
            {
                geometry.VertexBuffer = Game.Graphics.Device.CreateVertexBufferWrite(vertList.ToArray());
                geometry.VertexBufferBinding = new VertexBufferBinding(geometry.VertexBuffer, vertList[0].Stride, 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteBuffer(geometry.VertexBuffer, vertList.ToArray());
            }

            if (geometry.IndexBuffer == null)
            {
                geometry.IndexBuffer = Game.Graphics.Device.CreateIndexBufferImmutable(ci);
            }

            geometry.IndexCount = ci.Length;
        }
    }
}
