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
        /// Tab button area size
        /// </summary>
        private float tabButtonsAreaSize;
        /// <summary>
        /// Tab button area padding
        /// </summary>
        private Padding tabButtonsPadding;
        /// <summary>
        /// Tab button area spacing
        /// </summary>
        private Spacing tabButtonsSpacing;
        /// <summary>
        /// Padding size
        /// </summary>
        private Padding tabPanelsPadding;
        /// <summary>
        /// Tab button text padding
        /// </summary>
        private Padding tabButtonPadding;
        /// <summary>
        /// Tab panel internal padding
        /// </summary>
        private Padding tabPanelPadding;
        /// <summary>
        /// Tab panel internal spacing
        /// </summary>
        private Spacing tabPanelSpacing;
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
        public int TabCount
        {
            get
            {
                return tabButtons?.Count() ?? 0;
            }
        }
        /// <summary>
        /// Gets or sets the tab button area size
        /// </summary>
        public float TabButtonsAreaSize
        {
            get
            {
                return tabButtonsAreaSize;
            }
            set
            {
                if (tabButtonsAreaSize == value)
                {
                    return;
                }

                tabButtonsAreaSize = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the tab button area padding
        /// </summary>
        public Padding TabButtonsPadding
        {
            get
            {
                return tabButtonsPadding;
            }
            set
            {
                if (tabButtonsPadding == value)
                {
                    return;
                }

                tabButtonsPadding = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the tab button area spacing
        /// </summary>
        public Spacing TabButtonsSpacing
        {
            get
            {
                return tabButtonsSpacing;
            }
            set
            {
                if (tabButtonsSpacing == value)
                {
                    return;
                }

                tabButtonsSpacing = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Gets or sets the tab panel area padding
        /// </summary>
        public Padding TabPanelsPadding
        {
            get
            {
                return tabPanelsPadding;
            }
            set
            {
                if (tabPanelsPadding == value)
                {
                    return;
                }

                tabPanelsPadding = value;

                updateLayout = true;
            }
        }
        /// <summary>
        /// Tab button text padding
        /// </summary>
        public Padding TabButtonPadding
        {
            get
            {
                return tabButtonPadding;
            }
            set
            {
                tabButtonPadding = value;

                foreach (var but in tabButtons)
                {
                    but.Caption.Padding = tabButtonPadding;
                }
            }
        }
        /// <summary>
        /// Tab panel internal padding
        /// </summary>
        public Padding TabPanelPadding
        {
            get
            {
                return tabPanelPadding;
            }
            set
            {
                tabPanelPadding = value;

                foreach (var pan in tabPanels)
                {
                    pan.Padding = tabPanelPadding;
                }
            }
        }
        /// <summary>
        /// Tab panel internal spacing
        /// </summary>
        public Spacing TabPanelSpacing
        {
            get
            {
                return tabPanelSpacing;
            }
            set
            {
                tabPanelSpacing = value;

                foreach (var pan in tabPanels)
                {
                    pan.Spacing = tabPanelSpacing;
                }
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

        /// <inheritdoc/>
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;

                var selected = tabPanels.ElementAtOrDefault(selectedTabIndex);
                if (selected != null)
                {
                    tabPanels.ForEach(p => p.Visible = false);
                    selected.Visible = value;
                }
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
            tabButtonsAreaSize = description.TabButtonsAreaSize;
            tabButtonsPadding = description.TabButtonsPadding;
            tabButtonsSpacing = description.TabButtonsSpacing;
            tabPanelsPadding = description.TabPanelsPadding;
            
            tabButtonPadding = description.TabButtonPadding;
            tabPanelPadding = description.TabPanelPadding;
            tabPanelSpacing = description.TabPanelSpacing;

            if (description.Background != null)
            {
                Background = new Sprite(scene, description.Background)
                {
                    Name = $"{description.Name}.Background",
                };

                AddChild(Background);
            }

            if (description.Tabs > 0)
            {
                var buttonDesc = description.ButtonDescription ?? UIButtonDescription.Default(description.BaseColor);
                var panelDesc = description.PanelDescription ?? UIPanelDescription.Default(description.BaseColor);

                for (int i = 0; i < description.Tabs; i++)
                {
                    buttonDesc.Name = $"{description.Name}.Button_{i}";
                    panelDesc.Name = $"{description.Name}.Panel_{i}";

                    var button = new UIButton(scene, buttonDesc);
                    button.Caption.Text = description.TabCaptions?.ElementAtOrDefault(i) ?? $"Button_{i}";
                    button.Caption.Padding = tabButtonPadding;
                    button.Pressed += Button_Pressed;
                    button.JustPressed += Button_JustPressed;
                    button.JustReleased += Button_JustReleased;

                    tabButtons.Add(button);

                    var panel = new UIPanel(scene, panelDesc);
                    panel.Padding = tabPanelPadding;
                    panel.Spacing = tabPanelSpacing;

                    tabPanels.Add(panel);

                    AddChild(button, false);
                    AddChild(panel, false);
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
            if (TabCount <= 0)
            {
                return;
            }

            int tabs = TabCount;
            var bounds = AbsoluteRectangle;
            float buttonWidth = (bounds.Width - (tabButtonsSpacing.Horizontal * (tabs - 1)) - (tabButtonsPadding.Left + tabButtonsPadding.Right)) / tabs;
            float buttonHeight = tabButtonsAreaSize - (tabButtonsPadding.Top + tabButtonsPadding.Bottom);

            float panelWidth = bounds.Width - tabPanelsPadding.Left - tabPanelsPadding.Right;
            float panelHeight = bounds.Height - tabButtonsAreaSize - (tabPanelsPadding.Top + tabPanelsPadding.Bottom);

            for (int i = 0; i < tabs; i++)
            {
                tabButtons[i].SetPosition((i * buttonWidth) + (i * tabButtonsSpacing.Horizontal) + tabButtonsPadding.Left, tabButtonsPadding.Top);
                tabButtons[i].Width = buttonWidth;
                tabButtons[i].Height = buttonHeight;

                tabPanels[i].SetPosition(tabPanelsPadding.Left, tabButtonsAreaSize + tabPanelsPadding.Top);
                tabPanels[i].Width = panelWidth;
                tabPanels[i].Height = panelHeight;
            }

            tabPanels.ForEach(p => p.Visible = false);
            tabPanels[selectedTabIndex].Visible = true;

            tabButtons.ForEach(b => b.State = UIButtonState.Released);
            tabButtons[selectedTabIndex].State = UIButtonState.Pressed;
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

            RemoveChild(oldButton, true);
            AddChild(button, false);

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

            RemoveChild(oldPanel, true);
            AddChild(panel, false);

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
            TabPressed?.Invoke(
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
            TabJustPressed?.Invoke(
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
            TabJustReleased?.Invoke(
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
