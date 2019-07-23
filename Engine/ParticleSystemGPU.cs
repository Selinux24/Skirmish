using SharpDX;
using SharpDX.Direct3D;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using SharpDX.Direct3D11;

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
        private readonly VertexBufferBinding[] emitterBinding;
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
        private readonly int inputStride;
        /// <summary>
        /// First run flag
        /// </summary>
        private bool firstRun = true;
        /// <summary>
        /// Random instance
        /// </summary>
        private readonly Random rnd = new Random();

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
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Name</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Emitter</param>
        public ParticleSystemGpu(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
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

            this.TimeToEnd = this.Emitter.Duration + this.Parameters.MaxDuration;

            var data = Helper.CreateArray(1, new VertexGpuParticle()
            {
                Position = this.Emitter.Position,
                Velocity = this.Emitter.Velocity,
                RandomValues = new Vector4(),
                MaxAge = 0,

                Type = 0,
                EmissionTime = this.Emitter.Duration,
            });

            int size = this.GetBufferSize();

            this.emittersBuffer = game.Graphics.CreateBuffer<VertexGpuParticle>(description.Name, data, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
            this.drawingBuffer = game.Graphics.CreateBuffer<VertexGpuParticle>(description.Name, size, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.streamOutBuffer = game.Graphics.CreateBuffer<VertexGpuParticle>(description.Name, size, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.inputStride = default(VertexGpuParticle).GetStride();

            this.emitterBinding = new[] { new VertexBufferBinding(this.emittersBuffer, this.inputStride, 0) };
            this.drawingBinding = new[] { new VertexBufferBinding(this.drawingBuffer, this.inputStride, 0) };
            this.streamOutBinding = new[] { new StreamOutputBufferBinding(this.streamOutBuffer, 0) };

            var effect = DrawerPool.EffectDefaultGPUParticles;
            this.streamOutInputLayout = game.Graphics.CreateInputLayout(effect.ParticleStreamOut.GetSignature(), VertexGpuParticle.Input(BufferSlot));
            this.rotatingInputLayout = game.Graphics.CreateInputLayout(effect.RotationDraw.GetSignature(), VertexGpuParticle.Input(BufferSlot));
            this.nonRotatingInputLayout = game.Graphics.CreateInputLayout(effect.NonRotationDraw.GetSignature(), VertexGpuParticle.Input(BufferSlot));
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
                if (emittersBuffer != null)
                {
                    emittersBuffer.Dispose();
                    emittersBuffer = null;
                }
                if (drawingBuffer != null)
                {
                    drawingBuffer.Dispose();
                    drawingBuffer = null;
                }
                if (streamOutBuffer != null)
                {
                    streamOutBuffer.Dispose();
                    streamOutBuffer = null;
                }
                if (streamOutInputLayout != null)
                {
                    streamOutInputLayout.Dispose();
                    streamOutInputLayout = null;
                }
                if (rotatingInputLayout != null)
                {
                    rotatingInputLayout.Dispose();
                    rotatingInputLayout = null;
                }
                if (nonRotatingInputLayout != null)
                {
                    nonRotatingInputLayout.Dispose();
                    nonRotatingInputLayout = null;
                }
            }
        }

        /// <summary>
        /// Estimates the size of the buffer based on the maximum number of particles, and the maximum duration of each particle
        /// </summary>
        /// <returns>Returns the estimated buffer size</returns>
        /// <remarks>The maximum size of the buffer is 5000</remarks>
        protected int GetBufferSize()
        {
            int size = (int)(this.MaxConcurrentParticles * (this.Emitter.Duration == 0 ? 60 : this.Emitter.Duration));

            return Math.Min(size, 5000);
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
            var drawerMode = context.DrawerMode;

            if ((drawerMode.HasFlag(DrawerModes.OpaqueOnly) && !this.Parameters.Transparent) ||
                (drawerMode.HasFlag(DrawerModes.TransparentOnly) && this.Parameters.Transparent))
            {
                var effect = DrawerPool.EffectDefaultGPUParticles;

                #region Per frame update

                var state = new EffectParticleState
                {
                    TotalTime = this.Emitter.TotalTime,
                    ElapsedTime = this.Emitter.ElapsedTime,
                    EmissionRate = this.Emitter.EmissionRate,
                    VelocitySensitivity = this.Parameters.EmitterVelocitySensitivity,
                    HorizontalVelocity = this.Parameters.HorizontalVelocity,
                    VerticalVelocity = this.Parameters.VerticalVelocity,
                    RandomValues = this.rnd.NextVector4(Vector4.Zero, Vector4.One),
                    MaxDuration = this.Parameters.MaxDuration,
                    MaxDurationRandomness = this.Parameters.MaxDurationRandomness,
                    EndVelocity = this.Parameters.EndVelocity,
                    Gravity = this.Parameters.Gravity,
                    StartSize = this.Parameters.StartSize,
                    EndSize = this.Parameters.EndSize,
                    MinColor = this.Parameters.MinColor,
                    MaxColor = this.Parameters.MaxColor,
                    RotateSpeed = this.Parameters.RotateSpeed,
                };

                effect.UpdatePerFrame(
                    context.ViewProjection,
                    context.EyePosition,
                    state,
                    this.TextureCount,
                    this.Texture);

                #endregion

                this.StreamOut(effect);

                this.ToggleBuffers();

                this.Draw(effect, context.DrawerMode);
            }
        }

        /// <summary>
        /// Stream output
        /// </summary>
        /// <param name="effect">Effect for stream out</param>
        private void StreamOut(EffectDefaultGpuParticles effect)
        {
            var graphics = this.Game.Graphics;

            graphics.IAInputLayout = this.streamOutInputLayout;
            graphics.IASetVertexBuffers(BufferSlot, this.firstRun ? this.emitterBinding : this.drawingBinding);
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;
            graphics.SetDepthStencilNone();

            graphics.SetStreamOutputTargets(this.streamOutBinding);

            var techniqueForStreamOut = effect.ParticleStreamOut;

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
        private void Draw(EffectDefaultGpuParticles effect, DrawerModes drawerMode)
        {
            var rot = this.Parameters.RotateSpeed != Vector2.Zero;

            var techniqueForDrawing = rot ? effect.RotationDraw : effect.NonRotationDraw;

            if (!drawerMode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
            }

            var graphics = this.Game.Graphics;

            graphics.IAInputLayout = rot ? this.rotatingInputLayout : this.nonRotatingInputLayout;
            graphics.IASetVertexBuffers(BufferSlot, this.drawingBinding);
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
