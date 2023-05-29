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

        IUIControl capturedCtrl = null;

        public PerlinNoiseScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await LoadResourcesAsync(InitializeUI(), (res) => { res.ThrowExceptions(); });

            await LoadResourcesAsync(InitializeTextureRenderer(), (res) => { res.ThrowExceptions(); });

            ResizeTextureRenderer();
            ResizeUI();
        }
        public async Task InitializeUI()
        {
            Color4 pBackground = Color.RosyBrown;
            Color4 bColor1 = Color.Brown;
            Color4 bColor2 = Color4.AdjustSaturation(Color.Brown, 1.5f);
            Color4 pColor = Color.DeepSkyBlue;

            var defaultFont16 = TextDrawerDescription.FromFamily(fontFamily, 16);
            var defaultFont14 = TextDrawerDescription.FromFamily(fontFamily, 14);
            var defaultFont12 = TextDrawerDescription.FromFamily(fontFamily, 12);

            var defaultText16 = UITextAreaDescription.Default(defaultFont16);
            var defaultText14 = UITextAreaDescription.Default(defaultFont14);
            var defaultText12 = UITextAreaDescription.Default(defaultFont12);

            var defaultButton = UIButtonDescription.DefaultTwoStateButton(defaultFont16, bColor1, bColor2);

            backGround = await AddComponentUI<UIPanel, UIPanelDescription>("backGround", "backGround", UIPanelDescription.Screen(this, pBackground));
            btnExit = await AddComponentUI<UIButton, UIButtonDescription>("btnExit", "Exit", defaultButton);

            txtScale = await AddComponentUI<UITextArea, UITextAreaDescription>("txtScale", "Scale", defaultText16);
            txtLacunarity = await AddComponentUI<UITextArea, UITextAreaDescription>("txtLacunarity", "Lacunarity", defaultText16);
            txtPersistance = await AddComponentUI<UITextArea, UITextAreaDescription>("txtPersistance", "Persistance", defaultText16);
            txtOctaves = await AddComponentUI<UITextArea, UITextAreaDescription>("txtOctaves", "Octaves", defaultText16);
            txtHelpOffset = await AddComponentUI<UITextArea, UITextAreaDescription>("txtHelpOffset", "HelpOffset", defaultText12);
            txtOffset = await AddComponentUI<UITextArea, UITextAreaDescription>("txtOffset", "Offset", defaultText14);
            txtHelpSeed = await AddComponentUI<UITextArea, UITextAreaDescription>("txtHelpSeed", "HelpSeed", defaultText12);
            txtSeed = await AddComponentUI<UITextArea, UITextAreaDescription>("txtSeed", "Seed", defaultText14);

            var pbFont = TextDrawerDescription.FromFamily(fontFamily, 12);

            var pbDescription = UIProgressBarDescription.Default(pbFont);

            pbScale = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("pbScale", "Scale", pbDescription);
            pbLacunarity = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("pbLacunarity", "Lacunarity", pbDescription);
            pbPersistance = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("pbPersistance", "Persistance", pbDescription);
            pbOctaves = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("pbOctaves", "Octaves", pbDescription);

            btnSave = await AddComponentUI<UIButton, UIButtonDescription>("btnSave", "Save", defaultButton);

            btnExit.MouseClick += BtnExitClick;

            txtScale.Text = "Scale";

            pbScale.ProgressColor = pColor;
            pbScale.ProgressValue = Scale;
            pbScale.EventsEnabled = true;
            pbScale.MouseJustPressed += PbJustPressed;
            pbScale.MousePressed += PbPressed;
            pbScale.MouseJustReleased += PbJustReleased;

            txtLacunarity.Text = "Lacunarity";

            pbLacunarity.ProgressColor = pColor;
            pbLacunarity.ProgressValue = Lacunarity;
            pbLacunarity.EventsEnabled = true;
            pbLacunarity.MouseJustPressed += PbJustPressed;
            pbLacunarity.MousePressed += PbPressed;
            pbLacunarity.MouseJustReleased += PbJustReleased;

            txtPersistance.Text = "Persistance";

            pbPersistance.ProgressColor = pColor;
            pbPersistance.ProgressValue = Persistance;
            pbPersistance.EventsEnabled = true;
            pbPersistance.MouseJustPressed += PbJustPressed;
            pbPersistance.MousePressed += PbPressed;
            pbPersistance.MouseJustReleased += PbJustReleased;

            txtOctaves.Text = "Octaves";

            pbOctaves.ProgressColor = pColor;
            pbOctaves.ProgressValue = Octaves;
            pbOctaves.EventsEnabled = true;
            pbOctaves.MouseJustPressed += PbJustPressed;
            pbOctaves.MousePressed += PbPressed;
            pbOctaves.MouseJustReleased += PbJustReleased;

            btnSave.MouseClick += BtnSaveClick;
        }
        public async Task InitializeTextureRenderer()
        {
            texture = await Game.ResourceManager.RequestResource(Guid.NewGuid(), Array.Empty<Color4>(), mapSize, true);

            perlinRenderer = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("perlinRenderer", "Renderer", UITextureRendererDescription.Default());
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

            texture.Update(Game, noiseMap?.MapImage.Flatten());

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
            btnExit.Caption.TextHorizontalAlign = TextHorizontalAlign.Center;
            btnExit.Caption.TextVerticalAlign = TextVerticalAlign.Middle;

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
            btnSave.Caption.TextHorizontalAlign = TextHorizontalAlign.Center;
            btnSave.Caption.TextVerticalAlign = TextVerticalAlign.Middle;
            btnSave.Caption.Text = "Save to File";
        }

        private void PbPressed(IUIControl sender, MouseEventArgs e)
        {
            if (sender is UIProgressBar pb && e.Buttons.HasFlag(MouseButtons.Left))
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
        private void PbJustPressed(IUIControl sender, MouseEventArgs e)
        {
            if (capturedCtrl != null)
            {
                return;
            }

            if (e.Buttons.HasFlag(MouseButtons.Left))
            {
                capturedCtrl = sender;
            }
        }
        private void PbJustReleased(IUIControl sender, MouseEventArgs e)
        {
            capturedCtrl = null;
        }
        private void BtnSaveClick(IUIControl sender, MouseEventArgs e)
        {
            if (noiseMap == null)
            {
                return;
            }

            if (e.Buttons.HasFlag(MouseButtons.Left))
            {
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
        }
        private void BtnExitClick(IUIControl sender, MouseEventArgs e)
        {
            if (e.Buttons.HasFlag(MouseButtons.Left))
            {
                Game.SetScene<StartScene>();
            }
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
