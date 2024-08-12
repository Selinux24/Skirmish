using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Deferred;
using Engine.BuiltIn.Format;
using Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
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
        private EngineBuffer lightGeometryVertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private EngineVertexBufferBinding lightGeometryVertexBufferBinding;
        /// <summary>
        /// Window index buffer
        /// </summary>
        private EngineBuffer lightGeometryIndexBuffer;
        /// <summary>
        /// Lights geometry vertices
        /// </summary>
        private readonly List<VertexPosition> lightGeometryVertices = [];
        /// <summary>
        /// Light geometry indices
        /// </summary>
        private readonly List<uint> lightGeometryIndices = [];
        /// <summary>
        /// Update light geometry buffers flag
        /// </summary>
        private bool updateLightBuffers = false;
        /// <summary>
        /// Input layout for directional and hemispheric lights
        /// </summary>
        private EngineInputLayout globalLightInputLayout;
        /// <summary>
        /// Input layout for point lights
        /// </summary>
        private EngineInputLayout pointLightInputLayout;
        /// <summary>
        /// Input layout for spot ligths
        /// </summary>
        private EngineInputLayout spotLightInputLayout;
        /// <summary>
        /// Input layout for result light map
        /// </summary>
        private EngineInputLayout combineLightsInputLayout;
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
            globalLightInputLayout = graphics.CreateInputLayout<VertexPosition>("EffectDeferredComposer.DeferredDirectionalLight", ld.GetVertexShader().Shader.GetShaderBytecode(), BufferSlot);

            var lp = BuiltInShaders.GetDrawer<BuiltInLightPoint>();
            pointLightInputLayout = graphics.CreateInputLayout<VertexPosition>("EffectDeferredComposer.DeferredPointLight", lp.GetVertexShader().Shader.GetShaderBytecode(), BufferSlot);

            var ls = BuiltInShaders.GetDrawer<BuiltInLightSpot>();
            spotLightInputLayout = graphics.CreateInputLayout<VertexPosition>("EffectDeferredComposer.DeferredSpotLight", ls.GetVertexShader().Shader.GetShaderBytecode(), BufferSlot);

            var composer = BuiltInShaders.GetDrawer<BuiltInComposer>();
            combineLightsInputLayout = graphics.CreateInputLayout<VertexPosition>("EffectDeferredComposer.DeferredCombineLights", composer.GetVertexShader().Shader.GetShaderBytecode(), BufferSlot);

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
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        public void Update(int width, int height)
        {
            lightGeometryVertices.Clear();
            lightGeometryIndices.Clear();

            AddRectangle(width, height);

            AddPointLight();

            AddSpotLight();

            updateLightBuffers = true;
        }
        /// <summary>
        /// Creates the geometry to draw the screen
        /// </summary>
        /// <param name="width">Screen width</param>
        /// <param name="height">Screen height</param>
        private void AddRectangle(int width, int height)
        {
            var screen = GeometryUtil.CreateScreen(width, height);

            screenGeometry.Offset = lightGeometryIndices.Count;
            screenGeometry.IndexCount = screen.Indices.Count();

            screen.Indices.ToList().ForEach(i =>
            {
                //Sum offsets
                lightGeometryIndices.Add(i + (uint)lightGeometryVertices.Count);
            });

            lightGeometryVertices.AddRange(VertexPosition.Generate(screen.Vertices));
        }
        /// <summary>
        /// Creates the geometry to draw a point light
        /// </summary>
        private void AddPointLight()
        {
            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 1, 16, 16);

            pointLightGeometry.Offset = lightGeometryIndices.Count;
            pointLightGeometry.IndexCount = sphere.Indices.Count();

            sphere.Indices.ToList().ForEach(i =>
            {
                //Sum offsets
                lightGeometryIndices.Add(i + (uint)lightGeometryVertices.Count);
            });

            lightGeometryVertices.AddRange(VertexPosition.Generate(sphere.Vertices));
        }
        /// <summary>
        /// Creates the geometry to draw a spot light
        /// </summary>
        private void AddSpotLight()
        {
            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 1, 16, 16);

            spotLightGeometry.Offset = lightGeometryIndices.Count;
            spotLightGeometry.IndexCount = sphere.Indices.Count();

            sphere.Indices.ToList().ForEach(i =>
            {
                //Sum offsets
                lightGeometryIndices.Add(i + (uint)lightGeometryVertices.Count);
            });

            lightGeometryVertices.AddRange(VertexPosition.Generate(sphere.Vertices));
        }

        /// <summary>
        /// Write light geometry in buffers
        /// </summary>
        /// <param name="dc">Device context</param>
        public void WriteBuffers(IEngineDeviceContext dc)
        {
            if (!updateLightBuffers)
            {
                return;
            }

            if (lightGeometryVertexBuffer == null)
            {
                lightGeometryVertexBuffer = Graphics.CreateVertexBuffer("Deferred Redenderer Light Geometry", lightGeometryVertices, true);
                lightGeometryVertexBufferBinding = new EngineVertexBufferBinding(lightGeometryVertexBuffer, lightGeometryVertices[0].GetStride(), 0);
            }
            else
            {
                dc.WriteDiscardBuffer(lightGeometryVertexBuffer, [.. lightGeometryVertices]);
            }

            if (lightGeometryIndexBuffer == null)
            {
                lightGeometryIndexBuffer = Graphics.CreateIndexBuffer("Deferred Redenderer Light Geometry", lightGeometryIndices, true);
            }
            else
            {
                dc.WriteDiscardBuffer(lightGeometryIndexBuffer, [.. lightGeometryIndices]);
            }

            updateLightBuffers = true;
        }
        /// <summary>
        /// Draws a single light
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="geometry">Geometry</param>
        /// <param name="drawer">Drawer</param>
        private void DrawSingleLight(IEngineDeviceContext dc, LightGeometry geometry, IDrawer drawer)
        {
            drawer.Draw(dc, BufferSlot, lightGeometryVertexBufferBinding, lightGeometryIndexBuffer, Topology.TriangleList, geometry.IndexCount, geometry.Offset);
        }
        /// <summary>
        /// Binds the hemispheric/directional (global) light input layout to the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        public void BindGlobalLight(IEngineDeviceContext dc)
        {
            dc.IAInputLayout = globalLightInputLayout;
        }
        /// <summary>
        /// Draws a directional light
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawer">Drawer</param>
        public void DrawDirectional(IEngineDeviceContext dc, IDrawer drawer)
        {
            drawer.Draw(dc, BufferSlot, lightGeometryVertexBufferBinding, lightGeometryIndexBuffer, Topology.TriangleList, screenGeometry.IndexCount, screenGeometry.Offset);
        }
        /// <summary>
        /// Binds the point light input layout to the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        public void BindPoint(IEngineDeviceContext dc)
        {
            dc.IAInputLayout = pointLightInputLayout;
        }
        /// <summary>
        /// Draws a point light
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="stencilDrawer">Stencil drawer</param>
        /// <param name="drawer">Drawer</param>
        public void DrawPoint(IEngineDeviceContext dc, IDrawer stencilDrawer, IDrawer drawer)
        {
            var geometry = pointLightGeometry;

            SetRasterizerStencilPass(dc);
            SetDepthStencilVolumeMarking(dc);
            dc.ClearDepthStencilBuffer(Graphics.DefaultDepthStencil, false, true);
            DrawSingleLight(dc, geometry, stencilDrawer);

            SetRasterizerLightingPass(dc);
            SetDepthStencilVolumeDrawing(dc);
            DrawSingleLight(dc, geometry, drawer);
        }
        /// <summary>
        /// Binds the spot light input layout to the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        public void BindSpot(IEngineDeviceContext dc)
        {
            dc.IAInputLayout = spotLightInputLayout;
        }
        /// <summary>
        /// Draws a spot light
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="stencilDrawer">Stencil drawer</param>
        /// <param name="drawer">Drawer</param>
        public void DrawSpot(IEngineDeviceContext dc, IDrawer stencilDrawer, IDrawer drawer)
        {
            var geometry = spotLightGeometry;

            SetRasterizerStencilPass(dc);
            SetDepthStencilVolumeMarking(dc);
            dc.ClearDepthStencilBuffer(Graphics.DefaultDepthStencil, false, true);
            DrawSingleLight(dc, geometry, stencilDrawer);

            SetRasterizerLightingPass(dc);
            SetDepthStencilVolumeDrawing(dc);
            DrawSingleLight(dc, geometry, drawer);
        }
        /// <summary>
        /// Binds the result box input layout to the input assembler
        /// </summary>
        /// <param name="dc">Device context</param>
        public void BindResult(IEngineDeviceContext dc)
        {
            dc.IAInputLayout = combineLightsInputLayout;
        }
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawer">Effect</param>
        public void DrawResult(IEngineDeviceContext dc, IDrawer drawer)
        {
            drawer.Draw(dc, BufferSlot, lightGeometryVertexBufferBinding, lightGeometryIndexBuffer, Topology.TriangleList, screenGeometry.IndexCount, screenGeometry.Offset);
        }

        /// <summary>
        /// Sets stencil pass rasterizer
        /// </summary>
        /// <param name="dc">Device context</param>
        private void SetRasterizerStencilPass(IEngineDeviceContext dc)
        {
            dc.SetRasterizerState(rasterizerStencilPass);
        }
        /// <summary>
        /// Stes lighting pass rasterizer
        /// </summary>
        /// <param name="dc">Device context</param>
        public void SetRasterizerLightingPass(IEngineDeviceContext dc)
        {
            dc.SetRasterizerState(rasterizerLightingPass);
        }
        /// <summary>
        /// Sets depth stencil for volume marking
        /// </summary>
        /// <param name="dc">Device context</param>
        public void SetDepthStencilVolumeMarking(IEngineDeviceContext dc)
        {
            dc.SetDepthStencilState(depthStencilVolumeMarking);
        }
        /// <summary>
        /// Sets depth stencil for volume drawing
        /// </summary>
        /// <param name="dc">Device context</param>
        public void SetDepthStencilVolumeDrawing(IEngineDeviceContext dc)
        {
            dc.SetDepthStencilState(depthStencilVolumeDrawing);
        }
    }
}
