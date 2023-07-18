using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Particles;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Particle system
    /// </summary>
    public class ParticleSystemGpu : IParticleSystem
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
        /// Game instance
        /// </summary>
        protected Game Game = null;

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
        public int ActiveParticles { get; private set; }
        /// <summary>
        /// Gets the maximum number of concurrent particles
        /// </summary>
        public int MaxConcurrentParticles { get; private set; }
        /// <summary>
        /// Time to end
        /// </summary>
        public float TimeToEnd { get; private set; }

        /// <summary>
        /// Particle system Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Particle emitter
        /// </summary>
        public ParticleEmitter Emitter { get; private set; }
        /// <summary>
        /// Gets if the current particle system is active
        /// </summary>
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
        /// <param name="game"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="emitter"></param>
        /// <returns></returns>
        public static async Task<ParticleSystemGpu> Create(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            var pParameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new FileArrayImageContent(description.ContentPath, description.TextureName);
            var texture = await game.ResourceManager.RequestResource(imgContent);
            var textureCount = (uint)imgContent.Count;

            emitter.UpdateBounds(pParameters);
            int maxConcurrentParticles = emitter.GetMaximumConcurrentParticles(description.MaxDuration);
            float timeToEnd = emitter.Duration + pParameters.MaxDuration;

            var res = new ParticleSystemGpu
            {
                Game = game,
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
        /// <summary>
        /// Dispose resources
        /// </summary>
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
            int length = (int)(MaxConcurrentParticles * (Emitter.Duration == 0 ? 60 : Emitter.Duration));

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

            particleDrawer = BuiltInShaders.GetDrawer<BuiltInParticles>();
            var particleVsSignature = particleDrawer.GetVertexShader().Shader.GetShaderBytecode();
            emittersBuffer = new EngineVertexBuffer<VertexGpuParticle>(Game.Graphics, $"{Name}_Emitter", data, VertexBufferParams.Default);
            emittersBuffer.CreateInputLayout(nameof(ParticlesVs), particleVsSignature, BufferSlot);

            particleStreamOut = BuiltInShaders.GetDrawer<BuiltInStreamOut>();
            var streamoutVsSignature = particleStreamOut.GetVertexShader().Shader.GetShaderBytecode();
            int length = GetBufferLength();
            drawingBuffer = new EngineVertexBuffer<VertexGpuParticle>(Game.Graphics, $"{Name}_SO1", length, VertexBufferParams.StreamOut);
            drawingBuffer.CreateInputLayout($"{Name}_SO1_IL", streamoutVsSignature, BufferSlot);
            streamOutBuffer = new EngineVertexBuffer<VertexGpuParticle>(Game.Graphics, $"{Name}_SO2", length, VertexBufferParams.StreamOut);
            streamOutBuffer.CreateInputLayout($"{Name}_SO2_IL", streamoutVsSignature, BufferSlot);
        }

        /// <summary>
        /// Updating
        /// </summary>
        /// <param name="context">Context</param>
        public void Update(UpdateContext context)
        {
            Emitter.Update(context);

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

            StreamOut();

            ToggleBuffers();

            return Draw(context.DrawerMode);
        }

        /// <summary>
        /// Gets current particle system parameters
        /// </summary>
        /// <returns>Returns the particle system parameters configuration</returns>
        public ParticleSystemParams GetParameters()
        {
            return parameters;
        }
        /// <summary>
        /// Sets the particle system parameters
        /// </summary>
        /// <param name="particleParameters">Particle system parameters</param>
        public void SetParameters(ParticleSystemParams particleParameters)
        {
            parameters = particleParameters;

            Emitter?.UpdateBounds(particleParameters);
        }

        /// <summary>
        /// Stream output
        /// </summary>
        /// <param name="effect">Effect for stream out</param>
        private void StreamOut()
        {
            Game.Graphics.SetDepthStencilNone();

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
            particleStreamOut.Update(soState);

            particleStreamOut.StreamOut(
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
        /// <param name="effect">Effect for drawing</param>
        /// <param name="drawerMode">Drawe mode</param>
        private bool Draw(DrawerModes drawerMode)
        {
            var graphics = Game.Graphics;
            graphics.SetDepthStencilRDZEnabled();
            graphics.SetBlendState(parameters.BlendMode);

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
            particleDrawer.Update(state, TextureCount, Texture);

            if (!particleDrawer.DrawAuto(drawingBuffer, Topology.PointList))
            {
                return false;
            }

            if (!drawerMode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
            }

            return true;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {ActiveParticles}; Total: {Emitter.TotalTime:0.00}/{Emitter.Duration:0.00}; ToEnd: {TimeToEnd:0.00};";
        }
    }
}
