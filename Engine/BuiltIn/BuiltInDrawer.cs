using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn
{
    using Engine.Common;

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
        protected virtual void PrepareShaders()
        {
            Graphics.SetVertexShader(vertexShader?.Shader);
            vertexShader?.SetShaderResources();

            Graphics.SetGeometryShader(geometryShader?.Shader);
            geometryShader?.SetShaderResources();

            Graphics.SetPixelShader(pixelShader?.Shader);
            pixelShader?.SetShaderResources();
        }

        /// <inheritdoc/>
        public virtual void UpdateCastingLight(DrawContextShadows context)
        {

        }
        /// <inheritdoc/>
        public virtual void UpdateMesh(BuiltInDrawerMeshState state)
        {

        }
        /// <inheritdoc/>
        public virtual void UpdateMaterial(BuiltInDrawerMaterialState state)
        {

        }

        /// <inheritdoc/>
        public virtual bool Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0)
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
            PrepareShaders();

            bool instanced = instances > 0;

            foreach (var mesh in meshes)
            {
                // Set the index buffer
                if (!bufferManager.SetIndexBuffer(mesh.IndexBuffer))
                {
                    continue;
                }

                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology, instanced))
                {
                    continue;
                }

                // Render the mesh.
                if (instanced)
                {
                    mesh.Draw(Graphics, instances, startInstanceLocation);
                }
                else
                {
                    mesh.Draw(Graphics);
                }
            }

            return true;
        }
        /// <inheritdoc/>
        public virtual bool Draw(BufferManager bufferManager, DrawOptions options)
        {
            if (bufferManager == null)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders();

            // Set the index buffer
            if (!bufferManager.SetIndexBuffer(options.IndexBuffer))
            {
                return false;
            }

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, options.VertexBuffer, options.Topology, options.Instanced))
            {
                return false;
            }

            // Render the primitives.
            options.Draw(Graphics);

            return true;
        }
        /// <inheritdoc/>
        public virtual bool Draw(IEngineVertexBuffer buffer, Topology topology, int drawCount)
        {
            if (buffer == null)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders();

            // Set the vertex input layout.
            buffer.SetVertexBuffers();
            buffer.SetInputLayout();

            Graphics.IAPrimitiveTopology = topology;

            // Render the primitives.
            buffer.Draw(drawCount);

            return true;
        }

        /// <inheritdoc/>
        public virtual bool DrawAuto(IEngineVertexBuffer buffer, Topology topology)
        {
            if (buffer == null)
            {
                return false;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            PrepareShaders();

            // Set the vertex input layout.
            buffer.SetVertexBuffers();
            buffer.SetInputLayout();

            Graphics.IAPrimitiveTopology = topology;

            // Render the primitives.
            buffer.DrawAuto();

            return true;
        }

        /// <inheritdoc/>
        public void StreamOut(bool firstRun, IEngineVertexBuffer buffer, IEngineVertexBuffer streamOutBuffer, Topology topology)
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
            PrepareShaders();

            // Set the vertex input layout.
            buffer.SetVertexBuffers();

            // Set the stream-out target.
            streamOutBuffer.SetInputLayout();
            streamOutBuffer.SetStreamOutputTargets();

            Graphics.IAPrimitiveTopology = topology;

            if (firstRun)
            {
                streamOutBuffer.Draw(1);
            }
            else
            {
                streamOutBuffer.DrawAuto();
            }

            Graphics.SetStreamOutputTargets(null);
        }
    }
}
