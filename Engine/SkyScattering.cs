using SharpDX;
using SharpDX.Direct3D;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Scattered sky
    /// </summary>
    public class SkyScattering : Drawable
    {
        /// <summary>
        /// Buffer manager
        /// </summary>
        private BufferManager bufferManager = new BufferManager();
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        private int vertexBufferOffset = -1;
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        private int vertexBufferSlot = -1;
        /// <summary>
        /// Vertex count
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        private int indexBufferOffset = -1;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
        /// <summary>
        /// Rayleigh scattering constant value
        /// </summary>
        private float rayleighScattering;
        /// <summary>
        /// Mie scattering constant value
        /// </summary>
        private float mieScattering;
        /// <summary>
        /// Wave length
        /// </summary>
        private Color4 wavelength;
        /// <summary>
        /// Sphere inner radius
        /// </summary>
        private float sphereInnerRadius;
        /// <summary>
        /// Sphere outer radius
        /// </summary>
        private float sphereOuterRadius;

        /// <summary>
        /// Rayleigh scattering * 4 * PI
        /// </summary>
        protected float RayleighScattering4PI { get; private set; }
        /// <summary>
        /// Mie scattering * 4 * PI
        /// </summary>
        protected float MieScattering4PI { get; private set; }
        /// <summary>
        /// Inverse wave length * 4
        /// </summary>
        protected Color4 InvWaveLength4 { get; private set; }
        /// <summary>
        /// Scattering Scale
        /// </summary>
        protected float ScatteringScale { get; private set; }

        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return 1;
            }
        }
        /// <summary>
        /// Planet radius
        /// </summary>
        public float PlanetRadius { get; set; }
        /// <summary>
        /// Planet atmosphere radius from surface
        /// </summary>
        public float PlanetAtmosphereRadius { get; set; }
        /// <summary>
        /// Rayleigh scattering constant value
        /// </summary>
        public float RayleighScattering
        {
            get
            {
                return this.rayleighScattering;
            }
            set
            {
                this.rayleighScattering = value;
                this.RayleighScattering4PI = value * 4.0f * MathUtil.Pi;
            }
        }
        /// <summary>
        /// Rayleigh scale depth value
        /// </summary>
        public float RayleighScaleDepth { get; set; }
        /// <summary>
        /// Mie scattering constant value
        /// </summary>
        public float MieScattering
        {
            get
            {
                return this.mieScattering;
            }
            set
            {
                this.mieScattering = value;
                this.MieScattering4PI = value * 4.0f * MathUtil.Pi;
            }
        }
        /// <summary>
        /// Mie phase assymetry value
        /// </summary>
        public float MiePhaseAssymetry { get; set; }
        /// <summary>
        /// Mie scale depth value
        /// </summary>
        public float MieScaleDepth { get; set; }
        /// <summary>
        /// Light wave length
        /// </summary>
        public Color4 WaveLength
        {
            get
            {
                return this.wavelength;
            }
            set
            {
                this.wavelength = value;
                this.InvWaveLength4 = new Color4(
                    1f / (float)Math.Pow(value.Red, 4.0f),
                    1f / (float)Math.Pow(value.Green, 4.0f),
                    1f / (float)Math.Pow(value.Blue, 4.0f),
                    1.0f);
            }
        }
        /// <summary>
        /// Sky brightness
        /// </summary>
        public float Brightness { get; set; }
        /// <summary>
        /// Sphere inner radius
        /// </summary>
        public float SphereInnerRadius
        {
            get
            {
                return this.sphereInnerRadius;
            }
            set
            {
                this.sphereInnerRadius = value;

                this.CalcScale();
            }
        }
        /// <summary>
        /// Sphere outter radius
        /// </summary>
        public float SphereOuterRadius
        {
            get
            {
                return this.sphereOuterRadius;
            }
            set
            {
                this.sphereOuterRadius = value;

                this.CalcScale();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Sky scattering description class</param>
        public SkyScattering(Game game, SkyScatteringDescription description)
            : base(game, description)
        {
            this.Cull = false;

            this.PlanetRadius = description.PlanetRadius;
            this.PlanetAtmosphereRadius = description.PlanetAtmosphereRadius;

            this.RayleighScattering = description.RayleighScattering;
            this.RayleighScaleDepth = description.RayleighScaleDepth;
            this.MieScattering = description.MieScattering;
            this.MiePhaseAssymetry = description.MiePhaseAssymetry;
            this.MieScaleDepth = description.MieScaleDepth;

            this.WaveLength = description.WaveLength;
            this.Brightness = description.Brightness;

            this.sphereInnerRadius = 1.0f;
            this.sphereOuterRadius = this.sphereInnerRadius * 1.025f;
            this.CalcScale();

            this.InitializeBuffers();

            this.bufferManager.CreateBuffers(game.Graphics, this.Name, false, 0);
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.bufferManager);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draws content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var keyLight = context.Lights.KeyLight;
            if (keyLight != null && this.indexCount > 0)
            {
                this.bufferManager.SetBuffers(this.Game.Graphics);

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexCount / 3;
                }

                var effect = DrawerPool.EffectDefaultSkyScattering;
                var technique = effect.GetTechnique(VertexTypes.Position, false, DrawingStages.Drawing, context.DrawerMode);

                this.bufferManager.SetInputAssembler(this.Game.Graphics, technique, VertexTypes.Position, PrimitiveTopology.TriangleList);

                effect.UpdatePerFrame(
                    Matrix.Translation(context.EyePosition),
                    context.ViewProjection,
                    this.PlanetRadius,
                    this.PlanetAtmosphereRadius,
                    this.SphereOuterRadius,
                    this.SphereInnerRadius,
                    this.Brightness,
                    this.RayleighScattering,
                    this.RayleighScattering4PI,
                    this.MieScattering,
                    this.MieScattering4PI,
                    this.InvWaveLength4,
                    this.ScatteringScale,
                    this.RayleighScaleDepth,
                    context.Lights.FogColor,
                    keyLight.Direction);

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, this.indexBufferOffset, this.vertexBufferOffset);

                    Counters.DrawCallsPerFrame++;
                }
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        private void InitializeBuffers()
        {
            Vector3[] vData;
            uint[] iData;
            GeometryUtil.CreateSphere(1, 10, 75, out vData, out iData);

            VertexPosition[] vertices = VertexPosition.Generate(vData);

            var indices = GeometryUtil.ChangeCoordinate(iData);

            this.bufferManager.Add(0, vertices, out this.vertexBufferOffset, out this.vertexBufferSlot);
            this.bufferManager.Add(0, indices, out this.indexBufferOffset);

            this.vertexCount = vertices.Length;
            this.indexCount = indices.Length;
        }
        /// <summary>
        /// Calc current scattering scale from sphere radius values
        /// </summary>
        private void CalcScale()
        {
            this.ScatteringScale = 1.0f / (this.SphereOuterRadius - this.SphereInnerRadius);
        }
    }
}
