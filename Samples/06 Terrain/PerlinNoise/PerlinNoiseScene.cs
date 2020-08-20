using Engine;
using Engine.Common;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terrain.PerlinNoise
{
    using Terrain.Start;

    class PerlinNoiseScene : Scene
    {
        readonly string fontFamily = "Consolas";

        UIProgressBar pbScale;
        UIProgressBar pbLacunarity;
        UIProgressBar pbPersistance;
        UIProgressBar pbOctaves;
        UITextArea txtOffset;
        UITextArea txtSeed;

        EngineShaderResourceView texture;

        readonly int mapSize = 256;
        readonly float maxScale = 10;
        readonly float maxLacunarity = 20;
        readonly float maxPersistance = 1;
        readonly int maxOctaves = 8;

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

            Cursor.Show();

            await LoadResourcesAsync(InitializeUI());

            await LoadResourcesAsync(InitializeTextureRenderer());
        }
        public async Task InitializeUI()
        {
            var backGround = await this.AddComponentUIPanel(UIPanelDescription.Screen(this, Color.SandyBrown));

            var txtScale = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            var txtLacunarity = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            var txtPersistance = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            var txtOctaves = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 16));
            var txtHelpOffset = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 14));
            txtOffset = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 14));
            var txtHelpSeed = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 14));
            txtSeed = await this.AddComponentUITextArea(UITextAreaDescription.FromFamily(fontFamily, 14));

            pbScale = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbLacunarity = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbPersistance = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));
            pbOctaves = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFamily(fontFamily, 12));

            Color pColor = Color.DeepSkyBlue;

            float borderSize = (this.Game.Form.RenderWidth - (this.Game.Form.RenderHeight * 0.8f)) * 0.5f;

            float marginLeft = 25;
            float marginTop = this.Game.Form.RenderHeight * 0.1f;
            float separation = 20;
            float width = borderSize - (marginLeft * 2);
            float height = 15;

            int lineIndex = 0;

            txtScale.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtScale.Text = "Scale";

            pbScale.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbScale.Width = width;
            pbScale.Height = height;
            pbScale.ProgressColor = pColor;
            pbScale.ProgressValue = Scale;
            pbScale.JustPressed += PbJustPressed;
            pbScale.Pressed += PbPressed;
            pbScale.JustReleased += PbJustReleased;

            txtLacunarity.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtLacunarity.Text = "Lacunarity";

            pbLacunarity.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbLacunarity.Width = width;
            pbLacunarity.Height = height;
            pbLacunarity.ProgressColor = pColor;
            pbLacunarity.ProgressValue = Lacunarity;
            pbLacunarity.JustPressed += PbJustPressed;
            pbLacunarity.Pressed += PbPressed;
            pbLacunarity.JustReleased += PbJustReleased;

            txtPersistance.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtPersistance.Text = "Persistance";

            pbPersistance.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbPersistance.Width = width;
            pbPersistance.Height = height;
            pbPersistance.ProgressColor = pColor;
            pbPersistance.ProgressValue = Persistance;
            pbPersistance.JustPressed += PbJustPressed;
            pbPersistance.Pressed += PbPressed;
            pbPersistance.JustReleased += PbJustReleased;

            txtOctaves.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtOctaves.Text = "Octaves";

            pbOctaves.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            pbOctaves.Width = width;
            pbOctaves.Height = height;
            pbOctaves.ProgressColor = pColor;
            pbOctaves.ProgressValue = Octaves;
            pbOctaves.JustPressed += PbJustPressed;
            pbOctaves.Pressed += PbPressed;
            pbOctaves.JustReleased += PbJustReleased;

            lineIndex++;

            txtHelpOffset.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtHelpOffset.AdjustAreaWithText = false;
            txtHelpOffset.Width = width;
            txtHelpOffset.Text = "Use W A S D keys to displace the Noise map";

            txtOffset.SetPosition(marginLeft, marginTop + (separation * lineIndex++));

            lineIndex++;

            txtHelpSeed.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
            txtHelpSeed.AdjustAreaWithText = false;
            txtHelpSeed.Width = width;
            txtHelpSeed.Text = "Use X key to change the seed";

            txtSeed.SetPosition(marginLeft, marginTop + (separation * lineIndex++));
        }
        public async Task InitializeTextureRenderer()
        {
            texture = this.Game.ResourceManager.RequestResource(Guid.NewGuid(), GenerateMap(), mapSize, true);

            float size = this.Game.Form.RenderHeight * 0.8f;

            var perlinRenderer = await this.AddComponentUITextureRenderer(UITextureRendererDescription.Default(size, size));
            perlinRenderer.CenterHorizontally = CenterTargets.Screen;
            perlinRenderer.CenterVertically = CenterTargets.Screen;
            perlinRenderer.Texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<StartScene>();
            }

            bool updateMap = false;

            if (UpdateInput(gameTime))
            {
                updateMap = true;
            }

            if (UpdateUI())
            {
                updateMap = true;
            }

            if (!updateMap)
            {
                return;
            }

            mapScale = Math.Max(mapScale, 0);

            mapOffset.X = Math.Max(mapOffset.X, 1f);
            mapOffset.Y = Math.Max(mapOffset.Y, 1f);

            texture.Update(this.Game, GenerateMap());
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

            if (this.Game.Input.KeyJustPressed(Keys.X))
            {
                mapSeed += 100 + (int)(delta * 10000f);
                updateMap = true;
            }

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

                pb.ProgressValue = mouse / rect.Width;
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

        private IEnumerable<Color4> GenerateMap()
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

            return NoiseMap.CreateNoiseTexture(nmDesc);
        }
    }
}
