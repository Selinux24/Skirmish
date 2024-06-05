using Engine.UI;
using System;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Checkbox group editor
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="checkbox">Checkbox</param>
    class EditorCheckbox(UICheckbox checkbox)
    {
        /// <summary>
        /// Checkbox
        /// </summary>
        public UICheckbox Checkbox { get; set; } = checkbox ?? throw new ArgumentNullException(nameof(checkbox));

        /// <summary>
        /// Sets the caption
        /// </summary>
        /// <param name="caption">Caption text</param>
        public void SetCaption(string caption)
        {
            Checkbox.Caption.Text = caption;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <returns>Returns the value</returns>
        public bool GetValue()
        {
            return Checkbox.Checked;
        }
        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">Value</param>
        public void SetValue(bool value)
        {
            Checkbox.Checked = value;
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
            Editor.NextLine(padding, ref top, null);

            Checkbox.SetPosition(left, top);
            Checkbox.Width = width;
            Checkbox.Visible = visible;

            Editor.NextLine(padding, ref top, Checkbox);
        }
    }
}
