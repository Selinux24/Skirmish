
namespace Engine.Common
{
    /// <summary>
    /// Pass context
    /// </summary>
    public class PassContext
    {
        /// <summary>
        /// Pass index
        /// </summary>
        public int PassIndex { get; set; }
        /// <summary>
        /// Pass name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Device context
        /// </summary>
        public EngineDeviceContext DeviceContext { get; set; }
    }
}
