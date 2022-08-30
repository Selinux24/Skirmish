using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Cubemap;
    using Engine.Common;

    /// <summary>
    /// Skymap drawer
    /// </summary>
    public class BasicSkymap : BuiltInDrawer, IDisposable
    {
        #region Buffer  

        /// <summary>
        /// Per cube data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerCube : IBufferData
        {
            public static PerCube Build(float textureIndex)
            {
                return new PerCube
                {
                    TextureIndex = textureIndex,
                };
            }

            /// <summary>
            /// Texture index
            /// </summary>
            [FieldOffset(0)]
            public float TextureIndex;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerCube));
            }
        }

        #endregion

        /// <summary>
        /// Per cube constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCube> cbPerCube;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicSkymap(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkymapVs>();
            SetPixelShader<SkymapPs>();

            cbPerCube = new EngineConstantBuffer<PerCube>(graphics, nameof(BasicSkymap) + "." + nameof(PerCube));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicSkymap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                cbPerCube?.Dispose();
            }
        }

        /// <summary>
        /// Updates the texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void Update(EngineShaderResourceView texture, uint textureIndex)
        {
            cbPerCube.WriteData(PerCube.Build(textureIndex));

            var pixelShader = GetPixelShader<SkymapPs>();
            pixelShader?.SetPerCubeConstantBuffer(cbPerCube);
            pixelShader?.SetTexture(texture);
            pixelShader?.SetSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
