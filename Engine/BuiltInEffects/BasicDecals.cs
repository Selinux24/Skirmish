using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Decals;
    using Engine.Common;

    /// <summary>
    /// Decals drawer
    /// </summary>
    public class BasicDecals : BuiltInDrawer, IDisposable
    {
        #region Buffers

        /// <summary>
        /// Per emitter data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerDecal : IBufferData
        {
            public static PerDecal Build(float totalTime, bool rotation, uint textureCount, Color4 tintColor)
            {
                return new PerDecal
                {
                    TotalTime = totalTime,
                    Rotation = rotation,
                    TextureCount = textureCount,
                    TintColor = tintColor,
                };
            }

            /// <summary>
            /// Total time
            /// </summary>
            [FieldOffset(0)]
            public float TotalTime;
            /// <summary>
            /// Rotation
            /// </summary>
            [FieldOffset(4)]
            public bool Rotation;
            /// <summary>
            /// Texture count
            /// </summary>
            [FieldOffset(8)]
            public uint TextureCount;

            /// <summary>
            /// Tint color
            /// </summary>
            [FieldOffset(16)]
            public Color4 TintColor;

            /// <inheritdoc/>
            public int GetStride()
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
        public BasicDecals(Graphics graphics) : base(graphics)
        {
            SetVertexShader<DecalsVs>();
            SetGeometryShader<DecalsGS>();
            SetPixelShader<DecalsPs>();

            cbPerDecal = new EngineConstantBuffer<PerDecal>(graphics, nameof(BasicDecals) + "." + nameof(PerDecal));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicDecals()
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
                cbPerDecal?.Dispose();
            }
        }

        /// <summary>
        /// Updates the particle drawer
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="state">Particle state</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture array</param>
        public void Update(float totalTime, bool rotation, uint textureCount, Color4 tintColor, EngineShaderResourceView textures)
        {
            cbPerDecal.WriteData(PerDecal.Build(totalTime, rotation, textureCount, tintColor));

            var vertexShader = GetVertexShader<DecalsVs>();
            vertexShader?.SetPerDecalConstantBuffer(cbPerDecal);

            var pixelShader = GetPixelShader<DecalsPs>();
            pixelShader?.SetPerDecalConstantBuffer(cbPerDecal);
            pixelShader?.SetTextureArray(textures);
        }
    }
}
