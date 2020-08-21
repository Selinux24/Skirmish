using Engine;
using Engine.Common;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;
using DialogResult = System.Windows.Forms.DialogResult;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Terrain.PerlinNoise
{
    using Terrain.Start;

    class PerlinNoiseScene : Scene
    {
        readonly string fontFamily = "Microsoft Sans Serif";

        UIPanel backGround;
        UIButton btnExit;

        UITextArea txtScale;
        UIProgressBar pbScale;
        UITextArea txtLacunarity;
        UIProgressBar pbLacunarity;
        UITextArea txtPersistance;
        UIProgressBar pbPersistance;
        UITextArea txtOctaves;
        UIProgressBar pbOctaves;
        UITextArea txtHelpOffset;
        UITextArea txtOffset;
        UITextArea txtHelpSeed;
        UITextArea txtSeed;

        UIButton btnSave;

        UITextureRenderer perlinRenderer;
        EngineShaderResourceView texture;
        NoiseMap noiseMap;
        bool noiseMapDirty = true;
        bool generatingMap = false;

        readonly int mapSize = 128;
        readonly float maxScale = 100;
        readonly float maxLacunarity = 20;
        readonly float maxPersistance = 1;
        readonly int maxOctaves = 16;

        float mapScale = 0.5f;
        float mapLacunarity = 2;
        float mapPersistance = 0.5f;
        int mapOctaves = 5;
        Vector2 mapOffset = Vector2.One;
        int mapSeed = 0;

        private float Scale
        {
            get
            {
                return mapScale / maxScale;
            }
            set
            {
                mapScale = value * maxScale;
            }
        }
        private float Lacunarity
        {
            get
            {
                return mapLacunarity / maxLacunarity;
            }
            set
            {
                mapLacunarity = value * maxLacunarity;
            }
        }
        private float Persistance
        {
            get
            {
                return mapPersistance / maxPersistance;
            }
            set
            {
                mapPersistance = value * maxPersistance;
            }
        }
        private float Octaves
        {
            get
            {
                return mapOctaves / (float)maxOctaves;
            }
            set
            {
                mapOctaves = (int)(value * maxOctaves);
            }
        }

        UIControl capturedCtrl = null;

        public PerlinNoiseScene(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await LoadResourcesAsync(InitializeUI());

            await LoadResourcesAsync(InitializeTextureRenderer());

            ResizeTextureRenderer();
            ResizeUI();

            Cursor.Show();
        }
        public async Task InitializeUI()
        {
            Color4 bColor1 = Color.Brown;
            Color4 bColor2 = Color4.AdjustSaturation(Color.Brown, 1.5f);
            Color4 pColor = Color.DeepSkyBlue;

            backGround = await this.AddComponentUIPanel(UIPanelDescription.Screen(this, Color.SandyBrown));
            btnExit = await this.AddComponentUIButton(UIButtonDescription.DefaultTwoStateButton(bColor1, bColor2, TextDrawerDescription.FromFamily(fontFamily, 16)));

            txtScale = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            txtLacunarity = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            txtPersistance = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            txtOctaves = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            txtHelpOffset = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 12));
            txtOffset = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 14));
            txtHelpSeed = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 12));
            txtSeed = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 14));

            pbScale = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbLacunarity = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbPersistance = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbOctaves = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));

            btnSave = await this.AddComponentUIButton(UIButtonDescription.DefaultTwoStateButton(bColor1, bColor2, TextDrawerDescription.FromFamily(fontFamily, 16)));

            btnExit.JustReleased += BtnExit_JustReleased;

            txtScale.Text = "Scale";

            pbScale.ProgressColor = pColor;
            pbScale.ProgressValue = Scale;
            pbScale.JustPressed += PbJustPressed;
            pbScale.Pressed += PbPressed;
            pbScale.JustReleased += PbJustReleased;

            txtLacunarity.Text = "Lacunarity";

            pbLacunarity.ProgressColor = pColor;
            pbLacunarity.ProgressValue = Lacunarity;
            pbLacunarity.JustPressed += PbJustPressed;
            pbLacunarity.Pressed += PbPressed;
            pbLacunarity.JustReleased += PbJustReleased;

            txtPersistance.Text = "Persistance";

            pbPersistance.ProgressColor = pColor;
            pbPersistance.ProgressValue = Persistance;
            pbPersistance.JustPressed += PbJustPressed;
            pbPersistance.Pressed += PbPressed;
            pbPersistance.JustReleased += PbJustReleased;

            txtOctaves.Text = "Octaves";

            pbOctaves.ProgressColor = pColor;
            pbOctaves.ProgressValue = Octaves;
            pbOctaves.JustPressed += PbJustPressed;
            pbOctaves.Pressed += PbPressed;
            pbOctaves.JustReleased += PbJustReleased;

            btnSave.JustReleased += BtnSave_JustReleased;
        }
        public async Task InitializeTextureRenderer()
        {
            texture = this.Game.ResourceManager.RequestResource(Guid.NewGuid(), new Color4[] { }, mapSize, true);

            perlinRenderer = await this.AddComponentUITextureRenderer(UITextureRendererDescription.Default());
            perlinRenderer.Texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<StartScene>();
            }

            if (UpdateInput(gameTime))
            {
                noiseMapDirty = true;
            }

            if (UpdateUI())
            {
                noiseMapDirty = true;
            }

            texture.Update(this.Game, noiseMap?.CreateColors());

            if (noiseMapDirty)
            {
                GenerateMap();

                return;
            }
        }
        private bool UpdateInput(GameTime gameTime)
        {
            bool updateMap = false;

            float delta = gameTime.ElapsedSeconds;

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                mapOffset.Y -= delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                mapOffset.Y += delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                mapOffset.X -= delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                mapOffset.X += delta;
                updateMap = true;
            }

            mapOffset.X = Math.Max(mapOffset.X, 1f);
            mapOffset.Y = Math.Max(mapOffset.Y, 1f);

            if (this.Game.Input.KeyJustPressed(Keys.X))
            {
                mapSeed += 100 + (int)(delta * 10000f);
                updateMap = true;
            }

            mapSeed = Math.Max(mapSeed, 0);

            return updateMap;
        }
        private bool UpdateUI()
        {
            pbScale.Caption.Text = $"{mapScale}";
            pbLacunarity.Caption.Text = $"{mapLacunarity}";
            pbPersistance.Caption.Text = $"{mapPersistance}";
            pbOctaves.Caption.Text = $"{mapOctaves}";
            txtOffset.Text = $"Offset: {mapOffset}";
            txtSeed.Text = $"Map seed: {mapSeed}";

            bool updateMap = false;

            if (Scale != pbScale.ProgressValue)
            {
                Scale = pbScale.ProgressValue;
                updateMap = true;
            }

            if (Lacunarity != pbLacunarity.ProgressValue)
            {
                Lacunarity = pbLacunarity.ProgressValue;
                updateMap = true;
            }

            if (Persistance != pbPersistance.ProgressValue)
            {
                Persistance = pbPersistance.ProgressValue;
                updateMap = true;
            }

            if (Octaves != pbOctaves.ProgressValue)
            {
                Octaves = pbOctaves.ProgressValue;
                updateMap = true;
            }

            return updateMap;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            ResizeTextureRenderer();
            ResizeUI();
        }
        public void ResizeTextureRenderer()
        {
            float size = Math.Min(this.Game.Form.RenderHeight, this.Game.Form.RenderWidth) * 0.8f;

            perlinRenderer.Width = size;
            perlinRenderer.Height = size;
            perlinRenderer.CenterHorizontally = CenterTargets.Screen;
            perlinRenderer.CenterVertically = CenterTargets.Screen;
        }
        public void ResizeUI()
        {
            backGround.Width = this.Game.Form.RenderWidth;
            backGround.Height = this.Game.Form.RenderHeight;

            float perlinRendererSize = Math.Min(this.Game.Form.RenderHeight, this.Game.Form.RenderWidth) * 0.8f;

            float borderSize = (this.Game.Form.RenderWidth - perlinRendererSize) * 0.5f;

            float marginLeft = 25;
            float marginTop = this.Game.Form.RenderHeight * 0.1f;
            float separation = 20;
            float width = borderSize - (marginLeft * 2);
            float height = 15;

            int lineIndex = 0;

            btnExit.SetPosition(this.Game.Form.RenderWidth - 30, 0);
            btnExit.Width = 30;
            btnExit.Height = 30;
            btnExit.Caption.Text = "X";
            btnExit.Caption.HorizontalAlign = HorizontalTextAlign.Center;
            btnExit.Caption.VerticalAlign = VerticalTextAlign.Middle;

            txtScale.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            pbScale.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbScale.Width = width;
            pbScale.Height = height;

            txtLacunarity.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            pbLacunarity.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbLacunarity.Width = width;
            pbLacunarity.Height = height;

            txtPersistance.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            pbPersistance.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbPersistance.Width = width;
            pbPersistance.Height = height;

            txtOctaves.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            pbOctaves.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbOctaves.Width = width;
            pbOctaves.Height = height;

            lineIndex++;

            txtHelpOffset.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtHelpOffset.AdjustAreaWithText = false;
            txtHelpOffset.Width = width;
            txtHelpOffset.Height = 0;
            txtHelpOffset.Text = "Use W A S D keys to displace the Noise map";

            txtOffset.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            lineIndex++;

            txtHelpSeed.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtHelpSeed.AdjustAreaWithText = false;
            txtHelpSeed.Width = width;
            txtHelpSeed.Height = 0;
            txtHelpSeed.Text = "Use X key to change the seed";

            txtSeed.SetPosition(marginLeft, marginTop + (separation * lineIndex));

            btnSave.Width = 200;
            btnSave.Height = 50;
            btnSave.SetPosition(this.Game.Form.RenderCenter.X + (perlinRendererSize / 2) - btnSave.Width, this.Game.Form.RenderCenter.Y + (perlinRendererSize / 2));
            btnSave.Caption.HorizontalAlign = HorizontalTextAlign.Center;
            btnSave.Caption.VerticalAlign = VerticalTextAlign.Middle;
            btnSave.Caption.Text = "Save to File";
        }

        private void PbPressed(object sender, EventArgs e)
        {
            if (sender is UIProgressBar pb)
            {
                if (capturedCtrl != pb)
                {
                    return;
                }

                var rect = pb.GetRenderArea();
                var mouse = this.Game.Input.MouseX - rect.Left;

                if (pb == pbOctaves)
                {
                    pb.ProgressValue = (float)Math.Round(mouse / rect.Width, 1);
                }
                else
                {
                    pb.ProgressValue = mouse / rect.Width;
                }
            }
        }
        private void PbJustPressed(object sender, EventArgs e)
        {
            if (sender is UIControl control)
            {
                capturedCtrl = control;
            }
        }
        private void PbJustReleased(object sender, EventArgs e)
        {
            capturedCtrl = null;
        }
        private void BtnSave_JustReleased(object sender, EventArgs e)
        {
            if (noiseMap == null)
            {
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.DefaultExt = ".png";
                dlg.FileName = "Noisemap.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    noiseMap.SaveMapToFile(dlg.FileName);
                }
            }
        }
        private void BtnExit_JustReleased(object sender, EventArgs e)
        {
            this.Game.SetScene<StartScene>();
        }

        private bool GenerateMap()
        {
            if (generatingMap)
            {
                return false;
            }

            generatingMap = true;

            Task.Run(() =>
            {
                try
                {
                    NoiseMapDescriptor nmDesc = new NoiseMapDescriptor
                    {
                        MapWidth = mapSize,
                        MapHeight = mapSize,
                        Scale = mapScale,
                        Lacunarity = mapLacunarity,
                        Persistance = mapPersistance,
                        Octaves = mapOctaves,
                        Offset = mapOffset,
                        Seed = mapSeed,
                    };

                    noiseMap = NoiseMap.CreateNoiseMap(nmDesc);

                    noiseMapDirty = false;
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex.Message);
                }

                generatingMap = false;
            });

            return true;
        }
    }
}
