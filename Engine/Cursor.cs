using SharpDX;
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
        /// Shows the cursor
        /// </summary>
        public static void Show()
        {
            System.Windows.Forms.Cursor.Show();
        }
        /// <summary>
        /// Hides the cursor
        /// </summary>
        public static void Hide()
        {
            System.Windows.Forms.Cursor.Hide();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Sprite description</param>
        /// <param name="centered">Cursor positioned on center of the image</param>
        public Cursor(Game game, SpriteDescription description, bool centered = true)
            : base(game, description)
        {
            this.Centered = centered;
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

            this.CursorPosition = new Vector2((int)left, (int)top);

            if (this.Centered && this.Game.Form.IsFullscreen)
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
}
