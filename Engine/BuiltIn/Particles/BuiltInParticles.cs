using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Particle drawer
    /// </summary>
    public class BuiltInParticles : BuiltInDrawer
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
                    MaxDuration = state.MaxDuration,
                    MaxDurationRandomness = state.MaxDurationRandomness,
                    TotalTime = state.TotalTime,
                    ElapsedTime = state.ElapsedTime,

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
            /// Max duration
            /// </summary>
            [FieldOffset(0)]
            public float MaxDuration;
            /// <summary>
            /// Max duration randomness
            /// </summary>
            [FieldOffset(4)]
            public float MaxDurationRandomness;
            /// <summary>
            /// Total particle time (not game time)
            /// </summary>
            [FieldOffset(8)]
            public float TotalTime;
            /// <summary>
            /// Elapsed particle time (not game time)
            /// </summary>
            [FieldOffset(12)]
            public float ElapsedTime;

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
        public BuiltInParticles(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ParticlesVs>();
            SetGeometryShader<ParticlesGS>();
            SetPixelShader<ParticlesPs>();

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

            var vertexShader = GetVertexShader<ParticlesVs>();
            vertexShader?.SetPerEmitterConstantBuffer(cbPerEmitter);

            var geometryShader = GetGeometryShader<ParticlesGS>();
            geometryShader?.SetPerEmitterConstantBuffer(cbPerEmitter);

            var pixelShader = GetPixelShader<ParticlesPs>();
            pixelShader?.SetPerEmitterConstantBuffer(cbPerEmitter);
            pixelShader?.SetTextureArray(textures);
        }
    }
}
