using Engine.Common;
using Engine.UI;
using System;
using System.Threading.Tasks;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// Text box
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UITextBox(Scene scene, string id, string name) : UIControl<UITextBoxDescription>(scene, id, name)
    {
        /// <summary>
        /// Focus flag
        /// </summary>
        private bool hasFocus = false;
        /// <summary>
        /// Text area
        /// </summary>
        private UITextArea textArea = null;
        /// <summary>
        /// Text string
        /// </summary>
        private string text = string.Empty;
        /// <summary>
        /// Text string change flag
        /// </summary>
        private bool textChanged = false;

        /// <summary>
        /// Cursor character
        /// </summary>
        public char Cursor { get; set; }
        /// <summary>
        /// Tab space count
        /// </summary>
        public int TabSpaces { get; set; }
        /// <summary>
        /// Maximum text size
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Enables multi line text
        /// </summary>
        public bool MultiLine { get; set; }
        /// <summary>
        /// Gets or sets the box text
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text == value)
                {
                    return;
                }

                text = value;
                textChanged = true;
            }
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(UITextBoxDescription description)
        {
            await base.ReadAssets(description);

            text = Description.Text;
            Cursor = Description.Cursor;
            TabSpaces = Description.TabSpaces;
            Size = Description.Size;
            MultiLine = Description.MultiLine;

            if (Description.Background != null)
            {
                var background = await CreateBackground();
                AddChild(background, true);

                textArea = await CreateText();
                background.AddChild(textArea, false);
            }
            else
            {
                textArea = await CreateText();
                AddChild(textArea, false);
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
            var textArea = await Scene.CreateComponent<UITextArea, UITextAreaDescription>(
                $"{Id}.Text",
                $"{Name}.Text",
                Description);

            textArea.EventsEnabled = true;
            textArea.GrowControlWithText = false;

            return textArea;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            string copy = text;
            bool changed = textChanged;

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                SetFocusLost();
            }
            else if (Game.Input.KeyJustReleased(Keys.Back))
            {
                changed = DoBack(ref copy) || changed;
            }
            else if (Game.Input.KeyJustReleased(Keys.Enter))
            {
                changed = DoEnter(ref copy, MultiLine) || changed;
            }
            else if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                changed = DoTab(ref copy, TabSpaces) || changed;
            }
            else
            {
                changed = SetText(ref copy, Game.Input.GetStrokes()) || changed;
            }

            if (!changed)
            {
                return;
            }

            if (!EvaluateSize(ref copy, Size))
            {
                return;
            }

            text = copy;
            textChanged = changed;

            SetTextValue();
        }

        /// <summary>
        /// Sets the control text
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="newText">Text to add</param>
        /// <returns>Returns true if the text changes</returns>
        private static bool SetText(ref string currText, string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return false;
            }

            if (string.IsNullOrEmpty(currText))
            {
                currText = newText;

                return true;
            }

            currText += newText;

            return true;
        }
        /// <summary>
        /// Does the back operation. Removes the last character
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <returns>Returns true if the text changes</returns>
        private static bool DoBack(ref string currText)
        {
            if (string.IsNullOrEmpty(currText))
            {
                //No text
                return false;
            }

            if (currText.EndsWith(Environment.NewLine))
            {
                //Removes the new line string
                int nl = Environment.NewLine.Length;
                currText = currText.Remove(currText.Length - nl, nl);

                return true;
            }

            //Removes the last character
            currText = currText.Remove(currText.Length - 1, 1);

            return true;
        }
        /// <summary>
        /// Does the enter operation. Adds a new line
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="multiLine">Multi-line</param>
        /// <returns>Returns true if the text changes</returns>
        private static bool DoEnter(ref string currText, bool multiLine)
        {
            if (!multiLine)
            {
                return false;
            }

            currText += Environment.NewLine;

            return true;
        }
        /// <summary>
        /// Does the tab operation. Adds a number of white spaces
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="tabSpaces">Tab spaces</param>
        /// <returns>Returns true if the text changes</returns>
        private static bool DoTab(ref string currText, int tabSpaces)
        {
            if (tabSpaces <= 0)
            {
                return false;
            }

            currText += string.Empty.PadRight(Math.Max(1, tabSpaces));

            return true;
        }
        /// <summary>
        /// Evaluates the size limit
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="size">Maximum size</param>
        /// <returns>Returns true if the text changes</returns>
        private static bool EvaluateSize(ref string currText, int size)
        {
            if (size <= 0)
            {
                //No size limit
                return true;
            }

            return (currText?.Length ?? 0) < size;
        }
        /// <summary>
        /// Sets the text value to the text area
        /// </summary>
        private void SetTextValue()
        {
            if (!textChanged)
            {
                return;
            }

            textChanged = false;

            if (!hasFocus)
            {
                textArea.Text = text;

                return;
            }

            textArea.Text = text + Cursor.ToString();
        }

        /// <inheritdoc/>
        protected override void FireSetFocusEvent()
        {
            base.FireSetFocusEvent();

            hasFocus = true;
        }
        /// <inheritdoc/>
        protected override void FireLostFocusEvent()
        {
            base.FireLostFocusEvent();

            hasFocus = false;
        }
    }
}
