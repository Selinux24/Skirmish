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
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Helpers;
    using Engine.Effects;

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
                    generator.AddParticle(this.Game);

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
                    this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;

                    this.Game.Graphics.SetDepthStencilRDZEnabled();
                    this.Game.Graphics.SetBlendDefaultAlpha();

                    foreach (var generator in this.particleGenerators)
                    {
                        #region Per frame update

                        effect.UpdatePerFrame(
                            context.World,
                            context.ViewProjection,
                            this.Game.Graphics.Viewport.Height,
                            context.EyePosition,
                            generator.ParticleSystem.TotalTime,
                            generator.ParticleSystem.MaxDuration,
                            generator.ParticleSystem.MaxDurationRandomness,
                            generator.ParticleSystem.EndVelocity,
                            generator.ParticleSystem.Gravity,
                            generator.ParticleSystem.StartSize,
                            generator.ParticleSystem.EndSize,
                            generator.ParticleSystem.MinColor,
                            generator.ParticleSystem.MaxColor,
                            generator.ParticleSystem.RotateSpeed,
                            generator.ParticleSystem.TextureCount,
                            generator.ParticleSystem.Texture);

                        #endregion

                        generator.ParticleSystem.SetBuffer(this.Game);

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            generator.ParticleSystem.Draw(this.Game);
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
}
