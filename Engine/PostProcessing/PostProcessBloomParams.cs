using System;

namespace Engine.PostProcessing
{
    /// <summary>
    /// Post-process Bloom parameters
    /// </summary>
    public class PostProcessBloomParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default Bloom parameters
        /// </summary>
        public static PostProcessBloomParams Default
        {
            get
            {
                return new PostProcessBloomParams
                {
                    Intensity = 0.25f,
                    Directions = 16,
                    Quality = 3,
                    Size = 4,
                };
            }
        }
        /// <summary>
        /// Low Bloom parameters
        /// </summary>
        public static PostProcessBloomParams Low
        {
            get
            {
                return new PostProcessBloomParams
                {
                    Intensity = 0.15f,
                    Directions = 16,
                    Quality = 3,
                    Size = 4,
                };
            }
        }
        /// <summary>
        /// High Bloom parameters
        /// </summary>
        public static PostProcessBloomParams High
        {
            get
            {
                return new PostProcessBloomParams
                {
                    Intensity = 0.35f,
                    Directions = 16,
                    Quality = 3,
                    Size = 4,
                };
            }
        }

        /// <summary>
        /// Gets whether the effect is active or not
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// Gets the effect intensity
        /// </summary>
        /// <remarks>From 1 = full to 0 = none</remarks>
        public float EffectIntensity { get; set; } = 1f;
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; set; }
        /// <summary>
        /// Directions
        /// </summary>
        /// <remarks>16 by default</remarks>
        public float Directions { get; set; }
        /// <summary>
        /// Quality
        /// </summary>
        /// <remarks>3 by default</remarks>
        public float Quality { get; set; }
        /// <summary>
        /// Size
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Sets the propery value by name
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        public void SetProperty<T>(string name, T value)
        {
            if (string.Equals(name, nameof(Intensity), StringComparison.OrdinalIgnoreCase))
            {
                Intensity = (float)(object)value;

                return;
            }

            if (string.Equals(name, nameof(Directions), StringComparison.OrdinalIgnoreCase))
            {
                Directions = (float)(object)value;

                return;
            }

            if (string.Equals(name, nameof(Quality), StringComparison.OrdinalIgnoreCase))
            {
                Quality = (float)(object)value;

                return;
            }

            if (string.Equals(name, nameof(Size), StringComparison.OrdinalIgnoreCase))
            {
                Size = (float)(object)value;

                return;
            }

            throw new ArgumentException($"Property {name} not found.", nameof(name));
        }
        /// <summary>
        /// Gets the property value by name
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Property name</param>
        /// <returns>Gets the property value</returns>
        public T GetProperty<T>(string name)
        {
            if (string.Equals(name, nameof(Intensity), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Intensity;
            }

            if (string.Equals(name, nameof(Directions), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Directions;
            }

            if (string.Equals(name, nameof(Quality), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Quality;
            }

            if (string.Equals(name, nameof(Size), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Size;
            }

            throw new ArgumentException($"Property {name} not found.", nameof(name));
        }
    }
}
