using Engine;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using System.Threading.Tasks;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Navigation mesh editor
    /// </summary>
    /// <param name="scene">Scene</param>
    class NavMeshEditor(Scene scene) : Editor(scene)
    {
        private const string ObjectName = nameof(NavMeshEditor);

        private BuildSettings settings;

        private UIPanel mainPanel;

        private UITextArea title;

        private EditorSlider cellSize;
        private EditorSlider cellHeight;
        private EditorSlider regionMinSize;
        private EditorSlider regionMergeSize;
        private EditorSlider edgeMaxLenght;
        private EditorSlider edgeMaxError;
        private EditorSlider vertsPerPoly;
        private EditorSlider detailSampleDist;
        private EditorSlider detailSampleMaxError;
        private EditorSlider tileSize;

        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize
        {
            get { return cellSize.Slider.GetValue(0); }
            set
            {
                cellSize.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Cell heigth
        /// </summary>
        public float CellHeight
        {
            get { return cellHeight.Slider.GetValue(0); }
            set
            {
                cellHeight.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Region minimum size
        /// </summary>
        public float RegionMinSize
        {
            get { return regionMinSize.Slider.GetValue(0); }
            set
            {
                regionMinSize.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Region merge size
        /// </summary>
        public float RegionMergeSize
        {
            get { return regionMergeSize.Slider.GetValue(0); }
            set
            {
                regionMergeSize.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        /// <summary>
        /// Max edge length
        /// </summary>
        public float EdgeMaxLength
        {
            get { return edgeMaxLenght.Slider.GetValue(0); }
            set
            {
                edgeMaxLenght.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        public float EdgeMaxError
        {
            get { return edgeMaxError.Slider.GetValue(0); }
            set
            {
                edgeMaxError.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        public float VertsPerPoly
        {
            get { return vertsPerPoly.Slider.GetValue(0); }
            set
            {
                vertsPerPoly.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        public float DetailSampleDist
        {
            get { return detailSampleDist.Slider.GetValue(0); }
            set
            {
                detailSampleDist.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        public float DetailSampleMaxError
        {
            get { return detailSampleMaxError.Slider.GetValue(0); }
            set
            {
                detailSampleMaxError.Slider.SetValue(0, value);
                isDirty = true;
            }
        }
        public float TileSize
        {
            get { return tileSize.Slider.GetValue(0); }
            set
            {
                tileSize.Slider.SetValue(0, value);
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

            title = await InitializeText($"{ObjectName}_NavMesh.Title", "NavMesh.Title", fontTitle, "NavMesh Parameters");

            cellSize = await InitializeProperty(ObjectName, "Cell Size", font, 0.1f, 1f, 0.01f, (index, value) => { CellSize = value; });
            cellHeight = await InitializeProperty(ObjectName, "Cell Height", font, 0.1f, 1f, 0.01f, (index, value) => { CellHeight = value; });
            regionMinSize = await InitializeProperty(ObjectName, "Region Min Size", font, 0f, 150f, 1f, (index, value) => { RegionMinSize = value; });
            regionMergeSize = await InitializeProperty(ObjectName, "Merge Region Size", font, 0f, 150f, 1f, (index, value) => { RegionMergeSize = value; });
            edgeMaxLenght = await InitializeProperty(ObjectName, "Max Edge Length", font, 0f, 50f, 1f, (index, value) => { EdgeMaxLength = value; });
            edgeMaxError = await InitializeProperty(ObjectName, "Max Edge Error", font, 0.1f, 3f, 0.1f, (index, value) => { EdgeMaxError = value; });
            vertsPerPoly = await InitializeProperty(ObjectName, "Verts Per Poly", font, 3f, 12f, 1f, (index, value) => { VertsPerPoly = value; });
            detailSampleDist = await InitializeProperty(ObjectName, "Detail Sample Dist", font, 0f, 16f, 1f, (index, value) => { DetailSampleDist = value; });
            detailSampleMaxError = await InitializeProperty(ObjectName, "Detail Sample Max Error", font, 0f, 16f, 1f, (index, value) => { DetailSampleMaxError = value; });
            tileSize = await InitializeProperty(ObjectName, "Tile Size", font, 16f, 1024f, 8f, (index, value) => { TileSize = value; });

            initialized = true;

            UpdateLayout();
        }

        /// <summary>
        /// Initializes settings parameters
        /// </summary>
        /// <param name="settings">Navmesh settings</param>
        public void InitializeSettings(BuildSettings settings)
        {
            this.settings = settings;

            CellSize = settings.CellSize;
            CellHeight = settings.CellHeight;
            RegionMinSize = settings.RegionMinSize;
            RegionMergeSize = settings.RegionMergeSize;
            EdgeMaxLength = settings.EdgeMaxLength;
            EdgeMaxError = settings.EdgeMaxError;
            VertsPerPoly = settings.VertsPerPoly;
            DetailSampleDist = settings.DetailSampleDist;
            DetailSampleMaxError = settings.DetailSampleMaxError;
            TileSize = settings.TileSize;

            UpdateLayout();
        }
        /// <summary>
        /// Updates the settings data
        /// </summary>
        public void UpdateSettings()
        {
            if (settings == null)
            {
                return;
            }

            settings.CellSize = CellSize;
            settings.CellHeight = CellHeight;
            settings.RegionMinSize = RegionMinSize;
            settings.RegionMergeSize = RegionMergeSize;
            settings.EdgeMaxLength = EdgeMaxLength;
            settings.EdgeMaxError = EdgeMaxError;
            settings.VertsPerPoly = (int)VertsPerPoly;
            settings.DetailSampleDist = DetailSampleDist;
            settings.DetailSampleMaxError = DetailSampleMaxError;
            settings.TileSize = TileSize;
        }

        /// <inheritdoc/>
        protected override void UpdateTextValues()
        {
            cellSize.Value.Text = $"{CellSize:0.00}";
            cellHeight.Value.Text = $"{CellHeight:0.00}";
            regionMinSize.Value.Text = $"{RegionMinSize:0}";
            regionMergeSize.Value.Text = $"{RegionMergeSize:0}";
            edgeMaxLenght.Value.Text = $"{EdgeMaxLength:0}";
            edgeMaxError.Value.Text = $"{EdgeMaxError:0.0}";
            vertsPerPoly.Value.Text = $"{VertsPerPoly:0}";
            detailSampleDist.Value.Text = $"{DetailSampleDist:0}";
            detailSampleMaxError.Value.Text = $"{DetailSampleMaxError:0}";
            tileSize.Value.Text = $"{TileSize:0}";
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
            SetGroupPosition(left, width, ref top, cellSize);
            SetGroupPosition(left, width, ref top, cellHeight);
            NextLine(ref top, null);

            SetGroupPosition(left, width, ref top, regionMinSize);
            SetGroupPosition(left, width, ref top, regionMergeSize);
            NextLine(ref top, null);

            SetGroupPosition(left, width, ref top, edgeMaxLenght);
            SetGroupPosition(left, width, ref top, edgeMaxError);
            SetGroupPosition(left, width, ref top, vertsPerPoly);
            NextLine(ref top, null);

            SetGroupPosition(left, width, ref top, detailSampleDist);
            SetGroupPosition(left, width, ref top, detailSampleMaxError);
            NextLine(ref top, null);

            SetGroupPosition(left, width, ref top, tileSize);

            mainPanel.SetPosition(Position);
            mainPanel.Width = Width;
            mainPanel.Height = top + VerticalMarging - Position.Y;
            mainPanel.Visible = Visible;
        }
    }
}
