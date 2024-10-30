using Engine;
using Engine.BuiltIn.UI;
using Engine.Helpers;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace AISamples.Common.Dialogs
{
    class UIOpenFileDialog(Scene scene, string name, float width, float height, int itemsPerPage, int layer = Scene.LayerUI)
    {
        private const string dlgName = nameof(UIOpenFileDialog);
        private readonly Scene scene = scene;
        private UIDialog dialog = null;
        private UIButton[] buttons;
        private UITextArea selectedText = null;
        private UITextArea folderText = null;
        private UIButton pageUpButton = null;
        private UIButton pageDownButton = null;
        private string searchPattern = null;
        private readonly FolderNavigator folderNavigator = new()
        {
            PageIndex = 0,
            ItemsPerPage = itemsPerPage
        };

        public string Name { get; private set; } = name;
        public float Width { get; private set; } = width;
        public float Height { get; private set; } = height;
        public int Layer { get; private set; } = layer;
        public int ItemsPerPage { get; private set; } = itemsPerPage;
        public Color4 ButtonColor { get; set; }
        public Color4 ButtonTextColor { get; set; }
        public Color4 BackgroundColor { get; set; }
        public string SelectedFileName
        {
            get
            {
                return selectedText.TooltipText;
            }
        }

        public event EventHandler OnAcceptHandler;
        public event EventHandler OnCancelHandler;

        public async Task Initialize(string resourcesFolder, string editorFont)
        {
            var textFont = FontDescription.FromFamily(editorFont, 16);
            textFont.ContentPath = resourcesFolder;

            var fileTextDesc = UITextAreaDescription.Default(textFont);
            fileTextDesc.TextForeColor = ButtonTextColor;
            fileTextDesc.StartsVisible = false;
            string selectedTextName = FormatControlName(nameof(selectedText));
            selectedText = await scene.AddComponentUI<UITextArea, UITextAreaDescription>(selectedTextName, selectedTextName, fileTextDesc, Layer + 1);
            string folderTextName = FormatControlName(nameof(folderText));
            folderText = await scene.AddComponentUI<UITextArea, UITextAreaDescription>(folderTextName, folderTextName, fileTextDesc, Layer + 1);

            var dlgButtonsFont = FontDescription.FromFamily(editorFont, 18);
            dlgButtonsFont.ContentPath = resourcesFolder;

            var fileDlgButtonDesc = UIButtonDescription.DefaultTwoStateButton(dlgButtonsFont);
            fileDlgButtonDesc.ContentPath = resourcesFolder;
            fileDlgButtonDesc.Width = 150;
            fileDlgButtonDesc.Height = 20;
            fileDlgButtonDesc.ColorReleased = ButtonColor;
            fileDlgButtonDesc.ColorPressed = new Color4(ButtonColor.RGB() * 1.2f, 1f);
            fileDlgButtonDesc.TextForeColor = ButtonTextColor;
            fileDlgButtonDesc.StartsVisible = false;

            var fileDialogDesc = UIDialogDescription.Default(Width, Height);
            fileDialogDesc.Padding = 10;
            fileDialogDesc.TextArea = fileTextDesc;
            fileDialogDesc.Buttons = fileDlgButtonDesc;
            fileDialogDesc.Background = UIPanelDescription.Default(BackgroundColor);
            fileDialogDesc.StartsVisible = false;

            string dialogName = FormatControlName(nameof(dialog));
            dialog = await scene.AddComponentUI<UIDialog, UIDialogDescription>(dialogName, dialogName, fileDialogDesc, Layer);
            dialog.OnAcceptHandler += (sender, args) =>
            {
                OnAcceptHandler?.Invoke(sender, args);
                HideDialog();
            };
            dialog.OnCancelHandler += (sender, args) =>
            {
                OnCancelHandler?.Invoke(sender, args);
                HideDialog();
            };

            var buttonsFont = FontDescription.FromFamily(editorFont, 14);
            buttonsFont.ContentPath = resourcesFolder;

            var fileButtonDesc = UIButtonDescription.Default(buttonsFont);
            fileButtonDesc.ContentPath = resourcesFolder;
            fileButtonDesc.Width = Width * 0.8f;
            fileButtonDesc.Height = 20;
            fileButtonDesc.ColorReleased = ButtonColor;
            fileButtonDesc.TextForeColor = ButtonTextColor;
            fileButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Left;
            fileButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            fileButtonDesc.StartsVisible = false;

            buttons = new UIButton[ItemsPerPage];
            for (int i = 0; i < ItemsPerPage; i++)
            {
                buttons[i] = await InitializeFileButton($"file_{i}", string.Empty, fileButtonDesc, Layer + 1);
            }

            var filePageButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont);
            filePageButtonDesc.ContentPath = resourcesFolder;
            filePageButtonDesc.ColorReleased = ButtonColor;
            filePageButtonDesc.ColorPressed = new Color4(ButtonColor.RGB() * 1.2f, 1f);
            filePageButtonDesc.TextForeColor = ButtonTextColor;
            filePageButtonDesc.StartsVisible = false;

            string pageUpButtonName = FormatControlName(nameof(pageUpButton));
            pageUpButton = await scene.AddComponentUI<UIButton, UIButtonDescription>(pageUpButtonName, pageUpButtonName, filePageButtonDesc, Layer + 1);
            pageUpButton.Caption.Text = "U";
            pageUpButton.MouseClick += (sender, e) =>
            {
                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                if (folderNavigator.PageUp())
                {
                    LoadFolder(folderText.TooltipText, searchPattern);
                }
            };

            string pageDownButtonName = FormatControlName(nameof(pageDownButton));
            pageDownButton = await scene.AddComponentUI<UIButton, UIButtonDescription>(pageDownButtonName, pageDownButtonName, filePageButtonDesc, Layer + 1);
            pageDownButton.Caption.Text = "D";
            pageDownButton.MouseClick += (sender, e) =>
            {
                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                if (folderNavigator.PageDown())
                {
                    LoadFolder(folderText.TooltipText, searchPattern);
                }
            };
        }
        private async Task<UIButton> InitializeFileButton(string name, string caption, UIButtonDescription desc, int layer)
        {
            string buttonName = FormatControlName(name);
            var button = await scene.AddComponentUI<UIButton, UIButtonDescription>(buttonName, buttonName, desc, layer);

            button.Caption.Text = caption;
            button.MouseClick += (sender, e) =>
            {
                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                if (sender is not UIButton button)
                {
                    return;
                }

                string fileName = button.Caption.Text;
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                string path = button.TooltipText;

                if (FolderNavigatorPath.FileNameIsPrevFolder(fileName) || FolderNavigatorPath.FileNameIsFolder(fileName))
                {
                    folderNavigator.PageIndex = 0;

                    LoadFolder(path, searchPattern);
                }
                else
                {
                    selectedText.Text = fileName;
                    selectedText.TooltipText = path;
                }
            };

            return button;
        }
        private string FormatControlName(string controlName)
        {
            return $"{dlgName}.{Name}.{controlName}";
        }

        private void LoadFolder(string folder, string searchPattern)
        {
            if (!folderNavigator.LoadFolder(folder, searchPattern, out var paths))
            {
                return;
            }

            this.searchPattern = searchPattern;

            folderText.Text = FormatFolderName(folderNavigator.SelectedFolder.Path, 40);
            folderText.TooltipText = folderNavigator.SelectedFolder.Path;

            selectedText.Text = null;
            selectedText.TooltipText = null;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i >= paths.Length)
                {
                    buttons[i].TooltipText = string.Empty;
                    buttons[i].Caption.Text = string.Empty;

                    continue;
                }

                var data = paths[i];

                buttons[i].TooltipText = data.Path;
                buttons[i].Caption.Text = data.GetFileName();
            }
        }
        private static string FormatFolderName(string folderName, int length)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return null;
            }

            if (folderName.Length <= length)
            {
                return folderName;
            }

            return $"...{folderName.Substring(folderName.Length - length, length)}";
        }

        public void ShowDialog(string caption, string folder, string searchPattern)
        {
            folderNavigator.PageIndex = 0;

            LoadFolder(folder, searchPattern);

            dialog.Visible = true;
            dialog.ShowDialog(caption);

            var first = buttons[0];
            var last = buttons[^1];
            var renderArea = dialog.GetRenderArea(true);
            var buttonWidth = renderArea.Width - first.Height;
            var buttonHeight = first.Height;
            float x = renderArea.Left;
            float y = renderArea.Top + buttonHeight + 5;

            folderText.SetPosition(x, y);
            folderText.Width = buttonWidth;
            folderText.Visible = true;
            y += folderText.Height + 1;

            foreach (var button in buttons)
            {
                button.SetPosition(x, y);
                button.Width = buttonWidth;
                button.Visible = true;
                y += button.Height + 1;
            }

            pageUpButton.SetPosition(first.Left + first.Width + 1, first.Top);
            pageUpButton.Width = first.Height;
            pageUpButton.Height = first.Height;
            pageUpButton.Visible = true;

            pageDownButton.SetPosition(last.Left + last.Width + 1, last.Top);
            pageDownButton.Width = last.Height;
            pageDownButton.Height = last.Height;
            pageDownButton.Visible = true;

            selectedText.SetPosition(x, y);
            selectedText.Visible = true;
        }
        public void HideDialog()
        {
            folderText.Visible = false;
            foreach (var button in buttons)
            {
                button.Caption.Text = string.Empty;
                button.TooltipText = string.Empty;
                button.Visible = false;
            }
            selectedText.Visible = false;
            pageUpButton.Visible = false;
            pageDownButton.Visible = false;

            dialog.Visible = false;
            dialog.CloseDialog();
        }
    }
}
