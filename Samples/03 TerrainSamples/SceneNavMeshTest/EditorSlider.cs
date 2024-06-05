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
        public UITextArea Caption { get; set; }
        /// <summary>
        /// Value text
        /// </summary>
        public UITextArea Value { get; set; }
        /// <summary>
        /// Slider
        /// </summary>
        public UISlider Slider { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="caption">Caption control</param>
        /// <param name="value">Value control</param>
        /// <param name="format">Value format</param>
        /// <param name="slider">Slider control</param>
        public EditorSlider(UITextArea caption, UITextArea value, string format, UISlider slider)
        {
            Caption = caption ?? throw new ArgumentNullException(nameof(caption));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Slider = slider ?? throw new ArgumentNullException(nameof(slider));

            Slider.OnValueChanged = (index, value) =>
            {
                Value.Text = string.Format(format ?? "{0:0.00}", value);

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
            return Slider.GetValue(0);
        }
        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">Value</param>
        public void SetValue(float value)
        {
            Slider.SetValue(0, value);
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
            Caption.SetPosition(groupLeft, top);
            Caption.Width = groupWidth;
            Caption.Visible = groupVisible;

            Value.GrowControlWithText = true;
            Value.SetPosition(groupLeft + groupWidth - Value.Width, top);
            Value.Visible = groupVisible;

            Editor.NextLine(verticalPadding, ref top, Caption);

            Slider.SetPosition(groupLeft, top);
            Slider.Width = groupWidth;
            Slider.Visible = groupVisible;

            Editor.NextLine(verticalPadding, ref top, Slider);
        }
    }
}
