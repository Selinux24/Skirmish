using SharpDX;

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

                this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, Topology.TriangleList);

                effect.UpdatePerFrame(
                    context.ViewProjection,
                    context.EyePosition + new Vector3(0, -Description.PlaneHeight, 0),
                    context.Lights,
                    this.Description.BaseColor,
                    this.Description.WaterColor,
                    this.Description.WaveHeight,
                    this.Description.WaveChoppy,
                    this.Description.WaveSpeed,
                    this.Description.WaveFrequency,
                    context.GameTime.TotalSeconds,
                    this.Description.HeightmapIterations,
                    this.Description.GeometryIterations,
                    this.Description.ColorIterations);

                var graphics = this.Game.Graphics;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    graphics.DrawIndexed(
                        this.indexBuffer.Count,
                        this.indexBuffer.Offset,
                        this.vertexBuffer.Offset);
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
            GeometryUtil.CreateXZPlane(
                planeSize, planeHeight,
                out Vector3[] vData, out Vector3[] nData, out Vector2[] uvs, out uint[] iData);

            var vertices = VertexPositionTexture.Generate(vData, uvs);
            this.vertexBuffer = this.BufferManager.Add(name, vertices, false, 0);

            this.indexBuffer = this.BufferManager.Add(name, iData, false);
        }
    }
}
