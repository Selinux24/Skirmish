using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

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
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sprite description</param>
        public UICursor(string name, Scene scene, UICursorDescription description)
            : base(name, scene, description)
        {
            Centered = description.Centered;
            Delta = description.Delta;
            EventsEnabled = false;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            float left;
            float top;

            if (Centered)
            {
                left = Game.Input.MouseX - (Width * 0.5f);
                top = Game.Input.MouseY - (Height * 0.5f);
            }
            else
            {
                left = Game.Input.MouseX;
                top = Game.Input.MouseY;
            }

            CursorPosition = new Vector2(left, top) + Delta;

            if (Game.Input.LockMouse)
            {
                SetPosition(Game.Form.RenderCenter);
            }
            else
            {
                SetPosition(CursorPosition);
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
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UICursor> AddComponentUICursor(this Scene scene, string name, UICursorDescription description, int layer = Scene.LayerCursor)
        {
            UICursor component = null;

            await Task.Run(() =>
            {
                component = new UICursor(name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
