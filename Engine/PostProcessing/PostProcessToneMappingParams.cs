using System;

namespace Engine.PostProcessing
{
    /// <summary>
    /// Post-process tone mapping parameters
    /// </summary>
    public class PostProcessToneMappingParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Linear tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams Linear
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.Linear,
                };
            }
        }
        /// <summary>
        /// Simple Reinhard tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams SimpleReinhard
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.SimpleReinhard,
                };
            }
        }
        /// <summary>
        /// Luma-based Reinhard tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams LumaBasedReinhard
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.LumaBasedReinhard,
                };
            }
        }
        /// <summary>
        /// White preserving Luma-based Reinhard tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams WhitePreservingLumaBasedReinhard
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.WhitePreservingLumaBasedReinhard,
                };
            }
        }
        /// <summary>
        /// RomBinDaHouse tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams RomBinDaHouse
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.RomBinDaHouse,
                };
            }
        }
        /// <summary>
        /// Filmic tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams Filmic
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.Filmic,
                };
            }
        }
        /// <summary>
        /// Uncharted 2 tone mapping parameters
        /// </summary>
        public static PostProcessToneMappingParams Uncharted2
        {
            get
            {
                return new PostProcessToneMappingParams
                {
                    Tone = ToneMappingTones.Uncharted2,
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
        public ToneMappingTones Tone { get; set; } = ToneMappingTones.None;

        /// <summary>
        /// Sets the propery value by name
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        public void SetProperty<T>(string name, T value)
        {
            if (string.Equals(name, nameof(Tone), StringComparison.OrdinalIgnoreCase))
            {
                Tone = (ToneMappingTones)(object)value;

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
            if (string.Equals(name, nameof(Tone), StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)Tone;
            }

            throw new ArgumentException($"Property {name} not found.", nameof(name));
        }
    }

    /// <summary>
    /// Tone mapping tones
    /// </summary>
    public enum ToneMappingTones : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Linear
        /// </summary>
        Linear,
        /// <summary>
        /// Simple Reinhard
        /// </summary>
        SimpleReinhard,
        /// <summary>
        /// Luma-based Reinhard
        /// </summary>
        LumaBasedReinhard,
        /// <summary>
        /// White preserving Luma-based Reinhard
        /// </summary>
        WhitePreservingLumaBasedReinhard,
        /// <summary>
        /// Roman Galashov's RomBinDaHouse
        /// </summary>
        RomBinDaHouse,
        /// <summary>
        /// Filmic
        /// </summary>
        Filmic,
        /// <summary>
        /// Uncharted 2
        /// </summary>
        Uncharted2,
    }
}
