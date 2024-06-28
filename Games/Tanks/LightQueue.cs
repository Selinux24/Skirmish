using Engine;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Tanks
{
    /// <summary>
    /// Light queue helper class
    /// </summary>
    static class LightQueue
    {
        /// <summary>
        /// Light tweener
        /// </summary>
        struct LightTweener
        {
            /// <summary>
            /// Light
            /// </summary>
            public SceneLightPoint Light;
            /// <summary>
            /// Tween description
            /// </summary>
            public LightTweenDescription Description;
            /// <summary>
            /// Maximum duration
            /// </summary>
            public float MaxDuration;
            /// <summary>
            /// Time of activation
            /// </summary>
            public float ActivationTime;
            /// <summary>
            /// Tweener active
            /// </summary>
            public bool Active;

            /// <summary>
            /// Updates the internal tweener state
            /// </summary>
            /// <param name="gameTime">Game time</param>
            public void UpdateTweener(IGameTime gameTime)
            {
                float deltaTime = gameTime.TotalSeconds - ActivationTime;
                if (deltaTime > MaxDuration)
                {
                    Active = false;
                    Light.Enabled = false;

                    return;
                }

                float d = deltaTime / MaxDuration;
                float invD = 1f - d;

                if (deltaTime > 0.4f)
                {
                    //Low light
                    float newRadius = Description.Radius * invD * 0.1f;
                    Light.Radius = MathUtil.Lerp(Light.Radius, newRadius, 0.3f);
                    Light.Intensity = Light.Radius * Description.IntensityCurve.Evaluate(d);

                    var col = Color3.Lerp(Description.StartColor, Description.EndColor, Description.ColorCurve.Evaluate(d));
                    Light.DiffuseColor = col;
                    Light.SpecularColor = col;
                }

                Light.Enabled = true;
            }
        }

        /// <summary>
        /// Lights collection
        /// </summary>
        private static readonly List<SceneLightPoint> lights = [];
        /// <summary>
        /// Tweeners collection
        /// </summary>
        private static readonly List<LightTweener> lightTweeners = [];

        /// <summary>
        /// Initializes the queue
        /// </summary>
        /// <param name="lightPoints">Light point collection</param>
        /// <remarks>The light collection must be added previously to the scene lights list</remarks>
        public static void Initialize(IEnumerable<SceneLightPoint> lightPoints)
        {
            lights.AddRange(lightPoints);
        }
        /// <summary>
        /// Queues a light at the specified position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="position">Position</param>
        /// <param name="description">Tween description</param>
        /// <param name="maxDuration">Maximum duration</param>
        public static void QueueLight(IGameTime gameTime, Vector3 position, LightTweenDescription description, float maxDuration)
        {
            if (lights.Count == 0)
            {
                return;
            }

            var freeLight = lights.Find(l => !l.Enabled);
            if (freeLight == null)
            {
                return;
            }

            freeLight.Position = position;
            freeLight.Radius = description.Radius;
            freeLight.Intensity = description.Intensity;
            freeLight.DiffuseColor = description.StartColor;
            freeLight.SpecularColor = description.StartColor;
            freeLight.Enabled = true;

            lightTweeners.Add(new LightTweener()
            {
                Light = freeLight,
                Description = description,
                MaxDuration = maxDuration,
                ActivationTime = gameTime.TotalSeconds,
                Active = true,
            });
        }
        /// <summary>
        /// Updates the queue state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public static void Update(IGameTime gameTime)
        {
            if (lightTweeners.Count == 0)
            {
                return;
            }

            lightTweeners.RemoveAll(l => !l.Active);

            lightTweeners.ForEach(l => l.UpdateTweener(gameTime));
        }
    }

    /// <summary>
    /// Light tween description
    /// </summary>
    struct LightTweenDescription
    {
        /// <summary>
        /// Explosion tween
        /// </summary>
        /// <param name="radius">Light radius</param>
        /// <param name="intensity">Intensity</param>
        /// <param name="startColor">Starting color</param>
        /// <param name="endColor">End color</param>
        /// <returns>Returns la explosion tween description</returns>
        public static LightTweenDescription ExplosionTween(float radius, float intensity, Color3 startColor, Color3 endColor)
        {
            var intensityCurve = new Curve();
            intensityCurve.Keys.Add(0f, 1f);
            intensityCurve.Keys.Add(0.1f, 0.5f);
            intensityCurve.Keys.Add(0.2f, 0.8f);
            intensityCurve.Keys.Add(0.5f, 0.2f);
            intensityCurve.Keys.Add(0.8f, 0.8f);
            intensityCurve.Keys.Add(1f, 0f);

            var colorCurve = new Curve();
            colorCurve.Keys.Add(0f, 0f);
            colorCurve.Keys.Add(0.15f, 0f);
            colorCurve.Keys.Add(0.2f, 0.5f);
            colorCurve.Keys.Add(1f, 1f);

            return new LightTweenDescription
            {
                IntensityCurve = intensityCurve,
                ColorCurve = colorCurve,
                Radius = radius,
                Intensity = intensity,
                StartColor = startColor,
                EndColor = endColor,
            };
        }
        /// <summary>
        /// Explosion tween
        /// </summary>
        /// <param name="radius">Light radius</param>
        /// <param name="intensity">Intensity</param>
        /// <param name="startColor">Starting color</param>
        /// <param name="endColor">End color</param>
        /// <returns>Returns la shot tween description</returns>
        public static LightTweenDescription ShootTween(float radius, float intensity, Color3 startColor, Color3 endColor)
        {
            var intensityCurve = new Curve();
            intensityCurve.Keys.Add(0f, 1f);
            intensityCurve.Keys.Add(0.1f, 0.7f);
            intensityCurve.Keys.Add(0.2f, 1f);
            intensityCurve.Keys.Add(0.5f, 0.2f);
            intensityCurve.Keys.Add(0.8f, 0.8f);
            intensityCurve.Keys.Add(1f, 0f);

            var colorCurve = new Curve();
            colorCurve.Keys.Add(0f, 0f);
            colorCurve.Keys.Add(1f, 1f);

            return new LightTweenDescription
            {
                IntensityCurve = intensityCurve,
                ColorCurve = colorCurve,
                Radius = radius,
                Intensity = intensity,
                StartColor = startColor,
                EndColor = endColor,
            };
        }

        /// <summary>
        /// Intensity curve
        /// </summary>
        public Curve IntensityCurve;
        /// <summary>
        /// Color curve
        /// </summary>
        public Curve ColorCurve;
        /// <summary>
        /// Light initial radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// Lights initial intensity
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Light initial color
        /// </summary>
        public Color3 StartColor;
        /// <summary>
        /// Light end color
        /// </summary>
        public Color3 EndColor;
    }
}
