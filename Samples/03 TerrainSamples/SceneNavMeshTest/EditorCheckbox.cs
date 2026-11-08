using Engine.BuiltIn.UI;
using System;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Checkbox editor
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
        private readonly UICheckbox checkbox = checkbox ?? throw new ArgumentNullException(nameof(checkbox));

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <returns>Returns the value</returns>
        public bool GetValue()
        {
            return checkbox.Checked;
        }
        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">Value</param>
        public void SetValue(bool value)
        {
            checkbox.Checked = value;
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

            checkbox.SetPosition(left, top);
            checkbox.Width = width;
            checkbox.Visible = visible;

            Editor.NextLine(padding, ref top, checkbox);
        }
    }
}
