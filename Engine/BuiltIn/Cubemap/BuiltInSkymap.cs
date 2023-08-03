using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Cubemap
{
    using Engine.Common;

    /// <summary>
    /// Skymap drawer
    /// </summary>
    public class BuiltInSkymap : BuiltInDrawer
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
            public readonly int GetStride()
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
        public BuiltInSkymap(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkymapVs>();
            SetPixelShader<SkymapPs>();

            cbPerCube = BuiltInShaders.GetConstantBuffer<PerCube>();
        }

        /// <summary>
        /// Updates the texture
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="texture">Texture</param>
        /// <param name="textureIndex">Texture index</param>
        public void Update(IEngineDeviceContext dc, EngineShaderResourceView texture, uint textureIndex)
        {
            cbPerCube.WriteData(PerCube.Build(textureIndex));
            dc.UpdateConstantBuffer(cbPerCube);

            var pixelShader = GetPixelShader<SkymapPs>();
            pixelShader?.SetPerSkyConstantBuffer(cbPerCube);
            pixelShader?.SetTexture(texture);
            pixelShader?.SetSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
