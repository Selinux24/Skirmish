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

            var defLights = new SceneLights();

            defLights.AddRange(lights);

            return defLights;
        }

        #endregion

        /// <summary>
        /// Directional lights
        /// </summary>
        private readonly List<SceneLightDirectional> directionalLights = new List<SceneLightDirectional>();
        /// <summary>
        /// Point lights
        /// </summary>
        private readonly List<SceneLightPoint> pointLights = new List<SceneLightPoint>();
        /// <summary>
        /// Spot lights
        /// </summary>
        private readonly List<SceneLightSpot> spotLights = new List<SceneLightSpot>();
        /// <summary>
        /// Visible lights
        /// </summary>
        private readonly List<SceneLight> visibleLights = new List<SceneLight>();

        /// <summary>
        /// Gets or sets the hemispheric ambient light
        /// </summary>
        public SceneLightHemispheric HemisphericLigth { get; set; }
        /// <summary>
        /// Gets or sets directional lights
        /// </summary>
        public SceneLightDirectional[] DirectionalLights
        {
            get
            {
                return this.directionalLights.ToArray();
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
        /// Far light distance definition for shadow maps
        /// </summary>
        public float FarLightsDistance { get; set; }
        /// <summary>
        /// Gets or sets the color palette use flag
        /// </summary>
        public bool UseSunColorPalette { get; set; }
        /// <summary>
        /// Sun color palette
        /// </summary>
        public List<Tuple<float, Color4>> SunColorPalette { get; set; }
        /// <summary>
        /// Sun color
        /// </summary>
        public Color4 SunColor { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SceneLights()
        {
            this.FogStart = 0f;
            this.FogRange = 0;
            this.FarLightsDistance = 1000000f;

            this.SunColor = Color.White;

            this.UseSunColorPalette = true;
            this.SunColorPalette = new List<Tuple<float, Color4>>();
            this.SunColorPalette.AddRange(new[]
            {
                new Tuple<float, Color4>(MathUtil.Pi * -1.00f, Color.Black),
                new Tuple<float, Color4>(MathUtil.Pi * 0.02f, Color.Orange),
                new Tuple<float, Color4>(MathUtil.Pi * 0.20f, Color.White),
                new Tuple<float, Color4>(MathUtil.Pi * 0.70f, Color.White),
                new Tuple<float, Color4>(MathUtil.Pi * 0.98f, Color.Orange),
                new Tuple<float, Color4>(MathUtil.Pi * 2.00f, Color.Black),
            });
        }

        /// <summary>
        /// Sets the hemispheric ambient light
        /// </summary>
        /// <param name="hemiLight">Hemispheric light</param>
        public void SetAmbient(SceneLightHemispheric hemiLight)
        {
            this.HemisphericLigth = hemiLight;
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
                    if (light is SceneLightDirectional dirLight) this.directionalLights.Add(dirLight);
                    else if (light is SceneLightPoint pointLight) this.pointLights.Add(pointLight);
                    else if (light is SceneLightSpot spotLight) this.spotLights.Add(spotLight);
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
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        public void Cull(ICullingVolume volume, Vector3 viewerPosition)
        {
            this.visibleLights.Clear();

            this.CullDirectionalLights();
            this.CullPointLights(volume, viewerPosition);
            this.CullSpotLights(volume, viewerPosition);
        }
        /// <summary>
        /// Cull test for directional lighs
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private void CullDirectionalLights()
        {
            var dLights = this.directionalLights.FindAll(l => l.Enabled);
            if (dLights.Count > 0)
            {
                this.visibleLights.AddRange(dLights);
            }
        }
        /// <summary>
        /// Cull test for point lights
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private void CullPointLights(ICullingVolume volume, Vector3 viewerPosition)
        {
            var pLights = this.pointLights.FindAll(l =>
            {
                if (l.Enabled && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                {
                    float d = Vector3.Distance(viewerPosition, l.Position);

                    return (l.Radius / d) >= (1f / GameEnvironment.LODDistanceLow);
                }

                return false;
            });
            if (pLights.Count > 0)
            {
                pLights.Sort((l1, l2) =>
                {
                    float d1 = Vector3.Distance(viewerPosition, l1.Position);
                    float d2 = Vector3.Distance(viewerPosition, l2.Position);

                    float f1 = l1.Radius / d1 == 0 ? 1 : d1;
                    float f2 = l2.Radius / d2 == 0 ? 1 : d2;

                    return f1.CompareTo(f2);
                });

                this.visibleLights.AddRange(pLights);
            }
        }
        /// <summary>
        /// Cull test for spot lights
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private void CullSpotLights(ICullingVolume volume, Vector3 viewerPosition)
        {
            var sLights = this.spotLights.FindAll(l =>
            {
                if (l.Enabled && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                {
                    float d = Vector3.Distance(viewerPosition, l.Position);

                    return (l.Radius / d) >= (1f / GameEnvironment.LODDistanceLow);
                }

                return false;
            });
            if (sLights.Count > 0)
            {
                sLights.Sort((l1, l2) =>
                {
                    float d1 = Vector3.Distance(viewerPosition, l1.Position);
                    float d2 = Vector3.Distance(viewerPosition, l2.Position);

                    float f1 = l1.Radius / d1 == 0 ? 1 : d1;
                    float f2 = l2.Radius / d2 == 0 ? 1 : d2;

                    return f1.CompareTo(f2);
                });

                this.visibleLights.AddRange(sLights);
            }
        }
        /// <summary>
        /// Gets the visible hemispheric light
        /// </summary>
        /// <returns>Returns the visible hemispheric light</returns>
        public SceneLightHemispheric GetVisibleHemisphericLight()
        {
            return this.HemisphericLigth != null && this.HemisphericLigth.Enabled ? this.HemisphericLigth : null;
        }
        /// <summary>
        /// Gets the visible directional lights
        /// </summary>
        /// <returns>Returns the visible directional lights array</returns>
        public SceneLightDirectional[] GetVisibleDirectionalLights()
        {
            return this.visibleLights
                .FindAll(l => l is SceneLightDirectional)
                .Cast<SceneLightDirectional>()
                .ToArray();
        }
        /// <summary>
        /// Gets the visible point lights
        /// </summary>
        /// <returns>Returns the visible point lights array</returns>
        public SceneLightPoint[] GetVisiblePointLights()
        {
            return this.visibleLights
                .FindAll(l => l is SceneLightPoint)
                .Cast<SceneLightPoint>()
                .ToArray();
        }
        /// <summary>
        /// Gets the visible spot lights
        /// </summary>
        /// <returns>Returns the visible spot lights array</returns>
        public SceneLightSpot[] GetVisibleSpotLights()
        {
            return this.visibleLights
                .FindAll(l => l is SceneLightSpot)
                .Cast<SceneLightSpot>()
                .ToArray();
        }

        /// <summary>
        /// Gets a collection of directional lights that cast shadow
        /// </summary>
        /// <returns>Returns a light collection</returns>
        public ISceneLightDirectional[] GetDirectionalShadowCastingLights()
        {
            return this.visibleLights
                .Where(l => l.CastShadow && l is SceneLightDirectional)
                .Select(l => (SceneLightDirectional)l)
                .ToArray();
        }
        /// <summary>
        /// Gets a collection of point lights that cast shadow
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public ISceneLightPoint[] GetPointShadowCastingLights(Vector3 eyePosition)
        {
            return this.visibleLights
                .Where(l => l.CastShadow && l is SceneLightPoint)
                .Select(l => (SceneLightPoint)l)
                .OrderBy(l => Vector3.DistanceSquared(l.Position, eyePosition))
                .ToArray();
        }
        /// <summary>
        /// Gets a collection of spot lights that cast shadow
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public ISceneLightSpot[] GetSpotShadowCastingLights(Vector3 eyePosition)
        {
            return this.visibleLights
                .Where(l => l.CastShadow && l is ISceneLightSpot)
                .Select(l => (ISceneLightSpot)l)
                .OrderBy(l => Vector3.DistanceSquared(l.Position, eyePosition))
                .ToArray();
        }

        /// <summary>
        /// Update directional lights with time of day controller
        /// </summary>
        /// <param name="timeOfDay">Time of day</param>
        public void UpdateLights(TimeOfDay timeOfDay)
        {
            if (timeOfDay.Updated)
            {
                float b = Math.Max(0, -(float)Math.Cos(timeOfDay.Elevation) + 0.15f) * 1.5f;
                this.Intensity = Math.Min(b, 1f);

                Vector3 keyDir = timeOfDay.LightDirection;
                Vector3 backDir = -Vector3.Reflect(keyDir, Vector3.Up);

                float tan = (float)Math.Tan(timeOfDay.Elevation);
                Vector3 fillDir = tan >= 0f ? Vector3.Cross(keyDir, backDir) : Vector3.Cross(backDir, keyDir);

                if (this.UseSunColorPalette)
                {
                    this.SunColor = this.GetSunColor(timeOfDay);
                }

                var keyLight = this.KeyLight;
                if (keyLight != null)
                {
                    keyLight.Brightness = keyLight.BaseBrightness * b;

                    keyLight.Direction = keyDir;

                    if (this.UseSunColorPalette)
                    {
                        keyLight.SpecularColor = this.SunColor * b;
                    }
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
        /// <summary>
        /// Gets the sun color based on time of day
        /// </summary>
        /// <param name="timeOfDay">Time of day class</param>
        /// <returns>Returns the color base on time of day meridian angle</returns>
        private Color4 GetSunColor(TimeOfDay timeOfDay)
        {
            float angle = MathUtil.Clamp(timeOfDay.MeridianAngle - MathUtil.PiOverTwo, 0, MathUtil.Pi);

            for (int i = 0; i < this.SunColorPalette.Count; i++)
            {
                if (this.SunColorPalette[i].Item1 > angle)
                {
                    if (i > 0)
                    {
                        var from = this.SunColorPalette[i - 1];
                        var to = this.SunColorPalette[i];
                        float amount = (angle - from.Item1) / (to.Item1 - from.Item1);
                        return Color4.Lerp(from.Item2, to.Item2, amount);
                    }
                    else
                    {
                        return this.SunColorPalette[i].Item2;
                    }
                }
            }

            return Color4.White;
        }
    }
}
