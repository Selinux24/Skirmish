using SharpDX;

namespace Engine
{
    /// <summary>
    /// Composición de luces de la escena
    /// </summary>
    public class SceneLight
    {
        /// <summary>
        /// Luz 1 habilitada
        /// </summary>
        public bool DirectionalLight1Enabled
        {
            get
            {
                return this.DirectionalLight1.Enabled;
            }
            set
            {
                this.DirectionalLight1.Enabled = value;
            }
        }
        /// <summary>
        /// Luz 2 habilitada
        /// </summary>
        public bool DirectionalLight2Enabled
        {
            get
            {
                return this.DirectionalLight2.Enabled;
            }
            set
            {
                this.DirectionalLight2.Enabled = value;
            }
        }
        /// <summary>
        /// Luz 3 habilitada
        /// </summary>
        public bool DirectionalLight3Enabled
        {
            get
            {
                return this.DirectionalLight3.Enabled;
            }
            set
            {
                this.DirectionalLight3.Enabled = value;
            }
        }
        /// <summary>
        /// Luz direccional 1
        /// </summary>
        public SceneLightDirectional DirectionalLight1 { get; set; }
        /// <summary>
        /// Luz direccional 2
        /// </summary>
        public SceneLightDirectional DirectionalLight2 { get; set; }
        /// <summary>
        /// Luz direccional 3
        /// </summary>
        public SceneLightDirectional DirectionalLight3 { get; set; }
        /// <summary>
        /// Luz habilitada
        /// </summary>
        public bool PointLightEnabled
        {
            get
            {
                return this.PointLight.Enabled;
            }
            set
            {
                this.PointLight.Enabled = value;
            }
        }
        /// <summary>
        /// Esfera de luz
        /// </summary>
        public SceneLightPoint PointLight { get; set; }
        /// <summary>
        /// Luz habilitada
        /// </summary>
        public bool SpotLightEnabled
        {
            get
            {
                return this.SpotLight.Enabled;
            }
            set
            {
                this.SpotLight.Enabled = value;
            }
        }
        /// <summary>
        /// Cono de luz
        /// </summary>
        public SceneLightSpot SpotLight { get; set; }
        /// <summary>
        /// Distancia de inicio de la niebla
        /// </summary>
        public float FogStart = 0f;
        /// <summary>
        /// Distancia de niebla
        /// </summary>
        public float FogRange = 0f;
        /// <summary>
        /// Color de la niebla
        /// </summary>
        public Color4 FogColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

        /// <summary>
        /// Constructor
        /// </summary>
        public SceneLight()
        {
            this.DirectionalLight1 = new SceneLightDirectional();
            this.DirectionalLight2 = new SceneLightDirectional();
            this.DirectionalLight3 = new SceneLightDirectional();
            this.PointLight = new SceneLightPoint();
            this.SpotLight = new SceneLightSpot();
        }
    }
}
