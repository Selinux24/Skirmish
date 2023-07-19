using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Decals
{
    using Engine.Common;

    /// <summary>
    /// Decals drawer
    /// </summary>
    public class BuiltInDecals : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per emitter data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerDecal : IBufferData
        {
            public static PerDecal Build(bool rotation, uint textureCount, Color4 tintColor)
            {
                return new PerDecal
                {
                    Rotation = rotation,
                    TextureCount = textureCount,
                    TintColor = tintColor,
                };
            }

            /// <summary>
            /// Rotation
            /// </summary>
            [FieldOffset(0)]
            public bool Rotation;
            /// <summary>
            /// Texture count
            /// </summary>
            [FieldOffset(4)]
            public uint TextureCount;

            /// <summary>
            /// Tint color
            /// </summary>
            [FieldOffset(16)]
            public Color4 TintColor;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerDecal));
            }
        }

        #endregion

        /// <summary>
        /// Per decal constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerDecal> cbPerDecal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInDecals(Graphics graphics) : base(graphics)
        {
            SetVertexShader<DecalsVs>();
            SetGeometryShader<DecalsGS>();
            SetPixelShader<DecalsPs>();

            cbPerDecal = BuiltInShaders.GetConstantBuffer<PerDecal>();
        }

        /// <summary>
        /// Updates the particle drawer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="tintColor">Tint color</param>
        /// <param name="textures">Texture array</param>
        public void Update(EngineDeviceContext dc, bool rotation, uint textureCount, Color4 tintColor, EngineShaderResourceView textures)
        {
            cbPerDecal.WriteData(dc, PerDecal.Build(rotation, textureCount, tintColor));

            var vertexShader = GetVertexShader<DecalsVs>();
            vertexShader?.SetPerDecalConstantBuffer(cbPerDecal);

            var pixelShader = GetPixelShader<DecalsPs>();
            pixelShader?.SetPerDecalConstantBuffer(cbPerDecal);
            pixelShader?.SetTextureArray(textures);
        }
    }
}
