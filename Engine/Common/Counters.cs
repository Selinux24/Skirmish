
namespace Engine.Common
{
    /// <summary>
    /// Game counters
    /// </summary>
    public static class Counters
    {
        /// <summary>
        /// Frame count
        /// </summary>
        public static long FrameCount = 0;
        /// <summary>
        /// Frame time
        /// </summary>
        public static float FrameTime = 0;
        /// <summary>
        /// Draw calls per frame
        /// </summary>
        public static int DrawCallsPerFrame = 0;
        /// <summary>
        /// Sum of single draw calls and instance draw calls * instance count of this call
        /// </summary>
        public static int InstancesPerFrame = 0;
        /// <summary>
        /// Updates per frame
        /// </summary>
        public static int UpdatesPerFrame = 0;
        /// <summary>
        /// Updates per object
        /// </summary>
        public static int UpdatesPerObject = 0;
        /// <summary>
        /// Updates per instance
        /// </summary>
        public static int UpdatesPerInstance = 0;
        /// <summary>
        /// Texture updates
        /// </summary>
        public static int TextureUpdates = 0;

        /// <summary>
        /// Clear counters
        /// </summary>
        public static void ClearAll()
        {
            FrameCount = 0;
            DrawCallsPerFrame = 0;
            InstancesPerFrame = 0;
            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;
            TextureUpdates = 0;
        }
        /// <summary>
        /// Clear per frame counters
        /// </summary>
        public static void ClearFrame()
        {
            DrawCallsPerFrame = 0;
            InstancesPerFrame = 0;
            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;
            TextureUpdates = 0;
        }
    }
}
