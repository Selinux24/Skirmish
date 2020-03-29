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

        private GameAudioEffect currentMusic = null;
        private readonly string[] musicList = new string[]
        {
            "Electro_1.wav",
            "HipHoppy_1.wav",
        };
        private int musicIndex = 0;
        private int musicLoops = 0;

        private readonly Manipulator3D emitterPosition = new Manipulator3D();
        private readonly Manipulator3D listenerPosition = new Manipulator3D();

        private bool userInterfaceInitialized = false;
        private Guid userInterfaceId = Guid.NewGuid();
        private bool gameAssetsInitialized = false;
        private bool gameAssetsInitializing = false;
        private Guid gameAssetsId = Guid.NewGuid();
        private bool gameReady = false;

        public SceneStart(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.Black;

            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            await LoadUserInteface();
        }
        public override void GameResourcesLoaded(Guid id)
        {
            if (id == userInterfaceId && !userInterfaceInitialized)
            {
                userInterfaceInitialized = true;

                SetBackground();

                this.Camera.Position = Vector3.BackwardLH * 8f;
                this.Camera.Interest = Vector3.Zero;

                PlayAudio();

                this.AudioManager.MasterVolume = 1;
                //this.AudioManager.Start();

                return;
            }

            if (id == gameAssetsId && !gameAssetsInitialized)
            {
                gameAssetsInitialized = true;

                SetControlPositions();

                gameReady = true;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!userInterfaceInitialized)
            {
                return;
            }

            if (!gameAssetsInitialized && !gameAssetsInitializing)
            {
                gameAssetsInitializing = true;

                Task.WhenAll(this.LoadGameAssets());

                return;
            }

            if (!gameReady)
            {
                return;
            }

            UpdateAudioInput(gameTime);
            UpdateListenerInput(gameTime);
            UpdateAudio();
        }

        private async Task LoadUserInteface()
        {
            this.userInterfaceInitialized = false;

            await this.LoadResourcesAsync(userInterfaceId,
                InitializeAudio(),
                InitializeBackGround());
        }
        private async Task LoadGameAssets()
        {
            gameAssetsInitialized = false;

            await this.LoadResourcesAsync(gameAssetsId,
                InitializeCursor(),
                InitializeControls());
        }
        private async Task InitializeCursor()
        {
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
            await this.AddComponentCursor(cursorDesc, SceneObjectUsages.UI, layerCursor);
        }
        private async Task InitializeBackGround()
        {
            var backGroundDesc = ModelDescription.FromXml("Background", "Resources/SceneStart", "SkyPlane.xml");
            this.backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);
        }
        private async Task InitializeControls()
        {
            //Title text
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
            this.title = await this.AddComponentTextDrawer(titleDesc, SceneObjectUsages.UI, layerHUD);

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
            this.sceneDungeonWallButton = await this.AddComponentSpriteButton(buttonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneNavMeshTestButton = await this.AddComponentSpriteButton(buttonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneDungeonButton = await this.AddComponentSpriteButton(buttonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneModularDungeonButton = await this.AddComponentSpriteButton(buttonDesc, SceneObjectUsages.UI, layerHUD);

            // Exit button
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
            this.exitButton = await this.AddComponentSpriteButton(exitButtonDesc, SceneObjectUsages.UI, layerHUD);
        }
        private async Task InitializeAudio()
        {
            //Sounds
            for (int i = 0; i < musicList.Length; i++)
            {
                this.AudioManager.LoadSound($"Music{i}", "Resources/Common", musicList[i]);
            }

            this.AudioManager.LoadSound("push", "Resources/Common", "push.wav");

            //Effects
            for (int i = 0; i < musicList.Length; i++)
            {
                this.AudioManager.AddEffectParams(
                    $"Music{i}",
                    new GameAudioEffectParameters
                    {
                        DestroyWhenFinished = false,
                        SoundName = $"Music{i}",
                        IsLooped = true,
                        UseAudio3D = true,
                    });
            }

            this.AudioManager.AddEffectParams(
                "push",
                new GameAudioEffectParameters
                {
                    SoundName = "push",
                });

            await Task.CompletedTask;
        }

        private void SetBackground()
        {
            this.backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);
        }
        private void SetControlPositions()
        {
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
        }

        private void UpdateAudioInput(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.AudioManager.UseMasteringLimiter = !this.AudioManager.UseMasteringLimiter;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                currentMusic.UseReverb = !currentMusic.UseReverb;
            }
            if (this.Game.Input.KeyJustReleased(Keys.H))
            {
                currentMusic.ReverbPreset = ReverbPresets.Forest;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                this.AudioManager.MasterVolume -= gameTime.ElapsedSeconds / 10;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                this.AudioManager.MasterVolume += gameTime.ElapsedSeconds / 10;
            }

            if (this.Game.Input.KeyPressed(Keys.ControlKey))
            {
                UpdateAudioPan();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Z))
            {
                UpdateAudioPlayback();
            }
        }
        private void UpdateAudioPlayback()
        {
            if (currentMusic == null)
            {
                return;
            }

            if (currentMusic.State == AudioState.Playing)
            {
                currentMusic.Pause();
            }
            else
            {
                currentMusic.Resume();
            }
        }
        private void UpdateAudioPan()
        {
            if (currentMusic == null)
            {
                return;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                currentMusic.Pan = -1;
            }
            else if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                currentMusic.Pan = 1;
            }
            else if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                currentMusic.Pan = 0;
            }
        }
        private void UpdateListenerInput(GameTime gameTime)
        {
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
        private void UpdateAudio()
        {
            if (currentMusic != null)
            {
                return;
            }

            PlayAudio();
        }

        private void SceneButtonClick(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var effect = this.AudioManager.CreateEffectInstance("push");
                effect.Play();

                await Task.Delay(effect.Duration);

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
                var effect = this.AudioManager.CreateEffectInstance("push");
                effect.Play();

                await Task.Delay(effect.Duration);

                this.Game.Exit();
            });
        }

        private void PlayAudio()
        {
            musicIndex++;
            musicIndex %= musicList.Length;

            string musicName = $"Music{musicIndex}";
            currentMusic = this.AudioManager.CreateEffectInstance(musicName, emitterPosition, listenerPosition);
            if (currentMusic != null)
            {
                currentMusic.LoopEnd += AudioManager_LoopEnd;
                currentMusic.Play();
            }
        }

        private void AudioManager_LoopEnd(object sender, GameAudioEventArgs e)
        {
            musicLoops++;

            if (musicLoops > 0)
            {
                musicLoops = 0;

                currentMusic.Stop(true);
                currentMusic.LoopEnd -= AudioManager_LoopEnd;
                currentMusic = null;
            }
        }
    }
}
