
namespace Engine.UI
{
    /// <summary>
    /// Panel description
    /// </summary>
    public class UIPanelDescription : UIControlDescription
    {
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
                    Color = SharpDX.Color.Black,
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
