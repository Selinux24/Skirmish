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
        public event EventHandler OnAcceptHandler;
        /// <summary>
        /// Fires when the cancel button was just released
        /// </summary>
        public event EventHandler OnCancelHandler;
        /// <summary>
        /// Fires when the close button was just released
        /// </summary>
        public event EventHandler OnCloseHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIDialog(string id, string name, Scene scene, UIDialogDescription description) :
            base(id, name, scene, description)
        {
            var backPanel = new UIPanel(
                $"{id}.BackPanel",
                $"{name}.BackPanel",
                scene,
                description.Background);
            AddChild(backPanel);

            dialogText = new UITextArea(
                $"{id}.DialogText",
                $"{name}.DialogText",
                scene,
                description.TextArea);
            backPanel.AddChild(dialogText);

            if (description.DialogButtons.HasFlag(UIDialogButtons.Accept))
            {
                butAccept = new UIButton(
                    $"{id}.AcceptButton",
                    $"{name}.AcceptButton",
                    scene,
                    description.Buttons);
                butAccept.Caption.Text = "Accept";
                butAccept.MouseClick += DialogAcceptClick;
                backPanel.AddChild(butAccept, false);

                buttonAreaHeight = butAccept.Height + 10;
            }

            if (description.DialogButtons.HasFlag(UIDialogButtons.Cancel) || description.DialogButtons.HasFlag(UIDialogButtons.Close))
            {
                if (description.DialogButtons.HasFlag(UIDialogButtons.Cancel))
                {
                    butClose = new UIButton(
                        $"{id}.CancelButton",
                        $"{name}.CancelButton",
                        scene,
                        description.Buttons);
                    butClose.Caption.Text = "Cancel";
                    butClose.MouseClick += DialogCancelClick;
                    backPanel.AddChild(butClose, false);
                }
                else
                {
                    butClose = new UIButton(
                        $"{id}.CloseButton",
                        $"{name}.CloseButton",
                        scene,
                        description.Buttons);
                    butClose.Caption.Text = "Close";
                    butClose.MouseClick += DialogCloseClick;
                    backPanel.AddChild(butClose, false);
                }

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
                butAccept.Top = dialogText.AbsoluteRectangle.Bottom + 5;
                butAccept.Left = buttonsSpace;
                buttonsSpace += butAccept.AbsoluteRectangle.Right + 5;
            }

            if (butClose != null)
            {
                butClose.Top = dialogText.AbsoluteRectangle.Bottom + 5;
                butClose.Left = buttonsSpace;
            }
        }

        private void DialogAcceptClick(IUIControl sender, MouseEventArgs e)
        {
            OnAcceptHandler?.Invoke(this, e);
        }
        private void DialogCancelClick(IUIControl sender, MouseEventArgs e)
        {
            OnCancelHandler?.Invoke(this, e);
        }
        private void DialogCloseClick(IUIControl sender, MouseEventArgs e)
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
        /// <param name="name">Name</param>
        /// <param name="id">Id</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIDialog> AddComponentUIDialog(this Scene scene, string id, string name, UIDialogDescription description, int layer = Scene.LayerUI)
        {
            UIDialog component = null;

            await Task.Run(() =>
            {
                component = new UIDialog(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
