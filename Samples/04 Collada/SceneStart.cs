using Engine;
using Engine.Audio;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Collada
{
    class SceneStart : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private Model backGround = null;
        private UITextArea title = null;
        private UIButton sceneDungeonWallButton = null;
        private UIButton sceneNavMeshTestButton = null;
        private UIButton sceneDungeonButton = null;
        private UIButton sceneModularDungeonButton = null;
        private UIButton exitButton = null;
        private UIButton[] sceneButtons = null;
        private UITextArea description = null;

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

        private bool gameReady = false;

        public SceneStart(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.Black;

            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            LoadUserInteface();

            await Task.CompletedTask;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            UpdateAudioInput(gameTime);
            UpdateListenerInput(gameTime);
            UpdateAudio();
        }

        private void LoadUserInteface()
        {
            _ = this.LoadResourcesAsync(
                new[]
                {
                    InitializeAudio(),
                    InitializeBackGround(),
                },
                () =>
                {
                    SetBackground();

                    this.Camera.Position = Vector3.BackwardLH * 8f;
                    this.Camera.Interest = Vector3.Zero;

                    PlayAudio();

                    this.AudioManager.MasterVolume = 0;
                    this.AudioManager.Start();

                    LoadGameAssets();
                });
        }
        private async Task InitializeBackGround()
        {
            var backGroundDesc = ModelDescription.FromXml("Background", "Resources/SceneStart", "SkyPlane.xml");
            this.backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);
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

        private void LoadGameAssets()
        {
            _ = this.LoadResourcesAsync(
                new[]
                {
                    InitializeCursor(),
                    InitializeControls(),
                },
                () =>
                {
                    SetControlPositions();

                    gameReady = true;
                });
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = new UICursorDescription()
            {
                Name = "Cursor",
                ContentPath = "Resources/Common",
                Textures = new[] { "pointer.png" },
                Height = 36,
                Width = 36,
                Centered = false,
                Color = Color.White,
            };
            await this.AddComponentUICursor(cursorDesc, layerCursor);
        }
        private async Task InitializeControls()
        {
            // Title text
            var titleDesc = new UITextAreaDescription
            {
                Width = this.Game.Form.RenderWidth,
                Height = this.Game.Form.RenderHeight,

                Font = new TextDrawerDescription()
                {
                    Name = "Title",
                    Font = "Viner Hand ITC",
                    FontSize = 90,
                    Style = FontMapStyles.Bold,
                    TextColor = Color.IndianRed,
                    ShadowColor = new Color4(Color.Brown.RGB(), 0.25f),
                    ShadowDelta = new Vector2(4, 4),
                },
            };
            this.title = await this.AddComponentUITextArea(titleDesc, layerHUD);

            // Buttons
            var buttonDesc = new UIButtonDescription()
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

                Font = new TextDrawerDescription()
                {
                    FontFileName = "common/HelveticaNeueHv.ttf",
                    FontSize = 16,
                    TextColor = Color.Gold,
                },
            };
            this.sceneDungeonWallButton = await this.AddComponentUIButton(buttonDesc, layerHUD);
            this.sceneNavMeshTestButton = await this.AddComponentUIButton(buttonDesc, layerHUD);
            this.sceneDungeonButton = await this.AddComponentUIButton(buttonDesc, layerHUD);
            this.sceneModularDungeonButton = await this.AddComponentUIButton(buttonDesc, layerHUD);

            // Exit button
            var exitButtonDesc = new UIButtonDescription()
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

                Font = new TextDrawerDescription()
                {
                    FontFileName = "common/HelveticaNeueHv.ttf",
                    FontSize = 16,
                    TextColor = Color.Gold,
                },
            };
            this.exitButton = await this.AddComponentUIButton(exitButtonDesc, layerHUD);

            // Description text
            var descriptionDesc = new UITextAreaDescription
            {
                Font = new TextDrawerDescription()
                {
                    Name = "Tooltip",
                    FontFileName = "common/HelveticaNeue Medium.ttf",
                    FontSize = 12,
                    TextColor = Color.LightGray,
                },
            };
            this.description = await this.AddComponentUITextArea(descriptionDesc, layerHUD);
        }
        private void SetControlPositions()
        {
            long tweenTime = 1000;
            long tweenInc = 250;

            this.title.Text = "Collada Loader Test";
            this.title.CenterHorizontally = CenterTargets.Screen;
            this.title.CenterVertically = CenterTargets.Screen;
            this.title.Top = this.Game.Form.RenderHeight * 0.25f;
            this.title.Show(2000);
            this.title.ScaleInScaleOut(1f, 1.05f, 10000);

            this.sceneDungeonWallButton.Text = "Dungeon Wall";
            this.sceneDungeonWallButton.TooltipText = "Shows a basic normal map scene demo.";
            this.sceneDungeonWallButton.Left = ((this.Game.Form.RenderWidth / 6) * 1) - (this.sceneDungeonWallButton.Width / 2);
            this.sceneDungeonWallButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneDungeonWallButton.Height / 2);
            this.sceneDungeonWallButton.JustReleased += SceneButtonClick;
            this.sceneDungeonWallButton.MouseOver += SceneButtonOver;
            this.sceneDungeonWallButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            this.sceneNavMeshTestButton.Text = "Navmesh Test";
            this.sceneNavMeshTestButton.TooltipText = "Shows a navigation mesh scene demo.";
            this.sceneNavMeshTestButton.Left = ((this.Game.Form.RenderWidth / 6) * 2) - (this.sceneNavMeshTestButton.Width / 2);
            this.sceneNavMeshTestButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneNavMeshTestButton.Height / 2);
            this.sceneNavMeshTestButton.JustReleased += SceneButtonClick;
            this.sceneNavMeshTestButton.MouseOver += SceneButtonOver;
            this.sceneNavMeshTestButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            this.sceneDungeonButton.Text = "Dungeon";
            this.sceneDungeonButton.TooltipText = "Shows a basic dungeon from a unique mesh scene demo.";
            this.sceneDungeonButton.Left = ((this.Game.Form.RenderWidth / 6) * 3) - (this.sceneDungeonButton.Width / 2);
            this.sceneDungeonButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneDungeonButton.Height / 2);
            this.sceneDungeonButton.JustReleased += SceneButtonClick;
            this.sceneDungeonButton.MouseOver += SceneButtonOver;
            this.sceneDungeonButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            this.sceneModularDungeonButton.Text = "Modular Dungeon";
            this.sceneModularDungeonButton.TooltipText = "Shows a modular dungeon scene demo.";
            this.sceneModularDungeonButton.Left = ((this.Game.Form.RenderWidth / 6) * 4) - (this.sceneModularDungeonButton.Width / 2);
            this.sceneModularDungeonButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneModularDungeonButton.Height / 2);
            this.sceneModularDungeonButton.JustReleased += SceneButtonClick;
            this.sceneModularDungeonButton.MouseOver += SceneButtonOver;
            this.sceneModularDungeonButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            this.exitButton.Text = "Exit";
            this.exitButton.TooltipText = "Closes the application.";
            this.exitButton.Left = (this.Game.Form.RenderWidth / 6) * 5 - (this.exitButton.Width / 2);
            this.exitButton.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.exitButton.Height / 2);
            this.exitButton.JustReleased += ExitButtonClick;
            this.exitButton.MouseOver += SceneButtonOver;
            this.exitButton.Show(tweenTime, tweenTime);

            this.sceneButtons = new[]
            {
                this.sceneDungeonWallButton,
                this.sceneNavMeshTestButton,
                this.sceneDungeonButton,
                this.sceneModularDungeonButton,
                this.exitButton,
            };
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
            var effect = this.AudioManager.CreateEffectInstance("push");

            HideAllButButton(sender as UIButton, (long)effect.Duration.TotalMilliseconds);

            Task.Run(async () =>
            {
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
            var effect = this.AudioManager.CreateEffectInstance("push");

            HideAllButButton(sender as UIButton, (long)effect.Duration.TotalMilliseconds);

            Task.Run(async () =>
            {
                effect.Play();

                await Task.Delay(effect.Duration);

                this.Game.Exit();
            });
        }
        private void HideAllButButton(UIButton button, long milliseconds)
        {
            foreach (var but in sceneButtons)
            {
                if (but == button)
                {
                    continue;
                }

                but.ClearTween();
                but.Hide(milliseconds);
            }
        }

        private void SceneButtonOver(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                description.Text = button.TooltipText;
                description.SetPosition(button.Left, button.Top + button.Height + 25);
                description.ClearTween();
                description.Hide(1000, 3f);
            }
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
