using SharpDX;
using System;

namespace Engine.UI
{
    /// <summary>
    /// UIConsole description
    /// </summary>
    public class UIConsoleDescription : UITextAreaDescription
    {
        /// <summary>
        /// Gets the default console description
        /// </summary>
        public static UIConsoleDescription Default()
        {
            return new UIConsoleDescription()
            {
                Background = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)),
                Font = TextDrawerDescription.Default(),
                LogLinesBig = 50,
                LogLinesSmall = 10,
            };
        }
        /// <summary>
        /// Gets the default console description
        /// </summary>
        /// <param name="backgroundColor">Background color</param>
        public static UIConsoleDescription Default(Color backgroundColor)
        {
            return new UIConsoleDescription()
            {
                Background = SpriteDescription.Default(backgroundColor),
                Font = TextDrawerDescription.Default(),
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
            return new UIConsoleDescription()
            {
                Background = SpriteDescription.FromFile(backgroundImage),
                Font = TextDrawerDescription.Default(),
                LogLinesBig = 50,
                LogLinesSmall = 10,
            };
        }

        /// <summary>
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; }
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
        /// Console text update interval
        /// </summary>
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMilliseconds(100);
    }
}
