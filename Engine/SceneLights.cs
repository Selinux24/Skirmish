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
        /// Create default set of lights
        /// </summary>
        /// <returns>Returns default set of ligths</returns>
        public static SceneLights CreateDefault()
        {
            var lights = new[]
            {
                SceneLightDirectional.KeyLight,
                SceneLightDirectional.FillLight,
                SceneLightDirectional.BackLight,
            };

            return new SceneLights()
            {
                GlobalAmbientLight = 0.1f,
                DirectionalLights = lights,
                visibleDirectionalLights = lights,
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
        private SceneLightDirectional[] visibleDirectionalLights = new SceneLightDirectional[] { };
        /// <summary>
        /// Visible position lights
        /// </summary>
        private List<ISceneLightPosition> visiblePositionLights = new List<ISceneLightPosition>();

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
        /// Fill light
        /// </summary>
        public SceneLightDirectional FillLight
        {
            get
            {
                return this.DirectionalLights.Length > 1 ? this.DirectionalLights[1] : null;
            }
        }
        /// <summary>
        /// Back light
        /// </summary>
        public SceneLightDirectional BackLight
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
        public float FogStart { get; set; }
        /// <summary>
        /// Fog range value
        /// </summary>
        public float FogRange { get; set; }
        /// <summary>
        /// Base fog color
        /// </summary>
        public Color4 BaseFogColor { get; set; }
        /// <summary>
        /// Fog color
        /// </summary>
        public Color4 FogColor { get; protected set; }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; protected set; }
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
        /// High definition shadows distance
        /// </summary>
        public float ShadowHDDistance { get; set; }
        /// <summary>
        /// Low definition shadows distance
        /// </summary>
        public float ShadowLDDistance { get; set; }
        /// <summary>
        /// Far light distance definition for shadow maps
        /// </summary>
        public float FarLightsDistance { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SceneLights()
        {
            this.FogStart = 0f;
            this.FogRange = 0;
            this.ShadowHDDistance = 50f;
            this.ShadowLDDistance = 150f;
            this.FarLightsDistance = 1000000f;
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

            var pLights = this.pointLights.FindAll(l =>
            {
                if (l.Enabled == true && frustum.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                {
                    float d = Vector3.Distance(viewerPosition, l.Position);

                    return (l.Radius / d) >= (1f / GameEnvironment.LODDistanceLow);
                }

                return false;
            });
            var sLights = this.spotLights.FindAll(l =>
            {
                if (l.Enabled == true && frustum.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                {
                    float d = Vector3.Distance(viewerPosition, l.Position);

                    return (l.Radius / d) >= (1f / GameEnvironment.LODDistanceLow);
                }

                return false;
            });

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
            if (timeOfDay.Updated)
            {
                float e = Math.Max(0, -(float)Math.Cos(timeOfDay.Elevation) + 0.15f) * 1.5f;
                float b = e;
                float ga = Math.Min(e, 0.5f);
                this.Intensity = Math.Min(e, 1f);

                Vector3 keyDir = timeOfDay.LightDirection;
                Vector3 backDir = new Vector3(-keyDir.X, keyDir.Y, -keyDir.Z);

                float tan = (float)Math.Tan(timeOfDay.Elevation);
                Vector3 fillDir = tan >= 0f ? Vector3.Cross(keyDir, backDir) : Vector3.Cross(backDir, keyDir);

                this.GlobalAmbientLight = ga;

                var keyLight = this.KeyLight;
                if (keyLight != null)
                {
                    keyLight.Brightness = keyLight.BaseBrightness * b;

                    keyLight.Direction = keyDir;
                }

                var fillLight = this.FillLight;
                if (fillLight != null)
                {
                    fillLight.Brightness = fillLight.BaseBrightness * b;

                    fillLight.Direction = fillDir;
                }

                var backLight = this.BackLight;
                if (backLight != null)
                {
                    backLight.Brightness = backLight.BaseBrightness * b;

                    backLight.Direction = backDir;
                }

                this.FogColor = this.BaseFogColor * this.Intensity;
            }
        }
    }
}
