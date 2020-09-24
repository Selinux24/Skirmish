using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
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

        private IAudioEffect currentMusic = null;
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

            await LoadUserInteface();
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

        private async Task LoadUserInteface()
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            await LoadResourcesAsync(
                new[]
                {
                    InitializeAudio(),
                    InitializeBackGround(),
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    SetBackground();

                    Camera.Position = Vector3.BackwardLH * 8f;
                    Camera.Interest = Vector3.Zero;

                    PlayAudio();

                    AudioManager.MasterVolume = 1;
                    AudioManager.Start();

                    LoadGameAssets();
                });
        }
        private async Task InitializeBackGround()
        {
            var backGroundDesc = ModelDescription.FromXml("Background", "Resources/SceneStart", "SkyPlane.xml");
            backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);
        }
        private async Task InitializeAudio()
        {
            //Sounds
            for (int i = 0; i < musicList.Length; i++)
            {
                AudioManager.LoadSound($"Music{i}", "Resources/Common", musicList[i]);
            }

            AudioManager.LoadSound("push", "Resources/Common", "push.wav");

            //Effects
            for (int i = 0; i < musicList.Length; i++)
            {
                AudioManager.AddEffectParams(
                    $"Music{i}",
                    new GameAudioEffectParameters
                    {
                        DestroyWhenFinished = false,
                        SoundName = $"Music{i}",
                        IsLooped = true,
                        UseAudio3D = true,
                    });
            }

            AudioManager.AddEffectParams(
                "push",
                new GameAudioEffectParameters
                {
                    SoundName = "push",
                });

            await Task.CompletedTask;
        }
        private void SetBackground()
        {
            backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);
        }

        private void LoadGameAssets()
        {
            _ = LoadResourcesAsync(
                new[]
                {
                    InitializeCursor(),
                    InitializeControls(),
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    SetControlPositions();

                    gameReady = true;
                });
        }
        private async Task InitializeCursor()
        {
            await this.AddComponentUICursor(UICursorDescription.Default("Resources/Common/pointer.png", 36, 36), layerCursor);
        }
        private async Task InitializeControls()
        {
            // Title text
            var titleDesc = UITextAreaDescription.FromFamily("Viner Hand ITC", 90, FontMapStyles.Bold);
            titleDesc.Name = "Title";
            titleDesc.Font.ForeColor = Color.IndianRed;
            titleDesc.Font.ShadowColor = new Color4(Color.Brown.RGB(), 0.25f);
            titleDesc.Font.ShadowDelta = new Vector2(4, 4);

            title = await this.AddComponentUITextArea(titleDesc, layerHUD);

            // Font description
            var buttonFont = TextDrawerDescription.FromFile("common/HelveticaNeueHv.ttf", 16, Color.Gold);
            buttonFont.HorizontalAlign = HorizontalTextAlign.Center;
            buttonFont.VerticalAlign = VerticalTextAlign.Middle;

            // Buttons
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton("common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f, UITextAreaDescription.Default(buttonFont));
            buttonDesc.Name = "Scene buttons";
            buttonDesc.Width = 200;
            buttonDesc.Height = 36;
            buttonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            buttonDesc.ColorReleased = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);

            sceneDungeonWallButton = await this.AddComponentUIButton(buttonDesc, layerHUD);
            sceneNavMeshTestButton = await this.AddComponentUIButton(buttonDesc, layerHUD);
            sceneDungeonButton = await this.AddComponentUIButton(buttonDesc, layerHUD);
            sceneModularDungeonButton = await this.AddComponentUIButton(buttonDesc, layerHUD);

            // Exit button
            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton("common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f, UITextAreaDescription.Default(buttonFont));
            exitButtonDesc.Name = "Exit button";
            exitButtonDesc.Width = 200;
            exitButtonDesc.Height = 36;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);

            exitButton = await this.AddComponentUIButton(exitButtonDesc, layerHUD);

            // Description text
            var tooltipDesc = UITextAreaDescription.FromFile("common/HelveticaNeue Medium.ttf", 12);
            tooltipDesc.Name = "Tooltip";
            tooltipDesc.Font.ForeColor = Color.LightGray;
            tooltipDesc.Width = 250;

            description = await this.AddComponentUITextArea(tooltipDesc, layerHUD);
        }
        private void SetControlPositions()
        {
            long tweenTime = 1000;
            long tweenInc = 250;

            title.Text = "Collada Loader Test";
            title.CenterHorizontally = CenterTargets.Screen;
            title.Top = Game.Form.RenderHeight * 0.25f;
            title.Show(2000);
            title.ScaleInScaleOut(1f, 1.05f, 10000);

            sceneDungeonWallButton.Caption.Text = "Dungeon Wall";
            sceneDungeonWallButton.TooltipText = "Shows a basic normal map scene demo.";
            sceneDungeonWallButton.Left = ((Game.Form.RenderWidth / 6) * 1) - (sceneDungeonWallButton.Width / 2);
            sceneDungeonWallButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneDungeonWallButton.Height / 2);
            sceneDungeonWallButton.JustReleased += SceneButtonClick;
            sceneDungeonWallButton.MouseOver += SceneButtonOver;
            sceneDungeonWallButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            sceneNavMeshTestButton.Caption.Text = "Navmesh Test";
            sceneNavMeshTestButton.TooltipText = "Shows a navigation mesh scene demo.";
            sceneNavMeshTestButton.Left = ((Game.Form.RenderWidth / 6) * 2) - (sceneNavMeshTestButton.Width / 2);
            sceneNavMeshTestButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneNavMeshTestButton.Height / 2);
            sceneNavMeshTestButton.JustReleased += SceneButtonClick;
            sceneNavMeshTestButton.MouseOver += SceneButtonOver;
            sceneNavMeshTestButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            sceneDungeonButton.Caption.Text = "Dungeon";
            sceneDungeonButton.TooltipText = "Shows a basic dungeon from a unique mesh scene demo.";
            sceneDungeonButton.Left = ((Game.Form.RenderWidth / 6) * 3) - (sceneDungeonButton.Width / 2);
            sceneDungeonButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneDungeonButton.Height / 2);
            sceneDungeonButton.JustReleased += SceneButtonClick;
            sceneDungeonButton.MouseOver += SceneButtonOver;
            sceneDungeonButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            sceneModularDungeonButton.Caption.Text = "Modular Dungeon";
            sceneModularDungeonButton.TooltipText = "Shows a modular dungeon scene demo.";
            sceneModularDungeonButton.Left = ((Game.Form.RenderWidth / 6) * 4) - (sceneModularDungeonButton.Width / 2);
            sceneModularDungeonButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneModularDungeonButton.Height / 2);
            sceneModularDungeonButton.JustReleased += SceneButtonClick;
            sceneModularDungeonButton.MouseOver += SceneButtonOver;
            sceneModularDungeonButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            exitButton.Caption.Text = "Exit";
            exitButton.TooltipText = "Closes the application.";
            exitButton.Left = (Game.Form.RenderWidth / 6) * 5 - (exitButton.Width / 2);
            exitButton.Top = (Game.Form.RenderHeight / 4) * 3 - (exitButton.Height / 2);
            exitButton.JustReleased += ExitButtonClick;
            exitButton.MouseOver += SceneButtonOver;
            exitButton.Show(tweenTime, tweenTime);

            sceneButtons = new[]
            {
                sceneDungeonWallButton,
                sceneNavMeshTestButton,
                sceneDungeonButton,
                sceneModularDungeonButton,
                exitButton,
            };
        }

        private void UpdateAudioInput(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.L))
            {
                AudioManager.UseMasteringLimiter = !AudioManager.UseMasteringLimiter;
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                currentMusic.SetReverb(null);
            }
            if (Game.Input.KeyJustReleased(Keys.H))
            {
                currentMusic.SetReverb(ReverbPresets.Forest);
            }

            if (Game.Input.KeyPressed(Keys.Subtract))
            {
                AudioManager.MasterVolume -= gameTime.ElapsedSeconds / 10;
            }

            if (Game.Input.KeyPressed(Keys.Add))
            {
                AudioManager.MasterVolume += gameTime.ElapsedSeconds / 10;
            }

            if (Game.Input.KeyPressed(Keys.ControlKey))
            {
                UpdateAudioPan();
            }

            if (Game.Input.KeyJustReleased(Keys.Z))
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

            if (Game.Input.KeyJustReleased(Keys.Left))
            {
                currentMusic.Pan = -1;
            }
            else if (Game.Input.KeyJustReleased(Keys.Right))
            {
                currentMusic.Pan = 1;
            }
            else if (Game.Input.KeyJustReleased(Keys.Down))
            {
                currentMusic.Pan = 0;
            }
        }
        private void UpdateListenerInput(GameTime gameTime)
        {
            bool shift = Game.Input.KeyPressed(Keys.Shift);

            var agentPosition = shift ? emitterPosition : listenerPosition;

            if (Game.Input.KeyPressed(Keys.W))
            {
                agentPosition.MoveForward(gameTime);
            }
            if (Game.Input.KeyPressed(Keys.S))
            {
                agentPosition.MoveBackward(gameTime);
            }
            if (Game.Input.KeyPressed(Keys.A))
            {
                agentPosition.MoveRight(gameTime);
            }
            if (Game.Input.KeyPressed(Keys.D))
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
            var effect = AudioManager.CreateEffectInstance("push");

            HideAllButButton(sender as UIButton, (long)effect.Duration.TotalMilliseconds);

            Task.Run(async () =>
            {
                effect.Play();

                await Task.Delay(effect.Duration);

                if (sender == sceneDungeonWallButton)
                {
                    Game.SetScene<SceneDungeonWall>();
                }
                else if (sender == sceneNavMeshTestButton)
                {
                    Game.SetScene<SceneNavmeshTest>();
                }
                else if (sender == sceneDungeonButton)
                {
                    Game.SetScene<SceneDungeon>();
                }
                else if (sender == sceneModularDungeonButton)
                {
                    Game.SetScene<SceneModularDungeon>(SceneModes.DeferredLightning);
                }
            });
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            var effect = AudioManager.CreateEffectInstance("push");

            HideAllButButton(sender as UIButton, (long)effect.Duration.TotalMilliseconds);

            Task.Run(async () =>
            {
                effect.Play();

                await Task.Delay(effect.Duration);

                Game.Exit();
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
            currentMusic = AudioManager.CreateEffectInstance(musicName, emitterPosition, listenerPosition);
            if (currentMusic != null)
            {
                currentMusic.LoopEnd += AudioManager_LoopEnd;
                currentMusic.Play();
                currentMusic.TweenVolumeUp(15000, ScaleFuncs.Linear);
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
