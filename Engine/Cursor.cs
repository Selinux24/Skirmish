
namespace Engine
{
    using Point = System.Drawing.Point;

    /// <summary>
    /// Game cursor
    /// </summary>
    public class Cursor
    {
        /// <summary>
        /// Times Cursor.Show() were called
        /// </summary>
        private static int showCount = 1;
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
                return System.Windows.Forms.Cursor.Position;
            }
            set
            {
                System.Windows.Forms.Cursor.Position = value;
            }
        }

        /// <summary>
        /// Shows the cursor
        /// </summary>
        public static void Show()
        {
            while (hideCount > 0)
            {
                hideCount--;
                System.Windows.Forms.Cursor.Show();
            }

            showCount++;
        }
        /// <summary>
        /// Hides the cursor
        /// </summary>
        public static void Hide()
        {
            while (showCount > 0)
            {
                showCount--;
                System.Windows.Forms.Cursor.Hide();
            }

            hideCount++;
        }
    }
}
