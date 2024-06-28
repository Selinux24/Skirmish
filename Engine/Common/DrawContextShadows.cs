
namespace Engine.Common
{
    /// <summary>
    /// Drawing context for shadow mapping
    /// </summary>
    public struct DrawContextShadows
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera { get; set; }
        /// <summary>
        /// Shadow map to fill
        /// </summary>
        public IShadowMap ShadowMap { get; set; }
        /// <summary>
        /// Pass context
        /// </summary>
        public PassContext PassContext { get; set; }
        /// <summary>
        /// Device context
        /// </summary>
        public readonly IEngineDeviceContext DeviceContext { get => PassContext.DeviceContext; }

        /// <summary>
        /// Clones the actual draw context
        /// </summary>
        /// <param name="name">New name</param>
        public DrawContextShadows Clone(string name)
        {
            return new DrawContextShadows
            {
                Name = name,
                Camera = Camera,
                ShadowMap = ShadowMap,
                PassContext = PassContext,
            };
        }
    }
}
