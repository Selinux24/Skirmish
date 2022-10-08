using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Deferred;
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Light drawer for deferred renderer
    /// </summary>
    class SceneRendererDeferredLights : IDisposable
    {
        /// <summary>
        /// Render helper geometry buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

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
        /// Input layout for directional and hemispheric lights
        /// </summary>
        private InputLayout globalLightInputLayout;
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
        /// Stencil pass rasterizer (No Cull, No depth limit)
        /// </summary>
        private EngineRasterizerState rasterizerStencilPass = null;
        /// <summary>
        /// Lighting pass rasterizer (Cull Front faces, No depth limit)
        /// </summary>
        private EngineRasterizerState rasterizerLightingPass = null;
        /// <summary>
        /// Depth stencil state for volume marking
        /// </summary>
        private EngineDepthStencilState depthStencilVolumeMarking = null;
        /// <summary>
        /// Depth stencil state for volume drawing
        /// </summary>
        private EngineDepthStencilState depthStencilVolumeDrawing = null;

        /// <summary>
        /// Graphics
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SceneRendererDeferredLights(Graphics graphics)
        {
            Graphics = graphics;

            var ld = BuiltInShaders.GetDrawer<BuiltInLightDirectional>();
            globalLightInputLayout = graphics.CreateInputLayout("EffectDeferredComposer.DeferredDirectionalLight", ld.GetVertexShader().Shader.GetShaderBytecode(), VertexPosition.Input(BufferSlot));

            var lp = BuiltInShaders.GetDrawer<BuiltInLightPoint>();
            pointLightInputLayout = graphics.CreateInputLayout("EffectDeferredComposer.DeferredPointLight", lp.GetVertexShader().Shader.GetShaderBytecode(), VertexPosition.Input(BufferSlot));

            var ls = BuiltInShaders.GetDrawer<BuiltInLightSpot>();
            spotLightInputLayout = graphics.CreateInputLayout("EffectDeferredComposer.DeferredSpotLight", ls.GetVertexShader().Shader.GetShaderBytecode(), VertexPosition.Input(BufferSlot));

            var composer = BuiltInShaders.GetDrawer<BuiltInComposer>();
            combineLightsInputLayout = graphics.CreateInputLayout("EffectDeferredComposer.DeferredCombineLights", composer.GetVertexShader().Shader.GetShaderBytecode(), VertexPosition.Input(BufferSlot));

            //Stencil pass rasterizer state
            rasterizerStencilPass = EngineRasterizerState.StencilPass(graphics, nameof(SceneRendererDeferredLights));

            //Counter clockwise cull rasterizer state
            rasterizerLightingPass = EngineRasterizerState.LightingPass(graphics, nameof(SceneRendererDeferredLights));

            //Depth-stencil state for volume marking (Value != 0 if object is inside of the current drawing volume)
            depthStencilVolumeMarking = EngineDepthStencilState.VolumeMarking(graphics, nameof(SceneRendererDeferredLights));

            //Depth-stencil state for volume drawing (Process pixels if stencil value != stencil reference)
            depthStencilVolumeDrawing = EngineDepthStencilState.VolumeDrawing(graphics, nameof(SceneRendererDeferredLights));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SceneRendererDeferredLights()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lightGeometryVertexBuffer?.Dispose();
                lightGeometryVertexBuffer = null;
                lightGeometryIndexBuffer?.Dispose();
                lightGeometryIndexBuffer = null;

                globalLightInputLayout?.Dispose();
                globalLightInputLayout = null;
                pointLightInputLayout?.Dispose();
                pointLightInputLayout = null;
                spotLightInputLayout?.Dispose();
                spotLightInputLayout = null;
                combineLightsInputLayout?.Dispose();
                combineLightsInputLayout = null;

                rasterizerStencilPass?.Dispose();
                rasterizerStencilPass = null;
                rasterizerLightingPass?.Dispose();
                rasterizerLightingPass = null;
                depthStencilVolumeMarking?.Dispose();
                depthStencilVolumeMarking = null;
                depthStencilVolumeDrawing?.Dispose();
                depthStencilVolumeDrawing = null;
            }
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

            CreateScreen(verts, indx, width, height);

            CreatePointLight(verts, indx);

            CreateSpotLight(verts, indx);

            if (lightGeometryVertexBuffer == null)
            {
                lightGeometryVertexBuffer = graphics.CreateVertexBuffer("Deferred Redenderer Light Geometry", verts, true);
                lightGeometryVertexBufferBinding = new VertexBufferBinding(lightGeometryVertexBuffer, verts[0].GetStride(), 0);
            }
            else
            {
                graphics.WriteDiscardBuffer(lightGeometryVertexBuffer, verts);
            }

            if (lightGeometryIndexBuffer == null)
            {
                lightGeometryIndexBuffer = graphics.CreateIndexBuffer("Deferred Redenderer Light Geometry", indx, true);
            }
            else
            {
                graphics.WriteDiscardBuffer(lightGeometryIndexBuffer, indx);
            }
        }
        /// <summary>
        /// Creates the geometry to draw the screen
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="indx">Index list</param>
        /// <param name="width">Screen width</param>
        /// <param name="height">Screen height</param>
        private void CreateScreen(List<VertexPosition> verts, List<uint> indx, int width, int height)
        {
            var screen = GeometryUtil.CreateScreen(width, height);

            screenGeometry.Offset = indx.Count;
            screenGeometry.IndexCount = screen.Indices.Count();

            screen.Indices.ToList().ForEach(i =>
            {
                //Sum offsets
                indx.Add(i + (uint)verts.Count);
            });

            verts.AddRange(VertexPosition.Generate(screen.Vertices));
        }
        /// <summary>
        /// Creates the geometry to draw a point light
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="indx">Index list</param>
        private void CreatePointLight(List<VertexPosition> verts, List<uint> indx)
        {
            var sphere = GeometryUtil.CreateSphere(1, 16, 16);

            pointLightGeometry.Offset = indx.Count;
            pointLightGeometry.IndexCount = sphere.Indices.Count();

            sphere.Indices.ToList().ForEach(i =>
            {
                //Sum offsets
                indx.Add(i + (uint)verts.Count);
            });

            verts.AddRange(VertexPosition.Generate(sphere.Vertices));
        }
        /// <summary>
        /// Creates the geometry to draw a spot light
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="indx">Index list</param>
        private void CreateSpotLight(List<VertexPosition> verts, List<uint> indx)
        {
            var sphere = GeometryUtil.CreateSphere(1, 16, 16);

            spotLightGeometry.Offset = indx.Count;
            spotLightGeometry.IndexCount = sphere.Indices.Count();

            sphere.Indices.ToList().ForEach(i =>
            {
                //Sum offsets
                indx.Add(i + (uint)verts.Count);
            });

            verts.AddRange(VertexPosition.Generate(sphere.Vertices));
        }

        /// <summary>
        /// Draws a single light
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <param name="drawer">Drawer</param>
        private void DrawSingleLight(LightGeometry geometry, IBuiltInDrawer drawer)
        {
            drawer.Draw(Topology.TriangleList, BufferSlot, lightGeometryVertexBufferBinding, lightGeometryIndexBuffer, geometry.IndexCount, geometry.Offset);
        }
        /// <summary>
        /// Binds the hemispheric/directional (global) light input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindGlobalLight(Graphics graphics)
        {
            graphics.IAInputLayout = globalLightInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws a directional light
        /// </summary>
        /// <param name="drawer">Drawer</param>
        public void DrawDirectional(BuiltInLightDirectional drawer)
        {
            drawer.Draw(Topology.TriangleList, BufferSlot, lightGeometryVertexBufferBinding, lightGeometryIndexBuffer, screenGeometry.IndexCount, screenGeometry.Offset);
        }
        /// <summary>
        /// Binds the point light input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindPoint(Graphics graphics)
        {
            graphics.IAInputLayout = pointLightInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws a point light
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="stencilDrawer">Stencil drawer</param>
        /// <param name="drawer">Drawer</param>
        public void DrawPoint(Graphics graphics, BuiltInStencil stencilDrawer, BuiltInLightPoint drawer)
        {
            var geometry = pointLightGeometry;

            SetRasterizerStencilPass();
            SetDepthStencilVolumeMarking();
            graphics.ClearDepthStencilBuffer(graphics.DefaultDepthStencil, false, true);
            DrawSingleLight(geometry, stencilDrawer);

            SetRasterizerLightingPass();
            SetDepthStencilVolumeDrawing();
            DrawSingleLight(geometry, drawer);
        }
        /// <summary>
        /// Binds the spot light input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindSpot(Graphics graphics)
        {
            graphics.IAInputLayout = spotLightInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws a spot light
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="stencilDrawer">Stencil drawer</param>
        /// <param name="drawer">Drawer</param>
        public void DrawSpot(Graphics graphics, BuiltInStencil stencilDrawer, BuiltInLightSpot drawer)
        {
            var geometry = spotLightGeometry;

            SetRasterizerStencilPass();
            SetDepthStencilVolumeMarking();
            graphics.ClearDepthStencilBuffer(graphics.DefaultDepthStencil, false, true);
            DrawSingleLight(geometry, stencilDrawer);

            SetRasterizerLightingPass();
            SetDepthStencilVolumeDrawing();
            DrawSingleLight(geometry, drawer);
        }
        /// <summary>
        /// Binds the result box input layout to the input assembler
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public void BindResult(Graphics graphics)
        {
            graphics.IAInputLayout = combineLightsInputLayout;
            Counters.IAInputLayoutSets++;
        }
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        /// <param name="Drawer">Effect</param>
        public void DrawResult(BuiltInComposer drawer)
        {
            drawer.Draw(Topology.TriangleList, BufferSlot, lightGeometryVertexBufferBinding, lightGeometryIndexBuffer, screenGeometry.IndexCount, screenGeometry.Offset);
        }

        /// <summary>
        /// Sets stencil pass rasterizer
        /// </summary>
        private void SetRasterizerStencilPass()
        {
            Graphics.SetRasterizerState(rasterizerStencilPass);
        }
        /// <summary>
        /// Stes lighting pass rasterizer
        /// </summary>
        public void SetRasterizerLightingPass()
        {
            Graphics.SetRasterizerState(rasterizerLightingPass);
        }
        /// <summary>
        /// Sets depth stencil for volume marking
        /// </summary>
        public void SetDepthStencilVolumeMarking()
        {
            Graphics.SetDepthStencilState(depthStencilVolumeMarking);
        }
        /// <summary>
        /// Sets depth stencil for volume drawing
        /// </summary>
        public void SetDepthStencilVolumeDrawing()
        {
            Graphics.SetDepthStencilState(depthStencilVolumeDrawing);
        }
    }
}
