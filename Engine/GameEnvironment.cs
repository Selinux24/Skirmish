using SharpDX;

namespace Engine
{
    /// <summary>
    /// Global variables 
    /// </summary>
    public static class GameEnvironment
    {
        /// <summary>
        /// Background color
        /// </summary>
        public static Color4 Background = Color.Black.ToColor4();
        /// <summary>
        /// Gravity
        /// </summary>
        public static readonly Vector3 Gravity = new Vector3(0, -9.8f, 0);
    }
}
