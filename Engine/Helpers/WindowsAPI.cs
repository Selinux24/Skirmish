using System.Runtime.InteropServices;

namespace Engine.Helpers
{
    /// <summary>
    /// Windows API functions
    /// </summary>
    static class WindowsAPI
    {
        /// <summary>
        /// Gets current keyboard state
        /// </summary>
        /// <param name="lpKeyState">Key state array</param>
        /// <returns>Returns true if the state retrieved</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetKeyboardState(byte[] lpKeyState);
    }
}
