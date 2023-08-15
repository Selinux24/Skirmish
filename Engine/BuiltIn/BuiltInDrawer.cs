﻿using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Format = SharpDX.DXGI.Format;

    /// <summary>
    /// Built-in drawer class
    /// </summary>
    public abstract class BuiltInDrawer : IBuiltInDrawer
    {
        /// <summary>
        /// Vertex shader
        /// </summary>
        private IBuiltInShader<EngineVertexShader> vertexShader = BuiltInShaders.GetVertexShader<Empty<EngineVertexShader>>();
        /// <summary>
        /// Hull shader
        /// </summary>
        private IBuiltInShader<EngineHullShader> hullShader = BuiltInShaders.GetHullShader<Empty<EngineHullShader>>();
        /// <summary>
        /// Domain shader
        /// </summary>
        private IBuiltInShader<EngineDomainShader> domainShader = BuiltInShaders.GetDomainShader<Empty<EngineDomainShader>>();
        /// <summary>
        /// Geometry shader
        /// </summary>
        private IBuiltInShader<EngineGeometryShader> geometryShader = BuiltInShaders.GetGeometryShader<Empty<EngineGeometryShader>>();
        /// <summary>
        /// Pixel shader
        /// </summary>
        private IBuiltInShader<EnginePixelShader> pixelShader = BuiltInShaders.GetPixelShader<Empty<EnginePixelShader>>();
        /// <summary>
        /// Compute shader
        /// </summary>
        private IBuiltInShader<EngineComputeShader> computeShader = BuiltInShaders.GetComputeShader<Empty<EngineComputeShader>>();

        /// <summary>
        /// Constructor
        /// </summary>
        protected BuiltInDrawer()
        {

        }

        /// <summary>
        /// Sets a vertex shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetVertexShader<T>(bool singleton = true) where T : class, IBuiltInShader<EngineVertexShader>
        {
            vertexShader = BuiltInShaders.GetVertexShader<T>(singleton);
        }
        /// <summary>
        /// Sets the vertex shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetVertexShader(IBuiltInShader<EngineVertexShader> shader)
        {
            vertexShader = shader;
        }
        /// <summary>
        /// Sets a hull shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetHullShader<T>(bool singleton = true) where T : class, IBuiltInShader<EngineHullShader>
        {
            hullShader = BuiltInShaders.GetHullShader<T>(singleton);
        }
        /// <summary>
        /// Sets the hull shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetHullShader(IBuiltInShader<EngineHullShader> shader)
        {
            hullShader = shader;
        }
        /// <summary>
        /// Sets a domain shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetDomainShader<T>(bool singleton = true) where T : class, IBuiltInShader<EngineDomainShader>
        {
            domainShader = BuiltInShaders.GetDomainShader<T>(singleton);
        }
        /// <summary>
        /// Sets the domain shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetDomainShader(IBuiltInShader<EngineDomainShader> shader)
        {
            domainShader = shader;
        }
        /// <summary>
        /// Sets a geometry shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetGeometryShader<T>(bool singleton = true) where T : class, IBuiltInShader<EngineGeometryShader>
        {
            geometryShader = BuiltInShaders.GetGeometryShader<T>(singleton);
        }
        /// <summary>
        /// Sets the geometry shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetGeometryShader(IBuiltInShader<EngineGeometryShader> shader)
        {
            geometryShader = shader;
        }
        /// <summary>
        /// Sets a pixel shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetPixelShader<T>(bool singleton = true) where T : class, IBuiltInShader<EnginePixelShader>
        {
            pixelShader = BuiltInShaders.GetPixelShader<T>(singleton);
        }
        /// <summary>
        /// Sets the pixel shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetPixelShader(IBuiltInShader<EnginePixelShader> shader)
        {
            pixelShader = shader;
        }
        /// <summary>
        /// Sets a compute shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public void SetComputeShader<T>(bool singleton = true) where T : class, IBuiltInShader<EngineComputeShader>
        {
            computeShader = BuiltInShaders.GetComputeShader<T>(singleton);
        }
        /// <summary>
        /// Sets the compute shader 
        /// </summary>
        /// <param name="shader">Shader</param>
        public void SetComputeShader(IBuiltInShader<EngineComputeShader> shader)
        {
            computeShader = shader;
        }

        /// <summary>
        /// Gets the vertex shader
        /// </summary>
        public IBuiltInShader<EngineVertexShader> GetVertexShader()
        {
            return vertexShader;
        }
        /// <summary>
        /// Gets the vertex shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetVertexShader<T>() where T : class, IBuiltInShader<EngineVertexShader>
        {
            return vertexShader as T;
        }
        /// <summary>
        /// Gets the hull shader
        /// </summary>
        public IBuiltInShader<EngineHullShader> GetHullShader()
        {
            return hullShader;
        }
        /// <summary>
        /// Gets the hull shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetHullShader<T>() where T : class, IBuiltInShader<EngineHullShader>
        {
            return hullShader as T;
        }
        /// <summary>
        /// Gets the domain shader
        /// </summary>
        public IBuiltInShader<EngineDomainShader> GetDomainShader()
        {
            return domainShader;
        }
        /// <summary>
        /// Gets the domain shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetDomainShader<T>() where T : class, IBuiltInShader<EngineDomainShader>
        {
            return domainShader as T;
        }
        /// <summary>
        /// Gets the geometry shader
        /// </summary>
        public IBuiltInShader<EngineGeometryShader> GetGeometryShader()
        {
            return geometryShader;
        }
        /// <summary>
        /// Gets the geometry shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetGeometryShader<T>() where T : class, IBuiltInShader<EngineGeometryShader>
        {
            return geometryShader as T;
        }
        /// <summary>
        /// Gets the pixel shader
        /// </summary>
        public IBuiltInShader<EnginePixelShader> GetPixelShader()
        {
            return pixelShader;
        }
        /// <summary>
        /// Gets the pixel shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetPixelShader<T>() where T : class, IBuiltInShader<EnginePixelShader>
        {
            return pixelShader as T;
        }
        /// <summary>
        /// Gets the compute shader
        /// </summary>
        public IBuiltInShader<EngineComputeShader> GetComputeShader()
        {
            return computeShader;
        }
        /// <summary>
        /// Gets the compute shader of the specified type
        /// </summary>
        /// <typeparam name="T">Shader type</typeparam>
        public T GetComputeShader<T>() where T : class, IBuiltInShader<EngineComputeShader>
        {
            return computeShader as T;
        }

        /// <summary>
        /// Prepares the internal shaders in the graphics device
        /// </summary>
        /// <param name="dc">Device context</param>
        protected virtual void PrepareShaders(IEngineDeviceContext dc)
        {
            dc.SetVertexShader(vertexShader?.Shader);
            vertexShader?.SetShaderResources(dc);

            dc.SetHullShader(hullShader?.Shader);
            hullShader?.SetShaderResources(dc);

            dc.SetDomainShader(domainShader?.Shader);
            domainShader?.SetShaderResources(dc);

            dc.SetGeometryShader(geometryShader?.Shader);
            geometryShader?.SetShaderResources(dc);

            dc.SetPixelShader(pixelShader?.Shader);
            pixelShader?.SetShaderResources(dc);

            dc.SetComputeShader(computeShader?.Shader);
            computeShader?.SetShaderResources(dc);
        }

        /// <inheritdoc/>
        public virtual void UpdateCastingLight(DrawContextShadows context)
        {

        }
        /// <inheritdoc/>
        public virtual void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {

        }
        /// <inheritdoc/>
        public virtual void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {

        }

        /// <inheritdoc/>
        public virtual bool Draw(IEngineDeviceContext dc, BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0)
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
        public virtual bool Draw(IEngineDeviceContext dc, BufferManager bufferManager, DrawOptions options)
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
        public virtual bool Draw(IEngineDeviceContext dc, IEngineVertexBuffer buffer, Topology topology, int drawCount)
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
        public virtual bool Draw(IEngineDeviceContext dc, int bufferSlot, EngineVertexBufferBinding vertexBufferBinding, EngineBuffer indexBuffer, Topology topology, int count, int startLocation)
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
        public virtual bool DrawAuto(IEngineDeviceContext dc, IEngineVertexBuffer buffer, Topology topology)
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
        public void StreamOut(IEngineDeviceContext dc, bool firstRun, IEngineVertexBuffer buffer, IEngineVertexBuffer streamOutBuffer, Topology topology)
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
