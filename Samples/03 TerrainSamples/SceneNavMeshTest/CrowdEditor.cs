using Engine;
using Engine.PathFinding;
using Engine.UI;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    class CrowdEditor(Scene scene) : Editor(scene)
    {
        private const string ObjectName = nameof(CrowdEditor);
        private const string uMask = "{0:0}";
        private const string cMask = "{0:0.00}";

        private EditorCheckbox optimizeVisibility;
        private EditorCheckbox optimizeTopology;
        private EditorCheckbox anticipateTurns;
        private EditorCheckbox obstacleAvoidance;
        private EditorSlider avoidanceQuality;
        private EditorCheckbox separation;
        private EditorSlider separationWeight;

        /// <summary>
        /// Initializes the editor
        /// </summary>
        /// <param name="fontTitle">Title font</param>
        /// <param name="font">Font</param>
        public async Task Initialize(TextDrawerDescription fontTitle, TextDrawerDescription font)
        {
            optimizeVisibility = await InitializePropertyCheckbox(ObjectName, "Optimize Visibility", font);
            optimizeTopology = await InitializePropertyCheckbox(ObjectName, "Optimize Topology", font);
            anticipateTurns = await InitializePropertyCheckbox(ObjectName, "Anticipate Turns", font);
            obstacleAvoidance = await InitializePropertyCheckbox(ObjectName, "Obstacle Avoidance", font);
            avoidanceQuality = await InitializePropertySlider(ObjectName, "Avoidance Quality", font, 0f, 3f, 1f, uMask);
            separation = await InitializePropertyCheckbox(ObjectName, "Separation", font);
            separationWeight = await InitializePropertySlider(ObjectName, "Separation Weight", font, 0f, 20f, 0.01f, cMask);

            await base.Initialize(fontTitle, "Crowd Settings");
        }

        /// <summary>
        /// Initializes settings parameters
        /// </summary>
        /// <param name="settings">Crowd settings</param>
        public void InitializeSettings(CrowdSettings settings)
        {
            optimizeVisibility.SetValue(settings.OptimizeVisibility);
            optimizeTopology.SetValue(settings.OptimizeTopology);
            anticipateTurns.SetValue(settings.AnticipateTurns);
            obstacleAvoidance.SetValue(settings.ObstacleAvoidance);
            avoidanceQuality.SetValue(settings.AvoidanceQuality);
            separation.SetValue(settings.Separation);
            separationWeight.SetValue(settings.SeparationWeight);

            UpdateLayout();
        }
        /// <summary>
        /// Updates the settings data
        /// </summary>
        /// <param name="settings">Settings to update</param>
        public void UpdateSettings(ref CrowdSettings settings)
        {
            settings.OptimizeVisibility = optimizeVisibility.GetValue();
            settings.OptimizeTopology = optimizeTopology.GetValue();
            settings.AnticipateTurns = anticipateTurns.GetValue();
            settings.ObstacleAvoidance = obstacleAvoidance.GetValue();
            settings.AvoidanceQuality = (int)avoidanceQuality.GetValue();
            settings.Separation = separation.GetValue();
            settings.SeparationWeight = separationWeight.GetValue();
        }

        /// <inheritdoc/>
        protected override void UpdateControlsLayout(float left, float width, ref float top)
        {
            SetGroupPosition(left, width, ref top, optimizeVisibility);
            SetGroupPosition(left, width, ref top, optimizeTopology);
            SetGroupPosition(left, width, ref top, anticipateTurns);
            SetGroupPosition(left, width, ref top, obstacleAvoidance);
            SetGroupPosition(left, width, ref top, avoidanceQuality);
            SetGroupPosition(left, width, ref top, separation);
            SetGroupPosition(left, width, ref top, separationWeight);
        }
    }
}
