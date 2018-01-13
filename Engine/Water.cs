using SharpDX;
using SharpDX.Direct3D;

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
        /// Internal resources disposition
        /// </summary>
        public override void Dispose()
        {

        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            if ((mode.HasFlag(DrawerModesEnum.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModesEnum.TransparentOnly) && this.Description.AlphaEnabled))
            {
                if (this.indexBuffer.Count > 0)
                {
                    var effect = DrawerPool.EffectDefaultWater;
                    var technique = DrawerPool.EffectDefaultWater.Water;

                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                    this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);
                    this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

                    var dwContext = context as DrawContext;

                    effect.UpdatePerFrame(
                        dwContext.World,
                        dwContext.ViewProjection,
                        dwContext.EyePosition + new Vector3(0, -Description.PlaneHeight, 0),
                        dwContext.Lights,
                        this.Description.BaseColor,
                        this.Description.WaterColor,
                        this.Description.WaveHeight,
                        this.Description.WaveChoppy,
                        this.Description.WaveSpeed,
                        this.Description.WaveFrequency,
                        dwContext.GameTime.TotalSeconds,
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
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="planeSize">Plane size</param>
        /// <param name="planeHeight">Plane height</param>
        private void InitializeBuffers(string name, float planeSize, float planeHeight)
        {
            Vector3[] vData;
            Vector3[] nData;
            Vector2[] uvs;
            uint[] iData;
            GeometryUtil.CreateXZPlane(planeSize, planeHeight, out vData, out nData, out uvs, out iData);

            var vertices = VertexPositionTexture.Generate(vData, uvs);
            this.vertexBuffer = this.BufferManager.Add(name, vertices, false, 0);

            this.indexBuffer = this.BufferManager.Add(name, iData, false);
        }
    }
}
