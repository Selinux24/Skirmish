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
        public static UITabPanelDescription Default(int tabs)
        {
            return new()
            {
                Background = SpriteDescription.Default(),
                ButtonDescription = UIButtonDescription.DefaultTwoStateButton(),
                PanelDescription = UIPanelDescription.Default(),
                Tabs = tabs,
            };
        }
        /// <summary>
        /// Gets the default tab panel description
        /// </summary>
        /// <param name="tabCaptions">Tab captions</param>
        public static UITabPanelDescription Default(string[] tabCaptions)
        {
            var desc = Default(tabCaptions.Length);
            desc.TabCaptions = tabCaptions;
            return desc;
        }
        /// <summary>
        /// Gets the default tab panel description
        /// </summary>
        /// <param name="tabs">Number of tabs</param>
        /// <param name="backgroundColor">Back color</param>
        /// <param name="baseColor">Control color</param>
        /// <param name="selectedColor">Highlight color</param>
        public static UITabPanelDescription Default(int tabs, Color4 backgroundColor, Color4 baseColor, Color4 selectedColor)
        {
            return new()
            {
                Background = SpriteDescription.Default(backgroundColor),
                ButtonDescription = UIButtonDescription.DefaultTwoStateButton(baseColor, selectedColor),
                PanelDescription = UIPanelDescription.Default(baseColor),
                Tabs = tabs,
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
            desc.TabCaptions = tabCaptions;
            return desc;
        }

        /// <summary>
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; } = SpriteDescription.Default();
        /// <summary>
        /// Button description
        /// </summary>
        public UIButtonDescription ButtonDescription { get; set; } = UIButtonDescription.DefaultTwoStateButton();
        /// <summary>
        /// Panel description
        /// </summary>
        public UIPanelDescription PanelDescription { get; set; } = UIPanelDescription.Default();
        /// <summary>
        /// Number of tabs
        /// </summary>
        public int Tabs { get; set; } = 1;
        /// <summary>
        /// Tab button captions
        /// </summary>
        public string[] TabCaptions { get; set; } = ["Tab 1"];

        /// <summary>
        /// Tab button text padding
        /// </summary>
        public Padding TabButtonPadding { get; set; } = UIConfiguration.Padding;
        /// <summary>
        /// Tab panel internal padding
        /// </summary>
        public Padding TabPanelPadding { get; set; } = UIConfiguration.Padding;
        /// <summary>
        /// Tab panel internal spacing
        /// </summary>
        public Spacing TabPanelSpacing { get; set; } = UIConfiguration.Spacing;

        /// <summary>
        /// Tab buttons area size
        /// </summary>
        public float TabButtonsAreaSize { get; set; } = 40;
        /// <summary>
        /// Tab buttons area padding
        /// </summary>
        public Padding TabButtonsPadding { get; set; } = UIConfiguration.Padding;
        /// <summary>
        /// Tab buttons area spacing
        /// </summary>
        public Spacing TabButtonsSpacing { get; set; } = UIConfiguration.Spacing;
        /// <summary>
        /// Tab panels area padding
        /// </summary>
        public Padding TabPanelsPadding { get; set; } = UIConfiguration.Padding;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITabPanelDescription()
            : base()
        {

        }
    }
}
