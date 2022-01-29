using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// CPU particle system
    /// </summary>
    public class ParticleSystemCpu : IParticleSystem
    {
        /// <summary>
        /// Assigned buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

        /// <summary>
        /// Particle list
        /// </summary>
        private VertexCpuParticle[] particles;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private EngineBuffer<VertexCpuParticle> buffer;
        /// <summary>
        /// Current particle index to update data
        /// </summary>
        private int currentParticleIndex = 0;
        /// <summary>
        /// Time to next particle emission
        /// </summary>
        private float timeToNextParticle = 0;
        /// <summary>
        /// Particle parameters
        /// </summary>
        private ParticleSystemParams parameters;

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
        public static async Task<ParticleSystemCpu> Create(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            var pParameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new FileArrayImageContent(description.ContentPath, description.TextureName);
            var texture = await game.ResourceManager.RequestResource(imgContent);
            var textureCount = (uint)imgContent.Count;

            emitter.UpdateBounds(pParameters);
            int maxConcurrentParticles = emitter.GetMaximumConcurrentParticles(description.MaxDuration);
            var vParticles = new VertexCpuParticle[maxConcurrentParticles];
            float timeToEnd = emitter.Duration + pParameters.MaxDuration;

            var pBuffer = new EngineBuffer<VertexCpuParticle>(game.Graphics, description.Name, vParticles, true);
            pBuffer.AddInputLayout(game.Graphics.CreateInputLayout("EffectDefaultCPUParticles.RotationDraw", DrawerPool.EffectDefaultCPUParticles.RotationDraw.GetSignature(), VertexCpuParticle.Input(BufferSlot)));
            pBuffer.AddInputLayout(game.Graphics.CreateInputLayout("EffectDefaultCPUParticles.NonRotationDraw", DrawerPool.EffectDefaultCPUParticles.NonRotationDraw.GetSignature(), VertexCpuParticle.Input(BufferSlot)));

            return new ParticleSystemCpu
            {
                Game = game,
                Name = name,

                parameters = pParameters,

                Texture = texture,
                TextureCount = textureCount,

                Emitter = emitter,
                MaxConcurrentParticles = maxConcurrentParticles,
                particles = vParticles,

                buffer = pBuffer,

                TimeToEnd = timeToEnd
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected ParticleSystemCpu()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ParticleSystemCpu()
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
                buffer?.Dispose();
                buffer = null;
            }
        }

        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="context">Context</param>
        public void Update(UpdateContext context)
        {
            Emitter.Update(context);

            if (Emitter.Active && timeToNextParticle <= 0)
            {
                AddParticle();

                timeToNextParticle = Emitter.EmissionRate;
            }

            timeToNextParticle -= Emitter.ElapsedTime;
            TimeToEnd -= Emitter.ElapsedTime;
        }
        /// <summary>
        /// Draw particles
        /// </summary>
        /// <param name="context">Context</param>
        public void Draw(DrawContext context)
        {
            if (ActiveParticles <= 0)
            {
                return;
            }

            bool isTransparent = parameters.BlendMode.HasFlag(BlendModes.Alpha) || parameters.BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(BlendModes.Default, isTransparent);
            if (!draw)
            {
                return;
            }

            var rot = parameters.RotateSpeed != Vector2.Zero;

            var effect = DrawerPool.EffectDefaultCPUParticles;
            var technique = rot ? effect.RotationDraw : effect.NonRotationDraw;

            var mode = context.DrawerMode;
            if (!mode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += ActiveParticles;
            }

            var graphics = Game.Graphics;

            graphics.IASetVertexBuffers(BufferSlot, buffer.VertexBufferBinding);
            graphics.IAInputLayout = rot ? buffer.InputLayouts[0] : buffer.InputLayouts[1];
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

            graphics.SetDepthStencilRDZEnabled();
            graphics.SetBlendState(parameters.BlendMode);

            var state = new EffectParticleState
            {
                TotalTime = Emitter.TotalTime,
                MaxDuration = parameters.MaxDuration,
                MaxDurationRandomness = parameters.MaxDurationRandomness,
                EndVelocity = parameters.EndVelocity,
                Gravity = parameters.Gravity,
                StartSize = parameters.StartSize,
                EndSize = parameters.EndSize,
                MinColor = parameters.MinColor,
                MaxColor = parameters.MaxColor,
                RotateSpeed = parameters.RotateSpeed,
            };

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                state,
                TextureCount,
                Texture);

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.Draw(ActiveParticles, 0);
            }
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
        /// Adds a new particle to system
        /// </summary>
        private void AddParticle()
        {
            int nextFreeParticle = currentParticleIndex + 1;

            if (ActiveParticles < nextFreeParticle)
            {
                ActiveParticles = nextFreeParticle;
            }

            if (nextFreeParticle >= particles.Length)
            {
                nextFreeParticle = 0;
            }

            Vector3 velocity = Emitter.CalcInitialVelocity(
                parameters,
                Helper.RandomGenerator.NextFloat(0, 1),
                Helper.RandomGenerator.NextFloat(0, 1),
                Helper.RandomGenerator.NextFloat(0, 1));

            Vector4 randomValues = Helper.RandomGenerator.NextVector4(Vector4.Zero, Vector4.One);

            particles[currentParticleIndex].Position = Emitter.Position;
            particles[currentParticleIndex].Velocity = velocity;
            particles[currentParticleIndex].RandomValues = randomValues;
            particles[currentParticleIndex].MaxAge = Emitter.TotalTime;

            Logger.WriteTrace(this, $"{Name} - AddParticle WriteDiscardBuffer");
            Game.Graphics.WriteDiscardBuffer(buffer.VertexBuffer, particles);

            currentParticleIndex = nextFreeParticle;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {ActiveParticles}; Total: {Emitter.TotalTime:0.00}/{Emitter.Duration:0.00}; ToEnd: {TimeToEnd:0.00};";
        }
    }
}
