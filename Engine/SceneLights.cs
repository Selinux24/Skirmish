using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene lights
    /// </summary>
    public class SceneLights
    {
        #region Preconfigured lights

        /// <summary>
        /// Default ligths
        /// </summary>
        public static readonly SceneLights Default = CreateDefault();
        /// <summary>
        /// Empty lights
        /// </summary>
        public static readonly SceneLights Empty = new SceneLights();

        /// <summary>
        /// Create default set of lights
        /// </summary>
        /// <returns>Returns default set of ligths</returns>
        public static SceneLights CreateDefault()
        {
            return new SceneLights()
            {
                GlobalAmbientLight = 0.1f,
                DirectionalLights = new[]
                {
                    SceneLightDirectional.Primary,
                    SceneLightDirectional.Secondary,
                    SceneLightDirectional.Tertiary,
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
        /// Visible directional lights
        /// </summary>
        private SceneLightDirectional[] visibleDirectionalLights = null;
        /// <summary>
        /// Visible position lights
        /// </summary>
        private List<ISceneLightPosition> visiblePositionLights = new List<ISceneLightPosition>();
        /// <summary>
        /// Fog color
        /// </summary>
        private Color4 fogColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

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
        /// Key light
        /// </summary>
        public SceneLightDirectional KeyLight
        {
            get
            {
                return this.DirectionalLights.Length > 0 ? this.DirectionalLights[0] : null;
            }
        }
        /// <summary>
        /// Back light
        /// </summary>
        public SceneLightDirectional BackLight
        {
            get
            {
                return this.DirectionalLights.Length > 1 ? this.DirectionalLights[1] : null;
            }
        }
        /// <summary>
        /// Fill light
        /// </summary>
        public SceneLightDirectional FillLight
        {
            get
            {
                return this.DirectionalLights.Length > 2 ? this.DirectionalLights[2] : null;
            }
        }
        /// <summary>
        /// Global ambient light
        /// </summary>
        public float GlobalAmbientLight { get; set; }
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
        public Color4 FogColor
        {
            get
            {
                return this.fogColor * (this.KeyLight != null ? this.KeyLight.Brightness : 1f);
            }
            set
            {
                this.fogColor = value;
            }
        }
        /// <summary>
        /// Gets light by name
        /// </summary>
        /// <param name="name">Light name</param>
        /// <returns>Returns the first light with the specified name.</returns>
        /// <remarks>
        /// The property searches into the three light collections:
        /// - Directional lights
        /// - Point lights
        /// - Spot lights
        /// Returns the first occurrence using that order
        /// </remarks>
        public SceneLight this[string name]
        {
            get
            {
                SceneLight light = null;

                light = this.directionalLights.Find(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
                if (light != null) return light;

                light = this.pointLights.Find(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
                if (light != null) return light;

                light = this.spotLights.Find(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
                if (light != null) return light;

                return null;
            }
        }
        /// <summary>
        /// Gets a collection of light that cast shadow
        /// </summary>
        public SceneLightDirectional[] ShadowCastingLights
        {
            get
            {
                return this.directionalLights.FindAll(l => l.CastShadow).ToArray();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SceneLights()
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
        /// Adds the specified light list to colection
        /// </summary>
        /// <param name="sceneLights">Lights</param>
        public void AddRange(IEnumerable<SceneLight> sceneLights)
        {
            if (sceneLights != null)
            {
                foreach (var light in sceneLights)
                {
                    if (light is SceneLightDirectional) this.directionalLights.Add((SceneLightDirectional)light);
                    else if (light is SceneLightPoint) this.pointLights.Add((SceneLightPoint)light);
                    else if (light is SceneLightSpot) this.spotLights.Add((SceneLightSpot)light);
                }
            }
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
        /// <summary>
        /// Cull test
        /// </summary>
        /// <param name="frustum">Viewing frustum</param>
        /// <param name="viewerPosition">Viewer position</param>
        public void Cull(BoundingFrustum frustum, Vector3 viewerPosition)
        {
            this.visibleDirectionalLights = this.directionalLights.FindAll(l => l.Enabled == true).ToArray();

            var pLights = this.pointLights.FindAll(l => l.Enabled == true && frustum.Contains(l.BoundingSphere) != ContainmentType.Disjoint);
            var sLights = this.spotLights.FindAll(l => l.Enabled == true && frustum.Contains(l.BoundingSphere) != ContainmentType.Disjoint);

            this.visiblePositionLights.Clear();
            if (pLights.Count > 0) pLights.ForEach(l => this.visiblePositionLights.Add(l));
            if (sLights.Count > 0) sLights.ForEach(l => this.visiblePositionLights.Add(l));

            this.visiblePositionLights.Sort((l1, l2) =>
            {
                float d1 = Vector3.Distance(viewerPosition, l1.Position);
                float d2 = Vector3.Distance(viewerPosition, l2.Position);

                float f1 = l1.Radius / d1 == 0 ? 1 : d1;
                float f2 = l2.Radius / d2 == 0 ? 1 : d2;

                return f1.CompareTo(f2);
            });
        }
        /// <summary>
        /// Gets the visible directional lights
        /// </summary>
        /// <returns>Returns the visible directional lights array</returns>
        public SceneLightDirectional[] GetVisibleDirectionalLights()
        {
            return this.visibleDirectionalLights;
        }
        /// <summary>
        /// Gets the visible point lights
        /// </summary>
        /// <returns>Returns the visible point lights array</returns>
        public SceneLightPoint[] GetVisiblePointLights()
        {
            return this.visiblePositionLights.FindAll(l => l is SceneLightPoint).Cast<SceneLightPoint>().ToArray();
        }
        /// <summary>
        /// Gets the visible spot lights
        /// </summary>
        /// <returns>Returns the visible spot lights array</returns>
        public SceneLightSpot[] GetVisibleSpotLights()
        {
            return this.visiblePositionLights.FindAll(l => l is SceneLightSpot).Cast<SceneLightSpot>().ToArray();
        }

        /// <summary>
        /// Update directional lights with time of day controller
        /// </summary>
        /// <param name="timeOfDay">Time of day</param>
        public void UpdateLights(TimeOfDay timeOfDay)
        {
            if (timeOfDay.Running)
            {
                float e = Math.Max(0, -(float)Math.Cos(timeOfDay.Elevation));
                float b = Math.Min(e + (8f * e), 1);
                float ga = MathUtil.Clamp(e, 0.2f, 0.8f);

                Vector3 keyDir = timeOfDay.LightDirection;
                Vector3 backDir = new Vector3(-keyDir.X, keyDir.Y, -keyDir.Z);

                this.GlobalAmbientLight = ga;

                var keyLight = this.KeyLight;
                if (keyLight != null)
                {
                    keyLight.Brightness = b;

                    keyLight.Direction = keyDir;
                }

                var backLight = this.BackLight;
                if (backLight != null)
                {
                    backLight.Brightness = b * 0.5f;

                    backLight.Direction = backDir;
                }
            }
        }
    }
}
