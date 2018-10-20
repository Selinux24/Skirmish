
namespace Engine.Helpers
{
    /// <summary>
    /// Helper methods for shaders and effects
    /// </summary>
    public static class HelperShaders
    {
        /// <summary>
        /// Vertex shader profile
        /// </summary>
        public static string VSProfile { get; set; } = "vs_5_0";
        /// <summary>
        /// Pixel shader profile
        /// </summary>
        public static string PSProfile { get; set; } = "ps_5_0";
        /// <summary>
        /// Effect profile
        /// </summary>
        public static string FXProfile { get; set; } = "fx_5_0";
    }
}
