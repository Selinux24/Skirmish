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

            var font = TextDrawerDescription.FromFamily(fontFamily, 16);
            var butDesc = UIButtonDescription.DefaultTwoStateButton(bColor1, bColor2);
            butDesc.Font = font;

            backGround = await this.AddComponentUIPanel("backGround", UIPanelDescription.Screen(this, Color.SandyBrown));
            btnExit = await this.AddComponentUIButton("btnExit", butDesc);

            txtScale = await this.AddComponentUITextArea("txtScale", UITextAreaDescription.DefaultFromFamily(fontFamily, 16));
            txtLacunarity = await this.AddComponentUITextArea("txtLacunarity", UITextAreaDescription.DefaultFromFamily(fontFamily, 16));
            txtPersistance = await this.AddComponentUITextArea("txtPersistance", UITextAreaDescription.DefaultFromFamily(fontFamily, 16));
            txtOctaves = await this.AddComponentUITextArea("txtOctaves", UITextAreaDescription.DefaultFromFamily(fontFamily, 16));
            txtHelpOffset = await this.AddComponentUITextArea("txtHelpOffset", UITextAreaDescription.DefaultFromFamily(fontFamily, 12));
            txtOffset = await this.AddComponentUITextArea("txtOffset", UITextAreaDescription.DefaultFromFamily(fontFamily, 14));
            txtHelpSeed = await this.AddComponentUITextArea("txtHelpSeed", UITextAreaDescription.DefaultFromFamily(fontFamily, 12));
            txtSeed = await this.AddComponentUITextArea("txtSeed", UITextAreaDescription.DefaultFromFamily(fontFamily, 14));

            pbScale = await this.AddComponentUIProgressBar("pbScale", UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbLacunarity = await this.AddComponentUIProgressBar("pbLacunarity", UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbPersistance = await this.AddComponentUIProgressBar("pbPersistance", UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbOctaves = await this.AddComponentUIProgressBar("pbOctaves", UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));

            btnSave = await this.AddComponentUIButton("btnSave", butDesc);

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
            texture = Game.ResourceManager.RequestResource(Guid.NewGuid(), new Color4[] { }, mapSize, true);

            perlinRenderer = await this.AddComponentUITextureRenderer("perlinRenderer", UITextureRendererDescription.Default());
            perlinRenderer.Texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<StartScene>();
            }

            if (UpdateInput(gameTime))
            {
                noiseMapDirty = true;
            }

            if (UpdateUI())
            {
                noiseMapDirty = true;
            }

            texture.Update(Game, noiseMap?.CreateColors());

            if (noiseMapDirty)
            {
                GenerateMap();
            }
        }
        private bool UpdateInput(GameTime gameTime)
        {
            bool updateMap = false;

            float delta = gameTime.ElapsedSeconds;

            if (Game.Input.KeyPressed(Keys.W))
            {
                mapOffset.Y -= delta;
                updateMap = true;
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                mapOffset.Y += delta;
                updateMap = true;
            }

            if (Game.Input.KeyPressed(Keys.A))
            {
                mapOffset.X -= delta;
                updateMap = true;
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                mapOffset.X += delta;
                updateMap = true;
            }

            mapOffset.X = Math.Max(mapOffset.X, 1f);
            mapOffset.Y = Math.Max(mapOffset.Y, 1f);

            if (Game.Input.KeyJustPressed(Keys.X))
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
            float size = Math.Min(Game.Form.RenderHeight, Game.Form.RenderWidth) * 0.8f;

            perlinRenderer.Width = size;
            perlinRenderer.Height = size;
            perlinRenderer.Anchor = Anchors.Center;
        }
        public void ResizeUI()
        {
            backGround.Width = Game.Form.RenderWidth;
            backGround.Height = Game.Form.RenderHeight;

            float perlinRendererSize = Math.Min(Game.Form.RenderHeight, Game.Form.RenderWidth) * 0.8f;

            float borderSize = (Game.Form.RenderWidth - perlinRendererSize) * 0.5f;

            float marginLeft = 25;
            float marginTop = Game.Form.RenderHeight * 0.1f;
            float separation = 20;
            float width = borderSize - (marginLeft * 2);
            float height = 15;

            int lineIndex = 0;

            btnExit.SetPosition(Game.Form.RenderWidth - 30, 0);
            btnExit.Width = 30;
            btnExit.Height = 30;
            btnExit.Caption.Text = "X";
            btnExit.Caption.TextHorizontalAlign = HorizontalTextAlign.Center;
            btnExit.Caption.TextVerticalAlign = VerticalTextAlign.Middle;

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
            txtHelpOffset.GrowControlWithText = false;
            txtHelpOffset.Width = width;
            txtHelpOffset.Height = 0;
            txtHelpOffset.Text = "Use W A S D keys to displace the Noise map";

            txtOffset.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            lineIndex++;

            txtHelpSeed.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtHelpSeed.GrowControlWithText = false;
            txtHelpSeed.Width = width;
            txtHelpSeed.Height = 0;
            txtHelpSeed.Text = "Use X key to change the seed";

            txtSeed.SetPosition(marginLeft, marginTop + (separation * lineIndex));

            btnSave.Width = 200;
            btnSave.Height = 50;
            btnSave.SetPosition(Game.Form.RenderCenter.X + (perlinRendererSize / 2) - btnSave.Width, Game.Form.RenderCenter.Y + (perlinRendererSize / 2));
            btnSave.Caption.TextHorizontalAlign = HorizontalTextAlign.Center;
            btnSave.Caption.TextVerticalAlign = VerticalTextAlign.Middle;
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

                var rect = pb.GetRenderArea(true);
                var mouse = Game.Input.MouseX - rect.Left;

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
            Game.SetScene<StartScene>();
        }

        private void GenerateMap()
        {
            if (generatingMap)
            {
                return;
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
                    Logger.WriteError(this, ex);
                }

                generatingMap = false;
            });
        }
    }
}
