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
    public class ParticleSystemCPU : IParticleSystem
    {
        public static int BufferSlot = 0;

        /// <summary>
        /// Random instance
        /// </summary>
        private readonly Random rnd = new Random();
        /// <summary>
        /// Particle list
        /// </summary>
        private readonly VertexCPUParticle[] particles;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private EngineBuffer<VertexCPUParticle> buffer;
        /// <summary>
        /// Current particle index to update data
        /// </summary>
        private int currentParticleIndex = 0;
        /// <summary>
        /// Time to next particle emission
        /// </summary>
        private float timeToNextParticle = 0;

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
        /// Particle system parameters
        /// </summary>
        public ParticleSystemParams Parameters { get; set; }
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
        public ParticleSystemCPU(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            this.Game = game;
            this.Name = name;

            this.Parameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.CreateResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            this.Emitter = emitter;
            this.Emitter.SetBoundingBox(ParticleEmitter.GenerateBBox(description.MaxDuration, this.Parameters.EndSize, this.Parameters.HorizontalVelocity, this.Parameters.VerticalVelocity));
            this.MaxConcurrentParticles = this.Emitter.GetMaximumConcurrentParticles(description.MaxDuration);

            this.particles = new VertexCPUParticle[this.MaxConcurrentParticles];

            this.buffer = new EngineBuffer<VertexCPUParticle>(game.Graphics, description.Name, this.particles, true);
            buffer.AddInputLayout(game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultCPUParticles.RotationDraw.GetSignature(), VertexCPUParticle.Input(BufferSlot)));
            buffer.AddInputLayout(game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultCPUParticles.NonRotationDraw.GetSignature(), VertexCPUParticle.Input(BufferSlot)));

            this.TimeToEnd = this.Emitter.Duration + this.Parameters.MaxDuration;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ParticleSystemCPU()
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
                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
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
            var mode = context.DrawerMode;

            if (mode.HasFlag(DrawerModesEnum.ShadowMap) ||
                (mode.HasFlag(DrawerModesEnum.OpaqueOnly) && !this.Parameters.Transparent) ||
                (mode.HasFlag(DrawerModesEnum.TransparentOnly) && this.Parameters.Transparent))
            {
                if (this.ActiveParticles > 0)
                {
                    var rot = this.Parameters.RotateSpeed != Vector2.Zero;

                    var effect = DrawerPool.EffectDefaultCPUParticles;
                    var technique = rot ? effect.RotationDraw : effect.NonRotationDraw;

                    if (!mode.HasFlag(DrawerModesEnum.ShadowMap))
                    {
                        Counters.InstancesPerFrame++;
                        Counters.PrimitivesPerFrame += this.ActiveParticles;
                    }

                    var graphics = this.Game.Graphics;

                    graphics.IASetVertexBuffers(BufferSlot, this.buffer.VertexBufferBinding);
                    graphics.IAInputLayout = rot ? this.buffer.InputLayouts[0] : this.buffer.InputLayouts[1];
                    graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

                    graphics.SetDepthStencilRDZEnabled();

                    if (this.Parameters.Additive)
                    {
                        graphics.SetBlendAdditive();
                    }
                    else if (this.Parameters.Transparent)
                    {
                        graphics.SetBlendDefaultAlpha();
                    }
                    else
                    {
                        graphics.SetBlendDefault();
                    }

                    effect.UpdatePerFrame(
                        context.ViewProjection,
                        context.EyePosition,
                        this.Emitter.TotalTime,
                        this.Parameters.MaxDuration,
                        this.Parameters.MaxDurationRandomness,
                        this.Parameters.EndVelocity,
                        this.Parameters.Gravity,
                        this.Parameters.StartSize,
                        this.Parameters.EndSize,
                        this.Parameters.MinColor,
                        this.Parameters.MaxColor,
                        this.Parameters.RotateSpeed,
                        this.TextureCount,
                        this.Texture);

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(this.ActiveParticles, 0);
                    }
                }
            }
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

            Vector3 velocity = this.Emitter.Velocity * this.Parameters.EmitterVelocitySensitivity;

            float horizontalVelocity = MathUtil.Lerp(
                this.Parameters.HorizontalVelocity.X,
                this.Parameters.HorizontalVelocity.Y,
                this.rnd.NextFloat(0, 1));

            double horizontalAngle = this.rnd.NextDouble() * MathUtil.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);
            velocity.Y += MathUtil.Lerp(this.Parameters.VerticalVelocity.X, this.Parameters.VerticalVelocity.Y, this.rnd.NextFloat(0, 1));

            Vector4 randomValues = this.rnd.NextVector4(Vector4.Zero, Vector4.One);

            this.particles[this.currentParticleIndex].Position = this.Emitter.Position;
            this.particles[this.currentParticleIndex].Velocity = velocity;
            this.particles[this.currentParticleIndex].RandomValues = randomValues;
            this.particles[this.currentParticleIndex].MaxAge = this.Emitter.TotalTime;

            this.Game.Graphics.WriteDiscardBuffer(this.buffer.VertexBuffer, this.particles);

            this.currentParticleIndex = nextFreeParticle;
        }

        /// <summary>
        /// Gets the text representation of the particle system
        /// </summary>
        /// <returns>Returns the text representation of the particle system</returns>
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
