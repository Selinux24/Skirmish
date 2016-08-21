
namespace Engine
{
    /// <summary>
    /// Automatic camera translation modes
    /// </summary>
    public enum CameraTranslations
    {
        /// <summary>
        /// No translation
        /// </summary>
        None,
        /// <summary>
        /// Use current camera movement delta
        /// </summary>
        UseDelta,
        /// <summary>
        /// Use current camera slow movement delta
        /// </summary>
        UseSlowDelta,
        /// <summary>
        /// Quick
        /// </summary>
        Quick,
    }
}
