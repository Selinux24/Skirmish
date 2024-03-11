using SharpDX;
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
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class DecalDrawer(Scene scene, string id, string name) : Drawable<DecalDrawerDescription>(scene, id, name)
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
        /// Update decals buffer flag
        /// </summary>
        private bool updateBuffer = false;
        /// <summary>
        /// Current decal index to update data
        /// </summary>
        private int currentDecalIndex = 0;
        /// <summary>
        /// Decal volume
        /// </summary>
        private BoundingSphere boundingVolume = new();
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
        public override async Task ReadAssets(DecalDrawerDescription description)
        {
            await base.ReadAssets(description);

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

            ActiveDecals = activeDecals.Length;

            if (ActiveDecals <= 0)
            {
                boundingVolume = new BoundingSphere();

                return;
            }

            boundingVolume = SharpDXExtensions.BoundingSphereFromPoints(activeDecals);
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (currentDecals <= 0)
            {
                return false;
            }

            bool isTransparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);
            bool draw = context.ValidateDraw(BlendMode, isTransparent);
            if (!draw)
            {
                return false;
            }

            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            if (updateBuffer)
            {
                Logger.WriteTrace(this, $"{Name} - {nameof(Draw)} Update decals buffer");
                buffer.Write(dc, decals);
                updateBuffer = false;
            }

            dc.SetDepthStencilState(graphics.GetDepthStencilRDZEnabled());
            dc.SetBlendState(graphics.GetBlendState(BlendMode));

            decalDrawer.Update(
                dc,
                RotateDecals,
                TextureCount,
                TintColor,
                Texture);

            return decalDrawer.Draw(dc, buffer, Topology.PointList, currentDecals);
        }

        /// <inheritdoc/>
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
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

            updateBuffer = true;

            currentDecalIndex = nextFreeDecal;
        }
        /// <summary>
        /// Gets the active decal list
        /// </summary>
        /// <returns>Returns a position list</returns>
        private Vector3[] GetActiveDecals()
        {
            if (decals.Length == 0)
            {
                return [];
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
            if (decals.Length == 0)
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
