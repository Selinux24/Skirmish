using Engine.UI;
using System;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Editor slider
    /// </summary>
    class EditorSlider
    {
        private bool groupVisible = true;
        private float groupTop = 0;
        private float groupLeft = 0;
        private float groupWidth = 0;
        private float verticalPadding = 0;

        /// <summary>
        /// Caption
        /// </summary>
        private readonly UITextArea caption;
        /// <summary>
        /// Value text
        /// </summary>
        private readonly UITextArea value;
        /// <summary>
        /// Slider
        /// </summary>
        private readonly UISlider slider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="caption">Caption control</param>
        /// <param name="value">Value control</param>
        /// <param name="format">Value format</param>
        /// <param name="slider">Slider control</param>
        public EditorSlider(UITextArea caption, UITextArea value, string format, UISlider slider)
        {
            this.caption = caption ?? throw new ArgumentNullException(nameof(caption));
            this.value = value ?? throw new ArgumentNullException(nameof(value));
            this.slider = slider ?? throw new ArgumentNullException(nameof(slider));

            this.slider.OnValueChanged = (index, value) =>
            {
                this.value.Text = string.Format(format ?? "{0:0.00}", value);

                float top = groupTop;
                SetPosition(ref top);
            };
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <returns>Returns the value</returns>
        public float GetValue()
        {
            return slider.GetValue(0);
        }
        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">Value</param>
        public void SetValue(float value)
        {
            slider.SetValue(0, value);
        }

        /// <summary>
        /// Sets the group position
        /// </summary>
        /// <param name="visible">Visible</param>
        /// <param name="left">Left position</param>
        /// <param name="width">Width</param>
        /// <param name="padding">Vertical padding</param>
        /// <param name="top">Top position</param>
        public void SetGroupPosition(bool visible, float left, float width, float padding, ref float top)
        {
            groupVisible = visible;
            groupTop = top;
            groupLeft = left;
            groupWidth = width;
            verticalPadding = padding;

            SetPosition(ref top);
        }
        /// <summary>
        /// Sets the position
        /// </summary>
        /// <param name="top">Top position</param>
        private void SetPosition(ref float top)
        {
            caption.SetPosition(groupLeft, top);
            caption.Width = groupWidth;
            caption.Visible = groupVisible;

            value.GrowControlWithText = true;
            value.SetPosition(groupLeft + groupWidth - value.Width, top);
            value.Visible = groupVisible;

            Editor.NextLine(verticalPadding, ref top, caption);

            slider.SetPosition(groupLeft, top);
            slider.Width = groupWidth;
            slider.Visible = groupVisible;

            Editor.NextLine(verticalPadding, ref top, slider);
        }
    }
}
