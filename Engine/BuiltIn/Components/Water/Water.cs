using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Water
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Water;
    using Engine.Common;

    /// <summary>
    /// Water drawer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class Water(Scene scene, string id, string name) : Drawable<WaterDescription>(scene, id, name)
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Water drawer
        /// </summary>
        private BuiltInWater waterDrawer = null;

        /// <summary>
        /// Water state
        /// </summary>
        public WaterState WaterState { get; private set; }
        /// <summary>
        /// Returns true if the buffers were ready
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                if (vertexBuffer?.Ready != true)
                {
                    return false;
                }

                if (indexBuffer?.Ready != true)
                {
                    return false;
                }

                if (indexBuffer.Count <= 0)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Water()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                BufferManager?.RemoveVertexData(vertexBuffer);
                BufferManager?.RemoveIndexData(indexBuffer);
            }
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(WaterDescription description)
        {
            await base.ReadAssets(description);

            WaterState = new WaterState
            {
                BaseColor = Description.BaseColor,
                WaterColor = Description.WaterColor,
                WaveHeight = Description.WaveHeight,
                WaveChoppy = Description.WaveChoppy,
                WaveSpeed = Description.WaveSpeed,
                WaveFrequency = Description.WaveFrequency,
                Steps = Description.HeightmapIterations,
                GeometryIterations = Description.GeometryIterations,
                ColorIterations = Description.ColorIterations,
            };

            InitializeBuffers(Name, Description.PlaneSize, Description.PlaneHeight);
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="planeSize">Plane size</param>
        /// <param name="planeHeight">Plane height</param>
        private void InitializeBuffers(string name, float planeSize, float planeHeight)
        {
            var plane = GeometryUtil.CreateXZPlane(planeSize, planeSize, planeHeight);

            var vertices = VertexPositionTexture.Generate(plane.Vertices, plane.Uvs);
            var indices = plane.Indices;

            vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(name, false, indices);

            waterDrawer = BuiltInShaders.GetDrawer<BuiltInWater>();
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            if (waterDrawer == null)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;

            var waterState = new BuiltInWaterState
            {
                BaseColor = WaterState.BaseColor,
                WaterColor = WaterState.WaterColor,
                WaveHeight = WaterState.WaveHeight,
                WaveChoppy = WaterState.WaveChoppy,
                WaveSpeed = WaterState.WaveSpeed,
                WaveFrequency = WaterState.WaveFrequency,
                Steps = WaterState.Steps,
                GeometryIterations = WaterState.GeometryIterations,
                ColorIterations = WaterState.ColorIterations,
            };
            waterDrawer.UpdateWater(dc, waterState);

            var drawOptions = new DrawOptions
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                Topology = Topology.TriangleList,
            };
            return waterDrawer.Draw(dc, drawOptions);
        }
    }
}
