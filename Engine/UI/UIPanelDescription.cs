using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Panel description
    /// </summary>
    public class UIPanelDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default panel description
        /// </summary>
        public static UIPanelDescription Default()
        {
            return new()
            {
                Background = SpriteDescription.Default(),
            };
        }
        /// <summary>
        /// Gets the default panel description
        /// </summary>
        /// <param name="color">Tint color</param>
        public static UIPanelDescription Default(Color4 color)
        {
            return new()
            {
                Background = SpriteDescription.Default(color),
            };
        }
        /// <summary>
        /// Gets the default panel description
        /// </summary>
        /// <param name="fileName">Texture file name for the background</param>
        public static UIPanelDescription Default(string fileName)
        {
            return new()
            {
                Background = SpriteDescription.Default(fileName),
            };
        }
        /// <summary>
        /// Gets a screen panel description
        /// </summary>
        /// <param name="scene">Scene</param>
        public static UIPanelDescription Screen(Scene scene)
        {
            return new()
            {
                Left = 0,
                Top = 0,
                Width = scene.Game.Form.RenderWidth,
                Height = scene.Game.Form.RenderHeight,
                Background = SpriteDescription.Default(),
            };
        }
        /// <summary>
        /// Gets a screen panel description
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="color">Tint color</param>
        public static UIPanelDescription Screen(Scene scene, Color4 color)
        {
            return new()
            {
                Left = 0,
                Top = 0,
                Width = scene.Game.Form.RenderWidth,
                Height = scene.Game.Form.RenderHeight,
                Background = SpriteDescription.Default(color),
            };
        }
        /// <summary>
        /// Gets a screen panel description
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="fileName">Texture file name for the background</param>
        public static UIPanelDescription Screen(Scene scene, string fileName)
        {
            return new()
            {
                Left = 0,
                Top = 0,
                Width = scene.Game.Form.RenderWidth,
                Height = scene.Game.Form.RenderHeight,
                Background = SpriteDescription.Default(fileName),
            };
        }

        /// <summary>
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; } = SpriteDescription.Default();
        /// <summary>
        /// Grid layout
        /// </summary>
        public GridLayout GridLayout { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UIPanelDescription()
            : base()
        {
            EventsEnabled = false;
        }
    }
}
