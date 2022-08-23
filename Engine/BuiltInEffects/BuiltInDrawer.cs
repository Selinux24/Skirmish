using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Built-in drawer class
    /// </summary>
    public abstract class BuiltInDrawer<VS, PS> : IBuiltInDrawer
        where VS : IBuiltInVertexShader
        where PS : IBuiltInPixelShader
    {
        /// <summary>
        /// Graphics
        /// </summary>
        protected readonly Graphics Graphics;
        /// <summary>
        /// Vertex shader
        /// </summary>
        protected readonly VS VertexShader;
        /// <summary>
        /// Pixel shader
        /// </summary>
        protected readonly PS PixelShader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        protected BuiltInDrawer(Graphics graphics)
        {
            Graphics = graphics;
            VertexShader = BuiltInShaders.GetVertexShader<VS>();
            PixelShader = BuiltInShaders.GetPixelShader<PS>();
        }

        /// <inheritdoc/>
        public virtual void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {

        }
        /// <inheritdoc/>
        public virtual void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            Graphics.SetVertexShader(VertexShader.Shader);
            Graphics.SetPixelShader(PixelShader.Shader);

            VertexShader.SetConstantBuffers();
            PixelShader.SetConstantBuffers();

            bool instanced = instances > 0;

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(VertexShader.Shader, mesh.VertexBuffer, mesh.Topology, instanced))
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
        }
        /// <inheritdoc/>
        public virtual void Draw(BufferManager bufferManager, DrawOptions options)
        {
            // Set the vertex and pixel shaders that will be used to render this mesh.
            Graphics.SetVertexShader(VertexShader.Shader);
            Graphics.SetPixelShader(PixelShader.Shader);

            VertexShader.SetConstantBuffers();
            PixelShader.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(VertexShader.Shader, options.VertexBuffer, options.Topology, options.Instanced))
            {
                return;
            }

            // Render the primitives.
            options.Draw(Graphics);
        }
    }
}
