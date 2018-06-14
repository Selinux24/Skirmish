using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Vernier scale
        /// </summary>
        /// <param name="cos">Cosine</param>
        /// <returns>Returns Vernier scale value</returns>
        private static float VernierScale(float cos)
        {
            float icos = 1.0f - cos;

            return 0.25f * (float)Math.Exp(-0.00287f + icos * (0.459f + icos * (3.83f + icos * ((-6.80f + (icos * 5.25f))))));
        }
        /// <summary>
        /// Mie phase
        /// </summary>
        /// <param name="cos">Cosine</param>
        /// <returns>Returns Mie phase value</returns>
        private static float GetMiePhase(float cos)
        {
            float coscos = cos * cos;
            float g = -0.991f;
            float gg = g * g;

            return 1.5f * ((1.0f - gg) / (2.0f + gg)) * (1.0f + coscos) / (float)Math.Pow(Math.Abs(1.0f + gg - 2.0f * g * cos), 1.5f);
        }

        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
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
        public float RayleighScattering4PI { get; private set; }
        /// <summary>
        /// Mie scattering * 4 * PI
        /// </summary>
        public float MieScattering4PI { get; private set; }
        /// <summary>
        /// Inverse wave length * 4
        /// </summary>
        public Color4 InvWaveLength4 { get; private set; }
        /// <summary>
        /// Scattering Scale
        /// </summary>
        public float ScatteringScale { get; private set; }

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
        /// HDR exposure
        /// </summary>
        public float HDRExposure { get; set; }
        /// <summary>
        /// Resolution
        /// </summary>
        public SkyScatteringResolutionEnum Resolution { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sky scattering description class</param>
        public SkyScattering(Scene scene, SkyScatteringDescription description)
            : base(scene, description)
        {
            this.PlanetRadius = description.PlanetRadius;
            this.PlanetAtmosphereRadius = description.PlanetAtmosphereRadius;

            this.RayleighScattering = description.RayleighScattering;
            this.RayleighScaleDepth = description.RayleighScaleDepth;
            this.MieScattering = description.MieScattering;
            this.MiePhaseAssymetry = description.MiePhaseAssymetry;
            this.MieScaleDepth = description.MieScaleDepth;

            this.WaveLength = description.WaveLength;
            this.Brightness = description.Brightness;
            this.HDRExposure = description.HDRExposure;
            this.Resolution = description.Resolution;

            this.sphereInnerRadius = 1.0f;
            this.sphereOuterRadius = this.sphereInnerRadius * 1.025f;
            this.CalcScale();

            this.InitializeBuffers(description.Name);
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {
            //Remove data from buffer manager
            this.BufferManager.RemoveVertexData(this.vertexBuffer);
            this.BufferManager.RemoveIndexData(this.indexBuffer);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            var keyLight = context.Lights.KeyLight;
            if (keyLight != null)
            {
                context.Lights.BaseFogColor = this.GetFogColor(keyLight.Direction);
            }
        }
        /// <summary>
        /// Draws content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            if (mode.HasFlag(DrawerModesEnum.OpaqueOnly))
            {
                var dwContext = context as DrawContext;

                var keyLight = dwContext.Lights.KeyLight;
                if (keyLight != null && this.indexBuffer.Count > 0)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                    var effect = DrawerPool.EffectDefaultSkyScattering;

                    EngineEffectTechnique technique = null;
                    if (this.Resolution == SkyScatteringResolutionEnum.High)
                    {
                        technique = effect.SkyScatteringHigh;
                    }
                    else if (this.Resolution == SkyScatteringResolutionEnum.Medium)
                    {
                        technique = effect.SkyScatteringMedium;
                    }
                    else
                    {
                        technique = effect.SkyScatteringLow;
                    }

                    this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);
                    this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, Topology.TriangleList);

                    effect.UpdatePerFrame(
                        Matrix.Translation(dwContext.EyePosition),
                        dwContext.ViewProjection,
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
                        dwContext.Lights.FogColor,
                        keyLight.Direction,
                        this.HDRExposure);

                    var graphics = this.Game.Graphics;

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.DrawIndexed(this.indexBuffer.Count, this.indexBuffer.Offset, this.vertexBuffer.Offset);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        private void InitializeBuffers(string name)
        {
            Vector3[] vData;
            uint[] iData;
            GeometryUtil.CreateSphere(1, 10, 75, out vData, out iData);

            var vertices = new List<VertexPosition>();

            foreach (var v in vData)
            {
                vertices.Add(new VertexPosition() { Position = v });
            }

            var indices = GeometryUtil.ChangeCoordinate(iData);

            this.vertexBuffer = this.BufferManager.Add(name, vertices.ToArray(), false, 0);
            this.indexBuffer = this.BufferManager.Add(name, indices, false);
        }
        /// <summary>
        /// Calc current scattering scale from sphere radius values
        /// </summary>
        private void CalcScale()
        {
            this.ScatteringScale = 1.0f / (this.SphereOuterRadius - this.SphereInnerRadius);
        }

        /// <summary>
        /// Gets the fog color base on light direction
        /// </summary>
        /// <param name="lightDirection">Light direction</param>
        /// <returns>Returns the fog color</returns>
        public Color4 GetFogColor(Vector3 lightDirection)
        {
            Color4 outColor = new Color4(0f, 0f, 0f, 0f);

            float yaw;
            float pitch;
            Helper.GetAnglesFromVector(Vector3.ForwardLH, out yaw, out pitch);
            float originalYaw = yaw;

            pitch = MathUtil.DegreesToRadians(10.0f);

            uint samples = 10;

            for (uint i = 0; i < samples; i++)
            {
                Vector3 scatterPos;
                Helper.GetVectorFromAngles(yaw, pitch, out scatterPos);

                scatterPos *= this.PlanetRadius + this.PlanetAtmosphereRadius;
                scatterPos.Y -= this.PlanetRadius;

                Color4 tmpColor;
                this.GetColor(scatterPos, lightDirection, out tmpColor);

                outColor += tmpColor;

                if (i <= samples / 2)
                {
                    yaw += MathUtil.DegreesToRadians(5.0f);
                }
                else
                {
                    originalYaw += MathUtil.DegreesToRadians(-5.0f);

                    yaw = originalYaw;
                }

                yaw = MathUtil.Mod(yaw, MathUtil.TwoPi);
            }

            if (samples > 0)
            {
                outColor *= (1f / (float)samples);
            }

            return outColor;
        }
        /// <summary>
        /// Gets the color at scatter position based on light direction
        /// </summary>
        /// <param name="scatterPosition">Scatter position</param>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="outColor">Resulting color</param>
        private void GetColor(Vector3 scatterPosition, Vector3 lightDirection, out Color4 outColor)
        {
            float viewerHeight = 1f;
            Vector3 eyePosition = new Vector3(0, viewerHeight, 0);

            float scale = 1.0f / (this.SphereOuterRadius - this.SphereInnerRadius);
            float scaleOverScaleDepth = scale / this.RayleighScaleDepth;
            float rayleighBrightness = this.RayleighScattering * this.Brightness * 0.25f;
            float mieBrightness = this.MieScattering * this.Brightness * 0.25f;

            Vector3 position = scatterPosition / this.PlanetRadius;
            position.Y += this.SphereInnerRadius;

            Vector3 eyeDirection = position - eyePosition;
            float sampleLength = eyeDirection.Length() * 0.5f;
            float scaledLength = sampleLength * this.MieScaleDepth;
            eyeDirection.Normalize();
            Vector3 sampleRay = eyeDirection * sampleLength;
            float startAngle = Vector3.Dot(eyeDirection, eyePosition);

            float scaleDepth = (float)Math.Exp(scaleOverScaleDepth * (this.SphereInnerRadius - viewerHeight));
            float startOffset = scaleDepth * VernierScale(startAngle);

            Vector3 samplePoint = eyePosition + sampleRay * 0.5f;

            Color3 frontColor = Color3.Black;

            for (uint i = 0; i < 2; i++)
            {
                float depth = (float)Math.Exp(scaleOverScaleDepth * (this.SphereInnerRadius - viewerHeight));

                float height = samplePoint.Length();
                float lightAngle = Vector3.Dot(-lightDirection, samplePoint) / height;
                float cameraAngle = Vector3.Dot(eyeDirection, samplePoint) / height;

                float scatter = (startOffset + depth * (VernierScale(lightAngle) - VernierScale(cameraAngle)));

                Color3 attenuate = Color3.Black;
                attenuate[0] = (float)Math.Exp(-scatter * (this.InvWaveLength4[0] * this.RayleighScattering4PI + this.MieScattering4PI));
                attenuate[1] = (float)Math.Exp(-scatter * (this.InvWaveLength4[1] * this.RayleighScattering4PI + this.MieScattering4PI));
                attenuate[2] = (float)Math.Exp(-scatter * (this.InvWaveLength4[2] * this.RayleighScattering4PI + this.MieScattering4PI));

                frontColor += attenuate * depth * scaledLength;

                samplePoint += sampleRay;
            }

            Color3 rayleighColor = frontColor * (this.InvWaveLength4.RGB() * rayleighBrightness);

            Vector3 direction = Vector3.Normalize(eyePosition - position);
            float miePhase = GetMiePhase(Vector3.Dot(-lightDirection, direction));
            Color3 mieColor = frontColor * mieBrightness;

            Color3 color = rayleighColor + (miePhase * mieColor);

            Vector3 expColor = Vector3.Zero;
            expColor.X = 1.0f - (float)Math.Exp(-this.HDRExposure * color.Red);
            expColor.Y = 1.0f - (float)Math.Exp(-this.HDRExposure * color.Green);
            expColor.Z = 1.0f - (float)Math.Exp(-this.HDRExposure * color.Blue);
            expColor.Normalize();

            outColor = new Color4(expColor, 1f);
        }
    }
}
