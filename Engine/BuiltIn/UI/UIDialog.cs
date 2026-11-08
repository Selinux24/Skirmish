using Engine.UI;
using System;
using System.Threading.Tasks;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// UI dialog
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UIDialog(Scene scene, string id, string name) : UIControl<UIDialogDescription>(scene, id, name)
    {
        /// <summary>
        /// Close button
        /// </summary>
        private UIButton butClose;
        /// <summary>
        /// Accept button
        /// </summary>
        private UIButton butAccept;
        /// <summary>
        /// Back panel
        /// </summary>
        private UIPanel backPanel;
        /// <summary>
        /// Dialog text
        /// </summary>
        private UITextArea dialogText;
        /// <summary>
        /// Button area height
        /// </summary>
        private float buttonAreaHeight = 0;

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

        /// <inheritdoc/>
        public override async Task ReadAssets(UIDialogDescription description)
        {
            await base.ReadAssets(description);

            backPanel = await CreateBackpanel();
            AddChild(backPanel);

            dialogText = await CreateDialogText();
            backPanel.AddChild(dialogText, true);

            if (Description.DialogButtons.HasFlag(UIDialogButtons.Accept))
            {
                butAccept = await CreateAcceptButton();
                backPanel.AddChild(butAccept);

                buttonAreaHeight = butAccept.Height + 10;
            }

            if (Description.DialogButtons.HasFlag(UIDialogButtons.Cancel) || Description.DialogButtons.HasFlag(UIDialogButtons.Close))
            {
                if (Description.DialogButtons.HasFlag(UIDialogButtons.Cancel))
                {
                    butClose = await CreateCancelButton();
                    backPanel.AddChild(butClose);
                }
                else
                {
                    butClose = await CreateCloseButton();
                    backPanel.AddChild(butClose);
                }

                buttonAreaHeight = butClose.Height + 10;
            }

            UpdateLayout();
        }
        private async Task<UIPanel> CreateBackpanel()
        {
            return await Scene.CreateComponent<UIPanel, UIPanelDescription>(
                $"{Id}.BackPanel",
                $"{Name}.BackPanel",
                Description.Background);
        }
        private async Task<UITextArea> CreateDialogText()
        {
            return await Scene.CreateComponent<UITextArea, UITextAreaDescription>(
                $"{Id}.DialogText",
                $"{Name}.DialogText",
                Description.TextArea);
        }
        private async Task<UIButton> CreateAcceptButton()
        {
            var button = await Scene.CreateComponent<UIButton, UIButtonDescription>(
                $"{Id}.AcceptButton",
                $"{Name}.AcceptButton",
                Description.Buttons);

            button.Caption.Text = "Accept";
            button.MouseClick += (sender, e) => { OnAcceptHandler?.Invoke(sender, e); };

            return button;
        }
        private async Task<UIButton> CreateCancelButton()
        {
            var button = await Scene.CreateComponent<UIButton, UIButtonDescription>(
                $"{Id}.CancelButton",
                $"{Name}.CancelButton",
                Description.Buttons);

            button.Caption.Text = "Cancel";
            button.MouseClick += (sender, e) => { OnCancelHandler?.Invoke(sender, e); };

            return button;
        }
        private async Task<UIButton> CreateCloseButton()
        {
            var button = await Scene.CreateComponent<UIButton, UIButtonDescription>(
                $"{Id}.CloseButton",
                $"{Name}.CloseButton",
                Description.Buttons);

            button.Caption.Text = "Close";
            button.MouseClick += (sender, e) => { OnCloseHandler?.Invoke(sender, e); };

            return button;
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
            backPanel.Top = 0;
            backPanel.Left = 0;
            backPanel.Width = Width;
            backPanel.Height = Height;

            dialogText.Top = Padding.Top;
            dialogText.Left = Padding.Left;
            dialogText.Width = Width - Padding.Horizontal;
            dialogText.Height = Height - buttonAreaHeight;

            var renderArea = GetRenderArea(false);
            float buttonsSpace = 0;
            int buttonPadding = Math.Max(5, (int)Padding.Left);

            if (butClose != null)
            {
                butClose.Top = renderArea.Height - butClose.Height - Padding.Top;
                butClose.Left = renderArea.Width - butClose.Width - Padding.Left;
                buttonsSpace += butClose.Width;
            }

            if (butAccept != null)
            {
                butAccept.Top = renderArea.Height - butAccept.Height - Padding.Top;
                butAccept.Left = renderArea.Width - butAccept.Width - Padding.Left - (buttonsSpace + buttonPadding);
            }
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
}
