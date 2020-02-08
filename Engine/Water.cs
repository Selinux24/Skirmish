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
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Water(Scene scene, WaterDescription description)
            : base(scene, description)
        {
            this.InitializeBuffers(description.Name, description.PlaneSize, description.PlaneHeight);
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
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);
            }
        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;
            var draw =
                (mode.HasFlag(DrawerModes.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModes.TransparentOnly) && this.Description.AlphaEnabled);

            if (draw && this.indexBuffer.Count > 0)
            {
                var effect = DrawerPool.EffectDefaultWater;
                var technique = DrawerPool.EffectDefaultWater.Water;

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                this.BufferManager.SetIndexBuffer(this.indexBuffer);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

                effect.UpdatePerFrame(
                    context.ViewProjection,
                    context.EyePosition + new Vector3(0, -Description.PlaneHeight, 0),
                    context.Lights,
                    new EffectWaterState
                    {
                        BaseColor = this.Description.BaseColor,
                        WaterColor = this.Description.WaterColor,
                        WaveHeight = this.Description.WaveHeight,
                        WaveChoppy = this.Description.WaveChoppy,
                        WaveSpeed = this.Description.WaveSpeed,
                        WaveFrequency = this.Description.WaveFrequency,
                        TotalTime = context.GameTime.TotalSeconds,
                        Steps = this.Description.HeightmapIterations,
                        GeometryIterations = this.Description.GeometryIterations,
                        ColorIterations = this.Description.ColorIterations,
                    });

                var graphics = this.Game.Graphics;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    graphics.DrawIndexed(
                        this.indexBuffer.Count,
                        this.indexBuffer.BufferOffset,
                        this.vertexBuffer.BufferOffset);
                }
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

            this.vertexBuffer = this.BufferManager.AddVertexData(name, false, vertices);
            this.indexBuffer = this.BufferManager.AddIndexData(name, false, indices);
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
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Water> AddComponentWater(this Scene scene, WaterDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Water component = null;

            await Task.Run(() =>
            {
                component = new Water(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
