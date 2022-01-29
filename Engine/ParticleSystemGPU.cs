using SharpDX;
using SharpDX.Direct3D;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using SharpDX.Direct3D11;
    using System.Threading.Tasks;

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
        public static async Task<ParticleSystemGpu> Create(Game game, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            var pParameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new FileArrayImageContent(description.ContentPath, description.TextureName);
            var texture = await game.ResourceManager.RequestResource(imgContent);
            var textureCount = (uint)imgContent.Count;

            emitter.UpdateBounds(pParameters);
            int maxConcurrentParticles = emitter.GetMaximumConcurrentParticles(description.MaxDuration);
            float timeToEnd = emitter.Duration + pParameters.MaxDuration;

            ParticleSystemGpu res = new ParticleSystemGpu
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
            int size = (int)(MaxConcurrentParticles * (Emitter.Duration == 0 ? 60 : Emitter.Duration));

            return Math.Min(size, 5000);
        }
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

            int size = GetBufferSize();

            emittersBuffer = Game.Graphics.CreateBuffer<VertexGpuParticle>(Name, data, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
            drawingBuffer = Game.Graphics.CreateBuffer<VertexGpuParticle>(Name, size, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            streamOutBuffer = Game.Graphics.CreateBuffer<VertexGpuParticle>(Name, size, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            inputStride = default(VertexGpuParticle).GetStride();

            emitterBinding = new[] { new VertexBufferBinding(emittersBuffer, inputStride, 0) };
            drawingBinding = new[] { new VertexBufferBinding(drawingBuffer, inputStride, 0) };
            streamOutBinding = new[] { new StreamOutputBufferBinding(streamOutBuffer, 0) };

            var effect = DrawerPool.EffectDefaultGPUParticles;
            streamOutInputLayout = Game.Graphics.CreateInputLayout("EffectDefaultGPUParticles.ParticleStreamOut", effect.ParticleStreamOut.GetSignature(), VertexGpuParticle.Input(BufferSlot));
            rotatingInputLayout = Game.Graphics.CreateInputLayout("EffectDefaultGPUParticles.RotationDraw", effect.RotationDraw.GetSignature(), VertexGpuParticle.Input(BufferSlot));
            nonRotatingInputLayout = Game.Graphics.CreateInputLayout("EffectDefaultGPUParticles.NonRotationDraw", effect.NonRotationDraw.GetSignature(), VertexGpuParticle.Input(BufferSlot));
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
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="context">Context</param>
        public void Draw(DrawContext context)
        {
            bool isTransparent = parameters.BlendMode.HasFlag(BlendModes.Alpha) || parameters.BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(BlendModes.Default, isTransparent);
            if (!draw)
            {
                return;
            }

            var effect = DrawerPool.EffectDefaultGPUParticles;

            #region Per frame update

            var state = new EffectParticleState
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
                RotateSpeed = parameters.RotateSpeed,
            };

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                state,
                TextureCount,
                Texture);

            #endregion

            StreamOut(effect);

            ToggleBuffers();

            Draw(effect, context.DrawerMode);
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
        private void StreamOut(EffectDefaultGpuParticles effect)
        {
            var graphics = Game.Graphics;

            graphics.IAInputLayout = streamOutInputLayout;
            graphics.IASetVertexBuffers(BufferSlot, firstRun ? emitterBinding : drawingBinding);
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;
            graphics.SetDepthStencilNone();

            graphics.SetStreamOutputTargets(streamOutBinding);

            var techniqueForStreamOut = effect.ParticleStreamOut;

            for (int p = 0; p < techniqueForStreamOut.PassCount; p++)
            {
                graphics.EffectPassApply(techniqueForStreamOut, p, 0);

                if (firstRun)
                {
                    graphics.Draw(1, 0);

                    firstRun = false;
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
            var temp = drawingBuffer;
            drawingBuffer = streamOutBuffer;
            streamOutBuffer = temp;

            drawingBinding = new[] { new VertexBufferBinding(drawingBuffer, inputStride, 0) };
            streamOutBinding = new[] { new StreamOutputBufferBinding(streamOutBuffer, 0) };
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="effect">Effect for drawing</param>
        /// <param name="drawerMode">Drawe mode</param>
        private void Draw(EffectDefaultGpuParticles effect, DrawerModes drawerMode)
        {
            var rot = parameters.RotateSpeed != Vector2.Zero;

            var techniqueForDrawing = rot ? effect.RotationDraw : effect.NonRotationDraw;

            if (!drawerMode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
            }

            var graphics = Game.Graphics;

            graphics.IAInputLayout = rot ? rotatingInputLayout : nonRotatingInputLayout;
            graphics.IASetVertexBuffers(BufferSlot, drawingBinding);
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

            graphics.SetDepthStencilRDZEnabled();
            graphics.SetBlendState(parameters.BlendMode);

            for (int p = 0; p < techniqueForDrawing.PassCount; p++)
            {
                graphics.EffectPassApply(techniqueForDrawing, p, 0);

                graphics.DrawAuto();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {ActiveParticles}; Total: {Emitter.TotalTime:0.00}/{Emitter.Duration:0.00}; ToEnd: {TimeToEnd:0.00};";
        }
    }
}
