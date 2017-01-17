using SharpDX;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// CPU particle system
    /// </summary>
    public class ParticleSystemCPU : IParticleSystem
    {
        /// <summary>
        /// Particle list
        /// </summary>
        private VertexCPUParticle[] particles;
        /// <summary>
        /// Particles buffer
        /// </summary>
        private Buffer vertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding[] vertexBufferBinding;
        /// <summary>
        /// Current particle index to update data
        /// </summary>
        private int currentParticleIndex = 0;
        /// <summary>
        /// Time to next particle emission
        /// </summary>
        private float timeToNextParticle = 0;
        /// <summary>
        /// Random instance
        /// </summary>
        private Random rnd = new Random();

        /// <summary>
        /// Game instance
        /// </summary>
        protected Game Game = null;

        /// <summary>
        /// Particle texture
        /// </summary>
        public ShaderResourceView Texture { get; private set; }
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
        /// Maximum particle age
        /// </summary>
        public float MaximumAge { get; private set; }
        /// <summary>
        /// Macimum age variation
        /// </summary>
        public float MaximumAgeVariation { get; private set; }
        /// <summary>
        /// Velocity at end
        /// </summary>
        public float VelocityAtEnd { get; private set; }
        /// <summary>
        /// Gravity vector
        /// </summary>
        public Vector3 Gravity { get; private set; }
        /// <summary>
        /// Starting size
        /// </summary>
        public Vector2 StartSize { get; private set; }
        /// <summary>
        /// Ending size
        /// </summary>
        public Vector2 EndSize { get; private set; }
        /// <summary>
        /// Minimum color
        /// </summary>
        public Color MinimumColor { get; private set; }
        /// <summary>
        /// Maximum color
        /// </summary>
        public Color MaximumColor { get; private set; }
        /// <summary>
        /// Horizontal velocity
        /// </summary>
        public Vector2 HorizontalVelocity { get; private set; }
        /// <summary>
        /// Vertical velocity
        /// </summary>
        public Vector2 VerticalVelocity { get; private set; }
        /// <summary>
        /// Rotation speed
        /// </summary>
        public Vector2 RotateSpeed { get; private set; }
        /// <summary>
        /// Velocity sensitivity
        /// </summary>
        public float VelocitySensitivity { get; private set; }
        /// <summary>
        /// Trasparent particles
        /// </summary>
        public bool Transparent { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        public ParticleSystemCPU(Game game, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            this.Game = game;

            this.MaximumAge = description.MaxDuration;
            this.MaximumAgeVariation = description.MaxDurationRandomness;
            this.VelocityAtEnd = description.EndVelocity;
            this.Gravity = description.Gravity;
            this.StartSize = new Vector2(description.MinStartSize, description.MaxStartSize);
            this.EndSize = new Vector2(description.MinEndSize, description.MaxEndSize);
            this.MinimumColor = description.MinColor;
            this.MaximumColor = description.MaxColor;
            this.HorizontalVelocity = new Vector2(description.MinHorizontalVelocity, description.MaxHorizontalVelocity);
            this.VerticalVelocity = new Vector2(description.MinVerticalVelocity, description.MaxVerticalVelocity);
            this.RotateSpeed = new Vector2(description.MinRotateSpeed, description.MaxRotateSpeed);
            this.VelocitySensitivity = description.EmitterVelocitySensitivity;
            this.Transparent = description.Transparent;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.CreateResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            this.Emitter = emitter;
            this.MaxConcurrentParticles = this.Emitter.GetMaximumConcurrentParticles(description.MaxDuration);

            this.particles = new VertexCPUParticle[this.MaxConcurrentParticles];

            this.vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(this.particles);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, default(VertexCPUParticle).GetStride(), 0),
            };

            this.TimeToEnd = this.Emitter.Duration + this.MaximumAge;
        }
        /// <summary>
        /// Resource disposal
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
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
            var effect = DrawerPool.EffectDefaultCPUParticles;
             
            var technique = effect.GetTechnique(
                VertexTypes.Particle, 
                false, 
                DrawingStages.Drawing, 
                context.DrawerMode,
                this.RotateSpeed != Vector2.Zero);

            this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
            Counters.IAInputLayoutSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            Counters.IAPrimitiveTopologySets++;

            this.Game.Graphics.SetDepthStencilRDZEnabled();

            if (this.Transparent)
            {
                this.Game.Graphics.SetBlendDefaultAlpha();
            }
            else
            {
                this.Game.Graphics.SetBlendDefault();
            }

            effect.UpdatePerFrame(
                context.World,
                context.ViewProjection,
                context.EyePosition,
                this.Emitter.TotalTime,
                this.MaximumAge,
                this.MaximumAgeVariation,
                this.VelocityAtEnd,
                this.Gravity,
                this.StartSize,
                this.EndSize,
                this.MinimumColor,
                this.MaximumColor,
                this.RotateSpeed,
                this.TextureCount,
                this.Texture);

            this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                this.Game.Graphics.DeviceContext.Draw(this.ActiveParticles, 0);

                Counters.DrawCallsPerFrame++;
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.ActiveParticles;
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

            Vector3 velocity = this.Emitter.Velocity * this.VelocitySensitivity;

            float horizontalVelocity = MathUtil.Lerp(
                this.HorizontalVelocity.X,
                this.HorizontalVelocity.Y,
                this.rnd.NextFloat(0, 1));

            double horizontalAngle = this.rnd.NextDouble() * MathUtil.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);
            velocity.Y += MathUtil.Lerp(this.VerticalVelocity.X, this.VerticalVelocity.Y, this.rnd.NextFloat(0, 1));

            Vector4 randomValues = this.rnd.NextVector4(Vector4.Zero, Vector4.One);

            this.particles[this.currentParticleIndex].Position = this.Emitter.Position;
            this.particles[this.currentParticleIndex].Velocity = velocity;
            this.particles[this.currentParticleIndex].RandomValues = randomValues;
            this.particles[this.currentParticleIndex].MaxAge = this.Emitter.TotalTime;

            this.Game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, this.particles);

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
