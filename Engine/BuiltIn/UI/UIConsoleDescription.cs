using Engine.UI;
using SharpDX;
using System;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// UIConsole description
    /// </summary>
    public class UIConsoleDescription : UITextAreaDescription
    {
        /// <summary>
        /// Gets the default console description
        /// </summary>
        public static new UIConsoleDescription Default()
        {
            return new()
            {
                Font = UIConfiguration.MonospacedFont,
                Background = SpriteDescription.Default(),
                LogLinesBig = 50,
                LogLinesSmall = 10,
            };
        }
        /// <summary>
        /// Gets the default console description
        /// </summary>
        /// <param name="backgroundColor">Background color</param>
        public static UIConsoleDescription Default(Color4 backgroundColor)
        {
            return new()
            {
                Font = UIConfiguration.MonospacedFont,
                Background = SpriteDescription.Default(backgroundColor),
                LogLinesBig = 50,
                LogLinesSmall = 10,
            };
        }
        /// <summary>
        /// Gets the default console description
        /// </summary>
        /// <param name="backgroundImage">Background image</param>
        public static UIConsoleDescription FromFile(string backgroundImage)
        {
            return new()
            {
                Font = UIConfiguration.MonospacedFont,
                Background = SpriteDescription.Default(backgroundImage),
                LogLinesBig = 50,
                LogLinesSmall = 10,
            };
        }

        /// <summary>
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; } = SpriteDescription.Default(UIConfiguration.TextBackgroundColor);
        /// <summary>
        /// Log lines in small size
        /// </summary>
        public int LogLinesSmall { get; set; } = 10;
        /// <summary>
        /// Log lines in big size
        /// </summary>
        public int LogLinesBig { get; set; } = 25;
        /// <summary>
        /// Log formatter function
        /// </summary>
        public Func<LogEntry, string> LogFormatterFunc { get; set; }
        /// <summary>
        /// Log filter function
        /// </summary>
        public Func<LogEntry, bool> LogFilterFunc { get; set; }
        /// <summary>
        /// Console text update interval
        /// </summary>
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Constructor
        /// </summary>
        public UIConsoleDescription() : base()
        {

        }
    }
}
