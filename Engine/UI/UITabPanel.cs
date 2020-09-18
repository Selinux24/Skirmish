using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Tab Panel
    /// </summary>
    public class UITabPanel : UIControl
    {
        /// <summary>
        /// Button list
        /// </summary>
        private readonly List<UIButton> tabButtons = new List<UIButton>();
        /// <summary>
        /// Panel list
        /// </summary>
        private readonly List<UIPanel> tabPanels = new List<UIPanel>();
        /// <summary>
        /// Update layout flag
        /// </summary>
        private bool updateLayout = true;
        /// <summary>
        /// Selected tab index
        /// </summary>
        private int selectedTabIndex = 0;
        /// <summary>
        /// Margin size
        /// </summary>
        private float margin;
        /// <summary>
        /// Spacing size
        /// </summary>
        private float spacing;
        /// <summary>
        /// Button area size
        /// </summary>
        private float buttonAreaSize;
        /// <summary>
        /// Base control color
        /// </summary>
        private Color4 baseColor;
        /// <summary>
        /// Selected button color
        /// </summary>
        private Color4 selectedColor;

        /// <summary>
        /// Background
        /// </summary>
        public Sprite Background { get; private set; }
        /// <summary>
        /// Gets the number of tabs
        /// </summary>
        public int Tabs
        {
            get
            {
                return tabButtons?.Count() ?? 0;
            }
        }
        /// <summary>
        /// Gets or sets the margin value
        /// </summary>
        public float Margin
        {
            get
            {
                return margin;
            }
            set
            {
                if (margin == value)
                {
                    return;
                }

                margin = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the spacing value
        /// </summary>
        public float Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (spacing == value)
                {
                    return;
                }

                spacing = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the button area size
        /// </summary>
        public float ButtonAreaSize
        {
            get
            {
                return buttonAreaSize;
            }
            set
            {
                if (buttonAreaSize == value)
                {
                    return;
                }

                buttonAreaSize = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the base color
        /// </summary>
        public Color4 BaseColor
        {
            get
            {
                return baseColor;
            }
            set
            {
                if (baseColor == value)
                {
                    return;
                }

                baseColor = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the button selected color
        /// </summary>
        public Color4 SelectedColor
        {
            get
            {
                return selectedColor;
            }
            set
            {
                if (selectedColor == value)
                {
                    return;
                }

                selectedColor = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets the tab button list
        /// </summary>
        public UIButton[] TabButtons
        {
            get
            {
                return tabButtons.ToArray();
            }
        }
        /// <summary>
        /// Gets the tab panel list
        /// </summary>
        public UIPanel[] TabPanels
        {
            get
            {
                return tabPanels.ToArray();
            }
        }

        /// <summary>
        /// Mouse pressed
        /// </summary>
        public event UITabPanelEventHandler TabPressed;
        /// <summary>
        /// Mouse just pressed
        /// </summary>
        public event UITabPanelEventHandler TabJustPressed;
        /// <summary>
        /// Mouse just released
        /// </summary>
        public event UITabPanelEventHandler TabJustReleased;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UITabPanel(Scene scene, UITabPanelDescription description) : base(scene, description)
        {
            margin = description.Margin;
            spacing = description.Spacing;
            baseColor = description.BaseColor;
            selectedColor = description.SelectedColor;
            buttonAreaSize = description.ButtonAreaSize;

            if (description.Background != null)
            {
                this.Background = new Sprite(scene, description.Background)
                {
                    Name = $"{description.Name}.Background",
                };

                this.AddChild(this.Background);
            }

            if (description.Tabs > 0)
            {
                var buttonDesc = description.ButtonDescription ?? UIButtonDescription.Default(description.TintColor);
                var panelDesc = description.PanelDescription ?? UIPanelDescription.Default(description.TintColor);

                for (int i = 0; i < description.Tabs; i++)
                {
                    buttonDesc.Name = $"{description.Name}.Button_{i}";
                    panelDesc.Name = $"{description.Name}.Panel_{i}";

                    var button = new UIButton(scene, buttonDesc);
                    var panel = new UIPanel(scene, panelDesc);

                    button.Caption.Text = description.Captions?.ElementAtOrDefault(i) ?? $"Button_{i}";

                    button.Pressed += Button_Pressed;
                    button.JustPressed += Button_JustPressed;
                    button.JustReleased += Button_JustReleased;

                    tabButtons.Add(button);
                    tabPanels.Add(panel);

                    this.AddChild(button, false);
                    this.AddChild(panel, false);
                }

                SetSelectedTab(0);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (updateLayout)
            {
                // Update layout before children processing
                UpdateLayout();

                updateLayout = false;
            }

            base.Update(context);
        }
        /// <summary>
        /// Updates the tab panel layout
        /// </summary>
        private void UpdateLayout()
        {
            if (Tabs <= 0)
            {
                return;
            }

            int tabs = Tabs;
            var bounds = AbsoluteRectangle;
            float buttonWidth = (bounds.Width - (spacing * (tabs - 1)) - (margin * 2)) / tabs;
            float buttonHeight = ButtonAreaSize - margin - spacing;

            float panelWidth = bounds.Width - margin - margin;
            float panelHeight = bounds.Height - ButtonAreaSize - margin;

            for (int i = 0; i < tabs; i++)
            {
                tabButtons[i].SetPosition((i * buttonWidth) + (i * spacing) + margin, margin);
                tabButtons[i].Width = buttonWidth;
                tabButtons[i].Height = buttonHeight;

                tabPanels[i].SetPosition(margin, ButtonAreaSize);
                tabPanels[i].Width = panelWidth;
                tabPanels[i].Height = panelHeight;
            }

            tabPanels.ForEach(p => p.Visible = false);
            tabPanels[selectedTabIndex].Visible = true;

            tabButtons.ForEach(b => b.TintColor = baseColor);
            tabButtons[selectedTabIndex].TintColor = SelectedColor;
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            updateLayout = true;
        }

        /// <inheritdoc/>
        protected override void UpdateInternalState()
        {
            base.UpdateInternalState();

            updateLayout = true;
        }

        /// <summary>
        /// Sets the selected tab
        /// </summary>
        /// <param name="index">Tab index</param>
        public void SetSelectedTab(int index)
        {
            selectedTabIndex = index;

            updateLayout = true;
        }
        /// <summary>
        /// Sets and replaces the tab button
        /// </summary>
        /// <param name="index">Tab index</param>
        /// <param name="buttonDescription">Button description</param>
        public void SetTabButton(int index, UIButtonDescription buttonDescription)
        {
            UIButton button = new UIButton(Scene, buttonDescription);

            var oldButton = tabButtons[index];
            tabButtons[index] = button;

            this.RemoveChild(oldButton, true);
            this.AddChild(button, false);

            updateLayout = true;
        }
        /// <summary>
        /// Sets and replaces the tab panel
        /// </summary>
        /// <param name="index">Tab index</param>
        /// <param name="panelDescription">Panel description</param>
        public void SetTabPanel(int index, UIPanelDescription panelDescription)
        {
            UIPanel panel = new UIPanel(Scene, panelDescription);

            var oldPanel = tabPanels[index];
            tabPanels[index] = panel;

            this.RemoveChild(oldPanel, true);
            this.AddChild(panel, false);

            updateLayout = true;
        }

        /// <summary>
        /// Button pressed event
        /// </summary>
        private void Button_Pressed(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                int index = tabButtons.IndexOf(button);
                if (index >= 0)
                {
                    FireTabPressedEvent(index);
                }
            }
        }
        /// <summary>
        /// Button just pressed event
        /// </summary>
        private void Button_JustPressed(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                int index = tabButtons.IndexOf(button);
                if (index >= 0)
                {
                    FireTabJustPressedEvent(index);
                }
            }
        }
        /// <summary>
        /// Button just released event
        /// </summary>
        private void Button_JustReleased(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                int index = tabButtons.IndexOf(button);
                if (index >= 0)
                {
                    SetSelectedTab(index);

                    FireTabJustReleasedEvent(index);
                }
            }
        }

        /// <summary>
        /// Fires on pressed event
        /// </summary>
        protected void FireTabPressedEvent(int index)
        {
            this.TabPressed?.Invoke(
                this,
                new UITabPanelEventArgs()
                {
                    TabIndex = index,
                    TabButton = tabButtons.ElementAtOrDefault(index),
                    TabPanel = tabPanels.ElementAtOrDefault(index),
                });
        }
        /// <summary>
        /// Fires on just pressed event
        /// </summary>
        protected void FireTabJustPressedEvent(int index)
        {
            this.TabJustPressed?.Invoke(
                this,
                new UITabPanelEventArgs()
                {
                    TabIndex = index,
                    TabButton = tabButtons.ElementAtOrDefault(index),
                    TabPanel = tabPanels.ElementAtOrDefault(index),
                });
        }
        /// <summary>
        /// Fires on just released event
        /// </summary>
        protected void FireTabJustReleasedEvent(int index)
        {
            this.TabJustReleased?.Invoke(
                this,
                new UITabPanelEventArgs()
                {
                    TabIndex = index,
                    TabButton = tabButtons.ElementAtOrDefault(index),
                    TabPanel = tabPanels.ElementAtOrDefault(index),
                });
        }
    }

    /// <summary>
    /// UI tab panel event handler delegate
    /// </summary>
    public delegate void UITabPanelEventHandler(object sender, UITabPanelEventArgs e);

    /// <summary>
    /// UI tab panel event arguments
    /// </summary>
    public class UITabPanelEventArgs : EventArgs
    {
        /// <summary>
        /// Tab index
        /// </summary>
        public int TabIndex { get; set; }
        /// <summary>
        /// Tab button
        /// </summary>
        public UIButton TabButton { get; set; }
        /// <summary>
        /// Tab panel
        /// </summary>
        public UIPanel TabPanel { get; set; }
    }

    /// <summary>
    /// UI Tab Panel extensions
    /// </summary>
    public static class UITabPanelExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UITabPanel> AddComponentUITabPanel(this Scene scene, UITabPanelDescription description, int order = 0)
        {
            UITabPanel component = null;

            await Task.Run(() =>
            {
                component = new UITabPanel(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
