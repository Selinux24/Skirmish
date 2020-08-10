
namespace Engine
{
    using Point = System.Drawing.Point;
    using SystemCursor = System.Windows.Forms.Cursor;

    /// <summary>
    /// Game cursor
    /// </summary>
    public static class Cursor
    {
        /// <summary>
        /// Times Cursor.Hide() were called
        /// </summary>
        private static int hideCount = 0;

        /// <summary>
        /// Mouse screen position
        /// </summary>
        public static Point ScreenPosition
        {
            get
            {
                return SystemCursor.Position;
            }
            set
            {
                SystemCursor.Position = value;
            }
        }

        /// <summary>
        /// Shows the cursor
        /// </summary>
        public static void Show()
        {
            for (int i = 0; i < hideCount; i++)
            {
                SystemCursor.Show();
            }

            hideCount = 0;
        }
        /// <summary>
        /// Hides the cursor
        /// </summary>
        public static void Hide()
        {
            SystemCursor.Hide();

            hideCount++;
        }
    }
}
