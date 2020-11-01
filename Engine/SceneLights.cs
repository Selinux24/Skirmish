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
        /// <param name="scene">Scene</param>
        /// <returns>Returns default set of ligths</returns>
        public static SceneLights CreateDefault(Scene scene)
        {
            var lights = new[]
            {
                SceneLightDirectional.KeyLight,
                SceneLightDirectional.FillLight,
                SceneLightDirectional.BackLight,
            };

            var defLights = new SceneLights(scene);

            defLights.AddRange(lights);

            return defLights;
        }

        #endregion

        /// <summary>
        /// Scene
        /// </summary>
        private readonly Scene scene = null;
        /// <summary>
        /// Directional lights
        /// </summary>
        private readonly List<ISceneLightDirectional> directionalLights = new List<ISceneLightDirectional>();
        /// <summary>
        /// Point lights
        /// </summary>
        private readonly List<ISceneLightPoint> pointLights = new List<ISceneLightPoint>();
        /// <summary>
        /// Spot lights
        /// </summary>
        private readonly List<ISceneLightSpot> spotLights = new List<ISceneLightSpot>();
        /// <summary>
        /// Visible lights
        /// </summary>
        private readonly List<ISceneLight> visibleLights = new List<ISceneLight>();

        /// <summary>
        /// Gets or sets the hemispheric ambient light
        /// </summary>
        public ISceneLightHemispheric HemisphericLigth { get; set; }
        /// <summary>
        /// Gets or sets directional lights
        /// </summary>
        public ISceneLightDirectional[] DirectionalLights
        {
            get
            {
                return directionalLights.ToArray();
            }
        }
        /// <summary>
        /// Gets or sets point lights
        /// </summary>
        public ISceneLightPoint[] PointLights
        {
            get
            {
                return pointLights.ToArray();
            }
        }
        /// <summary>
        /// Gets or sets spot lights
        /// </summary>
        public ISceneLightSpot[] SpotLights
        {
            get
            {
                return spotLights.ToArray();
            }
        }
        /// <summary>
        /// Key light
        /// </summary>
        public ISceneLightDirectional KeyLight
        {
            get
            {
                return DirectionalLights.Length > 0 ? DirectionalLights[0] : null;
            }
        }
        /// <summary>
        /// Fill light
        /// </summary>
        public ISceneLightDirectional FillLight
        {
            get
            {
                return DirectionalLights.Length > 1 ? DirectionalLights[1] : null;
            }
        }
        /// <summary>
        /// Back light
        /// </summary>
        public ISceneLightDirectional BackLight
        {
            get
            {
                return DirectionalLights.Length > 2 ? DirectionalLights[2] : null;
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
        /// Albedo
        /// </summary>
        public float Albedo { get; protected set; } = 0.5f;
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
        public ISceneLight this[string name]
        {
            get
            {
                ISceneLight light = null;

                if (string.Equals(HemisphericLigth?.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return HemisphericLigth;
                }

                light = directionalLights.Find(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
                if (light != null) return light;

                light = pointLights.Find(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
                if (light != null) return light;

                light = spotLights.Find(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
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
        /// <param name="scene">Scene</param>
        public SceneLights(Scene scene)
        {
            this.scene = scene;

            FogStart = 0f;
            FogRange = 0;
            FarLightsDistance = 1000000f;

            SunColor = Color.White;

            UseSunColorPalette = true;
            SunColorPalette = new List<Tuple<float, Color4>>();
            SunColorPalette.AddRange(new[]
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
        public void SetAmbient(ISceneLightHemispheric hemiLight)
        {
            HemisphericLigth = hemiLight;
        }
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Directional light</param>
        public void Add(ISceneLightDirectional light)
        {
            directionalLights.Add(light);
        }
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Point light</param>
        public void Add(ISceneLightPoint light)
        {
            pointLights.Add(light);
        }
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Spot light</param>
        public void Add(ISceneLightSpot light)
        {
            spotLights.Add(light);
        }
        /// <summary>
        /// Adds the specified new light to colection
        /// </summary>
        /// <param name="light">Light</param>
        public void Add(ISceneLight light)
        {
            if (light is ISceneLightHemispheric hemLight) HemisphericLigth = hemLight;
            else if (light is ISceneLightDirectional dirLight) directionalLights.Add(dirLight);
            else if (light is ISceneLightPoint pointLight) pointLights.Add(pointLight);
            else if (light is ISceneLightSpot spotLight) spotLights.Add(spotLight);
        }
        /// <summary>
        /// Adds the specified light list to colection
        /// </summary>
        /// <param name="sceneLights">Lights</param>
        public void AddRange(IEnumerable<ISceneLight> sceneLights)
        {
            if (sceneLights?.Any() != true)
            {
                return;
            }

            foreach (var light in sceneLights)
            {
                Add(light);
            }
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Directional light</param>
        public void Remove(ISceneLightDirectional light)
        {
            if (directionalLights.Contains(light))
            {
                directionalLights.Remove(light);
            }
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Point light</param>
        public void Remove(ISceneLightPoint light)
        {
            if (pointLights.Contains(light))
            {
                pointLights.Remove(light);
            }
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Spot light</param>
        public void Remove(ISceneLightSpot light)
        {
            if (spotLights.Contains(light))
            {
                spotLights.Remove(light);
            }
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Light</param>
        public void Remove(ISceneLight light)
        {
            if (light == HemisphericLigth) HemisphericLigth = null;
            else if (light is ISceneLightDirectional dirLight) Remove(dirLight);
            else if (light is ISceneLightPoint pointLight) Remove(pointLight);
            else if (light is ISceneLightSpot spotLight) Remove(spotLight);
        }
        /// <summary>
        /// Clear all lights
        /// </summary>
        public void Clear()
        {
            HemisphericLigth = null;
            ClearDirectionalLights();
            ClearPointLights();
            ClearSpotLights();
        }
        /// <summary>
        /// Clear all directional lights
        /// </summary>
        public void ClearDirectionalLights()
        {
            directionalLights.Clear();
        }
        /// <summary>
        /// Clear all point lights
        /// </summary>
        public void ClearPointLights()
        {
            pointLights.Clear();
        }
        /// <summary>
        /// Clear all spot lights
        /// </summary>
        public void ClearSpotLights()
        {
            spotLights.Clear();
        }
        /// <summary>
        /// Cull test
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        public void Cull(IIntersectionVolume volume, Vector3 viewerPosition)
        {
            visibleLights.Clear();

            visibleLights.AddRange(CullDirectionalLights());
            visibleLights.AddRange(CullPointLights(volume, viewerPosition));
            visibleLights.AddRange(CullSpotLights(volume, viewerPosition));
        }
        /// <summary>
        /// Cull test for directional lighs
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private IEnumerable<ISceneLight> CullDirectionalLights()
        {
            return directionalLights.Where(l => l.Enabled).ToArray();
        }
        /// <summary>
        /// Cull test for point lights
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private IEnumerable<ISceneLight> CullPointLights(IIntersectionVolume volume, Vector3 viewerPosition)
        {
            var pLights = pointLights
                .Where(l =>
                {
                    if (l.Enabled && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        float d = Vector3.DistanceSquared(viewerPosition, l.Position);

                        return (l.Radius / d) >= (1f / GameEnvironment.LODDistanceLow);
                    }

                    return false;
                })
                .ToList();

            if (pLights.Any())
            {
                pLights.Sort((l1, l2) =>
                {
                    float d1 = Vector3.DistanceSquared(viewerPosition, l1.Position);
                    float d2 = Vector3.DistanceSquared(viewerPosition, l2.Position);

                    float f1 = l1.Radius / (d1 == 0 ? 1 : d1);
                    float f2 = l2.Radius / (d2 == 0 ? 1 : d2);

                    return -f1.CompareTo(f2);
                });
            }

            return pLights;
        }
        /// <summary>
        /// Cull test for spot lights
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private IEnumerable<ISceneLight> CullSpotLights(IIntersectionVolume volume, Vector3 viewerPosition)
        {
            var sLights = spotLights
                .Where(l =>
                {
                    if (l.Enabled && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        float d = Vector3.DistanceSquared(viewerPosition, l.Position);

                        return (l.Radius / d) >= (1f / GameEnvironment.LODDistanceLow);
                    }

                    return false;
                })
                .ToList();

            if (sLights.Any())
            {
                sLights.Sort((l1, l2) =>
                {
                    float d1 = Vector3.DistanceSquared(viewerPosition, l1.Position);
                    float d2 = Vector3.DistanceSquared(viewerPosition, l2.Position);

                    float f1 = l1.Radius / (d1 == 0 ? 1 : d1);
                    float f2 = l2.Radius / (d2 == 0 ? 1 : d2);

                    return -f1.CompareTo(f2);
                });
            }

            return sLights;
        }
        /// <summary>
        /// Gets the visible hemispheric light
        /// </summary>
        /// <returns>Returns the visible hemispheric light</returns>
        public ISceneLightHemispheric GetVisibleHemisphericLight()
        {
            return HemisphericLigth != null && HemisphericLigth.Enabled ? HemisphericLigth : null;
        }
        /// <summary>
        /// Gets the visible directional lights
        /// </summary>
        /// <returns>Returns the visible directional lights array</returns>
        public IEnumerable<ISceneLightDirectional> GetVisibleDirectionalLights()
        {
            return visibleLights
                .OfType<SceneLightDirectional>()
                .ToArray();
        }
        /// <summary>
        /// Gets the visible point lights
        /// </summary>
        /// <returns>Returns the visible point lights array</returns>
        public IEnumerable<ISceneLightPoint> GetVisiblePointLights()
        {
            return visibleLights
                .OfType<SceneLightPoint>()
                .ToArray();
        }
        /// <summary>
        /// Gets the visible spot lights
        /// </summary>
        /// <returns>Returns the visible spot lights array</returns>
        public IEnumerable<ISceneLightSpot> GetVisibleSpotLights()
        {
            return visibleLights
                .OfType<SceneLightSpot>()
                .ToArray();
        }

        /// <summary>
        /// Gets a collection of directional lights that cast shadow
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public IEnumerable<ISceneLightDirectional> GetDirectionalShadowCastingLights(Vector3 eyePosition)
        {
            return visibleLights
                .OfType<ISceneLightDirectional>()
                .Where(l => l.MarkForShadowCasting(eyePosition))
                .ToArray();
        }
        /// <summary>
        /// Gets a collection of point lights that cast shadow
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public IEnumerable<ISceneLightPoint> GetPointShadowCastingLights(Vector3 eyePosition)
        {
            return visibleLights
                .OfType<ISceneLightPoint>()
                .Where(l => l.MarkForShadowCasting(eyePosition))
                .OrderBy(l => Vector3.DistanceSquared(l.Position, eyePosition))
                .ToArray();
        }
        /// <summary>
        /// Gets a collection of spot lights that cast shadow
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public IEnumerable<ISceneLightSpot> GetSpotShadowCastingLights(Vector3 eyePosition)
        {
            return visibleLights
                .OfType<ISceneLightSpot>()
                .Where(l => l.MarkForShadowCasting(eyePosition))
                .OrderBy(l => Vector3.DistanceSquared(l.Position, eyePosition))
                .ToArray();
        }

        /// <summary>
        /// Update directional lights with time of day controller
        /// </summary>
        public void Update()
        {
            var timeOfDay = scene.GameEnvironment.TimeOfDay;

            if (!timeOfDay.Updated)
            {
                return;
            }

            float b = Math.Max(0, -(float)Math.Cos(timeOfDay.Elevation) + 0.15f) * 1.5f;
            Albedo = Math.Min(b, 0.5f);

            Vector3 keyDir = timeOfDay.LightDirection;
            Vector3 backDir = -Vector3.Reflect(keyDir, Vector3.Up);

            float tan = (float)Math.Tan(timeOfDay.Elevation);
            Vector3 fillDir = tan >= 0f ? Vector3.Cross(keyDir, backDir) : Vector3.Cross(backDir, keyDir);

            if (UseSunColorPalette)
            {
                SunColor = GetSunColor(timeOfDay);
            }

            var keyLight = KeyLight;
            if (keyLight != null)
            {
                keyLight.Brightness = keyLight.BaseBrightness * b;

                keyLight.Direction = keyDir;

                if (UseSunColorPalette)
                {
                    keyLight.SpecularColor = SunColor * b;
                }
            }

            var fillLight = FillLight;
            if (fillLight != null)
            {
                fillLight.Brightness = fillLight.BaseBrightness * b;

                fillLight.Direction = fillDir;
            }

            var backLight = BackLight;
            if (backLight != null)
            {
                backLight.Brightness = backLight.BaseBrightness * b;

                backLight.Direction = backDir;
            }

            FogColor = BaseFogColor * Albedo;
        }
        /// <summary>
        /// Gets the sun color based on time of day
        /// </summary>
        /// <param name="timeOfDay">Time of day class</param>
        /// <returns>Returns the color base on time of day meridian angle</returns>
        private Color4 GetSunColor(TimeOfDay timeOfDay)
        {
            float angle = MathUtil.Clamp(timeOfDay.MeridianAngle - MathUtil.PiOverTwo, 0, MathUtil.Pi);

            for (int i = 0; i < SunColorPalette.Count; i++)
            {
                if (SunColorPalette[i].Item1 > angle)
                {
                    if (i > 0)
                    {
                        var from = SunColorPalette[i - 1];
                        var to = SunColorPalette[i];
                        float amount = (angle - from.Item1) / (to.Item1 - from.Item1);
                        return Color4.Lerp(from.Item2, to.Item2, amount);
                    }
                    else
                    {
                        return SunColorPalette[i].Item2;
                    }
                }
            }

            return Color4.White;
        }

        /// <summary>
        /// Enables the fog
        /// </summary>
        /// <param name="start">Starting distance</param>
        /// <param name="end">End distance</param>
        /// <param name="color">Fog color</param>
        public void EnableFog(float start, float end, Color color)
        {
            FogStart = start;
            FogRange = end - start;
            BaseFogColor = FogColor = color;
        }
        /// <summary>
        /// Disables de fog
        /// </summary>
        public void DisableFog()
        {
            FogStart = 0;
            FogRange = 0;
            BaseFogColor = FogColor = Color.Black;
        }
    }
}
