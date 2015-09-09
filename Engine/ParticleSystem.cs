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
            : base(game)
        {
            this.Opaque = description.Opaque;

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
                    //REVISION: it was camera position
                    Position = Vector3.Zero,
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

            this.textureArray = game.Graphics.Device.LoadTextureArray(ContentManager.FindContent(description.ContentPath, description.Textures));
            this.textureRandom = game.Graphics.Device.CreateRandomTexture(1024);

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
            Helper.Dispose(this.textureArray);
            Helper.Dispose(this.textureRandom);
        }
        /// <summary>
        /// Updating
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
        {
            this.Manipulator.Update(gameTime);
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            this.Game.Graphics.DeviceContext.OutputMerger.BlendState = null;
            this.Game.Graphics.DeviceContext.OutputMerger.BlendFactor = Color.Zero;
            this.Game.Graphics.DeviceContext.OutputMerger.BlendSampleMask = -1;

            this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);

            #region Per frame update

            DrawerPool.EffectParticles.UpdatePerFrame(
                context.World * this.Manipulator.LocalTransform,
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                (uint)this.textureArray.Description.Texture2DArray.ArraySize,
                this.textureArray,
                this.textureRandom,
                this.emitterAge,
                this.maximumAge,
                gameTime.TotalSeconds,
                gameTime.ElapsedSeconds,
                this.particleAcceleration);

            #endregion

            #region Stream out

            {
                VertexBufferBinding iaBinding = new VertexBufferBinding(this.firstRun ? this.emittersBuffer : this.drawingBuffer, this.inputStride, 0);
                StreamOutputBufferBinding soBinding = new StreamOutputBufferBinding(this.streamOutBuffer, 0);

                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
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

    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter
    {
        /// <summary>
        /// Particle size
        /// </summary>
        public float Size = 1f;
        /// <summary>
        /// Particle color
        /// </summary>
        public Color Color = Color.White;
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position = Vector3.Zero;
    }

    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemDescription
    {
        /// <summary>
        /// Emitter types
        /// </summary>
        public enum EmitterTypes
        {
            /// <summary>
            /// Emit from position
            /// </summary>
            FixedPosition,
            /// <summary>
            /// Emit from camera
            /// </summary>
            FromCamera,
        }

        /// <summary>
        /// Particle class
        /// </summary>
        public ParticleClasses ParticleClass = ParticleClasses.Unknown;
        /// <summary>
        /// Maximum particles
        /// </summary>
        public int MaximumParticles = 1000;
        /// <summary>
        /// Maximum age of particle
        /// </summary>
        public float MaximumAge = 5f;
        /// <summary>
        /// Particle age to emit new flares
        /// </summary>
        public float EmitterAge = 0.001f;
        /// <summary>
        /// Acceleration vector
        /// </summary>
        public Vector3 Acceleration = GameEnvironment.Gravity;
        /// <summary>
        /// Emitter type
        /// </summary>
        public EmitterTypes EmitterType = EmitterTypes.FromCamera;
        /// <summary>
        /// Texture list
        /// </summary>
        public string[] Textures = null;
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Is opaque object
        /// </summary>
        public bool Opaque = true;
        /// <summary>
        /// Emitters
        /// </summary>
        public ParticleEmitter[] Emitters = null;

        /// <summary>
        /// Creates a fire particle system
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Fire(ParticleEmitter emitter, params string[] textures)
        {
            return Fire(new[] { emitter }, textures);
        }
        /// <summary>
        /// Creates a fire particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Fire(ParticleEmitter[] emitters, params string[] textures)
        {
            return new ParticleSystemDescription()
            {
                ParticleClass = ParticleClasses.Fire,
                MaximumParticles = 500,
                MaximumAge = 0.5f,
                EmitterAge = 0.005f,
                Acceleration = new Vector3(0.0f, 5.8f, 0.0f),
                EmitterType = EmitterTypes.FixedPosition,
                Textures = textures,
                Emitters = emitters
            };
        }
        /// <summary>
        /// Creates a smoke particle system
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Smoke(ParticleEmitter emitter, params string[] textures)
        {
            return Smoke(new[] { emitter }, textures);
        }
        /// <summary>
        /// Creates a smoke particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Smoke(ParticleEmitter[] emitters, params string[] textures)
        {
            return new ParticleSystemDescription()
            {
                ParticleClass = ParticleClasses.Smoke,
                MaximumParticles = 500,
                MaximumAge = 1.0f,
                EmitterAge = 0.33f,
                Acceleration = new Vector3(0.0f, 2f, 0.0f),
                EmitterType = EmitterTypes.FixedPosition,
                Textures = textures,
                Emitters = emitters
            };
        }
        /// <summary>
        /// Creates a rain particle system
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Rain(ParticleEmitter emitter, params string[] textures)
        {
            return Rain(new[] { emitter }, textures);
        }
        /// <summary>
        /// Creates a rain particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Rain(ParticleEmitter[] emitters, params string[] textures)
        {
            return new ParticleSystemDescription()
            {
                ParticleClass = ParticleClasses.Rain,
                MaximumParticles = 10000,
                MaximumAge = 3.0f,
                EmitterAge = 0.002f,
                Acceleration = (GameEnvironment.Gravity + Vector3.UnitX),
                EmitterType = EmitterTypes.FromCamera,
                Textures = textures,
                Emitters = emitters
            };
        }
    }
}
