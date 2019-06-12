using Engine;
using Engine.Audio;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Collada
{
    class SceneStart : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        Model backGround = null;
        TextDrawer title = null;
        SpriteButton sceneDungeonWallButton = null;
        SpriteButton sceneNavMeshTestButton = null;
        SpriteButton sceneDungeonButton = null;
        SpriteButton sceneModularDungeonButton = null;
        SpriteButton exitButton = null;

        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.RosyBrown, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.OrangeRed, 1.5f);

        private GameAudioEffect pushButton = null;

        private readonly string[] audios = new string[]
        {
            "Electro_1.wav",
            "HipHoppy_1.wav",
        };
        private int audioIndex = 0;
        private int audioLoops = 0;
        private GameAudio currentAudio = null;
        private GameAudioEffectInstance currentEffectInstance = null;

        public SceneStart(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            #region Cursor

            var cursorDesc = new CursorDescription()
            {
                Name = "Cursor",
                ContentPath = "Resources/Common",
                Textures = new[] { "pointer.png" },
                Height = 36,
                Width = 36,
                Centered = false,
                Color = Color.White,
            };
            this.AddComponent<Cursor>(cursorDesc, SceneObjectUsages.UI, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = ModelDescription.FromXml("Background", "Resources/SceneStart", "SkyPlane.xml");
            this.backGround = this.AddComponent<Model>(backGroundDesc, SceneObjectUsages.UI).Instance;

            #endregion

            #region Title text

            var titleDesc = new TextDrawerDescription()
            {
                Name = "Title",
                Font = "Viner Hand ITC",
                FontSize = 90,
                Style = FontMapStyles.Bold,
                TextColor = Color.IndianRed,
                ShadowColor = new Color4(Color.Brown.RGB(), 0.55f),
                ShadowDelta = new Vector2(2, -3),
            };
            this.title = this.AddComponent<TextDrawer>(titleDesc, SceneObjectUsages.UI, layerHUD).Instance;

            #endregion

            #region Scene buttons

            var buttonDesc = new SpriteButtonDescription()
            {
                Name = "Scene buttons",

                Width = 200,
                Height = 36,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Buxton Sketch",
                    Style = FontMapStyles.Regular,
                    FontSize = 22,
                    TextColor = Color.Gold,
                }
            };
            this.sceneDungeonWallButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD).Instance;
            this.sceneNavMeshTestButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD).Instance;
            this.sceneDungeonButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD).Instance;
            this.sceneModularDungeonButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD).Instance;

            #endregion

            #region Exit button

            var exitButtonDesc = new SpriteButtonDescription()
            {
                Name = "Exit button",

                Width = 200,
                Height = 36,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Buxton Sketch",
                    Style = FontMapStyles.Bold,
                    FontSize = 22,
                    TextColor = Color.Gold,
                }
            };
            this.exitButton = this.AddComponent<SpriteButton>(exitButtonDesc, SceneObjectUsages.UI, layerHUD).Instance;

            #endregion
        }

        public override void Initialized()
        {
            base.Initialized();

            this.Camera.Position = Vector3.BackwardLH * 8f;
            this.Camera.Interest = Vector3.Zero;

            this.backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            this.title.Text = "Collada Loader Test";
            this.title.CenterHorizontally();
            this.title.Top = this.Game.Form.RenderHeight / 4;

            this.sceneDungeonWallButton.Text = "Dungeon Wall";
            this.sceneDungeonWallButton.Left = ((this.Game.Form.RenderWidth / 6) * 1) - (this.sceneDungeonWallButton.Width / 2);
            this.sceneDungeonWallButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneDungeonWallButton.Height / 2);
            this.sceneDungeonWallButton.Click += SceneButtonClick;

            this.sceneNavMeshTestButton.Text = "Navmesh Test";
            this.sceneNavMeshTestButton.Left = ((this.Game.Form.RenderWidth / 6) * 2) - (this.sceneNavMeshTestButton.Width / 2);
            this.sceneNavMeshTestButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneNavMeshTestButton.Height / 2);
            this.sceneNavMeshTestButton.Click += SceneButtonClick;

            this.sceneDungeonButton.Text = "Dungeon";
            this.sceneDungeonButton.Left = ((this.Game.Form.RenderWidth / 6) * 3) - (this.sceneDungeonButton.Width / 2);
            this.sceneDungeonButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneDungeonButton.Height / 2);
            this.sceneDungeonButton.Click += SceneButtonClick;

            this.sceneModularDungeonButton.Text = "Modular Dungeon";
            this.sceneModularDungeonButton.Left = ((this.Game.Form.RenderWidth / 6) * 4) - (this.sceneModularDungeonButton.Width / 2);
            this.sceneModularDungeonButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneModularDungeonButton.Height / 2);
            this.sceneModularDungeonButton.Click += SceneButtonClick;

            this.exitButton.Text = "Exit";
            this.exitButton.Left = (this.Game.Form.RenderWidth / 6) * 5 - (this.exitButton.Width / 2);
            this.exitButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.exitButton.Height / 2);
            this.exitButton.Click += ExitButtonClick;

            var effects = this.AudioManager.CreateAudio("effects");
            effects.MasterVolume = 0.5f;
            effects.UseMasteringLimiter = true;
            this.pushButton = this.AudioManager.CreateEffect("effects", "push", "Resources/Common", "push.wav");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateAudioInput(gameTime);

            PlayAudio();
        }
        private void UpdateAudioInput(GameTime gameTime)
        {
            if (currentAudio == null)
            {
                return;
            }

            if (this.Game.Input.KeyJustReleased(Keys.S))
            {
                if (currentEffectInstance.State == AudioState.Playing)
                {
                    currentEffectInstance.Pause();
                }
                else
                {
                    currentEffectInstance.Resume();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                currentAudio.UseMasteringLimiter = !currentAudio.UseMasteringLimiter;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                currentAudio.UseReverb = !currentAudio.UseReverb;
            }
            if (this.Game.Input.KeyJustReleased(Keys.H))
            {
                currentAudio.ReverbPreset = ReverbPresets.Default;
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                currentAudio.UseAudio3D = !currentAudio.UseAudio3D;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                currentAudio.MasterVolume -= gameTime.ElapsedSeconds / 10;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                currentAudio.MasterVolume += gameTime.ElapsedSeconds / 10;
            }
        }

        private void SceneButtonClick(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                this.pushButton.Create().Play();

                await Task.Delay(this.pushButton.Duration);

                if (sender == this.sceneDungeonWallButton)
                {
                    this.Game.SetScene<SceneDungeonWall>();
                }
                else if (sender == this.sceneNavMeshTestButton)
                {
                    this.Game.SetScene<SceneNavmeshTest>();
                }
                else if (sender == this.sceneDungeonButton)
                {
                    this.Game.SetScene<SceneDungeon>();
                }
                else if (sender == this.sceneModularDungeonButton)
                {
                    this.Game.SetScene<SceneModularDungeon>();
                }
            });
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                this.pushButton.Create().Play();

                await Task.Delay(this.pushButton.Duration);

                this.Game.Exit();
            });
        }

        private void PlayAudio()
        {
            if (currentEffectInstance != null)
            {
                return;
            }

            audioIndex++;
            audioIndex %= audios.Length;

            if (currentAudio == null)
            {
                currentAudio = this.AudioManager.CreateAudio("music");

                currentAudio.UseMasteringLimiter = true;
                currentAudio.SetMasteringLimit(8, 900);

                currentAudio.UseReverb = true;
                currentAudio.UseReverbFilter = true;

                currentAudio.MasterVolume = 0.01f;
            }

            var effect = this.AudioManager.CreateEffect("music", $"Music{audioIndex}", "Resources/Common", audios[audioIndex]);
            currentEffectInstance = effect.Create();
            currentEffectInstance.IsLooped = true;

            currentEffectInstance.AudioEnd += AudioManager_AudioEnd;
            currentEffectInstance.LoopEnd += AudioManager_LoopEnd;
            currentEffectInstance.Play();
        }

        private void AudioManager_AudioEnd(object sender, GameAudioEventArgs e)
        {
            currentEffectInstance = null;
        }
        private void AudioManager_LoopEnd(object sender, GameAudioEventArgs e)
        {
            audioLoops++;

            if (audioLoops > 4)
            {
                audioLoops = 0;

                currentEffectInstance.Stop(true);
                currentEffectInstance = null;
            }
        }
    }
}
