using SharpDX;
using SharpDX.DXGI;
using System;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using StreamOutputBufferBinding = SharpDX.Direct3D11.StreamOutputBufferBinding;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Particle system
    /// </summary>
    public class GPUParticleSystem : IDisposable
    {
        /// <summary>
        /// Random instance
        /// </summary>
        private Random rnd = new Random();
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
        /// Stride
        /// </summary>
        private int inputStride;

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
        /// Gets the maximum number of concurrent particles
        /// </summary>
        public int MaxConcurrentParticles { get; private set; }
        /// <summary>
        /// Total particle system time
        /// </summary>
        public float TotalTime { get; private set; }
        /// <summary>
        /// Elapsed time
        /// </summary>
        public float ElapsedTime { get; private set; }

        public bool Active = true;

        public bool FirstRun = true;

        /// <summary>
        /// Particle emitter
        /// </summary>
        public ParticleEmitter Emitter { get; private set; }
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
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle system description</param>
        public GPUParticleSystem(Game game, ParticleSystemDescription description, ParticleEmitter emitter)
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

            VertexGPUParticle[] data = Helper.CreateArray(1, new VertexGPUParticle()
            {
                Type = 0,
                EmissionTime = this.Emitter.Duration,
                EmissionRate = this.Emitter.EmissionRate,
                Energy = 0,
                Position = this.Emitter.Position,
                Velocity = this.Emitter.Velocity,
                Color = new Color4(),
            });

            this.emittersBuffer = game.Graphics.Device.CreateBuffer<VertexGPUParticle>(data, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
            this.drawingBuffer = game.Graphics.Device.CreateBuffer<VertexGPUParticle>(this.MaxConcurrentParticles, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.streamOutBuffer = game.Graphics.Device.CreateBuffer<VertexGPUParticle>(this.MaxConcurrentParticles, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.inputStride = default(VertexGPUParticle).Stride;
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.emittersBuffer);
            Helper.Dispose(this.drawingBuffer);
            Helper.Dispose(this.streamOutBuffer);
        }
        /// <summary>
        /// Updating
        /// </summary>
        /// <param name="context">Context</param>
        public void Update(UpdateContext context)
        {
            this.ElapsedTime = context.GameTime.ElapsedSeconds;
            this.TotalTime += this.ElapsedTime;
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="context">Context</param>
        public void Draw(DrawContext context)
        {
            var effect = DrawerPool.EffectGPUParticles;
            if (effect != null)
            {
                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                Counters.IAPrimitiveTopologySets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                #region Per frame update

                effect.UpdatePerFrame(
                    context.World,
                    context.ViewProjection,
                    context.EyePosition,
                    this.TotalTime,
                    this.ElapsedTime,
                    this.Emitter.EmissionRate,
                    this.VelocitySensitivity,
                    this.HorizontalVelocity,
                    this.VerticalVelocity,
                    new Color4(this.rnd.NextFloat(0,1),this.rnd.NextFloat(0,1),this.rnd.NextFloat(0,1),this.rnd.NextFloat(0,1)),
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

                #region Stream out

                {
                    var techniqueForStreamOut = effect.GetTechniqueForStreamOut(VertexTypes.GPUParticle);
                    
                    var inputLayout = effect.GetInputLayout(techniqueForStreamOut);
                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
                    Counters.IAInputLayoutSets++;

                    var iaBinding = new VertexBufferBinding(this.FirstRun ? this.emittersBuffer : this.drawingBuffer, this.inputStride, 0);
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
                    Counters.IAVertexBuffersSets++;

                    var soBinding = new StreamOutputBufferBinding(this.streamOutBuffer, 0);
                    this.Game.Graphics.DeviceContext.StreamOutput.SetTargets(new[] { soBinding });

                    for (int p = 0; p < techniqueForStreamOut.Description.PassCount; p++)
                    {
                        techniqueForStreamOut.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        if (this.FirstRun)
                        {
                            this.Game.Graphics.DeviceContext.Draw(1, 0);

                            this.FirstRun = false;
                        }
                        else
                        {
                            this.Game.Graphics.DeviceContext.DrawAuto();
                        }

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }

                    this.Game.Graphics.DeviceContext.StreamOutput.SetTargets(null);
                }

                #endregion

                this.ToggleBuffers();

                #region Draw

                {
                    var techniqueForDrawing = effect.GetTechniqueForDrawing(
                        VertexTypes.GPUParticle,
                        false,
                        DrawingStages.Drawing,
                        context.DrawerMode,
                        this.RotateSpeed != Vector2.Zero);

                    var inputLayout = effect.GetInputLayout(techniqueForDrawing);
                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
                    Counters.IAInputLayoutSets++;

                    this.Game.Graphics.SetDepthStencilRDZEnabled();

                    if (this.Transparent)
                    {
                        this.Game.Graphics.SetBlendDefaultAlpha();
                    }
                    else
                    {
                        this.Game.Graphics.SetBlendDefault();
                    }

                    var iaBinding = new VertexBufferBinding(this.drawingBuffer, this.inputStride, 0);
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
                    Counters.IAVertexBuffersSets++;

                    for (int p = 0; p < techniqueForDrawing.Description.PassCount; p++)
                    {
                        techniqueForDrawing.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.DrawAuto();

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Toggle stream out and drawing buffers
        /// </summary>
        private void ToggleBuffers()
        {
            var temp = this.drawingBuffer;
            this.drawingBuffer = this.streamOutBuffer;
            this.streamOutBuffer = temp;
        }
    }
}
