using Engine;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using System;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Build settings editor
    /// </summary>
    /// <param name="scene">Scene</param>
    class BuildSettingsEditor(Scene scene) : Editor(scene)
    {
        private const string ObjectName = nameof(BuildSettingsEditor);
        private const string uMask = "{0:0}";
        private const string dMask = "{0:0.0}";
        private const string cMask = "{0:0.00}";

        private EditorCheckboxGroup<BuildModes> buildMode;
        private EditorSlider cellSize;
        private EditorSlider cellHeight;
        private EditorSlider regionMinSize;
        private EditorSlider regionMergeSize;
        private EditorCheckboxGroup<SamplePartitionTypes> partition;
        private EditorCheckbox filterLedgeSpans;
        private EditorCheckbox filterLowHangingObstacles;
        private EditorCheckbox filterWalkableLowHeightSpans;
        private EditorSlider edgeMaxLength;
        private EditorSlider edgeMaxError;
        private EditorSlider vertsPerPoly;
        private EditorSlider detailSampleDist;
        private EditorSlider detailSampleMaxError;
        private EditorSlider tileSize;
        private EditorCheckbox useTileCache;
        private EditorCheckbox buildAllTiles;

        /// <summary>
        /// Initializes the editor
        /// </summary>
        /// <param name="fontTitle">Title font</param>
        /// <param name="font">Font</param>
        public async Task Initialize(FontDescription fontTitle, FontDescription font)
        {
            buildMode = await InitializePropertyCheckboxGroup(ObjectName, "Build Mode", font, Enum.GetValues<BuildModes>());
            cellSize = await InitializePropertySlider(ObjectName, "Cell Size", font, 0.1f, 1f, 0.01f, cMask);
            cellHeight = await InitializePropertySlider(ObjectName, "Cell Height", font, 0.1f, 1f, 0.01f, cMask);
            regionMinSize = await InitializePropertySlider(ObjectName, "Region Min Size", font, 0f, 150f, 1f, uMask);
            regionMergeSize = await InitializePropertySlider(ObjectName, "Merge Region Size", font, 0f, 150f, 1f, uMask);
            partition = await InitializePropertyCheckboxGroup(ObjectName, "Partition", font, Enum.GetValues<SamplePartitionTypes>());
            filterLedgeSpans = await InitializePropertyCheckbox(ObjectName, "Filter Ledge Spans", font);
            filterLowHangingObstacles = await InitializePropertyCheckbox(ObjectName, "Filter Low Hanging Obstacles", font);
            filterWalkableLowHeightSpans = await InitializePropertyCheckbox(ObjectName, "Filter Walkable Low Height Spans", font);
            edgeMaxLength = await InitializePropertySlider(ObjectName, "Max Edge Length", font, 0f, 50f, 1f, uMask);
            edgeMaxError = await InitializePropertySlider(ObjectName, "Max Edge Error", font, 0.1f, 3f, 0.1f, dMask);
            vertsPerPoly = await InitializePropertySlider(ObjectName, "Verts Per Poly", font, 3f, 12f, 1f, uMask);
            detailSampleDist = await InitializePropertySlider(ObjectName, "Detail Sample Dist", font, 0f, 16f, 1f, uMask);
            detailSampleMaxError = await InitializePropertySlider(ObjectName, "Detail Sample Max Error", font, 0f, 16f, 1f, uMask);
            tileSize = await InitializePropertySlider(ObjectName, "Tile Size", font, 16f, 1024f, 8f, uMask);
            useTileCache = await InitializePropertyCheckbox(ObjectName, "Use Tile Cache", font);
            buildAllTiles = await InitializePropertyCheckbox(ObjectName, "Build All Tiles", font);

            await base.Initialize(fontTitle, "Build Settings");
        }

        /// <summary>
        /// Initializes settings parameters
        /// </summary>
        /// <param name="settings">Navmesh settings</param>
        public void InitializeSettings(BuildSettings settings)
        {
            buildMode.SetValue(settings.BuildMode);
            cellSize.SetValue(settings.CellSize);
            cellHeight.SetValue(settings.CellHeight);
            regionMinSize.SetValue(settings.RegionMinSize);
            regionMergeSize.SetValue(settings.RegionMergeSize);
            partition.SetValue(settings.PartitionType);
            filterLedgeSpans.SetValue(settings.FilterLedgeSpans);
            filterLowHangingObstacles.SetValue(settings.FilterLowHangingObstacles);
            filterWalkableLowHeightSpans.SetValue(settings.FilterWalkableLowHeightSpans);
            edgeMaxLength.SetValue(settings.EdgeMaxLength);
            edgeMaxError.SetValue(settings.EdgeMaxError);
            vertsPerPoly.SetValue(settings.VertsPerPoly);
            detailSampleDist.SetValue(settings.DetailSampleDist);
            detailSampleMaxError.SetValue(settings.DetailSampleMaxError);
            tileSize.SetValue(settings.TileSize);
            useTileCache.SetValue(settings.UseTileCache);
            buildAllTiles.SetValue(settings.BuildAllTiles);

            UpdateLayout();
        }
        /// <summary>
        /// Updates the settings data
        /// </summary>
        /// <param name="settings">Settings to update</param>
        public void UpdateSettings(BuildSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.BuildMode = buildMode.GetValue();
            settings.CellSize = cellSize.GetValue();
            settings.CellHeight = cellHeight.GetValue();
            settings.RegionMinSize = regionMinSize.GetValue();
            settings.RegionMergeSize = regionMergeSize.GetValue();
            settings.PartitionType = partition.GetValue();
            settings.FilterLedgeSpans = filterLedgeSpans.GetValue();
            settings.FilterLowHangingObstacles = filterLowHangingObstacles.GetValue();
            settings.FilterWalkableLowHeightSpans = filterWalkableLowHeightSpans.GetValue();
            settings.EdgeMaxLength = edgeMaxLength.GetValue();
            settings.EdgeMaxError = edgeMaxError.GetValue();
            settings.VertsPerPoly = (int)vertsPerPoly.GetValue();
            settings.DetailSampleDist = detailSampleDist.GetValue();
            settings.DetailSampleMaxError = detailSampleMaxError.GetValue();
            settings.TileSize = tileSize.GetValue();
            settings.UseTileCache = useTileCache.GetValue();
            settings.BuildAllTiles = buildAllTiles.GetValue();
        }

        /// <inheritdoc/>
        protected override void UpdateControlsLayout(float left, float width, ref float top)
        {
            SetGroupPosition(left, width, ref top, buildMode);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, cellSize);
            SetGroupPosition(left, width, ref top, cellHeight);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, regionMinSize);
            SetGroupPosition(left, width, ref top, regionMergeSize);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, partition);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, filterLedgeSpans);
            SetGroupPosition(left, width, ref top, filterLowHangingObstacles);
            SetGroupPosition(left, width, ref top, filterWalkableLowHeightSpans);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, edgeMaxLength);
            SetGroupPosition(left, width, ref top, edgeMaxError);
            SetGroupPosition(left, width, ref top, vertsPerPoly);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, detailSampleDist);
            SetGroupPosition(left, width, ref top, detailSampleMaxError);
            NextLine(VerticalPadding, ref top, null);

            SetGroupPosition(left, width, ref top, tileSize);
            SetGroupPosition(left, width, ref top, useTileCache);
            SetGroupPosition(left, width, ref top, buildAllTiles);
        }
    }
}
