using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// UI dialog
    /// </summary>
    public class UIDialog : UIControl
    {
        /// <summary>
        /// Background panel
        /// </summary>
        private readonly UIPanel backPanel;
        /// <summary>
        /// Close button
        /// </summary>
        private readonly UIButton butClose;
        /// <summary>
        /// Accept button
        /// </summary>
        private readonly UIButton butAccept;
        /// <summary>
        /// Dialog text
        /// </summary>
        private readonly UITextArea dialogText;
        /// <summary>
        /// Button area height
        /// </summary>
        private readonly float buttonAreaHeight = 0;

        /// <summary>
        /// Gets whether the dialog is active or not
        /// </summary>
        public bool DialogActive { get; private set; } = false;
        /// <summary>
        /// Fires when the accept button was just released
        /// </summary>
        public EventHandler OnAcceptHandler;
        /// <summary>
        /// Fires when the cancel button was just released
        /// </summary>
        public EventHandler OnCancelHandler;
        /// <summary>
        /// Fires when the close button was just released
        /// </summary>
        public EventHandler OnCloseHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIDialog(Scene scene, UIDialogDescription description) : base(scene, description)
        {
            backPanel = new UIPanel(scene, description.Background);
            AddChild(backPanel);

            dialogText = new UITextArea(scene, description.TextArea);
            backPanel.AddChild(dialogText);

            if (description.DialogButtons.HasFlag(UIDialogButtons.Accept))
            {
                butAccept = new UIButton(scene, description.Buttons);
                butAccept.Caption.Text = "Accept";
                butAccept.JustReleased += DialogAcceptJustReleased;
                backPanel.AddChild(butAccept, false);

                buttonAreaHeight = butAccept.Height + 10;
            }

            if (description.DialogButtons.HasFlag(UIDialogButtons.Cancel) || description.DialogButtons.HasFlag(UIDialogButtons.Close))
            {
                butClose = new UIButton(scene, description.Buttons);
                if (description.DialogButtons.HasFlag(UIDialogButtons.Cancel))
                {
                    butClose.Caption.Text = "Cancel";
                    butClose.JustReleased += DialogCancelJustReleased;
                }
                else
                {
                    butClose.Caption.Text = "Close";
                    butClose.JustReleased += DialogCloseJustReleased;
                }
                backPanel.AddChild(butClose, false);

                buttonAreaHeight = butClose.Height + 10;
            }

            UpdateLayout();
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            UpdateLayout();
        }
        /// <summary>
        /// Update dialog layout
        /// </summary>
        private void UpdateLayout()
        {
            dialogText.Top = 0;
            dialogText.Left = 0;
            dialogText.Width = Width;
            dialogText.Height = Height - buttonAreaHeight;

            float buttonsSpace = 10;

            if (butAccept != null)
            {
                butAccept.Top = dialogText.Rectangle.Bottom + 5;
                butAccept.Left = buttonsSpace;
                buttonsSpace += butAccept.Rectangle.Right + 5;
            }

            if (butClose != null)
            {
                butClose.Top = dialogText.Rectangle.Bottom + 5;
                butClose.Left = buttonsSpace;
            }
        }

        private void DialogAcceptJustReleased(object sender, EventArgs e)
        {
            OnAcceptHandler?.Invoke(this, e);
        }
        private void DialogCancelJustReleased(object sender, EventArgs e)
        {
            OnCancelHandler?.Invoke(this, e);
        }
        private void DialogCloseJustReleased(object sender, EventArgs e)
        {
            OnCloseHandler?.Invoke(this, e);
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="action">Action</param>
        public void ShowDialog(string message, Action action = null)
        {
            dialogText.Text = message;

            action?.Invoke();

            DialogActive = true;
        }
        /// <summary>
        /// Closes the dialog
        /// </summary>
        /// <param name="action">Action</param>
        public void CloseDialog(Action action = null)
        {
            action?.Invoke();

            DialogActive = false;
        }
    }

    /// <summary>
    /// UI dialog extensions
    /// </summary>
    public static class UIDialogExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIDialog> AddComponentUIDialog(this Scene scene, UIDialogDescription description, int order = 0)
        {
            UIDialog component = null;

            await Task.Run(() =>
            {
                component = new UIDialog(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
