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

        private Player agent;

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
                IsDirty = true;
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
                IsDirty = true;
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
                IsDirty = true;
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
                IsDirty = true;
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
                IsDirty = true;
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
                IsDirty = true;
            }
        }

        /// <summary>
        /// Initializes the editor
        /// </summary>
        /// <param name="fontTitle">Title font</param>
        /// <param name="font">Font</param>
        public async Task Initialize(TextDrawerDescription fontTitle, TextDrawerDescription font)
        {
            height = await InitializePropertySlider(ObjectName, "Height", font, 0.1f, 5f, 0.1f, (index, value) => { AgentHeight = value; });
            radius = await InitializePropertySlider(ObjectName, "Radius", font, 0f, 5f, 0.1f, (index, value) => { AgentRadius = value; });
            maxClimb = await InitializePropertySlider(ObjectName, "Max Climb", font, 0.1f, 5f, 0.1f, (index, value) => { AgentMaxClimb = value; });
            maxSlope = await InitializePropertySlider(ObjectName, "Max Slope", font, 0f, 90f, 1f, (index, value) => { AgentMaxSlopes = value; });
            velocity = await InitializePropertySlider(ObjectName, "Velocity", font, 1f, 10f, 1f, (index, value) => { AgentVelocity = value; });
            velocitySlow = await InitializePropertySlider(ObjectName, "Velocity Slow", font, 1f, 10f, 1f, (index, value) => { AgentVelocitySlow = value; });

            await base.Initialize(fontTitle);
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
        protected override void UpdateControlsLayout(float left, float width, ref float top)
        {
            SetGroupPosition(left, width, ref top, height);
            SetGroupPosition(left, width, ref top, radius);
            SetGroupPosition(left, width, ref top, maxClimb);
            SetGroupPosition(left, width, ref top, maxSlope);
            SetGroupPosition(left, width, ref top, velocity);
            SetGroupPosition(left, width, ref top, velocitySlow);
        }
    }
}
