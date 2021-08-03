using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;
using System.Linq;
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
        /// Decal volume
        /// </summary>
        private BoundingSphere boundingVolume = new BoundingSphere();
        /// <summary>
        /// Current decal count
        /// </summary>
        private int currentDecals;

        /// <summary>
        /// Decal texture
        /// </summary>
        public EngineShaderResourceView Texture { get; private set; }
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount { get; private set; }
        /// <summary>
        /// Gets the maximum number of concurrent decals
        /// </summary>
        public int MaxDecalCount { get; private set; }
        /// <summary>
        /// Rotate decals
        /// </summary>
        public bool RotateDecals { get; private set; }
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; } = Color4.White;
        /// <summary>
        /// Gets the active decal count
        /// </summary>
        public int ActiveDecals { get; private set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Decal description</param>
        public DecalDrawer(string id, string name, Scene scene, DecalDrawerDescription description) :
            base(id, name, scene, description)
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
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            var activeDecals = GetActiveDecals();

            ActiveDecals = activeDecals.Count();

            if (ActiveDecals <= 0)
            {
                boundingVolume = new BoundingSphere();

                return;
            }

            boundingVolume = BoundingSphere.FromPoints(activeDecals.ToArray());
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (currentDecals <= 0)
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
                Counters.PrimitivesPerFrame += currentDecals;
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
                TintColor,
                TextureCount,
                Texture);

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.Draw(currentDecals, 0);
            }
        }

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (ActiveDecals <= 0)
            {
                return true;
            }

            var inside = volume.Contains(boundingVolume) != ContainmentType.Disjoint;
            if (inside)
            {
                distance = Vector3.DistanceSquared(volume.Position, boundingVolume.Center);
            }

            return !inside;
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

            if (currentDecals < nextFreeDecal)
            {
                currentDecals = nextFreeDecal;
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
        /// <summary>
        /// Gets the active decal list
        /// </summary>
        /// <returns>Returns a position list</returns>
        private IEnumerable<Vector3> GetActiveDecals()
        {
            return decals
                .Where(d => d.StartTime + d.MaxAge > Game.GameTime.TotalSeconds)
                .Select(d => d.Position)
                .ToArray();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(DecalDrawer)}. ActiveDecals: {ActiveDecals}";
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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<DecalDrawer> AddComponentDecalDrawer(this Scene scene, string id, string name, DecalDrawerDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerEffects)
        {
            DecalDrawer component = null;

            await Task.Run(() =>
            {
                component = new DecalDrawer(id, name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
