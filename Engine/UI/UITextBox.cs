using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

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

        /// <inheritdoc/>
        public override async Task ReadAssets(UITextBoxDescription description)
        {
            await base.ReadAssets(description);

            Cursor = Description.Cursor;
            TabSpaces = Description.TabSpaces;
            Size = Description.Size;
            MultiLine = Description.MultiLine;

            if (Description.Background != null)
            {
                var background = await CreateBackground();
                AddChild(background, true);

                textArea = await CreateText();
                background.AddChild(textArea, true);
            }
            else
            {
                textArea = await CreateText();
                AddChild(textArea, true);
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
                $"{Id}.Text",
                $"{Name}.Text",
                Description);

            text.EventsEnabled = true;
            text.GrowControlWithText = false;

            return text;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!hasFocus)
            {
                if (textArea.Text?.EndsWith(Cursor.ToString()) == true)
                {
                    textArea.Text = textArea.Text.Remove(textArea.Text.Length - 1);
                }

                return;
            }

            if (textArea.Text?.EndsWith(Cursor.ToString()) == false)
            {
                textArea.Text += Cursor.ToString();
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                SetFocusLost();

                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Back))
            {
                DoBack();

                return;
            }

            if (!EvaluateSize())
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Enter))
            {
                DoEnter();

                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                DoTab();

                return;
            }

            SetText(Game.Input.GetStrokes());
        }
        /// <summary>
        /// Evaluates the size limit
        /// </summary>
        /// <returns>Returns true until the size limit is reached</returns>
        private bool EvaluateSize()
        {
            if (Size <= 0)
            {
                //No size limit
                return true;
            }

            int textSize = (textArea.Text?.Length ?? 0) - 1;

            return textSize < Size;
        }

        /// <summary>
        /// Sets the control text
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="newText">Text to add</param>
        /// <returns>Returns the updated text</returns>
        private void SetText(string newText)
        {
            string currText = textArea.Text;

            if (string.IsNullOrEmpty(currText))
            {
                textArea.Text = newText;

                return;
            }

            textArea.Text = currText.Insert(currText.Length - 1, newText);
        }
        /// <summary>
        /// Does the back operation. Removes the last character
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="cursor">Cursor text</param>
        /// <returns>Returns the updated text</returns>
        private void DoBack()
        {
            string currText = textArea.Text;
            string cursor = Cursor.ToString();

            if (string.IsNullOrEmpty(currText))
            {
                //No text
                return;
            }

            if (currText == cursor)
            {
                //Cursor only
                return;
            }

            if (currText.EndsWith(Environment.NewLine + cursor))
            {
                //Removes the new line string
                textArea.Text = currText.Remove(currText.Length - 3, 2);

                return;
            }

            //Removes the last character
            textArea.Text = currText.Remove(currText.Length - 2, 1);
        }
        /// <summary>
        /// Does the enter operation. Adds a new line
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <returns>Returns the updated text</returns>
        private void DoEnter()
        {
            if (!MultiLine)
            {
                return;
            }

            SetText(Environment.NewLine);
        }
        /// <summary>
        /// Does the tab operation. Adds a number of white spaces
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="tabSpaces">Tab spaces</param>
        /// <returns>Returns the updated text</returns>
        private void DoTab()
        {
            SetText(string.Empty.PadRight(Math.Max(1, TabSpaces)));
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
