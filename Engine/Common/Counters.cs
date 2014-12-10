
namespace Engine.Common
{
    public static class Counters
    {
        public static int FrameCount = 0;
        public static float FrameTime = 0;
        public static int DrawCallsPerFrame = 0;
        public static int UpdatesPerFrame = 0;
        public static int UpdatesPerObject = 0;
        public static int UpdatesPerInstance = 0;
        public static int TextureUpdates = 0;

        public static void ClearAll()
        {
            FrameCount = 0;
            DrawCallsPerFrame = 0;
            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;
            TextureUpdates = 0;
        }

        public static void ClearFrame()
        {
            DrawCallsPerFrame = 0;
            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;
            TextureUpdates = 0;
        }
    }
}
