using System;

namespace Engine.UI
{
    /// <summary>
    /// UI dialog description
    /// </summary>
    public class UIDialogDescription : UIControlDescription
    {
        /// <summary>
        /// Gets a default dialog description
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static UIDialogDescription Default(float width, float height)
        {
            return new UIDialogDescription
            {
                Width = width,
                Height = height,
                Anchor = Anchors.Center,
            };
        }

        /// <summary>
        /// Background description
        /// </summary>
        public UIPanelDescription Background { get; set; } = UIPanelDescription.Default();
        /// <summary>
        /// Text area description
        /// </summary>
        public UITextAreaDescription TextArea { get; set; } = UITextAreaDescription.Default();
        /// <summary>
        /// Dialog buttons enum
        /// </summary>
        /// <remarks>Accept and Cancel by default</remarks>
        public UIDialogButtons DialogButtons { get; set; } = UIDialogButtons.Accept | UIDialogButtons.Cancel;
        /// <summary>
        /// Buttos anchor in the dialog
        /// </summary>
        /// <remarks>Bottom right by default</remarks>
        public Anchors ButtonsAnchor { get; set; } = Anchors.Bottom | Anchors.Right;
        /// <summary>
        /// Buttons description
        /// </summary>
        public UIButtonDescription Buttons { get; set; } = UIButtonDescription.DefaultTwoStateButton();

        /// <summary>
        /// Constructor
        /// </summary>
        public UIDialogDescription() : base()
        {

        }
    }

    /// <summary>
    /// Dialog buttons
    /// </summary>
    [Flags]
    public enum UIDialogButtons
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Accept
        /// </summary>
        Accept = 1,
        /// <summary>
        /// Cancel
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// Close
        /// </summary>
        Close = 4,
    }
}
