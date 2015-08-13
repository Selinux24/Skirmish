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
            return new SceneLight()
            {
                DirectionalLights = new[]
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
                        Ambient = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Specular = new Color4(0.25f, 0.25f, 0.25f, 1.0f),
                        Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, 0.57735f)),
                    },
                    new SceneLightDirectional()
                    {
                        Ambient = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Specular = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                        Direction = Vector3.Normalize(new Vector3(0.0f, -0.707f, -0.707f)),
                    },
                },
            };
        }

        #endregion

        /// <summary>
        /// Directional lights
        /// </summary>
        private List<SceneLightDirectional> directionalLights = new List<SceneLightDirectional>();
        /// <summary>
        /// Point lights
        /// </summary>
        private List<SceneLightPoint> pointLights = new List<SceneLightPoint>();
        /// <summary>
        /// Spot lights
        /// </summary>
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
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Directional light</param>
        public void Add(SceneLightDirectional light)
        {
            this.directionalLights.Add(light);
        }
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Point light</param>
        public void Add(SceneLightPoint light)
        {
            this.pointLights.Add(light);
        }
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Spot light</param>
        public void Add(SceneLightSpot light)
        {
            this.spotLights.Add(light);
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Directional light</param>
        public void Remove(SceneLightDirectional light)
        {
            if (this.directionalLights.Contains(light))
            {
                this.directionalLights.Remove(light);
            }
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Point light</param>
        public void Remove(SceneLightPoint light)
        {
            if (this.pointLights.Contains(light))
            {
                this.pointLights.Remove(light);
            }
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Spot light</param>
        public void Remove(SceneLightSpot light)
        {
            if (this.spotLights.Contains(light))
            {
                this.spotLights.Remove(light);
            }
        }
        /// <summary>
        /// Clear all lights
        /// </summary>
        public void Clear()
        {
            this.ClearDirectionalLights();
            this.ClearPointLights();
            this.ClearSpotLights();
        }
        /// <summary>
        /// Clear all directional lights
        /// </summary>
        public void ClearDirectionalLights()
        {
            this.directionalLights.Clear();
        }
        /// <summary>
        /// Clear all point lights
        /// </summary>
        public void ClearPointLights()
        {
            this.pointLights.Clear();
        }
        /// <summary>
        /// Clear all spot lights
        /// </summary>
        public void ClearSpotLights()
        {
            this.spotLights.Clear();
        }
    }
}
