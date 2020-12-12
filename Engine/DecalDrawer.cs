﻿using SharpDX;
using SharpDX.Direct3D;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Decal drawer class
    /// </summary>
    public class DecalDrawer : Drawable
    {
        /// <summary>
        /// Assigned buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

        /// <summary>
        /// Decal list
        /// </summary>
        private readonly VertexDecal[] decals;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private EngineBuffer<VertexDecal> buffer;
        /// <summary>
        /// Current decal index to update data
        /// </summary>
        private int currentDecalIndex = 0;

        /// <summary>
        /// Decal texture
        /// </summary>
        public EngineShaderResourceView Texture { get; private set; }
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount { get; private set; }
        /// <summary>
        /// Active decal count
        /// </summary>
        public int ActiveDecals { get; private set; }
        /// <summary>
        /// Gets the maximum number of concurrent decals
        /// </summary>
        public int MaxDecalCount { get; private set; }
        /// <summary>
        /// Rotate decals
        /// </summary>
        public bool RotateDecals { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Decal description</param>
        public DecalDrawer(string name, Scene scene, DecalDrawerDescription description) : base(name, scene, description)
        {
            MaxDecalCount = description.MaxDecalCount;
            RotateDecals = description.RotateDecals;

            var imgContent = ImageContent.Texture(description.TextureName);
            Texture = scene.Game.ResourceManager.RequestResource(imgContent);
            TextureCount = (uint)imgContent.Count;

            decals = new VertexDecal[MaxDecalCount];
            buffer = new EngineBuffer<VertexDecal>(scene.Game.Graphics, Name, decals, true);

            if (RotateDecals)
            {
                buffer.AddInputLayout(scene.Game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultDecals.Decal.GetSignature(), VertexDecal.Input(BufferSlot)));
            }
            else
            {
                buffer.AddInputLayout(scene.Game.Graphics.CreateInputLayout(DrawerPool.EffectDefaultDecals.DecalRotated.GetSignature(), VertexDecal.Input(BufferSlot)));
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DecalDrawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                buffer?.Dispose();
                buffer = null;
            }
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (ActiveDecals <= 0)
            {
                return;
            }

            bool isTransparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(BlendModes.Default, isTransparent);
            if (!draw)
            {
                return;
            }

            var effect = DrawerPool.EffectDefaultDecals;
            var technique = RotateDecals ? effect.DecalRotated : effect.Decal;

            var mode = context.DrawerMode;
            if (!mode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += ActiveDecals;
            }

            var graphics = Scene.Game.Graphics;

            graphics.IASetVertexBuffers(BufferSlot, buffer.VertexBufferBinding);
            graphics.IAInputLayout = buffer.InputLayouts[0];
            graphics.IAPrimitiveTopology = PrimitiveTopology.PointList;

            graphics.SetDepthStencilRDZEnabled();
            graphics.SetBlendState(BlendMode);

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.GameTime.TotalSeconds,
                TextureCount,
                Texture);

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.Draw(ActiveDecals, 0);
            }
        }

        /// <summary>
        /// Adds a new decal
        /// </summary>
        /// <param name="position">Decal position</param>
        /// <param name="normal">Decal normal</param>
        /// <param name="size">Decal size</param>
        /// <param name="maxAge">Max age in seconds</param>
        public void AddDecal(Vector3 position, Vector3 normal, Vector2 size, float maxAge)
        {
            int nextFreeDecal = currentDecalIndex + 1;

            if (ActiveDecals < nextFreeDecal)
            {
                ActiveDecals = nextFreeDecal;
            }

            if (nextFreeDecal >= decals.Length)
            {
                nextFreeDecal = 0;
            }

            decals[currentDecalIndex].Position = position + (normal * 0.01f);
            decals[currentDecalIndex].Normal = normal;
            decals[currentDecalIndex].Size = size;
            decals[currentDecalIndex].StartTime = Scene.Game.GameTime.TotalSeconds;
            decals[currentDecalIndex].MaxAge = maxAge;

            Logger.WriteTrace(this, $"{Name} - {nameof(AddDecal)} WriteDiscardBuffer");
            Scene.Game.Graphics.WriteDiscardBuffer(buffer.VertexBuffer, decals);

            currentDecalIndex = nextFreeDecal;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Count: {ActiveDecals}";
        }
    }

    /// <summary>
    /// Decal drawer extensions
    /// </summary>
    public static class DecalExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<DecalDrawer> AddComponentDecalDrawer(this Scene scene, string name, DecalDrawerDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerEffects)
        {
            DecalDrawer component = null;

            await Task.Run(() =>
            {
                component = new DecalDrawer(name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}