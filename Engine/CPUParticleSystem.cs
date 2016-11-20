using SharpDX;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
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
    public class CPUParticleSystem : IDisposable
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
        public ShaderResourceView Texture;
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount;
        /// <summary>
        /// Active particle count
        /// </summary>
        public int ActiveParticles { get; private set; }
        /// <summary>
        /// Total particle system time
        /// </summary>
        public float TotalTime { get; private set; }
        /// <summary>
        /// Time to end
        /// </summary>
        public float TimeToEnd { get; private set; }

        /// <summary>
        /// Emitter position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Emitter velocity vector
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// Emitter duration
        /// </summary>
        public float Duration { get; private set; }
        /// <summary>
        /// Emitter rate interval
        /// </summary>
        /// <remarks>Particles per second</remarks>
        public float EmissionRate { get; private set; }
        /// <summary>
        /// Gets if the current particle system is active
        /// </summary>
        public bool Active
        {
            get
            {
                return this.Duration <= 0 && this.TimeToEnd <= 0;
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
        /// Emitter velocity sensitivity
        /// </summary>
        public float EmitterVelocitySensitivity { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle system description</param>
        /// <param name="position">Initial position</param>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="duration">Total duration</param>
        /// <param name="emissionRate">Emission rate (particles per second)</param>
        public CPUParticleSystem(Game game, CPUParticleSystemDescription description, Vector3 position, Vector3 velocity, float duration, float emissionRate)
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
            this.EmitterVelocitySensitivity = description.EmitterVelocitySensitivity;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.CreateResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            this.Duration = duration;
            this.EmissionRate = emissionRate;
            this.Position = position;
            this.Velocity = velocity;

            float maxActiveParticles = description.MaxDuration * (1f / this.EmissionRate);
            maxActiveParticles = maxActiveParticles != (int)maxActiveParticles ? maxActiveParticles + 1 : maxActiveParticles;
            this.particles = new VertexCPUParticle[(int)maxActiveParticles];

            this.vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(this.particles);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, default(VertexCPUParticle).Stride, 0),
            };
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
            float elapsed = context.GameTime.ElapsedSeconds;

            this.Duration -= elapsed;

            if (this.Duration > 0 && this.timeToNextParticle <= 0)
            {
                this.timeToNextParticle = this.EmissionRate;

                this.AddParticle();
            }

            this.timeToNextParticle -= elapsed;

            this.TotalTime += elapsed;
            this.TimeToEnd -= elapsed;
        }
        /// <summary>
        /// Draw particles
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="effect">Effect</param>
        /// <param name="technique">Technique</param>
        public void Draw(DrawContext context, EffectCPUParticles effect, EffectTechnique technique)
        {
            this.Game.Graphics.SetDepthStencilRDZEnabled();
            this.Game.Graphics.SetBlendDefaultAlpha();

            effect.UpdatePerFrame(
                context.World,
                context.ViewProjection,
                this.Game.Graphics.Viewport.Height,
                context.EyePosition,
                this.TotalTime,
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
                Counters.TrianglesPerFrame += 2 * 1;
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

            Vector3 velocity = this.Velocity * this.EmitterVelocitySensitivity;

            float horizontalVelocity = MathUtil.Lerp(
                this.HorizontalVelocity.X,
                this.HorizontalVelocity.Y,
                this.rnd.NextFloat(0, 1));

            double horizontalAngle = this.rnd.NextDouble() * MathUtil.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);
            velocity.Y += MathUtil.Lerp(this.VerticalVelocity.X, this.VerticalVelocity.Y, this.rnd.NextFloat(0, 1));

            Color randomValues = new Color(
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1));

            this.particles[this.currentParticleIndex].Position = this.Position;
            this.particles[this.currentParticleIndex].Velocity = velocity;
            this.particles[this.currentParticleIndex].Color = randomValues;
            this.particles[this.currentParticleIndex].MaxAge = this.TotalTime;

            this.Game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, this.particles);

            this.currentParticleIndex = nextFreeParticle;
            this.TimeToEnd = Math.Max(this.TimeToEnd, this.MaximumAge);
        }

        /// <summary>
        /// Gets the text representation of the particle system
        /// </summary>
        /// <returns>Returns the text representation of the particle system</returns>
        public override string ToString()
        {
            return string.Format("Index: {0}; Count: {1}; Total: {2:0.00}/{3:0.00}; ToEnd: {4:0.00};", 
                this.currentParticleIndex, 
                this.ActiveParticles, 
                this.TotalTime, 
                this.Duration,
                this.TimeToEnd);
        }
    }
}
