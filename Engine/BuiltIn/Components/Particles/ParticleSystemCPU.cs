using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Particles;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;

namespace Engine.BuiltIn.Components.Particles
{
    /// <summary>
    /// CPU particle system
    /// </summary>
    public class ParticleSystemCpu : IParticleSystem<ParticleEmitter, ParticleSystemParams>
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
        private EngineVertexBuffer<VertexCpuParticle> buffer;
        /// <summary>
        /// Update particles flag
        /// </summary>
        private bool updateParticles = false;
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
        /// Drawer
        /// </summary>
        private BuiltInParticles particleDrawer;

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
        public int ActiveParticles { get; private set; }
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
        /// <param name="scene">Scene</param>
        /// <param name="name">System name</param>
        /// <param name="description">System description</param>
        /// <param name="emitter">Emitter</param>
        public static ParticleSystemCpu Create(Scene scene, string name, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            var pParameters = new ParticleSystemParams(description) * emitter.Scale;

            var imgContent = new FileArrayImageContent(description.ContentPath, description.TextureName);
            var texture = scene.Game.ResourceManager.RequestResource(imgContent);
            var textureCount = (uint)imgContent.Count;

            emitter.UpdateBounds(pParameters);
            int maxConcurrentParticles = emitter.GetMaximumConcurrentParticles(description.MaxDuration);
            var vParticles = new VertexCpuParticle[maxConcurrentParticles];
            float timeToEnd = emitter.Duration + pParameters.MaxDuration;

            var pBuffer = new EngineVertexBuffer<VertexCpuParticle>(scene.Game.Graphics, description.Name, vParticles, VertexBufferParams.Dynamic);

            var drawer = BuiltInShaders.GetDrawer<BuiltInParticles>();
            var signature = drawer.GetVertexShader().Shader.GetShaderBytecode();
            pBuffer.CreateInputLayout(nameof(ParticlesPs), signature, BufferSlot);

            return new ParticleSystemCpu
            {
                Scene = scene,
                Name = name,

                parameters = pParameters,
                particleDrawer = drawer,

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
                buffer?.Dispose();
                buffer = null;
            }
        }

        /// <inheritdoc/>
        public void Update(UpdateContext context)
        {
            Emitter.Update(context.GameTime, Scene.Camera.Position);

            if (Emitter.Active && timeToNextParticle <= 0)
            {
                AddParticle();

                timeToNextParticle = Emitter.EmissionRate;
            }

            timeToNextParticle -= Emitter.ElapsedTime;
            TimeToEnd -= Emitter.ElapsedTime;
        }
        /// <inheritdoc/>
        public bool Draw(DrawContext context)
        {
            if (ActiveParticles <= 0)
            {
                return false;
            }

            bool isTransparent = parameters.BlendMode.HasFlag(BlendModes.Alpha) || parameters.BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(parameters.BlendMode, isTransparent);
            if (!draw)
            {
                return false;
            }

            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            if (updateParticles)
            {
                Logger.WriteTrace(this, $"{Name} - updateParticles WriteDiscardBuffer");
                buffer.Write(dc, particles);
                updateParticles = false;
            }

            dc.SetDepthStencilState(graphics.GetDepthStencilRDZEnabled());
            dc.SetBlendState(graphics.GetBlendState(context.DrawerMode, parameters.BlendMode));

            var useRotation = parameters.RotateSpeed != Vector2.Zero;
            var state = new BuiltInParticlesState
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
                UseRotation = useRotation,
                RotateSpeed = parameters.RotateSpeed,
            };

            particleDrawer.Update(dc, state, TextureCount, Texture);

            return particleDrawer.Draw(context.DeviceContext, buffer, Topology.PointList, ActiveParticles);
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

            updateParticles = true;

            currentParticleIndex = nextFreeParticle;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {ActiveParticles}; Total: {Emitter.TotalTime:0.00}/{Emitter.Duration:0.00}; ToEnd: {TimeToEnd:0.00};";
        }
    }
}
