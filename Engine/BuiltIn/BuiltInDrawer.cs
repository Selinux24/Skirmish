using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;

    /// <summary>
    /// Built-in drawer class
    /// </summary>
    public abstract class BuiltInDrawer : IBuiltInDrawer
    {
        /// <summary>
        /// Vertex shader
        /// </summary>
        private IBuiltInVertexShader vertexShader = BuiltInShaders.GetVertexShader<EmptyVs>();
        /// <summary>
        /// Geometry shader
        /// </summary>
        private IBuiltInGeometryShader geometryShader = BuiltInShaders.GetGeometryShader<EmptyGs>();
        /// <summary>
        /// Pixel shader
        /// </summary>
        private IBuiltInPixelShader pixelShader = BuiltInShaders.GetPixelShader<EmptyPs>();

        /// <summary>
        /// Graphics
        /// </summary>
        protected readonly Graphics Graphics;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        protected BuiltInDrawer(Graphics graphics)
        {
            Graphics = graphics;
        }

        /// <summary>
        /// Sets a vertex shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetVertexShader<T>() where T : class, IBuiltInVertexShader
        {
            vertexShader = BuiltInShaders.GetVertexShader<T>();
        }
        /// <summary>
        /// Sets the vertex shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetVertexShader(IBuiltInVertexShader shader)
        {
            vertexShader = shader;
        }
        /// <summary>
        /// Sets a geometry shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetGeometryShader<T>() where T : class, IBuiltInGeometryShader
        {
            geometryShader = BuiltInShaders.GetGeometryShader<T>();
        }
        /// <summary>
        /// Sets the geometry shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetGeometryShader(IBuiltInGeometryShader shader)
        {
            geometryShader = shader;
        }
        /// <summary>
        /// Sets a pixel shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetPixelShader<T>() where T : class, IBuiltInPixelShader
        {
            pixelShader = BuiltInShaders.GetPixelShader<T>();
        }
        /// <summary>
        /// Sets the pixel shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetPixelShader(IBuiltInPixelShader shader)
        {
            pixelShader = shader;
        }

        /// <summary>
        /// Gets the vertex shader
        /// </summary>
        public IBuiltInVertexShader GetVertexShader()
        {
            return vertexShader;
        }
        /// <summary>
        /// Gets the vertex shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetVertexShader<T>() where T : class, IBuiltInVertexShader
        {
            return vertexShader as T;
        }
        /// <summary>
        /// Gets the geometry shader
        /// </summary>
        public IBuiltInGeometryShader GetGeometryShader()
        {
            return geometryShader;
        }
        /// <summary>
        /// Gets the geometry shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetGeometryShader<T>() where T : class, IBuiltInGeometryShader
        {
            return geometryShader as T;
        }
        /// <summary>
        /// Gets the pixel shader
        /// </summary>
        public IBuiltInPixelShader GetPixelShader()
        {
            return pixelShader;
        }
        /// <summary>
        /// Gets the pixel shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetPixelShader<T>() where T : class, IBuiltInPixelShader
        {
            return pixelShader as T;
        }

        /// <summary>
        /// Prepares the internal shaders in the graphics device
        /// </summary>
        /// <param name="dc">Device context</param>
        protected virtual void PrepareShaders(EngineDeviceContext dc)
        {
            dc.SetVertexShader(vertexShader?.Shader);
            vertexShader?.SetShaderResources(dc);

            dc.SetGeometryShader(geometryShader?.Shader);
            geometryShader?.SetShaderResources(dc);

            dc.SetPixelShader(pixelShader?.Shader);
            pixelShader?.SetShaderResources(dc);
        }

        /// <inheritdoc/>
        public virtual void UpdateCastingLight(DrawContextShadows context)
        {

        }
        /// <inheritdoc/>
        public virtual void UpdateMesh(EngineDeviceContext dc, BuiltInDrawerMeshState state)
        {

        }
        /// <inheritdoc/>
        public virtual void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {

        }

        /// <inheritdoc/>
        public virtual bool Draw(EngineDeviceContext dc, BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0)
        {
            if (bufferManager == null)
            {
                return false;
            }

            if (meshes?.Any() != true)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders(dc);

            bool instanced = instances > 0;

            foreach (var mesh in meshes)
            {
                // Set the index buffer
                if (!bufferManager.SetIndexBuffer(dc, mesh.IndexBuffer))
                {
                    continue;
                }

                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(dc, vertexShader.Shader, mesh.VertexBuffer, mesh.Topology, instanced))
                {
                    continue;
                }

                // Render the mesh.
                if (instanced)
                {
                    mesh.Draw(dc, instances, startInstanceLocation);
                }
                else
                {
                    mesh.Draw(dc);
                }
            }

            return true;
        }
        /// <inheritdoc/>
        public virtual bool Draw(EngineDeviceContext dc, BufferManager bufferManager, DrawOptions options)
        {
            if (bufferManager == null)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders(dc);

            // Set the index buffer
            if (!bufferManager.SetIndexBuffer(dc, options.IndexBuffer))
            {
                return false;
            }

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(dc, vertexShader.Shader, options.VertexBuffer, options.Topology, options.Instanced))
            {
                return false;
            }

            // Render the primitives.
            options.Draw(dc);

            return true;
        }
        /// <inheritdoc/>
        public virtual bool Draw(EngineDeviceContext dc, IEngineVertexBuffer buffer, Topology topology, int drawCount)
        {
            if (buffer == null)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders(dc);

            // Set the vertex input layout.
            buffer.SetVertexBuffers(dc);
            buffer.SetInputLayout(dc);

            dc.IAPrimitiveTopology = topology;

            // Render the primitives.
            buffer.Draw(dc, drawCount);

            return true;
        }
        /// <inheritdoc/>
        public virtual bool Draw(EngineDeviceContext dc, Topology topology, int bufferSlot, VertexBufferBinding vertexBufferBinding, Buffer indexBuffer, int count, int startLocation)
        {
            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders(dc);

            dc.IASetVertexBuffers(bufferSlot, vertexBufferBinding);
            dc.IASetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            dc.IAPrimitiveTopology = topology;

            if (indexBuffer != null)
            {
                dc.DrawIndexed(count, startLocation, 0);
            }
            else
            {
                dc.Draw(count, startLocation);
            }

            return true;
        }

        /// <inheritdoc/>
        public virtual bool DrawAuto(EngineDeviceContext dc, IEngineVertexBuffer buffer, Topology topology)
        {
            if (buffer == null)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders(dc);

            // Set the vertex input layout.
            buffer.SetVertexBuffers(dc);
            buffer.SetInputLayout(dc);

            dc.IAPrimitiveTopology = topology;

            // Render the primitives.
            buffer.DrawAuto(dc);

            return true;
        }

        /// <inheritdoc/>
        public void StreamOut(EngineDeviceContext dc, bool firstRun, IEngineVertexBuffer buffer, IEngineVertexBuffer streamOutBuffer, Topology topology)
        {
            if (buffer == null)
            {
                return;
            }

            if (streamOutBuffer == null)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders(dc);

            // Set the vertex input layout.
            buffer.SetVertexBuffers(dc);

            // Set the stream-out target.
            streamOutBuffer.SetInputLayout(dc);
            streamOutBuffer.SetStreamOutputTargets(dc);

            dc.IAPrimitiveTopology = topology;

            if (firstRun)
            {
                streamOutBuffer.Draw(dc, 1);
            }
            else
            {
                streamOutBuffer.DrawAuto(dc);
            }

            dc.SetGeometryShaderStreamOutputTargets(null);
        }
    }
}
