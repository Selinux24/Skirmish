using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene lights
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    public class SceneLights(Scene scene) : IHasGameState
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

            var defLights = new SceneLights(scene)
            {
                HemisphericLigth = new SceneLightHemispheric("Default")
            };

            defLights.AddRange(lights);

            return defLights;
        }

        #endregion

        /// <summary>
        /// Scene
        /// </summary>
        private readonly Scene scene = scene;
        /// <summary>
        /// Directional lights
        /// </summary>
        private readonly List<ISceneLightDirectional> directionalLights = [];
        /// <summary>
        /// Point lights
        /// </summary>
        private readonly List<ISceneLightPoint> pointLights = [];
        /// <summary>
        /// Spot lights
        /// </summary>
        private readonly List<ISceneLightSpot> spotLights = [];
        /// <summary>
        /// Visible lights
        /// </summary>
        private readonly List<ISceneLight> visibleLights = [];

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
                return [.. directionalLights];
            }
        }
        /// <summary>
        /// Gets or sets point lights
        /// </summary>
        public ISceneLightPoint[] PointLights
        {
            get
            {
                return [.. pointLights];
            }
        }
        /// <summary>
        /// Gets or sets spot lights
        /// </summary>
        public ISceneLightSpot[] SpotLights
        {
            get
            {
                return [.. spotLights];
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
        public float FogStart { get; set; } = 0f;
        /// <summary>
        /// Fog range value
        /// </summary>
        public float FogRange { get; set; } = 0;
        /// <summary>
        /// Base fog color
        /// </summary>
        public Color4 BaseFogColor { get; set; }
        /// <summary>
        /// Fog color
        /// </summary>
        public Color4 FogColor { get; set; }
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
        public float FarLightsDistance { get; set; } = 1000000f;
        /// <summary>
        /// Gets or sets the color palette use flag
        /// </summary>
        public bool UseSunColorPalette { get; set; } = true;
        /// <summary>
        /// Sun color palette
        /// </summary>
        public List<Tuple<float, Color3>> SunColorPalette { get; set; } =
            [
                .. new[]
                {
                    new Tuple<float, Color3>(MathUtil.Pi * -1.00f, Color.Black.RGB()),
                    new Tuple<float, Color3>(MathUtil.Pi * 0.02f, Color.Orange.RGB()),
                    new Tuple<float, Color3>(MathUtil.Pi * 0.20f, Color.White.RGB()),
                    new Tuple<float, Color3>(MathUtil.Pi * 0.70f, Color.White.RGB()),
                    new Tuple<float, Color3>(MathUtil.Pi * 0.98f, Color.Orange.RGB()),
                    new Tuple<float, Color3>(MathUtil.Pi * 2.00f, Color.Black.RGB()),
                },
            ];
        /// <summary>
        /// Sun color
        /// </summary>
        public Color3 SunColor { get; set; } = Color3.White;
        /// <summary>
        /// Gets or sets the shadow intensity value
        /// </summary>
        /// <remarks>From 0 (darker) to 1 (lighter)</remarks>
        public float ShadowIntensity { get; set; } = 0.5f;

        /// <summary>
        /// Gets the light count of all types
        /// </summary>
        public int Count()
        {
            return DirectionalLights.Length + PointLights.Length + SpotLights.Length;
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
            directionalLights.Remove(light);
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Point light</param>
        public void Remove(ISceneLightPoint light)
        {
            pointLights.Remove(light);
        }
        /// <summary>
        /// Removes the specified light
        /// </summary>
        /// <param name="light">Spot light</param>
        public void Remove(ISceneLightSpot light)
        {
            spotLights.Remove(light);
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
        /// <param name="distance">Light maximum distance</param>
        public void Cull(ICullingVolume volume, Vector3 viewerPosition, float distance)
        {
            visibleLights.Clear();

            visibleLights.AddRange(CullDirectionalLights());
            visibleLights.AddRange(CullPointLights(volume, viewerPosition, distance));
            visibleLights.AddRange(CullSpotLights(volume, viewerPosition, distance));
        }
        /// <summary>
        /// Cull test for directional lighs
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        private ISceneLight[] CullDirectionalLights()
        {
            return directionalLights.Where(l => l.Enabled).ToArray();
        }
        /// <summary>
        /// Cull test for point lights
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        /// <param name="distance">Light maximum distance</param>
        private List<ISceneLightPoint> CullPointLights(ICullingVolume volume, Vector3 viewerPosition, float distance)
        {
            var pLights = pointLights
                .Where(l =>
                {
                    if (l.Enabled && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        float d = Vector3.Distance(viewerPosition, l.Position) - l.Radius;

                        return d <= distance;
                    }

                    return false;
                })
                .ToList();

            if (pLights.Count != 0)
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
        /// <param name="distance">Light maximum distance</param>
        private List<ISceneLightSpot> CullSpotLights(ICullingVolume volume, Vector3 viewerPosition, float distance)
        {
            var sLights = spotLights
                .Where(l =>
                {
                    if (l.Enabled && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        float d = Vector3.Distance(viewerPosition, l.Position) - l.Radius;

                        return d <= distance;
                    }

                    return false;
                })
                .ToList();

            if (sLights.Count != 0)
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
                .OfType<ISceneLightDirectional>()
                .ToArray();
        }
        /// <summary>
        /// Gets the visible point lights
        /// </summary>
        /// <returns>Returns the visible point lights array</returns>
        public IEnumerable<ISceneLightPoint> GetVisiblePointLights()
        {
            return visibleLights
                .OfType<ISceneLightPoint>()
                .ToArray();
        }
        /// <summary>
        /// Gets the visible spot lights
        /// </summary>
        /// <returns>Returns the visible spot lights array</returns>
        public IEnumerable<ISceneLightSpot> GetVisibleSpotLights()
        {
            return visibleLights
                .OfType<ISceneLightSpot>()
                .ToArray();
        }

        /// <summary>
        /// Gets a collection of directional lights that cast shadow
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public IEnumerable<ISceneLightDirectional> GetDirectionalShadowCastingLights(GameEnvironment environment, Vector3 eyePosition)
        {
            return visibleLights
                .OfType<ISceneLightDirectional>()
                .Where(l => l.MarkForShadowCasting(environment, eyePosition))
                .ToArray();
        }
        /// <summary>
        /// Gets a collection of point lights that cast shadow
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public IEnumerable<ISceneLightPoint> GetPointShadowCastingLights(GameEnvironment environment, Vector3 eyePosition)
        {
            var scLights = visibleLights
                .OfType<ISceneLightPoint>()
                .Where(l => l.MarkForShadowCasting(environment, eyePosition))
                .OrderBy(l => Vector3.DistanceSquared(l.Position, eyePosition));

            return [.. scLights];
        }
        /// <summary>
        /// Gets a collection of spot lights that cast shadow
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns a light collection</returns>
        public IEnumerable<ISceneLightSpot> GetSpotShadowCastingLights(GameEnvironment environment, Vector3 eyePosition)
        {
            var scLights = visibleLights
                .OfType<ISceneLightSpot>()
                .Where(l => l.MarkForShadowCasting(environment, eyePosition))
                .OrderBy(l => Vector3.DistanceSquared(l.Position, eyePosition));

            return [.. scLights];
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

            FogColor = BaseFogColor * b;
        }
        /// <summary>
        /// Gets the sun color based on time of day
        /// </summary>
        /// <param name="timeOfDay">Time of day class</param>
        /// <returns>Returns the color base on time of day meridian angle</returns>
        private Color3 GetSunColor(TimeOfDay timeOfDay)
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
                        return Color3.Lerp(from.Item2, to.Item2, amount);
                    }
                    else
                    {
                        return SunColorPalette[i].Item2;
                    }
                }
            }

            return Color3.White;
        }

        /// <summary>
        /// Enables the fog
        /// </summary>
        /// <param name="start">Starting distance</param>
        /// <param name="end">End distance</param>
        /// <param name="color">Fog color</param>
        public void EnableFog(float start, float end, Color4 color)
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

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new SceneLightsState
            {
                DirectionalLights = directionalLights.Select(l => l.GetState()).ToArray(),
                PointLights = pointLights.Select(l => l.GetState()).ToArray(),
                SpotLights = spotLights.Select(l => l.GetState()).ToArray(),
                HemisphericLigth = HemisphericLigth.GetState(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not SceneLightsState sceneLightsState)
            {
                return;
            }

            for (int i = 0; i < sceneLightsState.DirectionalLights.Count(); i++)
            {
                var lightState = sceneLightsState.DirectionalLights.ElementAt(i);
                directionalLights[i].SetState(lightState);
            }

            for (int i = 0; i < sceneLightsState.PointLights.Count(); i++)
            {
                var lightState = sceneLightsState.PointLights.ElementAt(i);
                PointLights[i].SetState(lightState);
            }

            for (int i = 0; i < sceneLightsState.SpotLights.Count(); i++)
            {
                var lightState = sceneLightsState.SpotLights.ElementAt(i);
                spotLights[i].SetState(lightState);
            }

            HemisphericLigth.SetState(sceneLightsState.HemisphericLigth);
        }
    }
}
