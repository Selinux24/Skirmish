using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Global variables 
    /// </summary>
    public class GameEnvironment : IHasGameState
    {
        /// <summary>
        /// Degree of paralelism
        /// </summary>
        public static readonly int DegreeOfParalelism = (int)Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0);

        /// <summary>
        /// Background color
        /// </summary>
        public Color4 Background { get; set; } = Color.Black.ToColor4();
        /// <summary>
        /// Gravity
        /// </summary>
        public Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);

        /// <summary>
        /// Maximum distance for high level of detail models
        /// </summary>
        private float lodDistanceHigh = 100f;
        /// <summary>
        /// Maximum distance for medium level of detail models
        /// </summary>
        private float lodDistanceMedium = 200f;
        /// <summary>
        /// Maximum distance for low level of detail models
        /// </summary>
        private float lodDistanceLow = 500f;
        /// <summary>
        /// Maximum distance for minimum level of detail models
        /// </summary>
        private float lodDistanceMinimum = 1000f;

        /// <summary>
        /// The engine will discard all lights where: Distance / light radius < threshold
        /// </summary>
        private float shadowRadiusDistanceThreshold = 0.25f;

        /// <summary>
        /// Maximum distance for High level detailed shadows
        /// </summary>
        private float shadowDistanceHigh = 2f;
        /// <summary>
        /// Maximum distance for Medium level detailed shadows
        /// </summary>
        private float shadowDistanceMedium = 10f;
        /// <summary>
        /// Maximum distance for Low level detailed shadows
        /// </summary>
        private float shadowDistanceLow = 50f;

        /// <summary>
        /// Maximum distance for high level of detail models
        /// </summary>
        public float LODDistanceHigh
        {
            get
            {
                return lodDistanceHigh;
            }
            set
            {
                if (lodDistanceHigh != value)
                {
                    lodDistanceHigh = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// Maximum distance for medium level of detail models
        /// </summary>
        public float LODDistanceMedium
        {
            get
            {
                return lodDistanceMedium;
            }
            set
            {
                if (lodDistanceMedium != value)
                {
                    lodDistanceMedium = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// Maximum distance for low level of detail models
        /// </summary>
        public float LODDistanceLow
        {
            get
            {
                return lodDistanceLow;
            }
            set
            {
                if (lodDistanceLow != value)
                {
                    lodDistanceLow = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// Maximum distance for minimum level of detail models
        /// </summary>
        public float LODDistanceMinimum
        {
            get
            {
                return lodDistanceMinimum;
            }
            set
            {
                if (lodDistanceMinimum != value)
                {
                    lodDistanceMinimum = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// The engine will discard all lights where: Distance / light radius < threshold
        /// </summary>
        public float ShadowRadiusDistanceThreshold
        {
            get
            {
                return shadowRadiusDistanceThreshold;
            }
            set
            {
                if (shadowRadiusDistanceThreshold != value)
                {
                    shadowRadiusDistanceThreshold = value;

                    Modified = true;
                }
            }
        }

        /// <summary>
        /// Maximum distance for High level detailed shadows
        /// </summary>
        public float ShadowDistanceHigh
        {
            get
            {
                return shadowDistanceHigh;
            }
            set
            {
                if (shadowDistanceHigh != value)
                {
                    shadowDistanceHigh = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// Maximum distance for Medium level detailed shadows
        /// </summary>
        public float ShadowDistanceMedium
        {
            get
            {
                return shadowDistanceMedium;
            }
            set
            {
                if (shadowDistanceMedium != value)
                {
                    shadowDistanceMedium = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// Maximum distance for Low level detailed shadows
        /// </summary>
        public float ShadowDistanceLow
        {
            get
            {
                return shadowDistanceLow;
            }
            set
            {
                if (shadowDistanceLow != value)
                {
                    shadowDistanceLow = value;

                    Modified = true;
                }
            }
        }
        /// <summary>
        /// Shadow map sampling distances
        /// </summary>
        public float[] CascadeShadowMapsDistances
        {
            get
            {
                return new[]
                {
                    shadowDistanceHigh,
                    shadowDistanceMedium,
                    shadowDistanceLow,
                };
            }
        }

        /// <summary>
        /// Time of day controller
        /// </summary>
        public TimeOfDay TimeOfDay { get; private set; } = new TimeOfDay();

        /// <summary>
        /// Environment modified flag
        /// </summary>
        public bool Modified { get; private set; } = false;

        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public bool Update(GameTime gameTime)
        {
            TimeOfDay.Update(gameTime);

            if (Modified)
            {
                Modified = false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the level of detail
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="coarseBoundingSphere">Coarse bounding sphere</param>
        /// <param name="localTransform">Local transform</param>
        /// <returns>Returns the level of detail</returns>
        public LevelOfDetail GetLOD(Vector3 origin, BoundingSphere? coarseBoundingSphere, Matrix localTransform)
        {
            Vector3 position = localTransform.TranslationVector;
            float radius = 0f;

            if (coarseBoundingSphere.HasValue)
            {
                position = coarseBoundingSphere.Value.Center;
                radius = coarseBoundingSphere.Value.Radius;
            }

            float dist = Vector3.Distance(position, origin) - radius;
            if (dist < LODDistanceHigh)
            {
                return LevelOfDetail.High;
            }
            else if (dist < LODDistanceMedium)
            {
                return LevelOfDetail.Medium;
            }
            else if (dist < LODDistanceLow)
            {
                return LevelOfDetail.Low;
            }
            else if (dist < LODDistanceMinimum)
            {
                return LevelOfDetail.Minimum;
            }
            else
            {
                return LevelOfDetail.None;
            }
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new GameEnvironmentState
            {
                Background = Background,
                Gravity = Gravity,
                LodDistanceHigh = lodDistanceHigh,
                LodDistanceMedium = lodDistanceMedium,
                LodDistanceLow = lodDistanceLow,
                LodDistanceMinimum = lodDistanceMinimum,
                ShadowDistanceHigh = shadowDistanceHigh,
                ShadowDistanceMedium = shadowDistanceMedium,
                ShadowDistanceLow = shadowDistanceLow,
                ShadowRadiusDistanceThreshold = shadowRadiusDistanceThreshold,
                TimeOfDay = TimeOfDay,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (!(state is GameEnvironmentState environmentState))
            {
                return;
            }

            Background = environmentState.Background;
            Gravity = environmentState.Gravity;
            lodDistanceHigh = environmentState.LodDistanceHigh;
            lodDistanceMedium = environmentState.LodDistanceMedium;
            lodDistanceLow = environmentState.LodDistanceLow;
            lodDistanceMinimum = environmentState.LodDistanceMinimum;
            shadowDistanceHigh = environmentState.ShadowDistanceHigh;
            shadowDistanceMedium = environmentState.ShadowDistanceMedium;
            shadowDistanceLow = environmentState.ShadowDistanceLow;
            shadowRadiusDistanceThreshold = environmentState.ShadowRadiusDistanceThreshold;
            TimeOfDay = environmentState.TimeOfDay;
        }
    }
}
