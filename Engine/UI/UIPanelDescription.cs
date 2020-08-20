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
        /// <param name="color">Tint color</param>
        public static UIPanelDescription Default(Color4 color)
        {
            var blendMode = color.Alpha >= 1f ? BlendModes.Opaque : BlendModes.Alpha;

            return new UIPanelDescription()
            {
                Background = new SpriteDescription
                {
                    TintColor = color,
                    BlendMode = blendMode,
                },
                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets a screen panel description
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="color">Tint color</param>
        public static UIPanelDescription Screen(Scene scene, Color4 color)
        {
            var blendMode = color.Alpha >= 1f ? BlendModes.Opaque : BlendModes.Alpha;

            return new UIPanelDescription
            {
                Background = new SpriteDescription
                {
                    TintColor = color,
                    BlendMode = blendMode,
                },
                Left = 0,
                Top = 0,
                Width = scene.Game.Form.RenderWidth,
                Height = scene.Game.Form.RenderHeight,
                BlendMode = blendMode,
            };
        }

        /// <summary>
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UIPanelDescription()
            : base()
        {

        }
    }
}
