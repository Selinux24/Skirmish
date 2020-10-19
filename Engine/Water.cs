using SharpDX;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Water drawer
    /// </summary>
    public class Water : Drawable
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
        /// Water description
        /// </summary>
        protected new WaterDescription Description
        {
            get
            {
                return (WaterDescription)base.Description;
            }
        }

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
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Water(string name, Scene scene, WaterDescription description)
            : base(name, scene, description)
        {
            InitializeBuffers(name, description.PlaneSize, description.PlaneHeight);
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

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
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
                context.EyePosition + new Vector3(0, -Description.PlaneHeight, 0),
                context.Lights,
                new EffectWaterState
                {
                    BaseColor = Description.BaseColor,
                    WaterColor = Description.WaterColor,
                    WaveHeight = Description.WaveHeight,
                    WaveChoppy = Description.WaveChoppy,
                    WaveSpeed = Description.WaveSpeed,
                    WaveFrequency = Description.WaveFrequency,
                    TotalTime = context.GameTime.TotalSeconds,
                    Steps = Description.HeightmapIterations,
                    GeometryIterations = Description.GeometryIterations,
                    ColorIterations = Description.ColorIterations,
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
    }

    /// <summary>
    /// Water extensions
    /// </summary>
    public static class WaterExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Water> AddComponentWater(this Scene scene, string name, WaterDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Water component = null;

            await Task.Run(() =>
            {
                component = new Water(name, scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
