using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputLayout = SharpDX.Direct3D11.InputLayout;
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
    public class ParticleSystem : Drawable
    {
        /// <summary>
        /// Particle state types
        /// </summary>
        public enum ParticleTypes : uint
        {
            /// <summary>
            /// Emitter
            /// </summary>
            Emitter = 0,
            /// <summary>
            /// Flare
            /// </summary>
            Flare = 1,
        }

        /// <summary>
        /// First stream out flag
        /// </summary>
        private bool firstRun = true;

        /// <summary>
        /// Particle class
        /// </summary>
        private ParticleClasses particleClass = ParticleClasses.Unknown;
        /// <summary>
        /// Initial particles
        /// </summary>
        private int initialParticles = 0;
        /// <summary>
        /// Maximum particle count
        /// </summary>
        private int maximumParticles = 0;
        /// <summary>
        /// Maximum particle age
        /// </summary>
        private float maximumAge;
        /// <summary>
        /// Age of particle to emit new flares
        /// </summary>
        private float emitterAge;
        /// <summary>
        /// Acceleration vector
        /// </summary>
        private Vector3 particleAcceleration;
        /// <summary>
        /// Total game time
        /// </summary>
        private float totalTime = 0f;
        /// <summary>
        /// Elapsed game time
        /// </summary>
        private float elapsedTime = 0f;
        /// <summary>
        /// Local system transform
        /// </summary>
        private Matrix local = Matrix.Identity;

        /// <summary>
        /// Input layout
        /// </summary>
        private InputLayout inputLayout = null;
        /// <summary>
        /// Selected stream out technique
        /// </summary>
        private EffectTechnique techniqueForStreamOut = null;

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
        /// Textures
        /// </summary>
        private ShaderResourceView textureArray;
        /// <summary>
        /// Random texture
        /// </summary>
        private ShaderResourceView textureRandom;

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle system description</param>
        public ParticleSystem(Game game, ParticleSystemDescription description)
            : base(game, description)
        {
            this.particleClass = description.ParticleClass;
            this.maximumParticles = description.MaximumParticles;
            this.maximumAge = description.MaximumAge;
            this.emitterAge = description.EmitterAge;
            this.particleAcceleration = description.Acceleration;

            VertexParticle[] data = null;

            if (description.EmitterType == ParticleSystemDescription.EmitterTypes.FixedPosition)
            {
                this.initialParticles = description.Emitters.Length;

                data = new VertexParticle[this.initialParticles];

                for (int i = 0; i < this.initialParticles; i++)
                {
                    var emitter = description.Emitters[i];

                    data[i] = new VertexParticle
                    {
                        Position = emitter.Position,
                        Velocity = description.ParticleClass == ParticleClasses.Rain ? description.Acceleration : description.Acceleration * emitter.Size,
                        Size = new Vector2(emitter.Size),
                        Color = emitter.Color,
                        Age = 0,
                        Type = (uint)ParticleTypes.Emitter,
                    };
                }
            }
            else if (description.EmitterType == ParticleSystemDescription.EmitterTypes.FromCamera)
            {
                this.initialParticles = 1;

                var emitter = description.Emitters[0];

                VertexParticle p = new VertexParticle
                {
                    Position = Vector3.Zero, //HACK: must be camera position?
                    Velocity = description.ParticleClass == ParticleClasses.Rain ? description.Acceleration : description.Acceleration * emitter.Size,
                    Size = new Vector2(emitter.Size),
                    Color = emitter.Color,
                    Age = 0,
                    Type = (uint)ParticleTypes.Emitter,
                };

                data = new[] { p };
            }
            else
            {
                throw new Exception(string.Format("Bad emitter type: {0}", description.EmitterType));
            }

            this.techniqueForStreamOut = DrawerPool.EffectParticles.GetTechniqueForStreamOut(VertexTypes.Particle, description.ParticleClass);
            this.inputLayout = DrawerPool.EffectParticles.GetInputLayout(this.techniqueForStreamOut);

            this.emittersBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(data, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
            this.drawingBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(this.maximumParticles, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.streamOutBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(this.maximumParticles, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.inputStride = default(VertexParticle).Stride;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Textures),
            };

            this.textureArray = game.ResourceManager.CreateResource(imgContent);
            this.textureRandom = game.ResourceManager.CreateRandomTexture(Guid.NewGuid(), 1024);

            this.Manipulator = new Manipulator3D();
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.emittersBuffer);
            Helper.Dispose(this.drawingBuffer);
            Helper.Dispose(this.streamOutBuffer);
        }
        /// <summary>
        /// Updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            this.local = context.World * this.Manipulator.LocalTransform;
            this.totalTime = context.GameTime.TotalSeconds;
            this.elapsedTime = context.GameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            this.Game.Graphics.DeviceContext.OutputMerger.BlendState = null;
            this.Game.Graphics.DeviceContext.OutputMerger.BlendFactor = Color.Zero;
            this.Game.Graphics.DeviceContext.OutputMerger.BlendSampleMask = -1;

            this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
            Counters.IAInputLayoutSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            Counters.IAPrimitiveTopologySets++;
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;

            #region Per frame update

            DrawerPool.EffectParticles.UpdatePerFrame(
                this.local,
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                (uint)this.textureArray.Description.Texture2DArray.ArraySize,
                this.textureArray,
                this.textureRandom,
                this.emitterAge,
                this.maximumAge,
                this.totalTime,
                this.elapsedTime,
                this.particleAcceleration);

            #endregion

            #region Stream out

            {
                VertexBufferBinding iaBinding = new VertexBufferBinding(this.firstRun ? this.emittersBuffer : this.drawingBuffer, this.inputStride, 0);
                StreamOutputBufferBinding soBinding = new StreamOutputBufferBinding(this.streamOutBuffer, 0);

                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
                Counters.IAVertexBuffersSets++;
                this.Game.Graphics.DeviceContext.StreamOutput.SetTargets(new[] { soBinding });

                this.Game.Graphics.SetDepthStencilRDZDisabled();

                for (int p = 0; p < this.techniqueForStreamOut.Description.PassCount; p++)
                {
                    this.techniqueForStreamOut.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    if (this.firstRun)
                    {
                        this.Game.Graphics.DeviceContext.Draw(this.initialParticles, 0);

                        this.firstRun = false;
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
                VertexBufferBinding iaBinding = new VertexBufferBinding(this.drawingBuffer, this.inputStride, 0);

                var technique = DrawerPool.EffectParticles.GetTechniqueForDrawing(VertexTypes.Particle, this.particleClass, context.DrawerMode);

                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
                Counters.IAVertexBuffersSets++;

                this.Game.Graphics.SetDepthStencilRDZEnabled();

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    if (this.particleClass != ParticleClasses.Rain)
                    {
                        this.Game.Graphics.SetBlendAdditive();
                    }
                    else
                    {
                        this.Game.Graphics.SetBlendDefault();
                    }
                }

                if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    if (this.particleClass != ParticleClasses.Rain)
                    {
                        this.Game.Graphics.SetBlendDeferredComposerAdditive();
                    }
                    else
                    {
                        this.Game.Graphics.SetBlendDeferredComposer();
                    }
                }

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawAuto();

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
            }

            #endregion
        }
        /// <summary>
        /// Toggle stream out and drawing buffers
        /// </summary>
        private void ToggleBuffers()
        {
            Buffer temp = this.drawingBuffer;
            this.drawingBuffer = this.streamOutBuffer;
            this.streamOutBuffer = temp;
        }
    }
}
