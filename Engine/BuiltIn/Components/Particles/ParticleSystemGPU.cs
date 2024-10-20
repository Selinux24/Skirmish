﻿using SharpDX;
using System;

namespace Engine.BuiltIn.Components.Particles
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Particles;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Particle system
    /// </summary>
    public class ParticleSystemGpu : IParticleSystem<ParticleEmitter, ParticleSystemParams>
    {
        /// <summary>
        /// Assigned buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

        /// <summary>
        /// Emitter initialization buffer
        /// </summary>
        private EngineVertexBuffer<VertexGpuParticle> emittersBuffer;
        /// <summary>
        /// Drawing buffer
        /// </summary>
        private EngineVertexBuffer<VertexGpuParticle> drawingBuffer;
        /// <summary>
        /// Stream out buffer
        /// </summary>
        private EngineVertexBuffer<VertexGpuParticle> streamOutBuffer;
        /// <summary>
        /// First run flag
        /// </summary>
        private bool firstRun = true;
        /// <summary>
        /// Particle parameters
        /// </summary>
        private ParticleSystemParams parameters;
        /// <summary>
        /// Drawer
        /// </summary>
        private BuiltInParticles particleDrawer;
        /// <summary>
        /// Stream-out
        /// </summary>
        private BuiltInStreamOut particleStreamOut;

        /// <summary>
        /// Scene instance
        /// </summary>
        protected Scene Scene = null;

        /// <summary>
        /// Particle texture
        /// </summary>
        public EngineShaderResourceView Texture { get; private set; }
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount { get; private set; }
        /// <summary>
        /// Active particle count
        /// </summary>
        public int ActiveParticles { get; }
        /// <inheritdoc/>
        public int MaxConcurrentParticles { get; private set; }
        /// <summary>
        /// Time to end
        /// </summary>
        public float TimeToEnd { get; private set; }

        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public ParticleEmitter Emitter { get; private set; }
        /// <inheritdoc/>
        public bool Active
        {
            get
            {
                return Emitter.Active || TimeToEnd > 0;
            }
        }

        /// <summary>
        /// Creates a new CPU particle system
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="emitter"></param>
        public static ParticleSystemGpu Create(Scene scene, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            var pParameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new FileArrayImageContent(description.ContentPath, description.TextureName);
            var texture = scene.Game.ResourceManager.RequestResource(imgContent);
            var textureCount = (uint)imgContent.Count;

            emitter.UpdateBounds(pParameters);
            int maxConcurrentParticles = emitter.GetMaximumConcurrentParticles(description.MaxDuration);
            float timeToEnd = emitter.Duration + pParameters.MaxDuration;

            var res = new ParticleSystemGpu
            {
                Scene = scene,
                Name = name,

                parameters = pParameters,

                Texture = texture,
                TextureCount = textureCount,

                Emitter = emitter,
                MaxConcurrentParticles = maxConcurrentParticles,

                TimeToEnd = timeToEnd
            };

            res.InitializeBuffers();

            return res;
        }

        /// <summary>
        /// Contructor
        /// </summary>
        protected ParticleSystemGpu()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ParticleSystemGpu()
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
                emittersBuffer?.Dispose();
                emittersBuffer = null;

                drawingBuffer?.Dispose();
                drawingBuffer = null;

                streamOutBuffer?.Dispose();
                streamOutBuffer = null;
            }
        }

        /// <summary>
        /// Estimates the length of the buffer based on the maximum number of particles, and the maximum duration of each particle
        /// </summary>
        /// <returns>Returns the estimated buffer length</returns>
        /// <remarks>The maximum length of the buffer is 5000</remarks>
        protected int GetBufferLength()
        {
            int length = (int)(MaxConcurrentParticles * (MathUtil.IsZero(Emitter.Duration) ? 60 : Emitter.Duration));

            return Math.Min(length, 5000);
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        protected void InitializeBuffers()
        {
            var data = Helper.CreateArray(1, new VertexGpuParticle()
            {
                Position = Emitter.Position,
                Velocity = Emitter.Velocity,
                RandomValues = new Vector4(),
                MaxAge = 0,

                Type = 0,
                EmissionTime = Emitter.Duration,
            });

            var graphics = Scene.Game.Graphics;

            particleDrawer = BuiltInShaders.GetDrawer<BuiltInParticles>();
            var particleVsSignature = particleDrawer.GetVertexShader().Shader.GetShaderBytecode();
            emittersBuffer = new EngineVertexBuffer<VertexGpuParticle>(graphics, $"{Name}_Emitter", data, VertexBufferParams.Default);
            emittersBuffer.CreateInputLayout(nameof(ParticlesVs), particleVsSignature, BufferSlot);

            particleStreamOut = BuiltInShaders.GetDrawer<BuiltInStreamOut>();
            var streamoutVsSignature = particleStreamOut.GetVertexShader().Shader.GetShaderBytecode();
            int length = GetBufferLength();
            drawingBuffer = new EngineVertexBuffer<VertexGpuParticle>(graphics, $"{Name}_SO1", length, VertexBufferParams.StreamOut);
            drawingBuffer.CreateInputLayout($"{Name}_SO1_IL", streamoutVsSignature, BufferSlot);
            streamOutBuffer = new EngineVertexBuffer<VertexGpuParticle>(graphics, $"{Name}_SO2", length, VertexBufferParams.StreamOut);
            streamOutBuffer.CreateInputLayout($"{Name}_SO2_IL", streamoutVsSignature, BufferSlot);
        }

        /// <inheritdoc/>
        public void Update(UpdateContext context)
        {
            Emitter.Update(context.GameTime, Scene.Camera.Position);

            TimeToEnd -= Emitter.ElapsedTime;
        }
        /// <inheritdoc/>
        public bool Draw(DrawContext context)
        {
            bool isTransparent = parameters.BlendMode.HasFlag(BlendModes.Alpha) || parameters.BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(parameters.BlendMode, isTransparent);
            if (!draw)
            {
                return false;
            }

            StreamOut(context.DeviceContext);

            ToggleBuffers();

            return DrawInternal(context);
        }

        /// <inheritdoc/>
        public ParticleSystemParams GetParameters()
        {
            return parameters;
        }
        /// <inheritdoc/>
        public void SetParameters(ParticleSystemParams particleParameters)
        {
            parameters = particleParameters;

            Emitter?.UpdateBounds(particleParameters);
        }

        /// <summary>
        /// Stream output
        /// </summary>
        /// <param name="dc">Device context</param>
        private void StreamOut(IEngineDeviceContext dc)
        {
            dc.SetDepthStencilState(Scene.Game.Graphics.GetDepthStencilNone());

            var soState = new BuiltInStreamOutState
            {
                EmissionRate = Emitter.EmissionRate,
                VelocitySensitivity = parameters.EmitterVelocitySensitivity,
                TotalTime = Emitter.TotalTime,
                ElapsedTime = Emitter.ElapsedTime,
                HorizontalVelocity = parameters.HorizontalVelocity,
                VerticalVelocity = parameters.VerticalVelocity,
                RandomValues = Helper.RandomGenerator.NextVector4(Vector4.Zero, Vector4.One),
            };
            particleStreamOut.Update(dc, soState);

            particleStreamOut.StreamOut(
                dc,
                firstRun,
                firstRun ? emittersBuffer : drawingBuffer,
                streamOutBuffer,
                Topology.PointList);

            if (firstRun)
            {
                firstRun = false;
            }
        }
        /// <summary>
        /// Toggle stream out and drawing buffers
        /// </summary>
        private void ToggleBuffers()
        {
            (streamOutBuffer, drawingBuffer) = (drawingBuffer, streamOutBuffer);
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="drawerMode">Drawe mode</param>
        private bool DrawInternal(DrawContext context)
        {
            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            dc.SetDepthStencilState(graphics.GetDepthStencilRDZEnabled());
            dc.SetBlendState(graphics.GetBlendState(context.DrawerMode, parameters.BlendMode));

            var useRotation = parameters.RotateSpeed != Vector2.Zero;
            var state = new BuiltInParticlesState
            {
                TotalTime = Emitter.TotalTime,
                ElapsedTime = Emitter.ElapsedTime,
                EmissionRate = Emitter.EmissionRate,
                VelocitySensitivity = parameters.EmitterVelocitySensitivity,
                HorizontalVelocity = parameters.HorizontalVelocity,
                VerticalVelocity = parameters.VerticalVelocity,
                RandomValues = Helper.RandomGenerator.NextVector4(Vector4.Zero, Vector4.One),
                MaxDuration = parameters.MaxDuration,
                MaxDurationRandomness = parameters.MaxDurationRandomness,
                EndVelocity = parameters.EndVelocity,
                Gravity = parameters.Gravity,
                StartSize = parameters.StartSize,
                EndSize = parameters.EndSize,
                MinColor = parameters.MinColor,
                MaxColor = parameters.MaxColor,
                UseRotation = useRotation,
                RotateSpeed = parameters.RotateSpeed,
            };
            particleDrawer.Update(dc, state, TextureCount, Texture);

            return particleDrawer.DrawAuto(context.DeviceContext, drawingBuffer, Topology.PointList);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {ActiveParticles}; Total: {Emitter.TotalTime:0.00}/{Emitter.Duration:0.00}; ToEnd: {TimeToEnd:0.00};";
        }
    }
}
