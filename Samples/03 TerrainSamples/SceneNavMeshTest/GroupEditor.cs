using Engine;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Engine.UI;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Group editor
    /// </summary>
    /// <param name="scene"></param>
    class GroupEditor(Scene scene) : Editor(scene)
    {
        private const string ObjectName = nameof(GroupEditor);
        private const string uMask = "{0:0}";
        private const string dMask = "{0:0.0}";
        private const string cMask = "{0:0.00}";

        private float heigth;
        private float radius;

        private EditorSlider maxAcceleration;
        private EditorSlider maxSpeed;
        private EditorSlider slowDownRadiusFactor;
        private EditorSlider collisionQueryRange;
        private EditorSlider pathOptimizationRange;

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
        public async Task Initialize(FontDescription fontTitle, FontDescription font)
        {
            maxAcceleration = await InitializePropertySlider(ObjectName, "Max Acceleration", font, 0f, 10f, 0.01f, cMask);
            maxSpeed = await InitializePropertySlider(ObjectName, "Max Speed", font, 0f, 10f, 0.01f, cMask);
            slowDownRadiusFactor = await InitializePropertySlider(ObjectName, "Slow Down Radius Factor", font, 0f, 10f, 0.1f, cMask);
            collisionQueryRange = await InitializePropertySlider(ObjectName, "Collision Query Range", font, 0f, 15f, 0.1f, dMask);
            pathOptimizationRange = await InitializePropertySlider(ObjectName, "Path Optimization Range", font, 0f, 50f, 0.1f, dMask);
            optimizeVisibility = await InitializePropertyCheckbox(ObjectName, "Optimize Visibility", font);
            optimizeTopology = await InitializePropertyCheckbox(ObjectName, "Optimize Topology", font);
            anticipateTurns = await InitializePropertyCheckbox(ObjectName, "Anticipate Turns", font);
            obstacleAvoidance = await InitializePropertyCheckbox(ObjectName, "Obstacle Avoidance", font);
            avoidanceQuality = await InitializePropertySlider(ObjectName, "Avoidance Quality", font, 0f, 3f, 1f, uMask);
            separation = await InitializePropertyCheckbox(ObjectName, "Separation", font);
            separationWeight = await InitializePropertySlider(ObjectName, "Separation Weight", font, 0f, 20f, 0.01f, cMask);

            await base.Initialize(fontTitle, "Group Settings");
        }

        /// <summary>
        /// Initializes settings parameters
        /// </summary>
        /// <param name="settings">Settings</param>
        public void InitializeSettings(CrowdAgentSettings settings)
        {
            heigth = settings.Height;
            radius = settings.Radius;
            maxAcceleration.SetValue(settings.MaxAcceleration);
            maxSpeed.SetValue(settings.MaxSpeed);
            slowDownRadiusFactor.SetValue(settings.SlowDownRadiusFactor);
            collisionQueryRange.SetValue(settings.CollisionQueryRange);
            pathOptimizationRange.SetValue(settings.PathOptimizationRange);
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
        public void UpdateSettings(ref CrowdAgentSettings settings)
        {
            settings.Height = heigth;
            settings.Radius = radius;
            settings.MaxAcceleration = maxAcceleration.GetValue();
            settings.MaxSpeed = maxSpeed.GetValue();
            settings.SlowDownRadiusFactor = slowDownRadiusFactor.GetValue();
            settings.CollisionQueryRange = collisionQueryRange.GetValue();
            settings.PathOptimizationRange = pathOptimizationRange.GetValue();
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
            SetGroupPosition(left, width, ref top, maxAcceleration);
            SetGroupPosition(left, width, ref top, maxSpeed);
            SetGroupPosition(left, width, ref top, slowDownRadiusFactor);
            SetGroupPosition(left, width, ref top, collisionQueryRange);
            SetGroupPosition(left, width, ref top, pathOptimizationRange);
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
