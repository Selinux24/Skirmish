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

        private GameAudio music = null;
        private GameAudioEffectInstance currentMusic = null;
        private readonly string[] musicList = new string[]
        {
            "Electro_1.wav",
            "HipHoppy_1.wav",
        };
        private int musicIndex = 0;
        private int musicLoops = 0;

        private GameAudioEffect pushButtonEffect = null;

        private readonly Manipulator3D emitterPosition = new Manipulator3D();
        private readonly Manipulator3D listenerPosition = new Manipulator3D();

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
            effects.MasterVolume = 0.25f;
            this.pushButtonEffect = this.AudioManager.CreateEffect("effects", "push", "Resources/Common", "push.wav");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateAudioInput(gameTime);
            UpdateListenerInput(gameTime);

            PlayAudio();
        }
        private void UpdateAudioInput(GameTime gameTime)
        {
            if (music == null)
            {
                return;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Z))
            {
                if (currentMusic.State == AudioState.Playing)
                {
                    currentMusic.Pause();
                }
                else
                {
                    currentMusic.Resume();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                music.UseMasteringLimiter = !music.UseMasteringLimiter;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                music.UseReverb = !music.UseReverb;
            }
            if (this.Game.Input.KeyJustReleased(Keys.H))
            {
                music.ReverbPreset = ReverbPresets.Forest;
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                music.UseAudio3D = !music.UseAudio3D;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                music.MasterVolume -= gameTime.ElapsedSeconds / 10;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                music.MasterVolume += gameTime.ElapsedSeconds / 10;
            }
        }
        private void UpdateListenerInput(GameTime gameTime)
        {
            if (music == null)
            {
                return;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.Shift);

            var agentPosition = shift ? emitterPosition : listenerPosition;

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                agentPosition.MoveForward(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.S))
            {
                agentPosition.MoveBackward(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.A))
            {
                agentPosition.MoveRight(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.D))
            {
                agentPosition.MoveLeft(gameTime);
            }

            agentPosition.Update(gameTime);
        }

        private void SceneButtonClick(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                this.pushButtonEffect.Create().Play();

                await Task.Delay(this.pushButtonEffect.Duration);

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
                this.pushButtonEffect.Create().Play();

                await Task.Delay(this.pushButtonEffect.Duration);

                this.Game.Exit();
            });
        }

        private void PlayAudio()
        {
            if (currentMusic != null)
            {
                return;
            }

            musicIndex++;
            musicIndex %= musicList.Length;

            if (music == null)
            {
                music = this.AudioManager.CreateAudio("music");

                music.UseMasteringLimiter = true;
                music.SetMasteringLimit(8, 900);
                music.MasterVolume = 0.01f;
                music.UseAudio3D = true;
            }

            var musicEffect = this.AudioManager.CreateEffect("music", $"Music{musicIndex}", "Resources/Common", musicList[musicIndex]);
            currentMusic = musicEffect.Create();
            currentMusic.IsLooped = true;

            currentMusic.EmitterAgent.SetManipulator(emitterPosition);
            currentMusic.ListenerAgent.SetManipulator(listenerPosition);

            currentMusic.AudioEnd += AudioManager_AudioEnd;
            currentMusic.LoopEnd += AudioManager_LoopEnd;
            currentMusic.Play();
        }

        private void AudioManager_AudioEnd(object sender, GameAudioEventArgs e)
        {
            currentMusic = null;
        }
        private void AudioManager_LoopEnd(object sender, GameAudioEventArgs e)
        {
            musicLoops++;

            if (musicLoops > 4)
            {
                musicLoops = 0;

                currentMusic.Stop(true);
                currentMusic = null;
            }
        }
    }
}
