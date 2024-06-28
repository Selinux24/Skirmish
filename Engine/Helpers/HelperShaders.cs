
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
        /// Hull shader profile
        /// </summary>
        public static string HSProfile { get; set; } = "hs_5_0";
        /// <summary>
        /// Domain shader profile
        /// </summary>
        public static string DSProfile { get; set; } = "ds_5_0";
        /// <summary>
        /// Geometry shader profile
        /// </summary>
        public static string GSProfile { get; set; } = "gs_5_0";
        /// <summary>
        /// Pixel shader profile
        /// </summary>
        public static string PSProfile { get; set; } = "ps_5_0";
        /// <summary>
        /// Compute shader profile
        /// </summary>
        public static string CSProfile { get; set; } = "cs_5_0";
        /// <summary>
        /// Effect profile
        /// </summary>
        public static string FXProfile { get; set; } = "fx_5_0";
    }
}
