using Engine.UI;
using System;
using System.Linq;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Checkbox group editor
    /// </summary>
    class EditorCheckboxGroup<T>
    {
        /// <summary>
        /// Caption
        /// </summary>
        private readonly UITextArea caption;
        /// <summary>
        /// Checkbox list
        /// </summary>
        private readonly UICheckbox[] checkboxes;
        /// <summary>
        /// Value list
        /// </summary>
        private readonly T[] values;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checkboxes">Checkbox list</param>
        /// <param name="values">Value list</param>
        public EditorCheckboxGroup(UITextArea caption, UICheckbox[] checkboxes, T[] values)
        {
            this.caption = caption ?? throw new ArgumentNullException(nameof(caption));
            ArgumentNullException.ThrowIfNull(checkboxes, nameof(checkboxes));
            ArgumentNullException.ThrowIfNull(values, nameof(values));
            ArgumentOutOfRangeException.ThrowIfZero(checkboxes.Length, nameof(checkboxes));
            ArgumentOutOfRangeException.ThrowIfNotEqual(checkboxes.Length, values.Length, nameof(checkboxes));

            this.checkboxes = checkboxes;
            this.values = values;

            foreach (var checkbox in this.checkboxes)
            {
                checkbox.OnValueChanged = (value) =>
                {
                    if (!value)
                    {
                        return;
                    }

                    UpdateCheckbox(checkbox);
                };
            }
        }
        /// <summary>
        /// Updates the check box
        /// </summary>
        /// <param name="checkbox">Checked checkbox</param>
        private void UpdateCheckbox(UICheckbox checkbox)
        {
            for (int i = 0; i < checkboxes.Length; i++)
            {
                if (checkboxes[i] == checkbox)
                {
                    continue;
                }

                checkboxes[i].Checked = false;
            }
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <returns>Returns the value</returns>
        public T GetValue()
        {
            var chkBox = checkboxes.FirstOrDefault(c => c.Checked);
            int index = Array.IndexOf(checkboxes, chkBox);
            if (index < 0)
            {
                return default;
            }

            return values[index];
        }
        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">Value</param>
        public void SetValue(T value)
        {
            var index = Array.IndexOf(values, value);

            for (int i = 0; i < values.Length; i++)
            {
                checkboxes[i].Checked = i == index;
            }
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
            caption.SetPosition(left, top);
            caption.Width = width;
            caption.Visible = visible;
            Editor.NextLine(padding, ref top, caption);

            for (int i = 0; i < checkboxes.Length; i++)
            {
                var checkbox = checkboxes[i];

                checkbox.SetPosition(left, top);
                checkbox.Width = width;
                checkbox.Visible = visible;
                Editor.NextLine(padding, ref top, checkbox);
            }
        }
    }
}
