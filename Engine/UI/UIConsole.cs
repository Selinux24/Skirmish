using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Console
    /// </summary>
    public class UIConsole : UITextArea
    {
        /// <summary>
        /// Log lines
        /// </summary>
        private int logLines;
        /// <summary>
        /// Log lines when small sized
        /// </summary>
        private readonly int logLinesSmall;
        /// <summary>
        /// Log lines when big sized
        /// </summary>
        private readonly int logLinesBig;
        /// <summary>
        /// Log formatter function
        /// </summary>
        private readonly Func<LogEntry, string> fncFormatLog;
        /// <summary>
        /// Log filter function
        /// </summary>
        private readonly Func<LogEntry, bool> fncFilterLog;
        /// <summary>
        /// Elapsed seconds since last update
        /// </summary>
        private float elapsedSinceLastUpdate = 0;

        /// <summary>
        /// Log lines
        /// </summary>
        public int LogLines
        {
            get
            {
                return logLines;
            }
            set
            {
                if (logLines != value)
                {
                    logLines = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Console text update interval
        /// </summary>
        public TimeSpan UpdateInterval { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIConsole(string name, Scene scene, UIConsoleDescription description) : base(name, scene, description)
        {
            GrowControlWithText = false;

            if (description.Background != null)
            {
                var background = new Sprite($"{name}.Background", scene, description.Background);

                AddChild(background);
            }

            logLinesSmall = description.LogLinesSmall;
            logLinesBig = description.LogLinesBig;
            logLines = logLinesSmall;
            fncFormatLog = description.LogFormatterFunc ?? FormatLog;
            fncFilterLog = description.LogFilterFunc ?? FilterLog;
            UpdateInterval = description.UpdateInterval;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Visible)
            {
                return;
            }

            elapsedSinceLastUpdate += context.GameTime.ElapsedSeconds;
            if (elapsedSinceLastUpdate > UpdateInterval.TotalSeconds)
            {
                Text = Logger.ReadText(fncFilterLog, fncFormatLog, LogLines);
            }
            elapsedSinceLastUpdate %= (float)UpdateInterval.TotalSeconds;
        }
        /// <summary>
        /// Log text formatter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private string FormatLog(LogEntry logEntry)
        {
            Color4 defColor = TextForeColor;

            Color4 logColor;
            switch (logEntry.LogLevel)
            {
                case LogLevel.Debug:
                    logColor = Color.White;
                    break;
                case LogLevel.Information:
                    logColor = Color.Blue;
                    break;
                case LogLevel.Warning:
                    logColor = Color.Yellow;
                    break;
                case LogLevel.Error:
                    logColor = Color.Red;
                    break;
                default:
                    logColor = TextForeColor;
                    break;
            }

            return $"{logEntry.EventDate:HH:mm:ss.fff} {logColor}[{logEntry.LogLevel}]{defColor}> {logEntry.Text}{Environment.NewLine}";
        }
        /// <summary>
        /// Log filter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private bool FilterLog(LogEntry logEntry)
        {
            return logEntry != null;
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            Width = Game.Form.RenderWidth;

            base.Resize();
        }

        /// <summary>
        /// Toggles the console height
        /// </summary>
        public void Toggle()
        {
            if (!Visible)
            {
                Visible = true;
                LogLines = logLinesSmall;
            }
            else if (LogLines == logLinesSmall)
            {
                LogLines = logLinesBig;
            }
            else if (LogLines == logLinesBig)
            {
                Visible = false;
                LogLines = logLinesSmall;
            }

            Height = TextLineHeight * LogLines;
            Width = Game.Form.RenderWidth;
        }
    }

    /// <summary>
    /// UIConsole extensions
    /// </summary>
    public static class UIConsoleExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIConsole> AddComponentUIConsole(this Scene scene, string name, UIConsoleDescription description, int layer = Scene.LayerUI)
        {
            UIConsole component = null;

            await Task.Run(() =>
            {
                component = new UIConsole(name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
