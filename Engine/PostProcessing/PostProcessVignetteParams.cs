using System;

namespace Engine.PostProcessing
{
    /// <summary>
    /// Post-process Vignette parameters
    /// </summary>
    public class PostProcessVignetteParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default Vignette parameters
        /// </summary>
        public static PostProcessVignetteParams Default
        {
            get
            {
                return new PostProcessVignetteParams
                {
                    Outer = 1f,
                    Inner = 0.05f,
                };
            }
        }
        /// <summary>
        /// Thin Vignette parameters
        /// </summary>
        public static PostProcessVignetteParams Thin
        {
            get
            {
                return new PostProcessVignetteParams
                {
                    Outer = 1f,
                    Inner = 0.66f,
                };
            }
        }
        /// <summary>
        /// Strong Vignette parameters
        /// </summary>
        public static PostProcessVignetteParams Strong
        {
            get
            {
                return new PostProcessVignetteParams
                {
                    Outer = 0.5f,
                    Inner = 0.1f,
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
