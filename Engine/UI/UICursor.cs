using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// User interface sprite cursor
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UICursor(Scene scene, string id, string name) : UIControl<UICursorDescription>(scene, id, name)
    {
        /// <summary>
        /// Sprite
        /// </summary>
        private Sprite cursorSprite;

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

        /// <inheritdoc/>
        public override async Task ReadAssets(UICursorDescription description)
        {
            await base.ReadAssets(description);

            Centered = Description.Centered;
            Delta = Description.Delta;
            EventsEnabled = false;

            cursorSprite = await CreateSprite();
            AddChild(cursorSprite);
        }
        private async Task<Sprite> CreateSprite()
        {
            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Sprite",
                $"{Name}.Sprite",
                Description);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            float left = Game.Input.MouseX;
            float top = Game.Input.MouseY;
            Vector2 centerDelta = Centered ? new Vector2(-Width * 0.5f, -Height * 0.5f) : Vector2.Zero;

            CursorPosition = new Vector2(left, top);

            if (Game.Input.LockMouse)
            {
                cursorSprite.SetPosition(Game.Form.RenderCenter + centerDelta + Delta);
            }
            else
            {
                cursorSprite.SetPosition(CursorPosition + centerDelta + Delta);
            }

            base.Update(context);
        }
    }
}
