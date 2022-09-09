using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.CpuParticles
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class BuiltInCpuParticles : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per emitter data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 96)]
        struct PerEmitter : IBufferData
        {
            public static PerEmitter Build(EffectParticleState state, uint textureCount)
            {
                return new PerEmitter
                {
                    TotalTime = state.TotalTime,
                    MaxDuration = state.MaxDuration,
                    MaxDurationRandomness = state.MaxDurationRandomness,

                    Rotation = state.RotateSpeed != Vector2.Zero,
                    RotateSpeed = state.RotateSpeed,
                    TextureCount = textureCount,

                    Gravity = state.Gravity,
                    EndVelocity = state.EndVelocity,

                    StartSize = state.StartSize,
                    EndSize = state.EndSize,

                    MinColor = state.MinColor,
                    MaxColor = state.MaxColor,
                };
            }

            /// <summary>
            /// Total time
            /// </summary>
            [FieldOffset(0)]
            public float TotalTime;
            /// <summary>
            /// Max duration
            /// </summary>
            [FieldOffset(4)]
            public float MaxDuration;
            /// <summary>
            /// Max duration randomness
            /// </summary>
            [FieldOffset(8)]
            public float MaxDurationRandomness;

            /// <summary>
            /// Rotation
            /// </summary>
            [FieldOffset(16)]
            public bool Rotation;
            /// <summary>
            /// Rotate speed
            /// </summary>
            [FieldOffset(20)]
            public Vector2 RotateSpeed;
            /// <summary>
            /// Texture count
            /// </summary>
            [FieldOffset(28)]
            public uint TextureCount;

            /// <summary>
            /// Gravity vector
            /// </summary>
            [FieldOffset(32)]
            public Vector3 Gravity;
            /// <summary>
            /// End velocity
            /// </summary>
            [FieldOffset(44)]
            public float EndVelocity;

            /// <summary>
            /// Start size
            /// </summary>
            [FieldOffset(48)]
            public Vector2 StartSize;
            /// <summary>
            /// End size
            /// </summary>
            [FieldOffset(56)]
            public Vector2 EndSize;

            /// <summary>
            /// Min color
            /// </summary>
            [FieldOffset(64)]
            public Vector4 MinColor;

            /// <summary>
            /// Max color
            /// </summary>
            [FieldOffset(80)]
            public Vector4 MaxColor;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerEmitter));
            }
        }

        #endregion

        /// <summary>
        /// Per emiter constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerEmitter> cbPerEmitter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInCpuParticles(Graphics graphics) : base(graphics)
        {
            SetVertexShader<CpuParticlesVs>();
            SetGeometryShader<CpuParticlesGS>();
            SetPixelShader<CpuParticlesPs>();

            cbPerEmitter = BuiltInShaders.GetConstantBuffer<PerEmitter>();
        }

        /// <summary>
        /// Updates the particle drawer
        /// </summary>
        /// <param name="state">Particle state</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture array</param>
        public void Update(EffectParticleState state, uint textureCount, EngineShaderResourceView textures)
        {
            cbPerEmitter.WriteData(PerEmitter.Build(state, textureCount));

            var vertexShader = GetVertexShader<CpuParticlesVs>();
            vertexShader?.SetPerEmitterConstantBuffer(cbPerEmitter);

            var geometryShader = GetGeometryShader<CpuParticlesGS>();
            geometryShader?.SetPerEmitterConstantBuffer(cbPerEmitter);

            var pixelShader = GetPixelShader<CpuParticlesPs>();
            pixelShader?.SetPerEmitterConstantBuffer(cbPerEmitter);
            pixelShader?.SetTextureArray(textures);
        }
    }
}
