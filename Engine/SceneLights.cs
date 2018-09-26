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
                DirectionalLights = lights,
            };
        }

        #endregion

        /// <summary>
        /// Hemispheric ambient light
        /// </summary>
        private SceneLightHemispheric hemisphericLigth = null;
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
        public SceneLightHemispheric HemisphericLigth
        {
            get
            {
                return this.hemisphericLigth;
            }
            set
            {
                this.hemisphericLigth = value;
            }
        }
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
        /// Gets from light view * projection matrix
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="shadowDistance">Shadows visible distance</param>
        /// <returns>Returns the from light view * projection matrix</returns>
        public static Matrix GetFromLightViewProjection(Vector3 lightPosition, Vector3 eyePosition, float shadowDistance)
        {
            // View from light to scene center position
            var view = Matrix.LookAtLH(lightPosition, eyePosition, Vector3.Up);

            // Transform bounding sphere to light space.
            Vector3 sphereCenterLS = Vector3.TransformCoordinate(eyePosition, view);

            // Ortho frustum in light space encloses scene.
            float xleft = sphereCenterLS.X - shadowDistance;
            float xright = sphereCenterLS.X + shadowDistance;
            float ybottom = sphereCenterLS.Y - shadowDistance;
            float ytop = sphereCenterLS.Y + shadowDistance;
            float znear = sphereCenterLS.Z - shadowDistance;
            float zfar = sphereCenterLS.Z + shadowDistance;

            // Orthogonal projection from center
            var projection = Matrix.OrthoOffCenterLH(xleft, xright, ybottom, ytop, znear, zfar);

            return view * projection;
        }
        /// <summary>
        /// Gets from light view * projection matrix cube
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="radius">Light radius</param>
        /// <returns>Returns the from light view * projection matrix cube</returns>
        public static Matrix[] GetFromPointLightViewProjection(ISceneLightOmnidirectional light)
        {
            // Orthogonal projection from center
            var projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1f, 0.1f, light.Radius);

            return new Matrix[]
            {
                GetFromPointLightViewProjection(light.Position, Vector3.Right,      Vector3.Up)         * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.Left,       Vector3.Up)         * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.Up,         Vector3.BackwardLH) * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.Down,       Vector3.ForwardLH)  * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.ForwardLH,  Vector3.Up)         * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.BackwardLH, Vector3.Up)         * projection,
            };
        }
        /// <summary>
        /// Gets the point light from light view matrix
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="direction">Direction</param>
        /// <param name="up">Up vector</param>
        /// <returns>Returns the point light from light view matrix</returns>
        public static Matrix GetFromPointLightViewProjection(Vector3 lightPosition, Vector3 direction, Vector3 up)
        {
            // View from light to scene center position
            return Matrix.LookAtLH(lightPosition, lightPosition + direction, up);
        }
        /// <summary>
        /// Gets the spot light from light view matrix
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="direction">Direction</param>
        /// <param name="radius">Radius</param>
        /// <returns>Returns the spot light from light view matrix</returns>
        public static Matrix GetFromSpotLightViewProjection(Vector3 lightPosition, Vector3 direction, float radius)
        {
            var projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1f, 1f, radius);

            // View from light to scene center position
            return Matrix.LookAtLH(lightPosition, lightPosition + direction, Vector3.Up) * projection;
        }
        /// <summary>
        /// Gets cascades from light view * projection matrix array
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="direction">Light direction</param>
        /// <param name="cascadeSet">Cascade set</param>
        /// <returns>Returns the cascades from light view * projection matrix array</returns>
        public static Matrix[] GetCascadeFromLightViewProjection(Camera camera, Vector3 direction, ShadowMapCascadeSet cascadeSet)
        {
            cascadeSet.Update(camera, direction);

            return cascadeSet.GetWorldToCascadeProj();
        }

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
            this.hemisphericLigth = hemiLight;
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
        /// <param name="volume">Volume</param>
        /// <param name="viewerPosition">Viewer position</param>
        public void Cull(ICullingVolume volume, Vector3 viewerPosition)
        {
            this.visibleLights.Clear();

            var dLights = this.directionalLights.FindAll(l => l.Enabled == true);
            if (dLights.Count > 0)
            {
                this.visibleLights.AddRange(dLights);
            }

            var pLights = this.pointLights.FindAll(l =>
            {
                if (l.Enabled == true && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
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

            var sLights = this.spotLights.FindAll(l =>
            {
                if (l.Enabled == true && volume.Contains(l.BoundingSphere) != ContainmentType.Disjoint)
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
            return this.hemisphericLigth != null && this.hemisphericLigth.Enabled ? this.hemisphericLigth : null;
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
        public ISceneLightOmnidirectional[] GetPointShadowCastingLights(Vector3 eyePosition)
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
        public SceneLightSpot[] GetSpotShadowCastingLights(Vector3 eyePosition)
        {
            return this.visibleLights
                .Where(l => l.CastShadow && l is SceneLightSpot)
                .Select(l => (SceneLightSpot)l)
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
        /// <summary>
        /// Gets light position and direction for shadow casting directional lights
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="lightPosition">Light position</param>
        /// <param name="lightDirection">Light direction</param>
        /// <remarks>Returns true if the light has valid parameters</remarks>
        public bool GetDirectionalLightShadowParams(ISceneLightDirectional light, out Vector3 lightPosition, out Vector3 lightDirection)
        {
            lightPosition = Vector3.Zero;
            lightDirection = Vector3.Zero;

            if (light is SceneLightDirectional lightDir)
            {
                // Calc light position outside the scene volume
                lightPosition = lightDir.GetPosition(this.FarLightsDistance);
                lightDirection = lightDir.Direction;

                return true;
            }
            else if (light is SceneLightSpot lightSpot)
            {
                lightPosition = lightSpot.Position;
                lightDirection = lightSpot.Direction;

                return true;
            }

            return false;
        }
    }
}
