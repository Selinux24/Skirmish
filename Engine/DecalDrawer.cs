using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Decals;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Decal drawer class
    /// </summary>
    public sealed class DecalDrawer : Drawable<DecalDrawerDescription>
    {
        /// <summary>
        /// Assigned buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

        /// <summary>
        /// Decal list
        /// </summary>
        private VertexDecal[] decals;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private EngineVertexBuffer<VertexDecal> buffer;
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
        /// Decals drawer
        /// </summary>
        private BuiltInDecals decalDrawer;

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
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public DecalDrawer(Scene scene, string id, string name) :
            base(scene, id, name)
        {

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
        public override async Task InitializeAssets(DecalDrawerDescription description)
        {
            await base.InitializeAssets(description);

            MaxDecalCount = Description.MaxDecalCount;
            RotateDecals = Description.RotateDecals;

            var imgContent = new FileArrayImageContent(Description.TextureName);
            Texture = await Scene.Game.ResourceManager.RequestResource(imgContent);
            TextureCount = (uint)imgContent.Count;

            decals = new VertexDecal[MaxDecalCount];

            decalDrawer = BuiltInShaders.GetDrawer<BuiltInDecals>();

            buffer = new EngineVertexBuffer<VertexDecal>(Scene.Game.Graphics, Name, decals, VertexBufferParams.Dynamic);
            buffer.CreateInputLayout(nameof(BuiltInDecals), decalDrawer.GetVertexShader().Shader.GetShaderBytecode(), BufferSlot);
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

            var mode = context.DrawerMode;
            if (!mode.HasFlag(DrawerModes.ShadowMap))
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += currentDecals;
            }

            var graphics = Scene.Game.Graphics;
            graphics.SetDepthStencilRDZEnabled();
            graphics.SetBlendState(BlendMode);

            decalDrawer.Update(
                RotateDecals,
                TextureCount,
                TintColor,
                Texture);

            decalDrawer.Draw(buffer, Topology.PointList, currentDecals);
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
            buffer.Write(decals);

            currentDecalIndex = nextFreeDecal;
        }
        /// <summary>
        /// Gets the active decal list
        /// </summary>
        /// <returns>Returns a position list</returns>
        private IEnumerable<Vector3> GetActiveDecals()
        {
            if (!decals.Any())
            {
                return Enumerable.Empty<Vector3>();
            }

            return decals
                .Where(d => d.StartTime + d.MaxAge > Game.GameTime.TotalSeconds)
                .Select(d => d.Position)
                .ToArray();
        }
        /// <summary>
        /// Clears all decals
        /// </summary>
        public void Clear()
        {
            if (!decals.Any())
            {
                return;
            }

            for (int i = 0; i < decals.Length; i++)
            {
                decals[i].StartTime = 0;
                decals[i].MaxAge = 0;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(DecalDrawer)}. ActiveDecals: {ActiveDecals}";
        }
    }
}
