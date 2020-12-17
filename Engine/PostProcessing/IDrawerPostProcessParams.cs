
namespace Engine.PostProcessing
{
    /// <summary>
    /// Post processing parameters interface
    /// </summary>
    public interface IDrawerPostProcessParams
    {
        /// <summary>
        /// Gets whether the effect is active or not
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// Gets the effect intensity
        /// </summary>
        /// <remarks>From 1 = full to 0 = none</remarks>
        float EffectIntensity { get; set; }
        /// <summary>
        /// Sets the propery value by name
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        void SetProperty<T>(string name, T value);
        /// <summary>
        /// Gets the property value by name
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Property name</param>
        /// <returns>Gets the property value</returns>
        T GetProperty<T>(string name);
    }
}
