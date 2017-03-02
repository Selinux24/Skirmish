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
        /// Index buffer slot
        /// </summary>
        private int indexBufferSlot = -1;
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
        /// HDR exposure
        /// </summary>
        public float HDRExposure { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Sky scattering description class</param>
        public SkyScattering(Game game, BufferManager bufferManager, SkyScatteringDescription description)
            : base(game, bufferManager, description)
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
            this.HDRExposure = description.HDRExposure;

            this.sphereInnerRadius = 1.0f;
            this.sphereOuterRadius = this.sphereInnerRadius * 1.025f;
            this.CalcScale();

            this.InitializeBuffers();
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {

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
                this.BufferManager.SetIndexBuffer(this.indexBufferSlot);

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexCount / 3;
                }

                var effect = DrawerPool.EffectDefaultSkyScattering;
                var technique = effect.GetTechnique(VertexTypes.Position, false, DrawingStages.Drawing, context.DrawerMode);

                this.BufferManager.SetInputAssembler(technique, this.vertexBufferSlot, PrimitiveTopology.TriangleList);

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
                    keyLight.Direction,
                    this.HDRExposure);

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

            VertexPosition[] vertices = new VertexPosition[vData.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new VertexPosition() { Position = vData[i] };
            }

            var indices = GeometryUtil.ChangeCoordinate(iData);

            this.BufferManager.Add(this.Name, vertices, false, 0, out this.vertexBufferOffset, out this.vertexBufferSlot);
            this.BufferManager.Add(this.Name, indices, false, out this.indexBufferOffset, out this.indexBufferSlot);

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


        public Color4 GetFogColor(Vector3 lightDirection)
        {
            Color4 outColor = new Color4(0f, 0f, 0f, 0f);

            Vector3 fwd = Vector3.ForwardLH;

            float yaw = 0;
            float pitch = 0;
            Helper.GetAnglesFromVector(fwd, out yaw, out pitch);
            float originalYaw = yaw;
            pitch = MathUtil.DegreesToRadians(10.0f);

            uint i = 0;
            for (i = 0; i < 10; i++)
            {
                Vector3 scatterPos;
                Helper.GetVectorFromAngles(yaw, pitch, out scatterPos);

                scatterPos.X *= this.PlanetRadius + this.PlanetAtmosphereRadius;
                scatterPos.Y *= this.PlanetRadius + this.PlanetAtmosphereRadius;
                scatterPos.Z *= this.PlanetRadius + this.PlanetAtmosphereRadius;
                scatterPos.Y -= this.PlanetRadius;

                Color4 tmpColor;
                this.GetColor(scatterPos, lightDirection, out tmpColor);

                outColor += tmpColor;

                if (i <= 5)
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

            if (i > 0)
            {
                outColor = outColor * (1f / (float)i);
            }

            return outColor;
        }

        private void GetColor(Vector3 pos, Vector3 lightDirection, out Color4 outColor)
        {
            float scale = 1.0f / (this.SphereOuterRadius - this.SphereInnerRadius);
            float scaleOverScaleDepth = scale / this.RayleighScaleDepth;
            float rayleighBrightness = this.RayleighScattering * this.Brightness * 0.25f;
            float mieBrightness = this.MieScattering * this.Brightness * 0.25f;

            Vector3 v3Pos = pos / this.PlanetRadius;
            v3Pos.Z += this.SphereInnerRadius;

            float viewerHeight = 1;

            Vector3 newCamPos = new Vector3(0, 0, viewerHeight);

            Vector3 v3Ray = v3Pos - newCamPos;
            float fFar = v3Ray.Length();
            v3Ray.Normalize();

            Vector3 v3Start = newCamPos;
            float fDepth1 = (float)Math.Exp(scaleOverScaleDepth * (this.SphereInnerRadius - viewerHeight));
            float fStartAngle = Vector3.Dot(v3Ray, v3Start);

            float vStartAngle = this.VernierScale(fStartAngle);
            float fStartOffset = fDepth1 * vStartAngle;

            float fSampleLength = fFar / 2.0f;
            float fScaledLength = fSampleLength * this.MieScaleDepth;
            Vector3 v3SampleRay = v3Ray * fSampleLength;
            Vector3 v3SamplePoint = v3Start + v3SampleRay * 0.5f;

            Vector3 v3FrontColor = new Vector3(0, 0, 0);
            for (uint i = 0; i < 2; i++)
            {
                float fHeight = v3SamplePoint.Length();
                float fDepth = (float)Math.Exp(scaleOverScaleDepth * (this.SphereInnerRadius - viewerHeight));
                float fLightAngle = Vector3.Dot(lightDirection, v3SamplePoint) / fHeight;
                float fCameraAngle = Vector3.Dot(v3Ray, v3SamplePoint) / fHeight;

                float vLightAngle = this.VernierScale(fLightAngle);
                float vCameraAngle = this.VernierScale(fCameraAngle);

                float fScatter = (fStartOffset + fDepth * (vLightAngle - vCameraAngle));
                Vector3 v3Attenuate = new Vector3(0, 0, 0);

                float tmp = (float)Math.Exp(-fScatter * (this.InvWaveLength4[0] * this.RayleighScattering4PI + this.MieScattering4PI));
                v3Attenuate.X = tmp;

                tmp = (float)Math.Exp(-fScatter * (this.InvWaveLength4[1] * this.RayleighScattering4PI + this.MieScattering4PI));
                v3Attenuate.Y = tmp;

                tmp = (float)Math.Exp(-fScatter * (this.InvWaveLength4[2] * this.RayleighScattering4PI + this.MieScattering4PI));
                v3Attenuate.Z = tmp;

                v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
                v3SamplePoint += v3SampleRay;
            }

            Vector3 mieColor = v3FrontColor * mieBrightness;
            Vector3 rayleighColor = v3FrontColor * (this.InvWaveLength4.ToVector3() * rayleighBrightness);
            Vector3 v3Direction = newCamPos - v3Pos;
            v3Direction.Normalize();

            float fCos = Vector3.Dot(lightDirection, v3Direction) / v3Direction.Length();

            float miePhase = this.GetMiePhase(fCos);

            Vector3 color = rayleighColor + (miePhase * mieColor);
            Color4 tmpc = new Color4(color.X, color.Y, color.Z, color.Y);

            Vector3 expColor = new Vector3(0, 0, 0);
            expColor.X = 1.0f - (float)Math.Exp(-this.HDRExposure * color.X);
            expColor.Y = 1.0f - (float)Math.Exp(-this.HDRExposure * color.Y);
            expColor.Z = 1.0f - (float)Math.Exp(-this.HDRExposure * color.Z);

            tmpc = new Color4(expColor.X, expColor.Y, expColor.Z, 1.0f);

            float len = expColor.Length();
            if (len > 0)
                expColor /= len;

            outColor = new Color4(expColor.X, expColor.Y, expColor.Z, 1.0f);
        }

        private float VernierScale(float fCos)
        {
            float x = 1.0f - fCos;

            return 0.25f * (float)Math.Exp(-0.00287f + x * (0.459f + x * (3.83f + x * ((-6.80f + (x * 5.25f))))));
        }

        private float GetMiePhase(float fCos)
        {
            float fCos2 = fCos * fCos;
            float g = -0.991f;
            float g2 = g * g;

            return 1.5f * ((1.0f - g2) / (2.0f + g2)) * (1.0f + fCos2) / (float)Math.Pow(Math.Abs(1.0f + g2 - 2.0f * g * fCos), 1.5f);
        }
    }
}
