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
                Helper.Dispose(this.VertexBuffer);
                Helper.Dispose(this.IndexBuffer);
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
            Helper.Dispose(this.geometryBuffer);
            Helper.Dispose(this.lightBuffer);
            Helper.Dispose(this.lightGeometry);
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
#if DEBUG
            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            #region Initialization
#if DEBUG
            Stopwatch swPrepare = Stopwatch.StartNew();
#endif
            var deviceContext = this.Game.Graphics.DeviceContext;
            var effect = DrawerPool.EffectDeferred;

            effect.UpdatePerFrame(
                context.World,
                this.ViewProjection,
                context.EyePosition,
                context.Lights.FogStart,
                context.Lights.FogRange,
                context.Lights.FogColor,
                context.GeometryMap[0],
                context.GeometryMap[1],
                context.GeometryMap[2],
                context.GeometryMap[3],
                context.ShadowMap);

            this.Game.Graphics.SetDepthStencilNone();
            this.Game.Graphics.SetBlendDeferredLighting();
#if DEBUG
            swPrepare.Stop();
#endif
            #endregion

            #region Directional Lights
#if DEBUG
            Stopwatch swDirectional = Stopwatch.StartNew();
#endif
            SceneLightDirectional[] directionalLights = context.Lights.EnabledDirectionalLights;
            if (directionalLights != null && directionalLights.Length > 0)
            {
                var effectTechnique = effect.DeferredDirectionalLight;
                var geometry = this.lightGeometry[0];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                for (int i = 0; i < directionalLights.Length; i++)
                {
                    effect.UpdatePerLight(directionalLights[i]);

                    for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                    {
                        effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                        deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }
#if DEBUG
            swDirectional.Stop();
#endif
            #endregion

            #region Point Lights
#if DEBUG
            Stopwatch swPoint = Stopwatch.StartNew();
#endif
            SceneLightPoint[] pointLights = context.Lights.EnabledPointLights;
            if (pointLights != null && pointLights.Length > 0)
            {
                var effectTechnique = effect.DeferredPointLight;
                var geometry = this.lightGeometry[1];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                this.Game.Graphics.SetRasterizerCullFrontFace();

                for (int i = 0; i < pointLights.Length; i++)
                {
                    var light = pointLights[i];

                    if (context.Frustum.Contains(light.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        Matrix local = Matrix.Scaling(light.Radius) * Matrix.Translation(light.Position);

                        effect.UpdatePerLight(
                            light,
                            context.World * local,
                            context.ViewProjection);

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
#if DEBUG
            swPoint.Stop();
#endif
            #endregion

            #region Spot Lights
#if DEBUG
            Stopwatch swSpot = Stopwatch.StartNew();
#endif
            SceneLightSpot[] spotLights = context.Lights.EnabledSpotLights;
            if (spotLights != null && spotLights.Length > 0)
            {
                var effectTechnique = effect.DeferredSpotLight;
                var geometry = this.lightGeometry[2];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                this.Game.Graphics.SetRasterizerCullFrontFace();

                for (int i = 0; i < spotLights.Length; i++)
                {
                    var light = spotLights[i];

                    if (context.Frustum.Contains(light.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        Matrix local = Matrix.Scaling(light.Radius) * Matrix.Translation(light.Position);

                        effect.UpdatePerLight(
                            light,
                            context.World * local,
                            context.ViewProjection);

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
#if DEBUG
            swSpot.Stop();
#endif
            #endregion
#if DEBUG
            swTotal.Stop();
#endif
#if DEBUG
            long total = swPrepare.ElapsedTicks + swDirectional.ElapsedTicks + swPoint.ElapsedTicks + swSpot.ElapsedTicks;
            if (total > 0)
            {
                float prcPrepare = (float)swPrepare.ElapsedTicks / (float)total;
                float prcDirectional = (float)swDirectional.ElapsedTicks / (float)total;
                float prcPoint = (float)swPoint.ElapsedTicks / (float)total;
                float prcSpot = (float)swSpot.ElapsedTicks / (float)total;
                float prcWasted = (float)(swTotal.ElapsedTicks - total) / (float)total;

                Counters.SetStatistics("DeferredRenderer.DrawLights", string.Format(
                    "{0:000000}; Init {1:00}%; Directional {2:00}%; Point {3:00}%; Spot {4:00}%; Other {5:00}%",
                    swTotal.ElapsedTicks,
                    prcPrepare * 100f,
                    prcDirectional * 100f,
                    prcPoint * 100f,
                    prcSpot * 100f,
                    prcWasted * 100f));
            }

            float perDirectionalLight = 0f;
            float perPointLight = 0f;
            float perSpotLight = 0f;

            if (directionalLights != null && directionalLights.Length > 0)
            {
                long totalDirectional = swDirectional.ElapsedTicks;
                if (totalDirectional > 0)
                {
                    perDirectionalLight = (float)totalDirectional / (float)directionalLights.Length;
                }
            }

            if (pointLights != null && pointLights.Length > 0)
            {
                long totalPoint = swPoint.ElapsedTicks;
                if (totalPoint > 0)
                {
                    perPointLight = (float)totalPoint / (float)pointLights.Length;
                }
            }

            if (spotLights != null && spotLights.Length > 0)
            {
                long totalSpot = swSpot.ElapsedTicks;
                if (totalSpot > 0)
                {
                    perSpotLight = (float)totalSpot / (float)spotLights.Length;
                }
            }

            Counters.SetStatistics("DeferredRenderer.DrawLights.Types", string.Format(
                "Directional {0:000000}; Point {1:000000}; Spot {2:000000}",
                perDirectionalLight,
                perPointLight,
                perSpotLight));

            Counters.SetStatistics("DEFERRED_LIGHTING", new[]
            {
                swPrepare.ElapsedTicks,
                swDirectional.ElapsedTicks,
                swPoint.ElapsedTicks,
                swSpot.ElapsedTicks,
            });
#endif
        }
        /// <summary>
        /// Draw result
        /// </summary>
        /// <param name="context">Drawing context</param>
        public void DrawResult(Context context)
        {
#if DEBUG
            long total = 0;
            long init = 0;
            long draw = 0;

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            if (context.GeometryMap != null && context.LightMap != null)
            {
#if DEBUG
                Stopwatch swInit = Stopwatch.StartNew();
#endif
                var effect = DrawerPool.EffectDeferred;
                var effectTechnique = effect.DeferredCombineLights;

                effect.UpdateComposer(
                    context.World,
                    this.ViewProjection,
                    context.EyePosition,
                    context.GeometryMap[2],
                    context.LightMap);

                var deviceContext = this.Game.Graphics.DeviceContext;
                var geometry = this.lightGeometry[0];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                this.Game.Graphics.SetRasterizerDefault();
                this.Game.Graphics.SetBlendDefault();
#if DEBUG
                swInit.Stop();

                init = swInit.ElapsedTicks;
#endif
#if DEBUG
                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                {
                    effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                    deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
#if DEBUG
                swDraw.Stop();

                draw = swDraw.ElapsedTicks;
#endif
            }
#if DEBUG
            swTotal.Stop();

            total = swTotal.ElapsedTicks;
#endif
#if DEBUG
            Counters.SetStatistics("DEFERRED_COMPOSITION", new[]
            {
                init,
                draw,
            });
#endif
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
