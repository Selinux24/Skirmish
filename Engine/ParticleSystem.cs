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
using System.Collections.Generic;

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
        /// Emitter list
        /// </summary>
        private List<ParticleEmitter> emitters = new List<ParticleEmitter>();

        /// <summary>
        /// Random texture
        /// </summary>
        private ShaderResourceView textureRandom;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle system description</param>
        public ParticleSystem(Game game, ParticleSystemDescription description)
            : base(game, description)
        {
            List<VertexParticle> data = new List<VertexParticle>();

            foreach (var emitter in description.Emitters)
            {
                for (int i = 0; i < emitter.ParticleCountMax; i++)
                {
                    VertexParticle v = new VertexParticle()
                    {
                        Age = 0,
                        Type = (uint)ParticleTypes.Emitter,
                    };

                    data.Add(v);
                }

                ImageContent imgContent = new ImageContent()
                {
                    Streams = ContentManager.FindContent(emitter.ContentPath, emitter.Textures),
                };

                var textureCount = imgContent.Count;
                var textureArray = game.ResourceManager.CreateResource(imgContent);

                var emittersBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(data.ToArray(), ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
                var drawingBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(1000, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
                var streamOutBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(1000, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
                var inputStride = default(VertexParticle).Stride;

                var pEmitter = new ParticleEmitter()
                {
                    ParticleCountMax = emitter.ParticleCountMax,
                    EmissionRate = emitter.EmissionRate,
                    Ellipsoid = emitter.Ellipsoid,
                    OrbitPosition = emitter.OrbitPosition,
                    OrbitVelocity = emitter.OrbitVelocity,
                    OrbitAcceleration = emitter.OrbitAcceleration,
                    SizeStartMin = emitter.SizeStartMin,
                    SizeStartMax = emitter.SizeStartMax,
                    SizeEndMin = emitter.SizeEndMin,
                    SizeEndMax = emitter.SizeEndMax,
                    EnergyMin = emitter.EnergyMin,
                    EnergyMax = emitter.EnergyMax,
                    ColorStart = emitter.ColorStart,
                    ColorStartVariance = emitter.ColorStartVariance,
                    ColorEnd = emitter.ColorEnd,
                    ColorEndVariance = emitter.ColorEndVariance,
                    Position = emitter.Position,
                    PositionVariance = emitter.PositionVariance,
                    Velocity = emitter.Velocity,
                    VelocityVariance = emitter.VelocityVariance,
                    Acceleration = emitter.Acceleration,
                    AccelerationVariance = emitter.AccelerationVariance,
                    RotationParticleSpeedMin = emitter.RotationParticleSpeedMin,
                    RotationParticleSpeedMax = emitter.RotationParticleSpeedMax,
                    RotationSpeedMin = emitter.RotationSpeedMin,
                    RotationSpeedMax = emitter.RotationSpeedMax,
                    Angle = emitter.Angle,
                    RotationAxis = emitter.RotationAxis,
                    RotationAxisVariance = emitter.RotationAxisVariance,

                    TextureCount = (uint)textureCount,
                    TextureArray = textureArray,
                    EmittersBuffer = emittersBuffer,
                    DrawingBuffer = drawingBuffer,
                    StreamOutBuffer = streamOutBuffer,
                    InputStride = inputStride,
                };

                this.emitters.Add(pEmitter);
            }

            this.textureRandom = game.ResourceManager.CreateRandomTexture(Guid.NewGuid(), 1024);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.emitters);
        }
        /// <summary>
        /// Updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {

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

            var techniqueForStreamOut = DrawerPool.EffectParticles.GetTechniqueForStreamOut(VertexTypes.Particle);

            var inputLayout = DrawerPool.EffectParticles.GetInputLayout(techniqueForStreamOut);
            this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
            Counters.IAInputLayoutSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            Counters.IAPrimitiveTopologySets++;
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;

            #region Per frame update

            DrawerPool.EffectParticles.UpdatePerFrame(
                context.World,
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                context.GameTime.ElapsedSeconds,
                this.textureRandom);

            #endregion

            foreach (var emitter in this.emitters)
            {
                #region Per emitter update

                if (emitter.FirstRun)
                {
                    DrawerPool.EffectParticles.UpdatePerEmitter(
                        emitter.EmissionRate,
                        emitter.OrbitPosition,
                        emitter.OrbitVelocity,
                        emitter.OrbitAcceleration,
                        emitter.Ellipsoid,
                        emitter.Position,
                        emitter.PositionVariance,
                        emitter.Velocity,
                        emitter.VelocityVariance,
                        emitter.Acceleration,
                        emitter.AccelerationVariance,
                        emitter.ColorStart,
                        emitter.ColorStartVariance,
                        emitter.ColorEnd,
                        emitter.ColorEndVariance,
                        emitter.EnergyMax,
                        emitter.EnergyMin,
                        emitter.SizeStartMax,
                        emitter.SizeStartMin,
                        emitter.SizeEndMax,
                        emitter.SizeEndMin,
                        emitter.RotationParticleSpeedMin,
                        emitter.RotationParticleSpeedMax,
                        emitter.RotationAxis,
                        emitter.RotationAxisVariance,
                        emitter.RotationParticleSpeedMin,
                        emitter.RotationParticleSpeedMax,
                        emitter.TextureCount,
                        emitter.TextureArray);
                }

                #endregion

                #region Stream out

                {
                    this.Game.Graphics.SetDepthStencilRDZDisabled();

                    var iaBinding = new VertexBufferBinding(emitter.FirstRun ? emitter.EmittersBuffer : emitter.DrawingBuffer, emitter.InputStride, 0);
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
                    Counters.IAVertexBuffersSets++;
                    var soBinding = new StreamOutputBufferBinding(emitter.StreamOutBuffer, 0);
                    this.Game.Graphics.DeviceContext.StreamOutput.SetTargets(new[] { soBinding });

                    for (int p = 0; p < techniqueForStreamOut.Description.PassCount; p++)
                    {
                        techniqueForStreamOut.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        if (emitter.FirstRun)
                        {
                            this.Game.Graphics.DeviceContext.Draw(emitter.ParticleCountMax, 0);

                            emitter.FirstRun = false;
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

                emitter.ToggleBuffers();

                #region Draw

                {
                    this.Game.Graphics.SetDepthStencilRDZEnabled();

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        this.Game.Graphics.SetBlendDefault();
                    }

                    if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        this.Game.Graphics.SetBlendDeferredComposer();
                    }

                    var iaBinding = new VertexBufferBinding(emitter.DrawingBuffer, emitter.InputStride, 0);
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
                    Counters.IAVertexBuffersSets++;

                    var technique = DrawerPool.EffectParticles.GetTechniqueForDrawing(VertexTypes.Particle, context.DrawerMode);
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
        }
    }
}
