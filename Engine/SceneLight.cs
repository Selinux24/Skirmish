using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene lights
    /// </summary>
    public class SceneLight
    {
        /// <summary>
        /// Enables or disabled first directional light
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
        /// Enables or disabled second directional light
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
        /// Enables or disabled third directional light
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
        /// Gets or sets first directional light
        /// </summary>
        public SceneLightDirectional DirectionalLight1 { get; set; }
        /// <summary>
        /// Gets or sets second directional light
        /// </summary>
        public SceneLightDirectional DirectionalLight2 { get; set; }
        /// <summary>
        /// Gets or sets third directional light
        /// </summary>
        public SceneLightDirectional DirectionalLight3 { get; set; }
        /// <summary>
        /// Enables or disables first point light
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
        /// Gets or sets first point light
        /// </summary>
        public SceneLightPoint PointLight { get; set; }
        /// <summary>
        /// Enables or disables first spot light
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
        /// Gets or sets first spot light
        /// </summary>
        public SceneLightSpot SpotLight { get; set; }
        /// <summary>
        /// Fog start value
        /// </summary>
        public float FogStart = 0f;
        /// <summary>
        /// Fog range value
        /// </summary>
        public float FogRange = 0f;
        /// <summary>
        /// Fog color
        /// </summary>
        public Color4 FogColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Enable shadows
        /// </summary>
        public bool EnableShadows = false;

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
