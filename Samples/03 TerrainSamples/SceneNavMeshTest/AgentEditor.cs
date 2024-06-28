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
        private const string uMask = "{0:0}";
        private const string dMask = "{0:0.0}";

        private EditorSlider height;
        private EditorSlider radius;
        private EditorSlider maxClimb;
        private EditorSlider maxSlope;
        private EditorSlider velocity;
        private EditorSlider velocitySlow;

        /// <summary>
        /// Initializes the editor
        /// </summary>
        /// <param name="fontTitle">Title font</param>
        /// <param name="font">Font</param>
        public async Task Initialize(TextDrawerDescription fontTitle, TextDrawerDescription font)
        {
            height = await InitializePropertySlider(ObjectName, "Height", font, 0.1f, 5f, 0.1f, dMask);
            radius = await InitializePropertySlider(ObjectName, "Radius", font, 0f, 5f, 0.1f, dMask);
            maxClimb = await InitializePropertySlider(ObjectName, "Max Climb", font, 0.1f, 5f, 0.1f, dMask);
            maxSlope = await InitializePropertySlider(ObjectName, "Max Slope", font, 0f, 90f, 1f, uMask);
            velocity = await InitializePropertySlider(ObjectName, "Velocity", font, 1f, 10f, 1f, uMask);
            velocitySlow = await InitializePropertySlider(ObjectName, "Velocity Slow", font, 1f, 10f, 1f, uMask);

            await base.Initialize(fontTitle, "Agent Parameters");
        }

        /// <summary>
        /// Initializes agent parameters
        /// </summary>
        /// <param name="agent">Agent</param>
        public void InitializeAgentParameters(Player agent)
        {
            height.SetValue(agent?.Height ?? 0);
            radius.SetValue(agent?.Radius ?? 0);
            maxClimb.SetValue(agent?.MaxClimb ?? 0);
            maxSlope.SetValue(agent?.MaxSlope ?? 0);
            velocity.SetValue(agent?.Velocity ?? 0);
            velocitySlow.SetValue(agent?.VelocitySlow ?? 0);

            UpdateLayout();
        }
        /// <summary>
        /// Updates the agent data
        /// </summary>
        /// <param name="agent">Agent to update</param>
        public void UpdateAgent(Player agent)
        {
            if (agent == null)
            {
                return;
            }

            agent.Height = height.GetValue();
            agent.Radius = radius.GetValue();
            agent.MaxClimb = maxClimb.GetValue();
            agent.MaxSlope = maxSlope.GetValue();
            agent.Velocity = velocity.GetValue();
            agent.VelocitySlow = velocitySlow.GetValue();
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
