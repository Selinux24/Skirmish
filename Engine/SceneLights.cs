using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

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
        private SceneLightDirectional[] visibleDirectional = null;
        /// <summary>
        /// Visible point lights
        /// </summary>
        private SceneLightPoint[] visiblePoints = null;
        /// <summary>
        /// Visible spot lights
        /// </summary>
        private SceneLightSpot[] visibleSpots = null;

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
        public Color4 FogColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
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
            this.visibleDirectional = this.directionalLights.FindAll(l => l.Enabled == true).ToArray();

            var pLights = this.pointLights.FindAll(l => l.Enabled == true && frustum.Contains(l.BoundingSphere) != ContainmentType.Disjoint);
            pLights.Sort((l1, l2) =>
            {
                int i = -frustum.Contains(l1.Position).CompareTo(frustum.Contains(l2.Position));
                if (i != 0) return i;

                return Vector3.DistanceSquared(viewerPosition, l1.Position).CompareTo(Vector3.DistanceSquared(viewerPosition, l2.Position));
            });
            this.visiblePoints = pLights.ToArray();

            var sLights = this.spotLights.FindAll(l => l.Enabled == true && Intersection.FrustumContainsFrustum(frustum, l.BoundingFrustum) != ContainmentType.Disjoint);
            sLights.Sort((l1, l2) =>
            {
                int i = -frustum.Contains(l1.Position).CompareTo(frustum.Contains(l2.Position));
                if (i != 0) return i;

                return Vector3.DistanceSquared(viewerPosition, l1.Position).CompareTo(Vector3.DistanceSquared(viewerPosition, l2.Position));
            });
            this.visibleSpots = sLights.ToArray();
        }
        /// <summary>
        /// Gets the visible directional lights
        /// </summary>
        /// <returns>Returns the visible directional lights array</returns>
        public SceneLightDirectional[] GetVisibleDirectionalLights()
        {
            return this.visibleDirectional;
        }
        /// <summary>
        /// Gets the visible point lights
        /// </summary>
        /// <returns>Returns the visible point lights array</returns>
        public SceneLightPoint[] GetVisiblePointLights()
        {
            return this.visiblePoints;
        }
        /// <summary>
        /// Gets the visible spot lights
        /// </summary>
        /// <returns>Returns the visible spot lights array</returns>
        public SceneLightSpot[] GetVisibleSpotLights()
        {
            return this.visibleSpots;
        }

        /// <summary>
        /// Update directional lights with time of day controller
        /// </summary>
        /// <param name="timeOfDay">Time of day</param>
        public void UpdateLights(TimeOfDay timeOfDay)
        {
            var keyLight = this.KeyLight;
            if (keyLight != null)
            {
                //keyLight.DiffuseColor = timeOfDay.SunColor;
                //keyLight.SpecularColor = timeOfDay.SunBandColor;

                keyLight.Direction = timeOfDay.LightDirection;
            }

            //var backLight = this.BackLight;
            //if (backLight != null)
            //{
            //    backLight.Direction = -timeOfDay.LightDirection;
            //    backLight.Direction.Y = -backLight.Direction.Y;
            //}
        }
    }
}
