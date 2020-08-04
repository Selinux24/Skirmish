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
        public static UIPanelDescription Default
        {
            get
            {
                return new UIPanelDescription()
                {
                    Background = new SpriteDescription
                    {
                        TintColor = Color4.Black * 0.3333f,
                        BlendMode = BlendModes.Alpha,
                    },
                };
            }
        }

        /// <summary>
        /// Gets a screen panel description
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <returns></returns>
        public static UIPanelDescription Screen(Scene scene)
        {
            return new UIPanelDescription
            {
                Background = new SpriteDescription
                {
                    TintColor = SharpDX.Color.Black,
                },
                Left = 0,
                Top = 0,
                Width = scene.Game.Form.RenderWidth,
                Height = scene.Game.Form.RenderHeight,
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
