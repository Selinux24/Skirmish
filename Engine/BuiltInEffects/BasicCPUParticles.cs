using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class BasicCPUParticles : BuiltInDrawer<BasicCPUParticlesVs, BasicCPUParticlesGS, BasicCPUParticlesPs>, IDisposable
    {
        /// <summary>
        /// Per emitter data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 112)]
        struct PerEmitter : IBufferData
        {
            public static PerEmitter Build(Vector3 eyePositionWorld, EffectParticleState state, uint textureCount)
            {
                return new PerEmitter
                {
                    TotalTime = state.TotalTime,
                    EyePositionWorld = eyePositionWorld,

                    Rotation = state.RotateSpeed != Vector2.Zero,
                    RotateSpeed = state.RotateSpeed,
                    TextureCount = textureCount,

                    Gravity = state.Gravity,
                    EndVelocity = state.EndVelocity,

                    StartSize = state.StartSize,
                    EndSize = state.EndSize,

                    MinColor = state.MinColor,
                    MaxColor = state.MaxColor,

                    MaxDuration = state.MaxDuration,
                    MaxDurationRandomness = state.MaxDurationRandomness,
                };
            }

            /// <summary>
            /// Total time
            /// </summary>
            [FieldOffset(0)]
            public float TotalTime;
            /// <summary>
            /// Eye position world
            /// </summary>
            [FieldOffset(4)]
            public Vector3 EyePositionWorld;

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

            /// <summary>
            /// Max duration
            /// </summary>
            [FieldOffset(96)]
            public float MaxDuration;
            /// <summary>
            /// Max duration randomness
            /// </summary>
            [FieldOffset(100)]
            public float MaxDurationRandomness;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerEmitter));
            }
        }

        /// <summary>
        /// Per emiter constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerEmitter> cbPerEmitter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicCPUParticles(Graphics graphics) : base(graphics)
        {
            cbPerEmitter = new EngineConstantBuffer<PerEmitter>(graphics, nameof(BasicCPUParticles) + "." + nameof(PerEmitter));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicCPUParticles()
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
                cbPerEmitter?.Dispose();
            }
        }

        /// <summary>
        /// Updates the particle drawer
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="state">Particle state</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture array</param>
        public void Update(Vector3 eyePositionWorld, EffectParticleState state, uint textureCount, EngineShaderResourceView textures)
        {
            cbPerEmitter.WriteData(PerEmitter.Build(eyePositionWorld, state, textureCount));

            VertexShader.SetPerEmitterConstantBuffer(cbPerEmitter);

            GeometryShader.SetPerEmitterConstantBuffer(cbPerEmitter);

            PixelShader.SetPerEmitterConstantBuffer(cbPerEmitter);
            PixelShader.SetTextureArray(textures);
        }
    }
}
