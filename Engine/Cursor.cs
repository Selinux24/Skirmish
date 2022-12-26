using SharpDX;

namespace Engine
{
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
                var p = SystemCursor.Position;

                return new Point(p.X, p.Y);
            }
            set
            {
                var p = new System.Drawing.Point(value.X, value.Y);

                SystemCursor.Position = p;
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
