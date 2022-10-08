using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Water;
    using Engine.Common;

    /// <summary>
    /// Water drawer
    /// </summary>
    public sealed class Water : Drawable<WaterDescription>
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
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Water(Scene scene, string id, string name)
            : base(scene, id, name)
        {

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
        public override async Task InitializeAssets(WaterDescription description)
        {
            await base.InitializeAssets(description);

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
            var plane = GeometryUtil.CreateXZPlane(planeSize, planeHeight);

            var vertices = VertexPositionTexture.Generate(plane.Vertices, plane.Uvs);
            var indices = plane.Indices;

            vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(name, false, indices);

            waterDrawer = BuiltInShaders.GetDrawer<BuiltInWater>();
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            if (waterDrawer == null)
            {
                return;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return;
            }

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
            waterDrawer.UpdateWater(waterState);

            var drawOptions = new DrawOptions
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                Topology = Topology.TriangleList,
            };
            waterDrawer.Draw(BufferManager, drawOptions);

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;
        }
    }
}
