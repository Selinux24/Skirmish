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
        protected bool initialized = false;
        protected bool isDirty = false;
        private bool visible = false;

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
        /// Initializes a panel
        /// </summary>
        /// <param name="name">Name</param>
        protected async Task<UIPanel> InitializePanel(string id, string name)
        {
            var panelColor = new Color4(UIConfiguration.BaseColor.ToVector3(), 0.85f);
            var desc = UIPanelDescription.Default(panelColor);
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UIPanel, UIPanelDescription>(id, name, desc);
        }
        /// <summary>
        /// Initializes a property group
        /// </summary>
        /// <param name="objId">Object id</param>
        /// <param name="id">Id</param>
        /// <param name="font">Dont</param>
        /// <param name="callback">Setter callback function</param>
        protected async Task<EditorSlider> InitializeProperty(string objId, string id, TextDrawerDescription font, float min, float max, float step, Action<int, float> callback)
        {
            var caption = await InitializeText($"{objId}_Caption.{id}", "Caption.{id}", font, id);
            var value = await InitializeText($"{objId}_Value.{id}", "Value.{id}", font);
            var slider = await InitializeSlider($"{objId}.{id}", "{id}", min, max, step, callback);

            return new()
            {
                Caption = caption,
                Value = value,
                Slider = slider
            };
        }
        /// <summary>
        /// Initializes a text area
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        protected async Task<UITextArea> InitializeText(string id, string name, TextDrawerDescription font, string text = null)
        {
            var desc = UITextAreaDescription.Default(font, text);
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UITextArea, UITextAreaDescription>(id, name, desc);
        }
        /// <summary>
        /// Initializes a slider
        /// </summary>
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

            var slider = await scene.AddComponentUI<UISlider, UISliderDescription>(id, name, desc);
            slider.OnValueChanged = callback;

            return slider;
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

            if (isDirty)
            {
                UpdateTextValues();
                UpdateLayout();

                isDirty = false;
            }
        }
        /// <summary>
        /// Updates slider text values
        /// </summary>
        protected abstract void UpdateTextValues();

        /// <summary>
        /// Updates de editor layout
        /// </summary>
        public abstract void UpdateLayout();

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
        /// <param name="caption">Caption control</param>
        /// <param name="value">Value control</param>
        /// <param name="slider">Slider control</param>
        protected void SetGroupPosition(float left, float width, ref float top, UITextArea caption, UITextArea value, UISlider slider)
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

            if (slider != null)
            {
                slider.SetPosition(left, top);
                slider.Width = width;
                slider.Visible = visible;

                NextLine(ref top, slider);
            }

            NextLine(ref top, null);
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
