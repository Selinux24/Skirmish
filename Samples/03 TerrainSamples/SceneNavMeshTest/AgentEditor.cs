using Engine;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Agent editor
    /// </summary>
    /// <param name="scene">Scene</param>
    public class AgentEditor(Scene scene)
    {
        private const string ObjectName = nameof(AgentEditor);

        private readonly Scene scene = scene;
        private bool initialized = false;
        private bool isDirty = false;
        private bool visible = false;
        private Player agent;

        private UIPanel mainPanel;

        private UITextArea title;

        private UITextArea heightCaption;
        private UITextArea heightValue;
        private UISlider heightSlider;

        private UITextArea radiusCaption;
        private UITextArea radiusValue;
        private UISlider radiusSlider;

        private UITextArea maxClimbCaption;
        private UITextArea maxClimbValue;
        private UISlider maxClimbSlider;

        private UITextArea maxSlopeCaption;
        private UITextArea maxSlopeValue;
        private UISlider maxSlopeSlider;

        private UITextArea velocityCaption;
        private UITextArea velocityValue;
        private UISlider velocitySlider;

        private UITextArea velocitySlowCaption;
        private UITextArea velocitySlowValue;
        private UISlider velocitySlowSlider;

        /// <summary>
        /// Horizontal marging
        /// </summary>
        public float HorizontalMarging { get; set; } = 25;
        /// <summary>
        /// Vertical marging
        /// </summary>
        public float VerticalMarging { get; set; } = 25;
        /// <summary>
        /// Vertical padding
        /// </summary>
        public float VerticalPadding { get; set; } = 10;
        /// <summary>
        /// Editor top-left position
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.Zero;
        /// <summary>
        /// Editor width
        /// </summary>
        public float Width { get; set; } = 0;
        /// <summary>
        /// Editor visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;

            }
            set
            {
                if (visible == value)
                {
                    return;
                }

                visible = value;

                UpdateLayout();
            }
        }

        /// <summary>
        /// Agent height
        /// </summary>
        public float AgentHeight
        {
            get { return heightSlider.GetValue(0); }
            set
            {
                heightSlider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent radius
        /// </summary>
        public float AgentRadius
        {
            get { return radiusSlider.GetValue(0); }
            set
            {
                radiusSlider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent maximum climb height
        /// </summary>
        public float AgentMaxClimb
        {
            get { return maxClimbSlider.GetValue(0); }
            set
            {
                maxClimbSlider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent maximum slope angle
        /// </summary>
        public float AgentMaxSlopes
        {
            get { return maxSlopeSlider.GetValue(0); }
            set
            {
                maxSlopeSlider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent velocity
        /// </summary>
        public float AgentVelocity
        {
            get { return velocitySlider.GetValue(0); }
            set
            {
                velocitySlider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent velocity slow
        /// </summary>
        public float AgentVelocitySlow
        {
            get { return velocitySlowSlider.GetValue(0); }
            set
            {
                velocitySlowSlider.SetValue(0, value);
                isDirty = true;
            }
        }

        /// <summary>
        /// Initializes the editor
        /// </summary>
        /// <param name="fontTitle">Title font</param>
        /// <param name="font">Font</param>
        public async Task Initialize(TextDrawerDescription fontTitle, TextDrawerDescription font)
        {
            mainPanel = await InitializePanel("MainPanel");

            title = await InitializeText("Agent.Title", fontTitle, "Agent Parameters");

            heightCaption = await InitializeText("Caption.Height", font, "Height");
            heightValue = await InitializeText("Value.Height", font);
            heightSlider = await InitializeSlider("Agent.Height", 1, 0f, 5f, 0.1f, (index, value) => { AgentHeight = value; });

            radiusCaption = await InitializeText("Caption.Radius", font, "Radius");
            radiusValue = await InitializeText("Value.Radius", font);
            radiusSlider = await InitializeSlider("Agent.Radius", 1, 0f, 5f, 0.1f, (index, value) => { AgentRadius = value; });

            maxClimbCaption = await InitializeText("Caption.MaxClimb", font, "Maximum Climb Height");
            maxClimbValue = await InitializeText("Value.MaxClimb", font);
            maxClimbSlider = await InitializeSlider("Agent.MaxClimb", 1, 0f, 5f, 0.1f, (index, value) => { AgentMaxClimb = value; });

            maxSlopeCaption = await InitializeText("Caption.MaxSlope", font, "Maximum Slope Angle");
            maxSlopeValue = await InitializeText("Value.MaxSlope", font);
            maxSlopeSlider = await InitializeSlider("Agent.MaxSlope", 1, 0f, 90f, 1f, (index, value) => { AgentMaxSlopes = value; });

            velocityCaption = await InitializeText("Caption.Velocity", font, "Velocity");
            velocityValue = await InitializeText("Value.Velocity", font);
            velocitySlider = await InitializeSlider("Agent.Velocity", 1, 0f, 10f, 1f, (index, value) => { AgentVelocity = value; });

            velocitySlowCaption = await InitializeText("Caption.VelocitySlow", font, "Velocity Slow");
            velocitySlowValue = await InitializeText("Value.VelocitySlow", font);
            velocitySlowSlider = await InitializeSlider("Agent.VelocitySlow", 1, 0f, 10f, 1f, (index, value) => { AgentVelocitySlow = value; });

            initialized = true;

            UpdateLayout();
        }
        /// <summary>
        /// Initializes a panel
        /// </summary>
        /// <param name="name">Name</param>
        private async Task<UIPanel> InitializePanel(string name)
        {
            var panelColor = new Color4(UIConfiguration.BaseColor.ToVector3(), 0.85f);
            var desc = UIPanelDescription.Default(panelColor);
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UIPanel, UIPanelDescription>($"{ObjectName}_{name}", name, desc);
        }
        /// <summary>
        /// Initializes a text area
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        private async Task<UITextArea> InitializeText(string name, TextDrawerDescription font, string text = null)
        {
            var desc = UITextAreaDescription.Default(font, text);
            desc.StartsVisible = false;

            return await scene.AddComponentUI<UITextArea, UITextAreaDescription>($"{ObjectName}_{name}", name, desc);
        }
        /// <summary>
        /// Initializes a slider
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="rangeCount">Range count</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="step">Step value</param>
        /// <param name="callback">Value callback</param>
        private async Task<UISlider> InitializeSlider(string name, int rangeCount, float min, float max, float step, Action<int, float> callback)
        {
            var desc = UISliderDescription.Default(rangeCount);
            desc.Height = 20;
            desc.Width = (rangeCount + 1) * 100;
            desc.Minimum = min;
            desc.Maximum = max;
            desc.Step = step;
            desc.StartsVisible = false;

            var slider = await scene.AddComponentUI<UISlider, UISliderDescription>($"{ObjectName}_{name}", name, desc);
            slider.OnValueChanged = callback;

            return slider;
        }

        /// <summary>
        /// Initializes agent parameters
        /// </summary>
        /// <param name="agent">Agent</param>
        public void InitializeAgentParameters(Player agent)
        {
            this.agent = agent;

            AgentHeight = agent?.Height ?? 0;
            AgentRadius = agent?.Radius ?? 0;
            AgentMaxClimb = agent?.MaxClimb ?? 0;
            AgentMaxSlopes = agent?.MaxSlope ?? 0;
            AgentVelocity = agent?.Velocity ?? 0;
            AgentVelocitySlow = agent?.VelocitySlow ?? 0;

            UpdateLayout();
        }
        /// <summary>
        /// Updates agent data
        /// </summary>
        private void UpdateAgent()
        {
            if (agent == null)
            {
                return;
            }

            agent.Height = AgentHeight;
            agent.Radius = AgentRadius;
            agent.MaxClimb = AgentMaxClimb;
            agent.MaxSlope = AgentMaxSlopes;
            agent.Velocity = AgentVelocity;
            agent.VelocitySlow = AgentVelocitySlow;
        }

        /// <summary>
        /// Updates the editor
        /// </summary>
        public void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (isDirty)
            {
                heightValue.Text = $"{AgentHeight:0.0}";
                radiusValue.Text = $"{AgentRadius:0.0}";
                maxClimbValue.Text = $"{AgentMaxClimb:0.0}";
                maxSlopeValue.Text = $"{AgentMaxSlopes:0}";
                velocityValue.Text = $"{AgentVelocity:0}";
                velocitySlowValue.Text = $"{AgentVelocitySlow:0}";

                UpdateLayout();

                isDirty = false;
            }
        }

        /// <summary>
        /// Updates de editor layout
        /// </summary>
        public void UpdateLayout()
        {
            if (!initialized)
            {
                return;
            }

            float top = Position.Y + VerticalMarging;
            float left = Position.X + HorizontalMarging;
            float width = Width - (HorizontalMarging * 2);

            SetGroupPosition(left, width, ref top, title, null, null);
            SetGroupPosition(left, width, ref top, heightCaption, heightValue, heightSlider);
            SetGroupPosition(left, width, ref top, radiusCaption, radiusValue, radiusSlider);
            SetGroupPosition(left, width, ref top, maxClimbCaption, maxClimbValue, maxClimbSlider);
            SetGroupPosition(left, width, ref top, maxSlopeCaption, maxSlopeValue, maxSlopeSlider);
            SetGroupPosition(left, width, ref top, velocityCaption, velocityValue, velocitySlider);
            SetGroupPosition(left, width, ref top, velocitySlowCaption, velocitySlowValue, velocitySlowSlider);

            mainPanel.SetPosition(Position);
            mainPanel.Width = Width;
            mainPanel.Height = top + VerticalMarging - Position.Y;
            mainPanel.Visible = visible;
        }
        /// <summary>
        /// Sets the editor group position
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="width">Width</param>
        /// <param name="top">Top</param>
        /// <param name="caption">Caption control</param>
        /// <param name="value">Value control</param>
        /// <param name="slider">Slider control</param>
        private void SetGroupPosition(float left, float width, ref float top, UITextArea caption, UITextArea value, UISlider slider)
        {
            if (caption != null)
            {
                caption.SetPosition(left, top);
                caption.Width = width;
                caption.Visible = visible;

                if (value != null)
                {
                    value.GrowControlWithText = true;
                    value.SetPosition(left + width - value.Width, top);
                    value.Visible = visible;
                }

                NextLine(ref top, caption);
            }

            if (slider != null)
            {
                slider.SetPosition(left, top);
                slider.Width = width;
                slider.Visible = visible;

                NextLine(ref top, slider);
            }

            NextLine(ref top, null);
        }
        /// <summary>
        /// Next line
        /// </summary>
        /// <param name="top">Top position</param>
        /// <param name="control">Last control in the line</param>
        private void NextLine(ref float top, IUIControl control)
        {
            top += VerticalPadding + (control?.Height ?? 0f);
        }
    }
}
