using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Particle system
    /// </summary>
    public class ParticleSystemGPU : IParticleSystem
    {
        public static int BufferSlot = 13;

        /// <summary>
        /// Emitter initialization buffer
        /// </summary>
        private Buffer emittersBuffer;
        /// <summary>
        /// Drawing buffer
        /// </summary>
        private Buffer drawingBuffer;
        /// <summary>
        /// Stream out buffer
        /// </summary>
        private Buffer streamOutBuffer;
        /// <summary>
        /// Buffer binding for emitter buffer
        /// </summary>
        private VertexBufferBinding[] emitterBinding;
        /// <summary>
        /// Buffer binding for drawing buffer
        /// </summary>
        private VertexBufferBinding[] drawingBinding;
        /// <summary>
        /// Buffer binding for stream output buffer
        /// </summary>
        private StreamOutputBufferBinding[] streamOutBinding;
        /// <summary>
        /// Input layout for stream out
        /// </summary>
        private InputLayout streamOutInputLayout;
        /// <summary>
        /// Input layout for rotating particles
        /// </summary>
        private InputLayout rotatingInputLayout;
        /// <summary>
        /// Input layout for non rotating particles
        /// </summary>
        private InputLayout nonRotatingInputLayout;
        /// <summary>
        /// Vertex input stride
        /// </summary>
        private int inputStride;
        /// <summary>
        /// First run flag
        /// </summary>
        private bool firstRun = true;
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
        public float VelocitySensitivity { get; private set; }
        /// <summary>
        /// Trasparent particles
        /// </summary>
        public bool Transparent { get; private set; }
        /// <summary>
        /// Additive particles
        /// </summary>
        public bool Additive { get; private set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Name</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Emitter</param>
        public ParticleSystemGPU(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            this.Game = game;
            this.Name = name;

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
            this.Additive = description.Additive;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.CreateResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            this.Emitter = emitter;
            this.Emitter.SetBoundingBox(ParticleEmitter.GenerateBBox(description.MaxDuration, this.EndSize, this.HorizontalVelocity, this.VerticalVelocity));
            this.MaxConcurrentParticles = this.Emitter.GetMaximumConcurrentParticles(description.MaxDuration);

            this.TimeToEnd = this.Emitter.Duration + this.MaximumAge;

            VertexGPUParticle[] data = Helper.CreateArray(1, new VertexGPUParticle()
            {
                Position = this.Emitter.Position,
                Velocity = this.Emitter.Velocity,
                RandomValues = new Vector4(),
                MaxAge = 0,

                Type = 0,
                EmissionTime = this.Emitter.Duration,
            });

            //TODO: should be another method for this
            int size = (int)(this.MaxConcurrentParticles * (this.Emitter.Duration == 0 ? 60 : this.Emitter.Duration));
            size = Math.Min(size, 5000);

            this.emittersBuffer = game.Graphics.CreateBuffer<VertexGPUParticle>(description.Name, data, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
            this.drawingBuffer = game.Graphics.CreateBuffer<VertexGPUParticle>(description.Name, size, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.streamOutBuffer = game.Graphics.CreateBuffer<VertexGPUParticle>(description.Name, size, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.inputStride = default(VertexGPUParticle).GetStride();

            this.emitterBinding = new[] { new VertexBufferBinding(this.emittersBuffer, this.inputStride, 0) };
            this.drawingBinding = new[] { new VertexBufferBinding(this.drawingBuffer, this.inputStride, 0) };
            this.streamOutBinding = new[] { new StreamOutputBufferBinding(this.streamOutBuffer, 0) };

            this.streamOutInputLayout = game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultGPUParticles.ParticleStreamOut.GetSignature(), VertexGPUParticle.Input(BufferSlot));
            this.rotatingInputLayout = game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultGPUParticles.RotationDraw.GetSignature(), VertexGPUParticle.Input(BufferSlot));
            this.nonRotatingInputLayout = game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultGPUParticles.NonRotationDraw.GetSignature(), VertexGPUParticle.Input(BufferSlot));
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.emittersBuffer);
            Helper.Dispose(this.drawingBuffer);
            Helper.Dispose(this.streamOutBuffer);
            Helper.Dispose(this.streamOutInputLayout);
            Helper.Dispose(this.rotatingInputLayout);
            Helper.Dispose(this.nonRotatingInputLayout);
        }

        /// <summary>
        /// Updating
        /// </summary>
        /// <param name="context">Context</param>
        public void Update(UpdateContext context)
        {
            this.Emitter.Update(context);

            this.TimeToEnd -= this.Emitter.ElapsedTime;
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="context">Context</param>
        public void Draw(DrawContext context)
        {
            var effect = DrawerPool.EffectDefaultGPUParticles;

            #region Per frame update

            effect.UpdatePerFrame(
                context.World,
                context.ViewProjection,
                context.EyePosition,
                this.Emitter.TotalTime,
                this.Emitter.ElapsedTime,
                this.Emitter.EmissionRate,
                this.VelocitySensitivity,
                this.HorizontalVelocity,
                this.VerticalVelocity,
                this.rnd.NextVector4(Vector4.Zero, Vector4.One),
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

            #endregion

            this.StreamOut(effect);

            this.ToggleBuffers();

            this.Draw(effect, context.DrawerMode);
        }

        /// <summary>
        /// Stream output
        /// </summary>
        /// <param name="effect">Effect for stream out</param>
        private void StreamOut(EffectDefaultGPUParticles effect)
        {
            var graphics = this.Game.Graphics;

            var techniqueForStreamOut = effect.GetTechniqueForStreamOut(VertexTypes.GPUParticle);

            graphics.IAInputLayout = this.streamOutInputLayout;
            graphics.IASetVertexBuffers(BufferSlot, this.firstRun ? this.emitterBinding : this.drawingBinding);
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

            graphics.SetDepthStencilNone();

            graphics.SetStreamOutputTargets(this.streamOutBinding);

            for (int p = 0; p < techniqueForStreamOut.PassCount; p++)
            {
                graphics.EffectPassApply(techniqueForStreamOut, p, 0);

                if (this.firstRun)
                {
                    graphics.Draw(1, 0);

                    this.firstRun = false;
                }
                else
                {
                    graphics.DrawAuto();
                }
            }

            graphics.SetStreamOutputTargets(null);
        }
        /// <summary>
        /// Toggle stream out and drawing buffers
        /// </summary>
        private void ToggleBuffers()
        {
            var temp = this.drawingBuffer;
            this.drawingBuffer = this.streamOutBuffer;
            this.streamOutBuffer = temp;

            this.drawingBinding = new[] { new VertexBufferBinding(this.drawingBuffer, this.inputStride, 0) };
            this.streamOutBinding = new[] { new StreamOutputBufferBinding(this.streamOutBuffer, 0) };
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="effect">Effect for drawing</param>
        /// <param name="drawerMode">Drawe mode</param>
        private void Draw(EffectDefaultGPUParticles effect, DrawerModesEnum drawerMode)
        {
            var graphics = this.Game.Graphics;

            if (drawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame++;
            }

            var rot = this.RotateSpeed != Vector2.Zero;

            var techniqueForDrawing = effect.GetTechniqueForDrawing(
                VertexTypes.GPUParticle,
                false,
                DrawingStages.Drawing,
                drawerMode,
                rot);

            graphics.IAInputLayout = rot ? this.rotatingInputLayout : this.nonRotatingInputLayout;
            graphics.IASetVertexBuffers(BufferSlot, this.drawingBinding);
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

            graphics.SetDepthStencilRDZEnabled();

            if (this.Additive)
            {
                graphics.SetBlendAdditive();
            }
            else if (this.Transparent)
            {
                graphics.SetBlendDefaultAlpha();
            }
            else
            {
                graphics.SetBlendDefault();
            }

            for (int p = 0; p < techniqueForDrawing.PassCount; p++)
            {
                graphics.EffectPassApply(techniqueForDrawing, p, 0);

                graphics.DrawAuto();
            }
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
