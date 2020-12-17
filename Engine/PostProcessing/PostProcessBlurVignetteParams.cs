using System;

namespace Engine.PostProcessing
{
    /// <summary>
    /// Post-process Blur + Vignette parameters
    /// </summary>
    public class PostProcessBlurVignetteParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default blur parameters
        /// </summary>
        public static PostProcessBlurVignetteParams Default
        {
            get
            {
                return new PostProcessBlurVignetteParams
                {
                    Directions = 16,
                    Quality = 3,
                    Size = 4,
                    Outer = 1f,
                    Inner = 0.05f,
                };
            }
        }
        /// <summary>
        /// Strong blur parameters
        /// </summary>
        public static PostProcessBlurVignetteParams Strong
        {
            get
            {
                return new PostProcessBlurVignetteParams
                {
                    Directions = 16,
                    Quality = 3,
                    Size = 8,
                    Outer = 1f,
                    Inner = 0.05f,
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
        /// Outer vignette ring
        /// </summary>
        /// <remarks>1 by default</remarks>
        public float Outer { get; set; }
        /// <summary>
        /// Inner vignette ring
        /// </summary>
        /// <remarks>0.05 by default</remarks>
        public float Inner { get; set; }

        /// <summary>
        /// Sets the propery value by name
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        public void SetProperty<T>(string name, T value)
        {
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

            if (string.Equals(name, nameof(Outer), StringComparison.OrdinalIgnoreCase))
            {
                Outer = (float)(object)value;

                return;
            }

            if (string.Equals(name, nameof(Inner), StringComparison.OrdinalIgnoreCase))
            {
                Inner = (float)(object)value;

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

            if (string.Equals(name, nameof(Outer), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Outer;
            }

            if (string.Equals(name, nameof(Inner), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Inner;
            }

            throw new ArgumentException($"Property {name} not found.", nameof(name));
        }
    }
}
