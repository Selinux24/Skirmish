using Engine;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Base editor
    /// </summary>
    /// <param name="scene">Scene</param>
    abstract class Editor(Scene scene)
    {
        private readonly Scene scene = scene;
        protected bool IsDirty = false;

        private bool visible = false;
        private bool initialized = false;

        private UIPanel mainPanel;
        private UITextArea title;
        private UIButton acceptButton;
        private UIButton exitButton;

        /// <summary>
        /// Horizontal marging
        /// </summary>
        public float HorizontalMarging { get; set; } = 20;
        /// <summary>
        /// Vertical marging
        /// </summary>
        public float VerticalMarging { get; set; } = 12;
        /// <summary>
        /// Vertical padding
        /// </summary>
        public float VerticalPadding { get; set; } = 6;
        /// <summary>
        /// Editor top-left position
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.Zero;
        /// <summary>
        /// Editor width
        /// </summary>
        public float Width { get; set; } = 0;
        /// <summary>
        /// Editor visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;

            }
            set
            {
                if (visible == value)
                {
                    return;
                }

                visible = value;

                UpdateLayout();
            }
        }
        /// <summary>
        /// Close callback
        /// </summary>
        public Action<bool> CloseCallback { get; set; }

        /// <summary>
        /// Initializes the editor
        /// </summary>
        /// <param name="font">Font</param>
        public virtual async Task Initialize(TextDrawerDescription font)
        {
            string id = GetType().Name;

            mainPanel = await InitializePanel($"{id}_MainPanel", "MainPanel");

            title = await InitializeText($"{id}_Agent.Title", "Agent.Title", font, "Agent Parameters");

            await InitializeButtons(id);

            initialized = true;

            UpdateLayout();
        }
        /// <summary>
        /// Initializes a panel
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        protected async Task<UIPanel> InitializePanel(string id, string name)
        {
            var panelColor = new Color4(UIConfiguration.BaseColor.ToVector3(), 0.85f);
            var desc = UIPanelDescription.Default(panelColor);
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UIPanel, UIPanelDescription>(id, name, desc, Scene.LayerUI);
        }
        /// <summary>
        /// Initializes a text area
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        protected async Task<UITextArea> InitializeText(string id, string name, TextDrawerDescription font, string text = null)
        {
            var desc = UITextAreaDescription.Default(font, text);
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UITextArea, UITextAreaDescription>(id, name, desc, Scene.LayerUI + 1);
        }
        /// <summary>
        /// Initializes the accept and exit button
        /// </summary>
        /// <param name="id">Id</param>
        protected async Task InitializeButtons(string id)
        {
            var font = TextDrawerDescription.FromFamily("Wingdings 2", 20, FontMapStyles.Bold, true);

            var desc = UIButtonDescription.DefaultTwoStateButton(font);
            desc.Width = 24;
            desc.Height = 22;
            desc.TextHorizontalAlign = TextHorizontalAlign.Center;
            desc.TextVerticalAlign = TextVerticalAlign.Middle;
            desc.ColorReleased = UIConfiguration.BaseColor;
            desc.ColorPressed = UIConfiguration.HighlightColor;
            desc.StartsVisible = false;

            acceptButton = await scene.AddComponentUI<UIButton, UIButtonDescription>($"{id}_AcceptButton", "AcceptButton", desc, Scene.LayerUI + 1);
            exitButton = await scene.AddComponentUI<UIButton, UIButtonDescription>($"{id}_ExitButton", "ExitButton", desc, Scene.LayerUI + 1);

            acceptButton.Caption.Text = "P";
            exitButton.Caption.Text = "O";

            acceptButton.MouseClick += (s, e) =>
            {
                CloseCallback?.Invoke(true);

                Visible = false;
            };
            exitButton.MouseClick += (s, e) =>
            {
                CloseCallback?.Invoke(false);

                Visible = false;
            };
        }
        /// <summary>
        /// Initializes a slider
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="step">Step value</param>
        protected async Task<UISlider> InitializeSlider(string id, string name, float min, float max, float step)
        {
            var desc = UISliderDescription.Default(1);
            desc.Height = 20;
            desc.Width = 200;
            desc.Minimum = min;
            desc.Maximum = max;
            desc.Step = step;
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UISlider, UISliderDescription>(id, name, desc, Scene.LayerUI + 1);
        }
        /// <summary>
        /// Initializes a checkbox
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="font">Font</param>
        /// <param name="text">Caption text</param>
        protected async Task<UICheckbox> InitializeCheckbox(string id, string name, TextDrawerDescription font, string text)
        {
            var desc = UICheckboxDescription.Default(font);
            desc.Text = text;
            desc.Height = 20;
            desc.Width = 200;
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UICheckbox, UICheckboxDescription>(id, name, desc, Scene.LayerUI + 1);
        }
        /// <summary>
        /// Initializes a property group
        /// </summary>
        /// <param name="objId">Object id</param>
        /// <param name="id">Id</param>
        /// <param name="font">Font</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="step">Step value</param>
        /// <param name="format">Format</param>
        protected async Task<EditorSlider> InitializePropertySlider(string objId, string id, TextDrawerDescription font, float min, float max, float step, string format)
        {
            var caption = await InitializeText($"{objId}_Caption.{id}", $"Caption.{id}", font, id);
            var value = await InitializeText($"{objId}_Value.{id}", $"Value.{id}", font);
            var slider = await InitializeSlider($"{objId}.{id}", id, min, max, step);

            return new(caption, value, format, slider);
        }
        /// <summary>
        /// Initializes a property group
        /// </summary>
        /// <param name="objId">Object id</param>
        /// <param name="id">Id</param>
        /// <param name="font">Font</param>
        protected async Task<EditorCheckbox> InitializePropertyCheckbox(string objId, string id, TextDrawerDescription font)
        {
            var checkbox = await InitializeCheckbox($"{objId}.{id}", id, font, id);

            return new(checkbox);
        }
        /// <summary>
        /// Initializes a property group
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="objId">Object id</param>
        /// <param name="id">Id</param>
        /// <param name="font">Font</param>
        /// <param name="values">Value list</param>
        protected async Task<EditorCheckboxGroup<T>> InitializePropertyCheckboxGroup<T>(string objId, string id, TextDrawerDescription font, T[] values)
        {
            var caption = await InitializeText($"{objId}_Caption.{id}", $"Caption.{id}", font, id);

            UICheckbox[] chkBoxes = new UICheckbox[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                chkBoxes[i] = await InitializeCheckbox($"{objId}.{id}.{i}", $"{id}.{i}", font, values[i].ToString());
            }

            return new(caption, chkBoxes, values);
        }

        /// <summary>
        /// Updates the editor
        /// </summary>
        public void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (IsDirty)
            {
                UpdateLayout();

                IsDirty = false;
            }
        }

        /// <summary>
        /// Updates de editor layout
        /// </summary>
        public void UpdateLayout()
        {
            if (!initialized)
            {
                return;
            }

            float top = Position.Y + VerticalMarging;
            float left = Position.X + HorizontalMarging;
            float width = Width - (HorizontalMarging * 2);

            acceptButton.SetPosition(Position.X + Width - acceptButton.Width - exitButton.Width - 1, Position.Y);
            acceptButton.Visible = Visible;
            exitButton.SetPosition(Position.X + Width - exitButton.Width, Position.Y);
            exitButton.Visible = Visible;

            SetTitlePosition(left, width, ref top, title);

            UpdateControlsLayout(left, width, ref top);

            mainPanel.SetPosition(Position);
            mainPanel.Width = Width;
            mainPanel.Height = top + VerticalMarging - Position.Y;
            mainPanel.Visible = Visible;
        }
        /// <summary>
        /// Updates the controls layout
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="width">Width</param>
        /// <param name="top">Current top position</param>
        protected abstract void UpdateControlsLayout(float left, float width, ref float top);
        /// <summary>
        /// Sets the editor group position
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="width">Width</param>
        /// <param name="top">Top</param>
        /// <param name="sliderEditor">Slider editor</param>
        protected void SetGroupPosition(float left, float width, ref float top, EditorSlider sliderEditor)
        {
            sliderEditor.SetGroupPosition(visible, left, width, VerticalPadding, ref top);
        }
        /// <summary>
        /// Sets the editor group position
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="width">Width</param>
        /// <param name="top">Top</param>
        /// <param name="checkboxEditor">Checkbox editor</param>
        protected void SetGroupPosition(float left, float width, ref float top, EditorCheckbox checkboxEditor)
        {
            checkboxEditor.SetGroupPosition(visible, left, width, VerticalPadding, ref top);
        }
        /// <summary>
        /// Sets the editor group position
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="width">Width</param>
        /// <param name="top">Top</param>
        /// <param name="checkboxEditor">Checkbox editor</param>
        protected void SetGroupPosition<T>(float left, float width, ref float top, EditorCheckboxGroup<T> checkboxEditor)
        {
            checkboxEditor.SetGroupPosition(visible, left, width, VerticalPadding, ref top);
        }
        /// <summary>
        /// Sets the editor group position
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="width">Width</param>
        /// <param name="top">Top</param>
        /// <param name="caption">Caption control</param>
        /// <param name="value">Text value control</param>
        /// <param name="ctrl">Property control</param>
        protected void SetTitlePosition(float left, float width, ref float top, UITextArea ctrl)
        {
            if (ctrl != null)
            {
                ctrl.SetPosition(left, top);
                ctrl.Width = width;
                ctrl.Visible = visible;

                NextLine(VerticalPadding, ref top, ctrl);
            }
        }
        /// <summary>
        /// Next line
        /// </summary>
        /// <param name="top">Top position</param>
        /// <param name="control">Last control in the line</param>
        public static void NextLine(float verticalPadding, ref float top, IUIControl control)
        {
            top += verticalPadding + (control?.Height ?? 0f);
        }
    }
}
