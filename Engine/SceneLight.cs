using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene lights
    /// </summary>
    public class SceneLight
    {
        #region Preconfigured lights

        /// <summary>
        /// Default ligths
        /// </summary>
        public static readonly SceneLight Default = CreateDefault();
        /// <summary>
        /// Empty lights
        /// </summary>
        public static readonly SceneLight Empty = new SceneLight();

        /// <summary>
        /// Create default set of lights
        /// </summary>
        /// <returns>Returns default set of ligths</returns>
        public static SceneLight CreateDefault()
        {
            SceneLight res = new SceneLight();

            res.DirectionalLights = new[]
            {
                new SceneLightDirectional()
                {
                    Ambient = new Color4(0.8f, 0.8f, 0.8f, 1.0f),
                    Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                    Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Direction = Vector3.Normalize(new Vector3(0.57735f, -0.57735f, 0.57735f)),
                },
                new SceneLightDirectional()
                {
                    Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                    Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Specular = new Color4(0.25f, 0.25f, 0.25f, 1.0f),
                    Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, 0.57735f)),
                },
                new SceneLightDirectional()
                {
                    Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                    Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Specular = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                    Direction = Vector3.Normalize(new Vector3(0.0f, -0.707f, -0.707f)),
                },
            };

            res.FogColor = Color.Transparent;
            res.FogStart = 0;
            res.FogRange = 0;

            res.EnableShadows = false;

            return res;
        }

        #endregion

        private List<SceneLightDirectional> directionalLights = new List<SceneLightDirectional>();
        private List<SceneLightPoint> pointLights = new List<SceneLightPoint>();
        private List<SceneLightSpot> spotLights = new List<SceneLightSpot>();

        /// <summary>
        /// Gets or sets directional lights
        /// </summary>
        public SceneLightDirectional[] DirectionalLights
        {
            get
            {
                return this.directionalLights.ToArray();
            }
            set
            {
                this.directionalLights.Clear();

                if (value != null && value.Length > 0)
                {
                    this.directionalLights.AddRange(value);
                }
            }
        }
        /// <summary>
        /// Gets or sets point lights
        /// </summary>
        public SceneLightPoint[] PointLights
        {
            get
            {
                return this.pointLights.ToArray();
            }
            set
            {
                this.pointLights.Clear();

                if (value != null && value.Length > 0)
                {
                    this.pointLights.AddRange(value);
                }
            }
        }
        /// <summary>
        /// Gets or sets spot lights
        /// </summary>
        public SceneLightSpot[] SpotLights
        {
            get
            {
                return this.spotLights.ToArray();
            }
            set
            {
                this.spotLights.Clear();

                if (value != null && value.Length > 0)
                {
                    this.spotLights.AddRange(value);
                }
            }
        }
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

        }
    }
}
