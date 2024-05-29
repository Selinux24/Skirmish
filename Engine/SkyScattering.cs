using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.SkyScattering;
    using Engine.Common;

    /// <summary>
    /// Scattered sky
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class SkyScattering(Scene scene, string id, string name) : Drawable<SkyScatteringDescription>(scene, id, name)
    {
        /// <summary>
        /// Vernier scale
        /// </summary>
        /// <param name="cos">Cosine</param>
        /// <returns>Returns Vernier scale value</returns>
        private static float VernierScale(float cos)
        {
            float icos = 1.0f - cos;

            return 0.25f * MathF.Exp(-0.00287f + icos * (0.459f + icos * (3.83f + icos * (-6.80f + (icos * 5.25f)))));
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

            return 1.5f * ((1.0f - gg) / (2.0f + gg)) * (1.0f + coscos) / MathF.Pow(MathF.Abs(1.0f + gg - 2.0f * g * cos), 1.5f);
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
        /// Sky drawer
        /// </summary>
        private BuiltInSkyScattering skyDrawer = null;
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
        private Color3 wavelength;
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
        public Color3 InvWaveLength4 { get; private set; }
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
                return rayleighScattering;
            }
            set
            {
                rayleighScattering = value;
                RayleighScattering4PI = value * 4.0f * MathUtil.Pi;
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
                return mieScattering;
            }
            set
            {
                mieScattering = value;
                MieScattering4PI = value * 4.0f * MathUtil.Pi;
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
        public Color3 WaveLength
        {
            get
            {
                return wavelength;
            }
            set
            {
                wavelength = value;
                InvWaveLength4 = new Color3(
                    1f / MathF.Pow(value.Red, 4.0f),
                    1f / MathF.Pow(value.Green, 4.0f),
                    1f / MathF.Pow(value.Blue, 4.0f));
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
                return sphereInnerRadius;
            }
            set
            {
                sphereInnerRadius = value;

                CalcScale();
            }
        }
        /// <summary>
        /// Sphere outter radius
        /// </summary>
        public float SphereOuterRadius
        {
            get
            {
                return sphereOuterRadius;
            }
            set
            {
                sphereOuterRadius = value;

                CalcScale();
            }
        }
        /// <summary>
        /// HDR exposure
        /// </summary>
        public float HDRExposure { get; set; }
        /// <summary>
        /// Resolution
        /// </summary>
        public SkyScatteringResolutions Resolution { get; set; }
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
        ~SkyScattering()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
        public override async Task ReadAssets(SkyScatteringDescription description)
        {
            await base.ReadAssets(description);

            PlanetRadius = Description.PlanetRadius;
            PlanetAtmosphereRadius = Description.PlanetAtmosphereRadius;

            RayleighScattering = Description.RayleighScattering;
            RayleighScaleDepth = Description.RayleighScaleDepth;
            MieScattering = Description.MieScattering;
            MiePhaseAssymetry = Description.MiePhaseAssymetry;
            MieScaleDepth = Description.MieScaleDepth;

            WaveLength = Description.WaveLength;
            Brightness = Description.Brightness;
            HDRExposure = Description.HDRExposure;
            Resolution = Description.Resolution;

            sphereInnerRadius = 1.0f;
            sphereOuterRadius = sphereInnerRadius * 1.025f;
            CalcScale();

            InitializeBuffers(Name);
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        private void InitializeBuffers(string name)
        {
            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 1, 10, 75);

            var vertices = VertexPosition.Generate(sphere.Vertices);
            var indices = GeometryUtil.ChangeCoordinate(sphere.Indices);

            vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(name, false, indices);

            skyDrawer = BuiltInShaders.GetDrawer<BuiltInSkyScattering>();
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            var lights = Scene.Lights;

            var keyLight = lights.KeyLight;
            if (keyLight != null)
            {
                lights.BaseFogColor = GetFogColor(keyLight.Direction);
            }
        }
        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            var keyLight = context.Lights.KeyLight;
            if (keyLight == null)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            if (skyDrawer == null)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;

            var skyState = new BuiltInSkyScatteringState
            {
                PlanetRadius = PlanetRadius,
                PlanetAtmosphereRadius = PlanetAtmosphereRadius,
                SphereOuterRadius = SphereOuterRadius,
                SphereInnerRadius = SphereInnerRadius,
                SkyBrightness = Brightness,
                RayleighScattering = RayleighScattering,
                RayleighScattering4PI = RayleighScattering4PI,
                MieScattering = MieScattering,
                MieScattering4PI = MieScattering4PI,
                InvWaveLength4 = InvWaveLength4,
                Scale = ScatteringScale,
                RayleighScaleDepth = RayleighScaleDepth,
                BackColor = context.Lights.FogColor,
                HdrExposure = HDRExposure,
                Samples = (uint)Resolution,
            };
            skyDrawer.Update(dc, keyLight.Direction, skyState);

            var drawOptions = new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            };
            return skyDrawer.Draw(dc, drawOptions);
        }

        /// <summary>
        /// Calc current scattering scale from sphere radius values
        /// </summary>
        private void CalcScale()
        {
            ScatteringScale = 1.0f / (SphereOuterRadius - SphereInnerRadius);
        }

        /// <summary>
        /// Gets the fog color base on light direction
        /// </summary>
        /// <param name="lightDirection">Light direction</param>
        /// <returns>Returns the fog color</returns>
        public Color4 GetFogColor(Vector3 lightDirection)
        {
            var outColor = new Color4(0f, 0f, 0f, 0f);

            Helper.GetAnglesFromVector(Vector3.ForwardLH, out float yaw, out _);
            float originalYaw = yaw;

            float pitch = MathUtil.DegreesToRadians(10.0f);

            uint samples = 10;

            for (uint i = 0; i < samples; i++)
            {
                Helper.GetVectorFromAngles(yaw, pitch, out Vector3 scatterPos);

                scatterPos *= PlanetRadius + PlanetAtmosphereRadius;
                scatterPos.Y -= PlanetRadius;

                GetColor(scatterPos, lightDirection, out Color4 tmpColor);

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

            outColor *= 1f / samples;

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
            var eyePosition = new Vector3(0, viewerHeight, 0);

            float scale = 1.0f / (SphereOuterRadius - SphereInnerRadius);
            float scaleOverScaleDepth = scale / RayleighScaleDepth;
            float rayleighBrightness = RayleighScattering * Brightness * 0.25f;
            float mieBrightness = MieScattering * Brightness * 0.25f;

            var position = scatterPosition / PlanetRadius;
            position.Y += SphereInnerRadius;

            var eyeDirection = position - eyePosition;
            float sampleLength = eyeDirection.Length() * 0.5f;
            float scaledLength = sampleLength * MieScaleDepth;
            eyeDirection.Normalize();
            var sampleRay = eyeDirection * sampleLength;
            float startAngle = Vector3.Dot(eyeDirection, eyePosition);

            float scaleDepth = MathF.Exp(scaleOverScaleDepth * (SphereInnerRadius - viewerHeight));
            float startOffset = scaleDepth * VernierScale(startAngle);

            var samplePoint = eyePosition + sampleRay * 0.5f;

            var frontColor = Color3.Black;

            for (uint i = 0; i < 2; i++)
            {
                float depth = MathF.Exp(scaleOverScaleDepth * (SphereInnerRadius - viewerHeight));

                float height = samplePoint.Length();
                float lightAngle = Vector3.Dot(-lightDirection, samplePoint) / height;
                float cameraAngle = Vector3.Dot(eyeDirection, samplePoint) / height;

                float scatter = startOffset + depth * (VernierScale(lightAngle) - VernierScale(cameraAngle));

                var attenuate = Color3.Black;
                attenuate[0] = MathF.Exp(-scatter * (InvWaveLength4[0] * RayleighScattering4PI + MieScattering4PI));
                attenuate[1] = MathF.Exp(-scatter * (InvWaveLength4[1] * RayleighScattering4PI + MieScattering4PI));
                attenuate[2] = MathF.Exp(-scatter * (InvWaveLength4[2] * RayleighScattering4PI + MieScattering4PI));

                frontColor += attenuate * depth * scaledLength;

                samplePoint += sampleRay;
            }

            var rayleighColor = frontColor * (InvWaveLength4 * rayleighBrightness);

            var direction = Vector3.Normalize(eyePosition - position);
            float miePhase = GetMiePhase(Vector3.Dot(-lightDirection, direction));
            var mieColor = frontColor * mieBrightness;

            var color = rayleighColor + (miePhase * mieColor);

            var expColor = Vector3.Zero;
            expColor.X = 1.0f - MathF.Exp(-HDRExposure * color.Red);
            expColor.Y = 1.0f - MathF.Exp(-HDRExposure * color.Green);
            expColor.Z = 1.0f - MathF.Exp(-HDRExposure * color.Blue);
            expColor.Normalize();

            outColor = new Color4(expColor, 1f);
        }
    }
}
