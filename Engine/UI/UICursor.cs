using Engine.Common;
using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// User interface sprite cursor
    /// </summary>
    public class UICursor : Sprite
    {
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
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sprite description</param>
        public UICursor(Scene scene, UICursorDescription description)
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
            if (!Active)
            {
                return;
            }

            float left;
            float top;

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

            this.CursorPosition = new Vector2(left, top) + this.Delta;

            if (this.Centered && this.Game.Input.LockMouse)
            {
                this.Manipulator.SetPosition(this.Game.Form.RelativeCenter);
            }
            else
            {
                this.Manipulator.SetPosition(this.CursorPosition - this.Game.Form.RelativeCenter);
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
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UICursor> AddComponentUICursor(this Scene scene, UICursorDescription description, int order = 0)
        {
            UICursor component = null;

            await Task.Run(() =>
            {
                component = new UICursor(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
