using SharpDX;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

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
                WaterColor = Description.WaterColor.RGB(),
                WaterTransparency = Description.WaterColor.Alpha,
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

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return;
            }

            var effect = DrawerPool.EffectDefaultWater;
            var technique = DrawerPool.EffectDefaultWater.Water;

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                new EffectWaterState
                {
                    BaseColor = WaterState.BaseColor,
                    WaterColor = new Color4(WaterState.WaterColor, WaterState.WaterTransparency),
                    WaveHeight = WaterState.WaveHeight,
                    WaveChoppy = WaterState.WaveChoppy,
                    WaveSpeed = WaterState.WaveSpeed,
                    WaveFrequency = WaterState.WaveFrequency,
                    Steps = WaterState.Steps,
                    GeometryIterations = WaterState.GeometryIterations,
                    ColorIterations = WaterState.ColorIterations,
                    TotalTime = context.GameTime.TotalSeconds,
                });

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(
                    indexBuffer.Count,
                    indexBuffer.BufferOffset,
                    vertexBuffer.BufferOffset);
            }
        }
    }

    /// <summary>
    /// Water state
    /// </summary>
    public class WaterState
    {
        /// <summary>
        /// Base color
        /// </summary>
        public Color3 BaseColor { get; set; }
        /// <summary>
        /// Water color
        /// </summary>
        public Color3 WaterColor { get; set; }
        /// <summary>
        /// Water color alpha component
        /// </summary>
        public float WaterTransparency { get; set; }
        /// <summary>
        /// Wave heigth
        /// </summary>
        public float WaveHeight { get; set; }
        /// <summary>
        /// Wave choppy
        /// </summary>
        public float WaveChoppy { get; set; }
        /// <summary>
        /// Wave speed
        /// </summary>
        public float WaveSpeed { get; set; }
        /// <summary>
        /// Wave frequency
        /// </summary>
        public float WaveFrequency { get; set; }
        /// <summary>
        /// Shader steps
        /// </summary>
        public int Steps { get; set; }
        /// <summary>
        /// Geometry iterations
        /// </summary>
        public int GeometryIterations { get; set; }
        /// <summary>
        /// Color iterations
        /// </summary>
        public int ColorIterations { get; set; }
    }
}
