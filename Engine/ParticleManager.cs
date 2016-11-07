using SharpDX;
using System;
using System.Collections.Generic;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Helpers;
    using Engine.Effects;

    public class CPUParticleManagerDescription : DrawableDescription
    {

    }

    public class CPUParticleManager : Drawable
    {
        private List<CPUParticleGenerator> particleGenerators = new List<CPUParticleGenerator>();
        private List<CPUParticleGenerator> toDelete = new List<CPUParticleGenerator>();

        public CPUParticleManager(Game game, CPUParticleManagerDescription description)
            : base(game, description)
        {

        }
        public override void Dispose()
        {
            Helper.Dispose(this.particleGenerators);
        }

        public override void Update(UpdateContext context)
        {
            float elapsed = context.GameTime.ElapsedSeconds;

            if (this.particleGenerators != null && this.particleGenerators.Count > 0)
            {
                toDelete.Clear();

                foreach (CPUParticleGenerator generator in this.particleGenerators)
                {
                    generator.ParticleSystem.TotalTime += elapsed;
                    generator.AddParticle();

                    generator.Duration -= elapsed;

                    if (generator.Duration <= 0)
                    {
                        toDelete.Add(generator);
                    }
                }

                if (toDelete.Count > 0)
                {
                    foreach (CPUParticleGenerator generator in toDelete)
                    {
                        this.particleGenerators.Remove(generator);
                    }
                }
            }
        }
        public override void Draw(DrawContext context)
        {
            if (this.particleGenerators != null && this.particleGenerators.Count > 0)
            {
                var effect = DrawerPool.EffectCPUParticles;
                if (effect != null)
                {
                    var technique = effect.GetTechnique(VertexTypes.Particle, false, DrawingStages.Drawing, context.DrawerMode);

                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    Counters.IAInputLayoutSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;

                    if (context.DrawerMode == DrawerModesEnum.Forward) this.Game.Graphics.SetBlendDefault();
                    else if (context.DrawerMode == DrawerModesEnum.Deferred) this.Game.Graphics.SetBlendDeferredComposer();

                    foreach (var generator in this.particleGenerators)
                    {
                        #region Per frame update

                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            effect.UpdatePerFrame(
                                context.World,
                                context.ViewProjection,
                                context.EyePosition,
                                generator.ParticleSystem.TotalTime,
                                generator.ParticleSystem.TextureCount,
                                generator.ParticleSystem.Texture);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            effect.UpdatePerFrame(
                                context.World,
                                context.ViewProjection,
                                context.EyePosition,
                                generator.ParticleSystem.TotalTime,
                                generator.ParticleSystem.TextureCount,
                                generator.ParticleSystem.Texture);
                        }

                        #endregion

                        effect.UpdatePerEmitter(
                            this.Game.Graphics.Viewport.Height,
                            (float)generator.ParticleSystem.Duration.TotalSeconds,
                            generator.ParticleSystem.DurationRandomness,
                            generator.ParticleSystem.EndVelocity,
                            generator.ParticleSystem.Gravity,
                            generator.ParticleSystem.StartSize,
                            generator.ParticleSystem.EndSize,
                            generator.ParticleSystem.MinColor,
                            generator.ParticleSystem.MaxColor,
                            generator.ParticleSystem.RotateSpeed);

                        generator.ParticleSystem.SetBuffer(this.Game);

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            this.Game.Graphics.DeviceContext.Draw(generator.ParticleSystem.VertexCount, 0);

                            Counters.DrawCallsPerFrame++;
                            Counters.InstancesPerFrame++;
                            Counters.TrianglesPerFrame += generator.ParticleSystem.VertexCount / 3;
                        }
                    }
                }

            }
        }

        public void AddParticleGenerator(CPUParticleSystemDescription description, float duration, Vector3 position, Vector3 velocity)
        {
            this.particleGenerators.Add(new CPUParticleGenerator(this.Game, description, duration, position, velocity));
        }
    }

    public enum CPUParticleSystemTypes
    {
        /// <summary>
        /// Sin especificar
        /// </summary>
        None,
        /// <summary>
        /// Polvo
        /// </summary>
        Dust,
        /// <summary>
        /// Explosión
        /// </summary>
        Explosion,
        /// <summary>
        /// Explosión con humo
        /// </summary>
        ExplosionSmoke,
        /// <summary>
        /// Fuego
        /// </summary>
        Fire,
        /// <summary>
        /// Motor de plasma
        /// </summary>
        PlasmaEngine,
        /// <summary>
        /// Traza de proyectil
        /// </summary>
        ProjectileTrail,
        /// <summary>
        /// Humo de motores
        /// </summary>
        SmokeEngine,
        /// <summary>
        /// Humo de incendio
        /// </summary>
        SmokePlume,
    }

    public class CPUParticleSystemDescription : DrawableDescription
    {
        public static CPUParticleSystemDescription InitializeDust(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 2;

            settings.Gravity = new Vector3(-0.15f, -0.15f, 0);

            settings.EndVelocity = 0.1f;

            settings.MinColor = Color.SandyBrown;
            settings.MaxColor = Color.SandyBrown;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 10;

            return settings;
        }
        public static CPUParticleSystemDescription InitializeExplosion(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 20;
            settings.MaxHorizontalVelocity = 30;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 20;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 200;

            settings.Transparent = true;

            return settings;
        }
        public static CPUParticleSystemDescription InitializeExplosionSmoke(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 50;

            settings.Gravity = new Vector3(0, -20, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 200;

            return settings;
        }
        public static CPUParticleSystemDescription InitializeFire(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 40;

            settings.Transparent = true;

            return settings;
        }
        public static CPUParticleSystemDescription InitializePlasmaEngine(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(0.5f);

            settings.DurationRandomness = 0f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = Color.AliceBlue;
            settings.MaxColor = Color.LightBlue;

            settings.MinStartSize = 1f;
            settings.MaxStartSize = 1f;

            settings.MinEndSize = 0.1f;
            settings.MaxEndSize = 0.1f;

            settings.Transparent = true;

            return settings;
        }
        public static CPUParticleSystemDescription InitializeProjectileTrail(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 250;

            settings.Duration = TimeSpan.FromSeconds(0.5f);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.MinColor = Color.Gray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = 1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 1f;

            settings.MinEndSize = 1f;
            settings.MaxEndSize = 2f;

            return settings;
        }
        public static CPUParticleSystemDescription InitializeSmokeEngine(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 2;

            settings.Gravity = new Vector3(-1, -1, 0);

            settings.EndVelocity = 0.15f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 4;

            return settings;
        }
        public static CPUParticleSystemDescription InitializeSmokePlume(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxParticles = 5000;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 20;

            settings.Gravity = new Vector3(-20, -5, 0);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 50;
            settings.MaxEndSize = 200;

            return settings;
        }

        public CPUParticleSystemTypes ParticleType { get; set; }

        public int MaxParticles { get; set; }

        public string ContentPath { get; set; }
        public string TextureName { get; set; }

        public TimeSpan Duration { get; set; }
        public float DurationRandomness { get; set; }

        public float MaxHorizontalVelocity { get; set; }
        public float MinHorizontalVelocity { get; set; }

        public float MaxVerticalVelocity { get; set; }
        public float MinVerticalVelocity { get; set; }

        public Vector3 Gravity { get; set; }

        public float EndVelocity { get; set; }

        public Color MinColor { get; set; }
        public Color MaxColor { get; set; }

        public float MinRotateSpeed { get; set; }
        public float MaxRotateSpeed { get; set; }

        public float MinStartSize { get; set; }
        public float MaxStartSize { get; set; }

        public float MinEndSize { get; set; }
        public float MaxEndSize { get; set; }

        public bool Transparent { get; set; }

        public float EmitterVelocitySensitivity { get; set; }
    }

    public class CPUParticleSystem : IDisposable
    {
        protected CPUParticleSystemDescription Settings = new CPUParticleSystemDescription();

        private VertexCPUParticle[] particles;
        private Buffer vertexBuffer;
        private VertexBufferBinding vertexBufferBinding;

        private int firstActiveParticle;
        private int firstNewParticle;
        private int firstFreeParticle;
        private int firstRetiredParticle;
        private Random rnd = new Random();

        public CPUParticleSystemTypes ParticleType
        {
            get
            {
                return this.Settings.ParticleType;
            }
        }
        public float TotalTime;
        public ShaderResourceView Texture;
        public uint TextureCount;
        public int VertexCount
        {
            get
            {
                return this.particles.Length;
            }
        }

        public TimeSpan Duration { get { return this.Settings.Duration; } }
        public float DurationRandomness { get { return this.Settings.DurationRandomness; } }
        public float EndVelocity { get { return this.Settings.EndVelocity; } }
        public Vector3 Gravity { get { return this.Settings.Gravity; } }
        public Vector2 StartSize { get { return new Vector2(this.Settings.MinStartSize, this.Settings.MaxStartSize); } }
        public Vector2 EndSize { get { return new Vector2(this.Settings.MinEndSize, this.Settings.MaxEndSize); } }
        public Color MinColor { get { return this.Settings.MinColor; } }
        public Color MaxColor { get { return this.Settings.MaxColor; } }
        public Vector2 RotateSpeed { get { return new Vector2(this.Settings.MinRotateSpeed, this.Settings.MaxRotateSpeed); } }

        public CPUParticleSystem(Game game, CPUParticleSystemDescription description)
        {
            this.Settings = description;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.CreateResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            int inputStride = default(VertexCPUParticle).Stride;
            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, inputStride, 0);

            this.particles = new VertexCPUParticle[description.MaxParticles];
            this.vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(this.particles);
        }
        public void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
        }

        private void RetireActiveParticles()
        {
            float particleDuration = (float)this.Settings.Duration.TotalSeconds;

            while (this.firstActiveParticle != this.firstNewParticle)
            {
                float particleAge = this.TotalTime - this.particles[this.firstActiveParticle].Energy;

                if (particleAge < particleDuration)
                {
                    break;
                }

                this.particles[this.firstActiveParticle].Energy = this.TotalTime;

                this.firstActiveParticle++;

                if (this.firstActiveParticle >= this.particles.Length)
                {
                    this.firstActiveParticle = 0;
                }
            }
        }
        private void FreeRetiredParticles()
        {
            while (this.firstRetiredParticle != this.firstActiveParticle)
            {
                float age = this.TotalTime - (int)this.particles[this.firstRetiredParticle].Energy;

                if (age < 3)
                {
                    break;
                }

                this.firstRetiredParticle++;

                if (this.firstRetiredParticle >= this.particles.Length)
                {
                    this.firstRetiredParticle = 0;
                }
            }
        }

        public void SetBuffer(Game game)
        {
            game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, this.particles);

            game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
        }

        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            int nextFreeParticle = this.firstFreeParticle + 1;

            if (nextFreeParticle >= this.particles.Length)
            {
                nextFreeParticle = 0;
            }

            if (nextFreeParticle == this.firstRetiredParticle)
            {
                return;
            }

            velocity *= this.Settings.EmitterVelocitySensitivity;

            float horizontalVelocity = MathUtil.Lerp(
                this.Settings.MinHorizontalVelocity,
                this.Settings.MaxHorizontalVelocity,
                this.rnd.NextFloat(0, 1));

            double horizontalAngle = this.rnd.NextDouble() * MathUtil.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            velocity.Y += MathUtil.Lerp(
                this.Settings.MinVerticalVelocity,
                this.Settings.MaxVerticalVelocity,
                this.rnd.NextFloat(0, 1));

            Color randomValues = new Color(
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1));

            this.particles[this.firstFreeParticle].Position = position;
            this.particles[this.firstFreeParticle].Velocity = velocity;
            this.particles[this.firstFreeParticle].Color = randomValues;
            this.particles[this.firstFreeParticle].Energy = this.TotalTime;

            this.firstFreeParticle = nextFreeParticle;
        }
    }

    public class CPUParticleGenerator : IDisposable
    {
        public CPUParticleSystem ParticleSystem { get; set; }
        public float Duration { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public CPUParticleGenerator(Game game, CPUParticleSystemDescription settings, float duration, Vector3 position, Vector3 velocity)
        {
            this.ParticleSystem = new CPUParticleSystem(game, settings);
            this.Duration = duration;
            this.Position = position;
            this.Velocity = velocity;
        }
        public void Dispose()
        {
            Helper.Dispose(this.ParticleSystem);
        }

        public void AddParticle()
        {
            this.ParticleSystem.AddParticle(this.Position, this.Velocity);
        }
    }
}
