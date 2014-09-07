using SharpDX;

namespace Common
{
    using Common.Utils;

    public class SceneLights
    {
        /// <summary>
        /// Luz 1 habilitada
        /// </summary>
        public bool DirectionalLight1Enabled
        {
            get
            {
                return this.DirectionalLight1.Padding == 1f;
            }
            set
            {
                this.DirectionalLight1.Padding = value ? 1f : 0f;
            }
        }
        /// <summary>
        /// Luz 2 habilitada
        /// </summary>
        public bool DirectionalLight2Enabled
        {
            get
            {
                return this.DirectionalLight2.Padding == 1f;
            }
            set
            {
                this.DirectionalLight2.Padding = value ? 1f : 0f;
            }
        }
        /// <summary>
        /// Luz 3 habilitada
        /// </summary>
        public bool DirectionalLight3Enabled
        {
            get
            {
                return this.DirectionalLight3.Padding == 1f;
            }
            set
            {
                this.DirectionalLight3.Padding = value ? 1f : 0f;
            }
        }
        /// <summary>
        /// Luz direccional 1
        /// </summary>
        public DirectionalLight DirectionalLight1;
        /// <summary>
        /// Luz direccional 2
        /// </summary>
        public DirectionalLight DirectionalLight2;
        /// <summary>
        /// Luz direccional 3
        /// </summary>
        public DirectionalLight DirectionalLight3;
        /// <summary>
        /// Luz habilitada
        /// </summary>
        public bool PointLightEnabled
        {
            get
            {
                return this.PointLight.Padding == 1f;
            }
            set
            {
                this.PointLight.Padding = value ? 1f : 0f;
            }
        }
        /// <summary>
        /// Esfera de luz
        /// </summary>
        public PointLight PointLight;
        /// <summary>
        /// Luz habilitada
        /// </summary>
        public bool SpotLightEnabled
        {
            get
            {
                return this.SpotLight.Padding == 1f;
            }
            set
            {
                this.SpotLight.Padding = value ? 1f : 0f;
            }
        }
        /// <summary>
        /// Cono de luz
        /// </summary>
        public SpotLight SpotLight;
        /// <summary>
        /// Distancia de inicio de la niebla
        /// </summary>
        public float FogStart { get; set; }
        /// <summary>
        /// Distancia de niebla
        /// </summary>
        public float FogRange { get; set; }
        /// <summary>
        /// Color de la niebla
        /// </summary>
        public Color4 FogColor { get; set; }
    }
}
