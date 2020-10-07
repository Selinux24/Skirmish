using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Tab Panel description
    /// </summary>
    public class UITabPanelDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default tab panel description
        /// </summary>
        /// <param name="tabs">Number of tabs</param>
        /// <param name="backgroundColor">Back color</param>
        /// <param name="baseColor">Control color</param>
        /// <param name="selectedColor">Highlight color</param>
        public static UITabPanelDescription Default(int tabs, Color4 backgroundColor, Color4 baseColor, Color4 selectedColor)
        {
            var backgroundDesc = SpriteDescription.Default(backgroundColor);
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton(baseColor, selectedColor);
            var panelDesc = UIPanelDescription.Default(baseColor);

            buttonDesc.Caption.Font.VerticalAlign = VerticalTextAlign.Middle;
            buttonDesc.Caption.Font.HorizontalAlign = HorizontalTextAlign.Center;

            return new UITabPanelDescription()
            {
                Background = backgroundDesc,
                ButtonDescription = buttonDesc,
                PanelDescription = panelDesc,
                Tabs = tabs,
                BaseColor = Color.Transparent,
            };
        }
        /// <summary>
        /// Gets the default tab panel description
        /// </summary>
        /// <param name="tabCaptions">Tab captions</param>
        /// <param name="backgroundColor">Back color</param>
        /// <param name="baseColor">Control color</param>
        /// <param name="selectedColor">Highlight color</param>
        public static UITabPanelDescription Default(string[] tabCaptions, Color4 backgroundColor, Color4 baseColor, Color4 selectedColor)
        {
            var desc = Default(tabCaptions.Length, backgroundColor, baseColor, selectedColor);
            desc.Captions = tabCaptions;

            return desc;
        }

        /// <summary>
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; } = SpriteDescription.Default(Color.Black);
        /// <summary>
        /// Button description
        /// </summary>
        public UIButtonDescription ButtonDescription { get; set; } = UIButtonDescription.Default(Color.White);
        /// <summary>
        /// Panel description
        /// </summary>
        public UIPanelDescription PanelDescription { get; set; } = UIPanelDescription.Default(Color.White);
        /// <summary>
        /// Number of tabs
        /// </summary>
        public int Tabs { get; set; } = 1;
        /// <summary>
        /// Tab button captions
        /// </summary>
        public string[] Captions { get; set; } = new string[] { "Tab 1" };
        /// <summary>
        /// Margin value
        /// </summary>
        public float Margin { get; set; } = 0f;
        /// <summary>
        /// Spacing value
        /// </summary>
        public float Spacing { get; set; } = 5f;
        /// <summary>
        /// Button area size
        /// </summary>
        public float ButtonAreaSize { get; set; } = 40;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITabPanelDescription()
            : base()
        {

        }
    }
}
