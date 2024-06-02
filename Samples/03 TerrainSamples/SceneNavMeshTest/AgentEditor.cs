using Engine;
using Engine.UI;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Agent editor
    /// </summary>
    /// <param name="scene">Scene</param>
    class AgentEditor(Scene scene) : Editor(scene)
    {
        private const string ObjectName = nameof(AgentEditor);

        private readonly Scene scene = scene;
        private Player agent;

        private UIPanel mainPanel;

        private UITextArea title;

        private EditorSlider height;
        private EditorSlider radius;
        private EditorSlider maxClimb;
        private EditorSlider maxSlope;
        private EditorSlider velocity;
        private EditorSlider velocitySlow;

        /// <summary>
        /// Agent height
        /// </summary>
        public float AgentHeight
        {
            get { return height.Slider.GetValue(0); }
            set
            {
                height.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent radius
        /// </summary>
        public float AgentRadius
        {
            get { return radius.Slider.GetValue(0); }
            set
            {
                radius.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent maximum climb height
        /// </summary>
        public float AgentMaxClimb
        {
            get { return maxClimb.Slider.GetValue(0); }
            set
            {
                maxClimb.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent maximum slope angle
        /// </summary>
        public float AgentMaxSlopes
        {
            get { return maxSlope.Slider.GetValue(0); }
            set
            {
                maxSlope.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent velocity
        /// </summary>
        public float AgentVelocity
        {
            get { return velocity.Slider.GetValue(0); }
            set
            {
                velocity.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Agent velocity slow
        /// </summary>
        public float AgentVelocitySlow
        {
            get { return velocitySlow.Slider.GetValue(0); }
            set
            {
                velocitySlow.Slider.SetValue(0, value);
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
            mainPanel = await InitializePanel($"{ObjectName}_MainPanel", "MainPanel");

            title = await InitializeText($"{ObjectName}_Agent.Title", "Agent.Title", fontTitle, "Agent Parameters");

            height = await InitializeProperty(ObjectName, "Height", font, 0.1f, 5f, 0.1f, (index, value) => { AgentHeight = value; });
            radius = await InitializeProperty(ObjectName, "Radius", font, 0f, 5f, 0.1f, (index, value) => { AgentRadius = value; });
            maxClimb = await InitializeProperty(ObjectName, "Max Climb", font, 0.1f, 5f, 0.1f, (index, value) => { AgentMaxClimb = value; });
            maxSlope = await InitializeProperty(ObjectName, "Max Slope", font, 0f, 90f, 1f, (index, value) => { AgentMaxSlopes = value; });
            velocity = await InitializeProperty(ObjectName, "Velocity", font, 1f, 10f, 1f, (index, value) => { AgentVelocity = value; });
            velocitySlow = await InitializeProperty(ObjectName, "Velocity Slow", font, 1f, 10f, 1f, (index, value) => { AgentVelocitySlow = value; });

            initialized = true;

            UpdateLayout();
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
        /// Updates the agent data
        /// </summary>
        public void UpdateAgent()
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

        /// <inheritdoc/>
        protected override void UpdateTextValues()
        {
            height.Value.Text = $"{AgentHeight:0.0}";
            radius.Value.Text = $"{AgentRadius:0.0}";
            maxClimb.Value.Text = $"{AgentMaxClimb:0.0}";
            maxSlope.Value.Text = $"{AgentMaxSlopes:0}";
            velocity.Value.Text = $"{AgentVelocity:0}";
            velocitySlow.Value.Text = $"{AgentVelocitySlow:0}";
        }

        /// <inheritdoc/>
        public override void UpdateLayout()
        {
            if (!initialized)
            {
                return;
            }

            float top = Position.Y + VerticalMarging;
            float left = Position.X + HorizontalMarging;
            float width = Width - (HorizontalMarging * 2);

            SetGroupPosition(left, width, ref top, title, null, null);
            SetGroupPosition(left, width, ref top, height);
            SetGroupPosition(left, width, ref top, radius);
            SetGroupPosition(left, width, ref top, maxClimb);
            SetGroupPosition(left, width, ref top, maxSlope);
            SetGroupPosition(left, width, ref top, velocity);
            SetGroupPosition(left, width, ref top, velocitySlow);

            mainPanel.SetPosition(Position);
            mainPanel.Width = Width;
            mainPanel.Height = top + VerticalMarging - Position.Y;
            mainPanel.Visible = Visible;
        }
    }
}
