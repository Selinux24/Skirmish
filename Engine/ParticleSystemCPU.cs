using SharpDX;
using SharpDX.Direct3D;
using System;

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
        private readonly VertexCpuParticle[] particles;
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
                return this.Emitter.Active || this.TimeToEnd > 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Name</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        public ParticleSystemCpu(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            this.Game = game;
            this.Name = name;

            this.parameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.RequestResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            this.Emitter = emitter;
            this.Emitter.UpdateBounds(this.parameters);
            this.MaxConcurrentParticles = this.Emitter.GetMaximumConcurrentParticles(description.MaxDuration);

            this.particles = new VertexCpuParticle[this.MaxConcurrentParticles];

            this.buffer = new EngineBuffer<VertexCpuParticle>(game.Graphics, description.Name, this.particles, true);
            buffer.AddInputLayout(game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultCPUParticles.RotationDraw.GetSignature(), VertexCpuParticle.Input(BufferSlot)));
            buffer.AddInputLayout(game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultCPUParticles.NonRotationDraw.GetSignature(), VertexCpuParticle.Input(BufferSlot)));

            this.TimeToEnd = this.Emitter.Duration + this.parameters.MaxDuration;
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
            this.Emitter.Update(context);

            if (this.Emitter.Active && this.timeToNextParticle <= 0)
            {
                this.AddParticle();

                this.timeToNextParticle = this.Emitter.EmissionRate;
            }

            this.timeToNextParticle -= this.Emitter.ElapsedTime;
            this.TimeToEnd -= this.Emitter.ElapsedTime;
        }
        /// <summary>
        /// Draw particles
        /// </summary>
        /// <param name="context">Context</param>
        public void Draw(DrawContext context)
        {
            if (this.ActiveParticles <= 0)
            {
                return;
            }

            bool isTransparent = this.parameters.BlendMode.HasFlag(BlendModes.Alpha) || this.parameters.BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(BlendModes.Default, isTransparent);
            if (!draw)
            {
                return;
            }

            var rot = this.parameters.RotateSpeed != Vector2.Zero;

            var effect = DrawerPool.EffectDefaultCPUParticles;
            var technique = rot ? effect.RotationDraw : effect.NonRotationDraw;

            var mode = context.DrawerMode;
            if (!mode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.ActiveParticles;
            }

            var graphics = this.Game.Graphics;

            graphics.IASetVertexBuffers(BufferSlot, this.buffer.VertexBufferBinding);
            graphics.IAInputLayout = rot ? this.buffer.InputLayouts[0] : this.buffer.InputLayouts[1];
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

            graphics.SetDepthStencilRDZEnabled();
            graphics.SetBlendState(this.parameters.BlendMode);

            var state = new EffectParticleState
            {
                TotalTime = this.Emitter.TotalTime,
                MaxDuration = this.parameters.MaxDuration,
                MaxDurationRandomness = this.parameters.MaxDurationRandomness,
                EndVelocity = this.parameters.EndVelocity,
                Gravity = this.parameters.Gravity,
                StartSize = this.parameters.StartSize,
                EndSize = this.parameters.EndSize,
                MinColor = this.parameters.MinColor,
                MaxColor = this.parameters.MaxColor,
                RotateSpeed = this.parameters.RotateSpeed,
            };

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                state,
                this.TextureCount,
                this.Texture);

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.Draw(this.ActiveParticles, 0);
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

            this.Emitter?.UpdateBounds(particleParameters);
        }

        /// <summary>
        /// Adds a new particle to system
        /// </summary>
        private void AddParticle()
        {
            int nextFreeParticle = this.currentParticleIndex + 1;

            if (this.ActiveParticles < nextFreeParticle)
            {
                this.ActiveParticles = nextFreeParticle;
            }

            if (nextFreeParticle >= this.particles.Length)
            {
                nextFreeParticle = 0;
            }

            Vector3 velocity = this.Emitter.CalcInitialVelocity(
                this.parameters,
                Helper.RandomGenerator.NextFloat(0, 1),
                Helper.RandomGenerator.NextFloat(0, 1),
                Helper.RandomGenerator.NextFloat(0, 1));

            Vector4 randomValues = Helper.RandomGenerator.NextVector4(Vector4.Zero, Vector4.One);

            this.particles[this.currentParticleIndex].Position = this.Emitter.Position;
            this.particles[this.currentParticleIndex].Velocity = velocity;
            this.particles[this.currentParticleIndex].RandomValues = randomValues;
            this.particles[this.currentParticleIndex].MaxAge = this.Emitter.TotalTime;

            Console.WriteLine($"{this.Name} - AddParticle WriteDiscardBuffer");
            this.Game.Graphics.WriteDiscardBuffer(this.buffer.VertexBuffer, this.particles);

            this.currentParticleIndex = nextFreeParticle;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("Count: {0}; Total: {1:0.00}/{2:0.00}; ToEnd: {3:0.00};",
                this.ActiveParticles,
                this.Emitter.TotalTime,
                this.Emitter.Duration,
                this.TimeToEnd);
        }
    }
}
