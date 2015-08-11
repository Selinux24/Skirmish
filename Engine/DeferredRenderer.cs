using System;
using System.Collections.Generic;
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
    public class DeferredRenderer : IDisposable
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
        /// Draws a quad fitted to screen to perform pixel by pixel mapping
        /// </summary>
        /// <param name="context">Drawing context</param>
        public void Draw(Context context)
        {
            if (this.Width != this.Game.Form.RenderWidth || this.Height != this.Game.Form.RenderHeight)
            {
                this.UpdateRectangleAndView();
            }

            var deviceContext = this.Game.Graphics.DeviceContext;

            var effect = DrawerPool.EffectDeferred;

            #region Directional Lights

            {
                var dirLights = new[]
                {
                    context.Lights.DirectionalLight1,
                    context.Lights.DirectionalLight2,
                    context.Lights.DirectionalLight3,
                };

                var effectTechnique = effect.DeferredDirectionalLight;
                var geometry = this.lightGeometry[0];

                for (int i = 0; i < dirLights.Length; i++)
                {
                    if (dirLights[i].Enabled)
                    {
                        deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                        deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                        deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                        effect.FrameBuffer.World = Matrix.Identity;
                        effect.FrameBuffer.WorldInverse = Matrix.Identity;
                        effect.FrameBuffer.WorldViewProjection = this.ViewProjection;
                        effect.UpdatePerFrame(context.GBuffer[0], context.GBuffer[1], context.GBuffer[2]);

                        effect.UpdatePerDirectionalLight(new BufferDirectionalLight(dirLights[i]));

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

            #region Point Lights

            {
                var pointLights = new[]
                {
                    context.Lights.PointLight,
                };

                var effectTechnique = effect.DeferredPointLight;
                var geometry = this.lightGeometry[1];

                for (int i = 0; i < pointLights.Length; i++)
                {
                    if (pointLights[i].Enabled)
                    {
                        deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                        deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                        deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                        Matrix world = Matrix.Scaling(pointLights[i].Range) * Matrix.Translation(pointLights[i].Position);

                        effect.FrameBuffer.World = world;
                        effect.FrameBuffer.WorldInverse = world;
                        effect.FrameBuffer.WorldViewProjection = world * this.ViewProjection;
                        effect.UpdatePerFrame(context.GBuffer[0], context.GBuffer[1], context.GBuffer[2]);

                        effect.UpdatePerPointLight(new BufferPointLight(pointLights[i]));

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

            #region Spot Lights

            {
                var spotLights = new[]
                {
                    context.Lights.SpotLight,
                };

                var effectTechnique = effect.DeferredSpotLight;
                var geometry = this.lightGeometry[2];

                for (int i = 0; i < spotLights.Length; i++)
                {
                    if (spotLights[i].Enabled)
                    {
                        deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                        deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                        deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);

                        effect.FrameBuffer.World = Matrix.Identity;
                        effect.FrameBuffer.WorldInverse = Matrix.Identity;
                        effect.FrameBuffer.WorldViewProjection = this.ViewProjection;
                        effect.UpdatePerFrame(context.GBuffer[0], context.GBuffer[1], context.GBuffer[2]);

                        effect.UpdatePerSpotLight(new BufferSpotLight(spotLights[i]));

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
        }
        /// <summary>
        /// Updates renderer parameters
        /// </summary>
        private void UpdateRectangleAndView()
        {
            this.Width = this.Game.Form.RenderWidth;
            this.Height = this.Game.Form.RenderHeight;

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
                1, 5, 5,
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
        /// Update spot light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdateSpotLightGeometry(ref LightGeometry geometry)
        {
            VertexData[] cv;
            uint[] ci;
            VertexData.CreateCone(
                1, 3, 3,
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
    }
}
