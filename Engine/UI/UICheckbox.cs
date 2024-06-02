using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// Checkbox control
    /// </summary>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public class UICheckbox(Scene scene, string id, string name) : UIControl<UICheckboxDescription>(scene, id, name)
    {
        private Sprite stateOn = null;
        private Sprite stateOff = null;
        private bool isChecked = false;

        /// <summary>
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; private set; } = null;
        /// <summary>
        /// Checked
        /// </summary>
        public bool Checked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (isChecked == value) return;

                isChecked = value;
                stateOn.Visible = isChecked;
            }
        }
        /// <summary>
        /// Value changed event
        /// </summary>
        public Action<bool> OnValueChanged { get; set; }

        /// <inheritdoc/>
        public override async Task ReadAssets(UICheckboxDescription description)
        {
            await base.ReadAssets(description);

            Caption = await CreateCaption();
            stateOff = await CreateSpriteOff();
            stateOn = await CreateSpriteOn();

            AddChild(Caption, false);
            AddChild(stateOff, false);
            AddChild(stateOn, false);

            float w = (stateOff.Width - stateOn.Width) * 0.5f;
            float h = (stateOff.Height - stateOn.Height) * 0.5f;

            stateOn.SetPosition(w, h);
            stateOff.SetPosition(0, 0);
            Caption.SetPosition(stateOff.Width + 5, 0);
        }
        private async Task<UITextArea> CreateCaption()
        {
            string captionName = $"{Id}.Caption";

            return await Scene.CreateComponent<UITextArea, UITextAreaDescription>(
                captionName,
                captionName,
                new()
                {
                    ContentPath = Description.ContentPath,
                    Font = Description.Font,
                    Text = Description.Text,
                    TextForeColor = Description.TextForeColor,
                    TextShadowColor = Description.TextShadowColor,
                    TextShadowDelta = Description.TextShadowDelta,
                    TextHorizontalAlign = Description.TextHorizontalAlign,
                    TextVerticalAlign = Description.TextVerticalAlign,

                    EventsEnabled = false,
                });
        }
        private async Task<Sprite> CreateSpriteOn()
        {
            var desc = new SpriteDescription()
            {
                Width = 12,
                Height = 12,
                ContentPath = Description.ContentPath,
                BaseColor = Description.StateOnColor,
                EventsEnabled = true,
            };

            if (!string.IsNullOrEmpty(Description.StateOnTexture))
            {
                desc.Textures = [Description.StateOnTexture];
                desc.UVMap = Description.StateOnTextureUVMap;
            }

            string stateName = $"{Id}.StateOn";

            var state = await Scene.CreateComponent<Sprite, SpriteDescription>(stateName, stateName, desc);
            state.MouseJustReleased += (sender, e) =>
            {
                if (e.Buttons != MouseButtons.Left) return;
                Checked = false;
                OnValueChanged?.Invoke(false);
            };

            return state;
        }
        private async Task<Sprite> CreateSpriteOff()
        {
            var desc = new SpriteDescription()
            {
                Width = 20,
                Height = 20,
                ContentPath = Description.ContentPath,
                BaseColor = Description.StateOffColor,
                EventsEnabled = true,
            };

            if (!string.IsNullOrEmpty(Description.StateOffTexture))
            {
                desc.Textures = [Description.StateOffTexture];
                desc.UVMap = Description.StateOffTextureUVMap;
            }

            string stateName = $"{Id}.StateOff";

            var state = await Scene.CreateComponent<Sprite, SpriteDescription>(stateName, stateName, desc);
            state.MouseJustReleased += (sender, e) =>
            {
                if (e.Buttons != MouseButtons.Left) return;
                Checked = true;
                OnValueChanged?.Invoke(true);
            };

            return state;
        }
    }
}
