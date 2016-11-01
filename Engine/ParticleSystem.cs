using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Particle system
    /// </summary>
    public class ParticleSystem : Drawable
    {
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
            foreach (var emitterDesc in description.Emitters)
            {
                var emitter = new ParticleEmitter(game, emitterDesc);

                this.emitters.Add(emitter);
            }

            this.textureRandom = game.ResourceManager.CreateRandomTexture(Guid.NewGuid(), 1024, 0, 1);
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
            this.emitters.ForEach(e => e.Update(context.GameTime));
        }
        /// <summary>
        /// Drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var effect = DrawerPool.EffectParticles;
            if (effect != null)
            {
                var techniqueForStreamOut = effect.GetTechniqueForStreamOut(VertexTypes.Particle);
                var inputLayout = effect.GetInputLayout(techniqueForStreamOut);

                this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
                Counters.IAInputLayoutSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                Counters.IAPrimitiveTopologySets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                #region Per frame update

                effect.UpdatePerFrame(
                    context.ViewProjection,
                    context.EyePosition,
                    context.Lights,
                    this.textureRandom);

                #endregion

                foreach (var emitter in this.emitters)
                {
                    #region Per emitter update

                    effect.UpdatePerEmitter(
                        emitter.TotalTime,
                        emitter.ElapsedTime,
                        emitter.Description.EmissionRate,
                        (uint)emitter.TextureArray.Description.Texture2DArray.ArraySize,
                        emitter.TextureArray,
                        emitter.Description.EnergyMin,
                        emitter.Description.EnergyMax,
                        emitter.Description.Ellipsoid,
                        emitter.Description.OrbitPosition,
                        emitter.Description.OrbitVelocity,
                        emitter.Description.OrbitAcceleration,
                        emitter.Description.SizeStartMin,
                        emitter.Description.SizeStartMax,
                        emitter.Description.SizeEndMin,
                        emitter.Description.SizeEndMax,
                        emitter.Description.ColorStart,
                        emitter.Description.ColorStartVar,
                        emitter.Description.ColorEnd,
                        emitter.Description.ColorEndVar,
                        emitter.Description.Position,
                        emitter.Description.PositionVar,
                        emitter.Description.Velocity,
                        emitter.Description.VelocityVar,
                        emitter.Description.Acceleration,
                        emitter.Description.AccelerationVar);

                    #endregion

                    #region Stream out

                    {
                        emitter.PrepareStreamOut(this.Game);

                        for (int p = 0; p < techniqueForStreamOut.Description.PassCount; p++)
                        {
                            techniqueForStreamOut.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            if (emitter.FirstRun)
                            {
                                this.Game.Graphics.DeviceContext.Draw(emitter.Description.ParticleCountMax, 0);

                                emitter.FirstRun = false;
                            }
                            else
                            {
                                this.Game.Graphics.DeviceContext.DrawAuto();
                            }

                            Counters.DrawCallsPerFrame++;
                            Counters.InstancesPerFrame++;
                        }

                        emitter.EndStreamOut(this.Game);
                    }

                    #endregion

                    emitter.ToggleBuffers();

                    #region Draw

                    {
                        emitter.PrepareDrawing(this.Game);

                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            this.Game.Graphics.SetBlendAdditive();
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            this.Game.Graphics.SetBlendDeferredComposerAdditive();
                        }

                        var technique = effect.GetTechniqueForDrawing(VertexTypes.Particle, context.DrawerMode);

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

        /// <summary>
        /// Adds new emitter to collection
        /// </summary>
        /// <param name="desc">Emitter description</param>
        /// <returns>Returns the new generated emitter</returns>
        public ParticleEmitter Add(ParticleEmitterDescription desc)
        {
            var emitter = new ParticleEmitter(this.Game, desc);

            this.emitters.Add(emitter);

            return emitter;
        }
    }
}
