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

        /// <summary>
        /// Horizontal marging
        /// </summary>
        public float HorizontalMarging { get; set; } = 25;
        /// <summary>
        /// Vertical marging
        /// </summary>
        public float VerticalMarging { get; set; } = 25;
        /// <summary>
        /// Vertical padding
        /// </summary>
        public float VerticalPadding { get; set; } = 10;
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
        /// Initializes the editor
        /// </summary>
        /// <param name="font">Font</param>
        public virtual async Task Initialize(TextDrawerDescription font)
        {
            string id = GetType().Name;

            mainPanel = await InitializePanel($"{id}_MainPanel", "MainPanel");

            title = await InitializeText($"{id}_Agent.Title", "Agent.Title", font, "Agent Parameters");

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
        /// Initializes a slider
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="step">Step value</param>
        /// <param name="callback">Value callback</param>
        protected async Task<UISlider> InitializeSlider(string id, string name, float min, float max, float step, Action<int, float> callback)
        {
            var desc = UISliderDescription.Default(1);
            desc.Height = 20;
            desc.Width = 200;
            desc.Minimum = min;
            desc.Maximum = max;
            desc.Step = step;
            desc.StartsVisible = false;

            var slider = await scene.AddComponentUI<UISlider, UISliderDescription>(id, name, desc, Scene.LayerUI + 1);
            slider.OnValueChanged = callback;

            return slider;
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
        /// <param name="callback">Value callback</param>
        protected async Task<EditorSlider> InitializePropertySlider(string objId, string id, TextDrawerDescription font, float min, float max, float step, Action<int, float> callback)
        {
            var caption = await InitializeText($"{objId}_Caption.{id}", $"Caption.{id}", font, id);
            var value = await InitializeText($"{objId}_Value.{id}", $"Value.{id}", font);
            var slider = await InitializeSlider($"{objId}.{id}", id, min, max, step, callback);

            return new()
            {
                Caption = caption,
                Value = value,
                Slider = slider
            };
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

            return new()
            {
                Checkbox = checkbox
            };
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
                UpdateTextValues();
                UpdateLayout();

                IsDirty = false;
            }
        }
        /// <summary>
        /// Updates slider text values
        /// </summary>
        protected abstract void UpdateTextValues();

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

            SetGroupPosition(left, width, ref top, title, null, null);

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
            SetGroupPosition(left, width, ref top, sliderEditor.Caption, sliderEditor.Value, sliderEditor.Slider);
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
            NextLine(ref top, null);

            SetGroupPosition(left, width, ref top, null, null, checkboxEditor.Checkbox);
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
        protected void SetGroupPosition(float left, float width, ref float top, UITextArea caption, UITextArea value, IUIControl ctrl)
        {
            if (caption != null)
            {
                caption.SetPosition(left, top);
                caption.Width = width;
                caption.Visible = visible;

                if (value != null)
                {
                    value.GrowControlWithText = true;
                    value.SetPosition(left + width - value.Width, top);
                    value.Visible = visible;
                }

                NextLine(ref top, caption);
            }

            if (ctrl != null)
            {
                ctrl.SetPosition(left, top);
                ctrl.Width = width;
                ctrl.Visible = visible;

                NextLine(ref top, ctrl);
            }
        }
        /// <summary>
        /// Next line
        /// </summary>
        /// <param name="top">Top position</param>
        /// <param name="control">Last control in the line</param>
        protected void NextLine(ref float top, IUIControl control)
        {
            top += VerticalPadding + (control?.Height ?? 0f);
        }
    }
}
