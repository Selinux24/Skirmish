using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Light drawer for deferred renderer
    /// </summary>
    class SceneRendererDeferredLights : IDisposable
    {
        /// <summary>
        /// Render helper geometry buffer slot
        /// </summary>
        public static int BufferSlot = 15;

        /// <summary>
        /// Light geometry
        /// </summary>
        struct LightGeometry
        {
            /// <summary>
            /// Geometry offset
            /// </summary>
            public int Offset;
            /// <summary>
            /// Index count
            /// </summary>
            public int IndexCount;
        }

        /// <summary>
        /// Window vertex buffer
        /// </summary>
        private Buffer lightGeometryVertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding lightGeometryVertexBufferBinding;
        /// <summary>
        /// Window index buffer
        /// </summary>
        private Buffer lightGeometryIndexBuffer;
        /// <summary>
        /// Input layout for directional lights
        /// </summary>
        private InputLayout dirLightInputLayout;
        /// <summary>
        /// Input layout for point lights
        /// </summary>
        private InputLayout pointLightInputLayout;
        /// <summary>
        /// Input layout for spot ligths
        /// </summary>
        private InputLayout spotLightInputLayout;
        /// <summary>
        /// Input layout for result light map
        /// </summary>
        private InputLayout combineLightsInputLayout;
        /// <summary>
        /// Screen geometry
        /// </summary>
        private LightGeometry screenGeometry;
        /// <summary>
        /// Point light geometry
        /// </summary>
        private LightGeometry pointLightGeometry;
        /// <summary>
        /// Spot ligth geometry
        /// </summary>
        private LightGeometry spotLightGeometry;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SceneRendererDeferredLights(Graphics graphics)
        {
            this.dirLightInputLayout = DrawerPool.EffectDeferredComposer.DeferredDirectionalLight.Create(graphics, VertexPosition.Input(BufferSlot));
            this.pointLightInputLayout = DrawerPool.EffectDeferredComposer.DeferredPointLight.Create(graphics, VertexPosition.Input(BufferSlot));
            this.spotLightInputLayout = DrawerPool.EffectDeferredComposer.DeferredSpotLight.Create(graphics, VertexPosition.Input(BufferSlot));
            this.combineLightsInputLayout = DrawerPool.EffectDeferredComposer.DeferredCombineLights.Create(graphics, VertexPosition.Input(BufferSlot));
        }

        /// <summary>
        /// Resources dispose
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.lightGeometryVertexBuffer);
            Helper.Dispose(this.lightGeometryIndexBuffer);

            Helper.Dispose(this.dirLightInputLayout);
            Helper.Dispose(this.pointLightInputLayout);
            Helper.Dispose(this.spotLightInputLayout);
            Helper.Dispose(this.combineLightsInputLayout);
        }

        /// <summary>
        /// Updates the internal buffers according to the new render dimension
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        public void Update(Graphics graphics, int width, int height)
        {
            List<VertexPosition> verts = new List<VertexPosition>();
            List<uint> indx = new List<uint>();

            {
                Vector3[] cv;
                uint[] indices;
                GeometryUtil.CreateScreen(
                    width, height,
                    out cv,
                    out indices);
                var vertices = new VertexPosition[cv.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new VertexPosition() { Position = cv[i] };
                }

                this.screenGeometry.Offset = indx.Count;
                this.screenGeometry.IndexCount = indices.Length;

                verts.AddRange(vertices);
                indx.AddRange(indices);
            }

            {
                Vector3[] cv;
                uint[] indices;
                GeometryUtil.CreateSphere(
                    1, 16, 16,
                    out cv,
                    out indices);
                var vertices = new VertexPosition[cv.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new VertexPosition() { Position = cv[i] };
                }

                this.pointLightGeometry.Offset = indx.Count;
                this.pointLightGeometry.IndexCount = indices.Length;

                //Sum offsets
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] += (uint)verts.Count;
                }

                verts.AddRange(vertices);
                indx.AddRange(indices);
            }

            {
                Vector3[] cv;
                uint[] indices;
                GeometryUtil.CreateSphere(
                    1, 16, 16,
                    out cv,
                    out indices);
                var vertices = new VertexPosition[cv.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new VertexPosition() { Position = cv[i] };
                }

                this.spotLightGeometry.Offset = indx.Count;
                this.spotLightGeometry.IndexCount = indices.Length;

                //Sum offsets
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] += (uint)verts.Count;
                }

                verts.AddRange(vertices);
                indx.AddRange(indices);
            }

            if (this.lightGeometryVertexBuffer == null)
            {
                this.lightGeometryVertexBuffer = graphics.CreateVertexBufferWrite("Deferred Redenderer Light Geometry", verts.ToArray());
                this.lightGeometryVertexBufferBinding = new VertexBufferBinding(this.lightGeometryVertexBuffer, verts[0].GetStride(), 0);
            }
            else
            {
                graphics.DeviceContext.WriteDiscardBuffer(this.lightGeometryVertexBuffer, verts.ToArray());
            }

            if (this.lightGeometryIndexBuffer == null)
            {
                this.lightGeometryIndexBuffer = graphics.CreateIndexBufferWrite("Deferred Redenderer Light Geometry", indx.ToArray());
            }
            else
            {
                graphics.DeviceContext.WriteDiscardBuffer(this.lightGeometryIndexBuffer, indx.ToArray());
            }
        }
        /// <summary>
        /// Draws a single light
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="geometry">Geometry</param>
        /// <param name="effectTechnique">Technique</param>
        private void DrawSingleLight(Graphics graphics, LightGeometry geometry, EngineEffectTechnique effectTechnique)
        {
            for (int p = 0; p < effectTechnique.PassCount; p++)
            {
                effectTechnique.Apply(graphics, p, 0);

                graphics.DeviceContext.DrawIndexed(geometry.IndexCount, geometry.Offset, 0);

                Counters.DrawCallsPerFrame++;
            }
        }
        /// <summary>
        /// Binds light geometry to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindGeometry(Graphics graphics)
        {
            graphics.IAPrimitiveTopology = PrimitiveTopology.TriangleList;
            graphics.IASetVertexBuffers(BufferSlot, this.lightGeometryVertexBufferBinding);
            graphics.IASetIndexBuffer(this.lightGeometryIndexBuffer, Format.R32_UInt, 0);
        }
        /// <summary>
        /// Binds the directional light input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindDirectional(Graphics graphics)
        {
            graphics.IAInputLayout = this.dirLightInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws a directional light
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect</param>
        public void DrawDirectional(Graphics graphics, EffectDeferredComposer effect)
        {
            var effectTechnique = effect.DeferredDirectionalLight;

            for (int p = 0; p < effectTechnique.PassCount; p++)
            {
                effectTechnique.Apply(graphics, p, 0);

                graphics.DeviceContext.DrawIndexed(
                    this.screenGeometry.IndexCount,
                    this.screenGeometry.Offset,
                    0);

                Counters.DrawCallsPerFrame++;
            }
        }
        /// <summary>
        /// Binds the point light input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindPoint(Graphics graphics)
        {
            graphics.IAInputLayout = this.pointLightInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws a point light
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect</param>
        public void DrawPoint(Graphics graphics, EffectDeferredComposer effect)
        {
            var geometry = this.pointLightGeometry;

            graphics.SetRasterizerStencilPass();
            graphics.SetDepthStencilVolumeMarking();
            graphics.ClearDepthStencilBuffer(graphics.DefaultDepthStencil, false, true);
            this.DrawSingleLight(graphics, geometry, effect.DeferredPointStencil);

            graphics.SetRasterizerLightingPass();
            graphics.SetDepthStencilVolumeDrawing(0);
            graphics.DeviceContext.OutputMerger.DepthStencilReference = 0;
            this.DrawSingleLight(graphics, geometry, effect.DeferredPointLight);
        }
        /// <summary>
        /// Binds the spot light input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindSpot(Graphics graphics)
        {
            graphics.IAInputLayout = this.spotLightInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws a spot light
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect</param>
        public void DrawSpot(Graphics graphics, EffectDeferredComposer effect)
        {
            var geometry = this.spotLightGeometry;

            graphics.SetRasterizerStencilPass();
            graphics.SetDepthStencilVolumeMarking();
            graphics.ClearDepthStencilBuffer(graphics.DefaultDepthStencil, false, true);
            this.DrawSingleLight(graphics, geometry, effect.DeferredSpotStencil);

            graphics.SetRasterizerLightingPass();
            graphics.SetDepthStencilVolumeDrawing(0);
            graphics.DeviceContext.OutputMerger.DepthStencilReference = 0;
            this.DrawSingleLight(graphics, geometry, effect.DeferredSpotLight);
        }
        /// <summary>
        /// Binds the result box input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindResult(Graphics graphics)
        {
            graphics.IAPrimitiveTopology = PrimitiveTopology.TriangleList;
            graphics.IASetVertexBuffers(BufferSlot, this.lightGeometryVertexBufferBinding);
            graphics.IASetIndexBuffer(this.lightGeometryIndexBuffer, Format.R32_UInt, 0);

            graphics.IAInputLayout = this.combineLightsInputLayout;
        }
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect</param>
        public void DrawResult(Graphics graphics, EffectDeferredComposer effect)
        {
            var effectTechnique = effect.DeferredCombineLights;

            for (int p = 0; p < effectTechnique.PassCount; p++)
            {
                effectTechnique.Apply(graphics, p, 0);

                graphics.DeviceContext.DrawIndexed(this.screenGeometry.IndexCount, this.screenGeometry.Offset, 0);

                Counters.DrawCallsPerFrame++;
            }
        }
    }
}
