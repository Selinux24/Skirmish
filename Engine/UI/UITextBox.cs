using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Text box
    /// </summary>
    public class UITextBox : UITextArea
    {
        /// <summary>
        /// Focus flag
        /// </summary>
        private bool hasFocus = false;

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
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UITextBox(string id, string name, Scene scene, UITextBoxDescription description) :
            base(id, name, scene, description)
        {
            GrowControlWithText = false;

            if (description.Background != null)
            {
                var background = new Sprite(
                    $"{id}.Background",
                    $"{name}.Background",
                    scene,
                    description.Background);

                AddChild(background);
            }

            EventsEnabled = true;

            Cursor = description.Cursor;
            TabSpaces = description.TabSpaces;
            Size = description.Size;
            MultiLine = description.MultiLine;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!hasFocus)
            {
                if (Text?.EndsWith(Cursor.ToString()) == true)
                {
                    Text = Text.Remove(Text.Length - 1);
                }

                return;
            }

            if (Text?.EndsWith(Cursor.ToString()) == false)
            {
                Text += Cursor.ToString();
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

            SetText(Helpers.NativeMethods.GetStrokes());
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

            int textSize = (Text?.Length ?? 0) - 1;

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
            string currText = Text;

            if (string.IsNullOrEmpty(currText))
            {
                Text = newText;

                return;
            }

            Text = currText.Insert(currText.Length - 1, newText);
        }
        /// <summary>
        /// Does the back operation. Removes the last character
        /// </summary>
        /// <param name="currText">Current text</param>
        /// <param name="cursor">Cursor text</param>
        /// <returns>Returns the updated text</returns>
        private void DoBack()
        {
            string currText = Text;
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
                Text = currText.Remove(currText.Length - 3, 2);

                return;
            }

            //Removes the last character
            Text = currText.Remove(currText.Length - 2, 1);
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

    /// <summary>
    /// UITextArea extensions
    /// </summary>
    public static class UITextBoxExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UITextBox> AddComponentUITextBox(this Scene scene, string id, string name, UITextBoxDescription description, int layer = Scene.LayerUI)
        {
            UITextBox component = null;

            await Task.Run(() =>
            {
                component = new UITextBox(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
