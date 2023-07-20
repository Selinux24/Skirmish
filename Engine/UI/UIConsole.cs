using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Console
    /// </summary>
    public sealed class UIConsole : UIControl<UIConsoleDescription>
    {
        /// <summary>
        /// Log lines
        /// </summary>
        private int logLines;
        /// <summary>
        /// Log lines when small sized
        /// </summary>
        private int logLinesSmall;
        /// <summary>
        /// Log lines when big sized
        /// </summary>
        private int logLinesBig;
        /// <summary>
        /// Log formatter function
        /// </summary>
        private Func<LogEntry, string> fncFormatLog;
        /// <summary>
        /// Log filter function
        /// </summary>
        private Func<LogEntry, bool> fncFilterLog;
        /// <summary>
        /// Elapsed seconds since last update
        /// </summary>
        private float elapsedSinceLastUpdate = 0;
        /// <summary>
        /// Text area
        /// </summary>
        private UITextArea textArea;

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
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public UIConsole(Scene scene, string id, string name) :
            base(scene, id, name)
        {

        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(UIConsoleDescription description)
        {
            await base.InitializeAssets(description);

            logLinesSmall = Description.LogLinesSmall;
            logLinesBig = Description.LogLinesBig;
            UpdateInterval = Description.UpdateInterval;

            logLines = logLinesSmall;
            fncFormatLog = Description.LogFormatterFunc ?? FormatLog;
            fncFilterLog = Description.LogFilterFunc ?? FilterLog;

            if (Description.Background != null)
            {
                var background = await CreateBackground();
                AddChild(background);

                textArea = await CreateText();
                background.AddChild(textArea);
            }
            else
            {
                textArea = await CreateText();
                AddChild(textArea);
            }
        }
        private async Task<Sprite> CreateBackground()
        {
            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Background",
                $"{Name}.Background",
                Description.Background);
        }
        private async Task<UITextArea> CreateText()
        {
            var text = await Scene.CreateComponent<UITextArea, UITextAreaDescription>(
                $"{Id}.TextArea",
                $"{Name}.TextArea",
                Description);

            text.GrowControlWithText = false;

            return text;
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
                textArea.Text = Logger.ReadText(fncFilterLog, fncFormatLog, LogLines);
            }
            elapsedSinceLastUpdate %= (float)UpdateInterval.TotalSeconds;
        }
        /// <summary>
        /// Log text formatter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private string FormatLog(LogEntry logEntry)
        {
            var defColor = textArea.TextForeColor;
            var logColor = logEntry.LogLevel switch
            {
                LogLevel.Debug => (Color4)Color.White,
                LogLevel.Information => (Color4)Color.Blue,
                LogLevel.Warning => (Color4)Color.Yellow,
                LogLevel.Error => (Color4)Color.Red,
                _ => textArea.TextForeColor,
            };
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

            Height = textArea.TextLineHeight * LogLines;
            Width = Game.Form.RenderWidth;
        }
    }
}
