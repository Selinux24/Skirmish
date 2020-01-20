using SharpDX;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Game cursor
    /// </summary>
    public class Cursor : Sprite
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
        /// Current cursor position
        /// </summary>
        public Vector2 CursorPosition { get; private set; }
        /// <summary>
        /// Gets or sets whether the cursor is positioned on center of the image
        /// </summary>
        public bool Centered { get; set; }
        /// <summary>
        /// Position delta
        /// </summary>
        public Vector2 Delta { get; set; }

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sprite description</param>
        public Cursor(Scene scene, CursorDescription description)
            : base(scene, description)
        {
            this.Centered = description.Centered;
            this.Delta = description.Delta;
        }

        /// <summary>
        /// Update cursor state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            float left = 0f;
            float top = 0f;

            if (this.Centered)
            {
                left = (this.Game.Input.MouseX - (this.Width * 0.5f));
                top = (this.Game.Input.MouseY - (this.Height * 0.5f));
            }
            else
            {
                left = (this.Game.Input.MouseX);
                top = (this.Game.Input.MouseY);
            }

            this.CursorPosition = new Vector2((int)left, (int)top) + this.Delta;

            if (this.Centered && this.Game.Input.LockMouse)
            {
                this.Manipulator.SetPosition(this.Game.Form.RelativeCenter);
            }
            else
            {
                this.Manipulator.SetPosition(this.CursorPosition);
            }

            base.Update(context);
        }
    }

    /// <summary>
    /// Cursor extensions
    /// </summary>
    public static class CursorExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Cursor> AddComponentCursor(this Scene scene, CursorDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Cursor component = null;

            await Task.Run(() =>
            {
                component = new Cursor(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
