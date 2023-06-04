using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Collada.Start
{
    class SceneStart : Scene
    {
        private const int layerHUD = 50;
        private const int layerCursor = 100;

        private readonly string resourcesFolder = "start/resources";
        private readonly string titleFontFamily = "Viner Hand ITC, Microsoft Sans Serif";
        private readonly string mediumControlsFont = "HelveticaNeueHv.ttf";
        private readonly string largeControlsFont = "HelveticaNeue Medium.ttf";

        private Model backGround = null;
        private UITextArea title = null;
        private UIButton sceneDungeonWallButton = null;
        private UIButton sceneNavMeshTestButton = null;
        private UIButton sceneDungeonButton = null;
        private UIButton sceneModularDungeonButton = null;
        private UIButton exitButton = null;
        private UIButton[] sceneButtons = null;
        private UITextArea description = null;
        private UITabPanel modularDungeonTabs = null;

        private readonly Color sceneButtonColor = Color.RosyBrown;
        private Color4 SceneButtonColorBase { get { return new Color4(sceneButtonColor.RGB(), 0.8f); } }
        private Color4 SceneButtonColorHighlight { get { return new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f); } }

        private readonly Color exitButtonColor = Color.OrangeRed;
        private Color4 ExitButtonColorBase { get { return new Color4(exitButtonColor.RGB(), 0.8f); } }
        private Color4 ExitButtonColorHighlight { get { return new Color4(exitButtonColor.RGB() * 1.2f, 0.9f); } }

        private IAudioEffect currentMusic = null;
        private readonly string[] musicList = new string[]
        {
            "Electro_1.wav",
            "HipHoppy_1.wav",
        };
        private int musicIndex = 0;
        private int musicLoops = 0;
        private bool musicFadingOff = false;
        private readonly int musicFadingMs = 10000;
        private readonly float musicVolume = 0.1f;

        private readonly Manipulator3D emitterPosition = new Manipulator3D();
        private readonly Manipulator3D listenerPosition = new Manipulator3D();

        private bool gameReady = false;

        public SceneStart(Game game) : base(game)
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            LoadUserInteface();
        }

        private void LoadUserInteface()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeAudio(),
                    InitializeBackGround()
                },
                LoadUserIntefaceCompleted);
        }
        private async Task InitializeBackGround()
        {
            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile(resourcesFolder, "SkyPlane.json"),
            };

            backGround = await AddComponentUI<Model, ModelDescription>("Background", "Background", backGroundDesc);
        }
        private async Task InitializeAudio()
        {
            //Sounds
            for (int i = 0; i < musicList.Length; i++)
            {
                AudioManager.LoadSound($"Music{i}", resourcesFolder, musicList[i]);
            }

            AudioManager.LoadSound("push", resourcesFolder, "push.wav");

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
        private void LoadUserIntefaceCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Camera.Position = Vector3.BackwardLH * 8f;
            Camera.Interest = Vector3.Zero;

            SetBackground();

            Renderer.ClearPostProcessingEffects();

            PlayAudio();

            AudioManager.MasterVolume = 1f;
            AudioManager.Start();

            LoadGameAssets();
        }
        private void SetBackground()
        {
            backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f, true);

            // Add a light
            Vector3 lightPosition = (Camera.Position + Vector3.Left) * 10f;
            Vector3 lightDirection = Vector3.ForwardLH;
            float lightIntensity = Vector3.Distance(lightPosition, backGround.Manipulator.Position) * 2f;
            var lightDesc = SceneLightSpotDescription.Create(lightPosition, lightDirection, 30, lightIntensity, lightIntensity * 2);
            Lights.Add(new SceneLightSpot("toBackGround", false, Color3.White, Color3.White, true, lightDesc));
        }

        private void LoadGameAssets()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeCursor(),
                    InitializeControls(),
                    InitializeModularDungeonTabs()
                },
                LoadGameAssetsCompleted);
        }
        private async Task InitializeCursor()
        {
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", UICursorDescription.Default(Path.Combine(resourcesFolder, "pointer.png"), 36, 36), layerCursor);
        }
        private async Task InitializeControls()
        {
            // Title text
            var titleFont = TextDrawerDescription.FromFamily(titleFontFamily, 90, FontMapStyles.Bold, true);

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.TextForeColor = Color.IndianRed;
            titleDesc.TextShadowColor = new Color4(Color.Brown.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc, layerHUD);

            // Font description
            var buttonFont = TextDrawerDescription.FromFile(Path.Combine(resourcesFolder, mediumControlsFont), 16);

            // Buttons
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton(buttonFont, Path.Combine(resourcesFolder, "buttons.png"), new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            buttonDesc.Width = 200;
            buttonDesc.Height = 36;
            buttonDesc.ColorReleased = SceneButtonColorBase;
            buttonDesc.ColorPressed = SceneButtonColorHighlight;
            buttonDesc.TextForeColor = Color.Gold;
            buttonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            buttonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            sceneDungeonWallButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonDungeonWall", "ButtonDungeonWall", buttonDesc, layerHUD);
            sceneDungeonWallButton.Visible = false;
            sceneNavMeshTestButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonNavMeshTest", "ButtonNavMeshTest", buttonDesc, layerHUD);
            sceneNavMeshTestButton.Visible = false;
            sceneDungeonButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonDungeon", "ButtonDungeon", buttonDesc, layerHUD);
            sceneDungeonButton.Visible = false;
            sceneModularDungeonButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonModularDungeon", "ButtonModularDungeon", buttonDesc, layerHUD);
            sceneModularDungeonButton.Visible = false;

            // Exit button
            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonFont, Path.Combine(resourcesFolder, "buttons.png"), new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            exitButtonDesc.Width = 200;
            exitButtonDesc.Height = 36;
            exitButtonDesc.ColorReleased = ExitButtonColorBase;
            exitButtonDesc.ColorPressed = ExitButtonColorHighlight;
            exitButtonDesc.TextForeColor = Color.Gold;
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            exitButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonExit", "ButtonExit", exitButtonDesc, layerHUD);
            exitButton.Visible = false;

            // Description text
            var tooltipFont = TextDrawerDescription.FromFile(Path.Combine(resourcesFolder, largeControlsFont), 12);

            var tooltipDesc = UITextAreaDescription.Default(tooltipFont);
            tooltipDesc.TextForeColor = Color.LightGray;
            tooltipDesc.Width = 250;
            tooltipDesc.EventsEnabled = false;

            description = await AddComponentUI<UITextArea, UITextAreaDescription>("Tooltip", "Tooltip", tooltipDesc, layerHUD);
            description.Visible = false;
        }
        private async Task InitializeModularDungeonTabs()
        {
            List<string> tabButtons = new List<string>();
            int basicIndex = -1;
            int backIndex = -1;

            string[] mapFiles = Directory.GetFiles("modulardungeon/resources/onepagedungeons", "*.json");
            tabButtons.AddRange(mapFiles.Select(m =>
            {
                string name = Path.GetFileNameWithoutExtension(m).Replace("_", " ");
                return string.Concat(name.First().ToString().ToUpper(), name.AsSpan(1));
            }));
            basicIndex = tabButtons.Count;
            tabButtons.Add("Basic Dungeon");
            backIndex = tabButtons.Count;
            tabButtons.Add("Back");

            var largeFont = TextDrawerDescription.FromFile(Path.Combine(resourcesFolder, largeControlsFont), 72);
            var mediumFont = TextDrawerDescription.FromFile(Path.Combine(resourcesFolder, mediumControlsFont), 12);
            var mediumClickFont = TextDrawerDescription.FromFile(Path.Combine(resourcesFolder, mediumControlsFont), 12);

            var desc = UITabPanelDescription.Default(tabButtons.ToArray(), Color.Transparent, SceneButtonColorBase, SceneButtonColorHighlight);

            desc.ButtonDescription.Font = mediumFont;
            desc.ButtonDescription.TextForeColor = Color.LightGoldenrodYellow;
            desc.ButtonDescription.TextHorizontalAlign = TextHorizontalAlign.Center;
            desc.ButtonDescription.TextVerticalAlign = TextVerticalAlign.Middle;

            desc.TabButtonsAreaSize *= 1.5f;
            desc.TabButtonsSpacing = new Spacing() { Horizontal = 10f };
            desc.TabButtonsPadding = new Padding() { Bottom = 0, Left = 5, Right = 5, Top = 5 };
            desc.TabButtonPadding = 5;

            desc.TabPanelsPadding = new Padding() { Bottom = 5, Left = 5, Right = 5, Top = 0 };
            desc.TabPanelPadding = 2;

            modularDungeonTabs = await AddComponentUI<UITabPanel, UITabPanelDescription>("ModularDungeonTabs", "ModularDungeonTabs", desc, layerHUD + 1);
            modularDungeonTabs.Visible = false;

            for (int i = 0; i < mapFiles.Length; i++)
            {
                string mapFile = mapFiles[i];
                string mapTexture = Path.ChangeExtension(mapFile, ".png");

                var buttonDesc = UIButtonDescription.Default(mediumClickFont, mapTexture);
                buttonDesc.Text = "Click image to load...";
                buttonDesc.TextForeColor = Color.DarkGray;
                buttonDesc.TextHorizontalAlign = TextHorizontalAlign.Right;
                buttonDesc.TextVerticalAlign = TextVerticalAlign.Bottom;
                var button = await CreateComponent<UIButton, UIButtonDescription>($"ModularDungeonTabs.Button_{i}", $"ModularDungeonTabs.Button_{i}", buttonDesc);
                button.MouseClick += (s, o) =>
                {
                    if (o.Buttons.HasFlag(MouseButtons.Left))
                    {
                        Game.SetScene(new ModularDungeon.SceneModularDungeon(Game, true, Path.GetFileName(mapFile), Path.GetFileName(mapTexture)), SceneModes.DeferredLightning);
                    }
                };

                modularDungeonTabs.TabPanels[i].AddChild(button);
            }

            var buttonBasicDesc = UIButtonDescription.Default(largeFont, "modulardungeon/resources/basicdungeon/basicdungeon.png");
            buttonBasicDesc.Text = "Basic Dungeon";
            buttonBasicDesc.TextForeColor = Color.Gold;
            buttonBasicDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            buttonBasicDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            var buttonBasic = await CreateComponent<UIButton, UIButtonDescription>("ModularDungeonTabs.ButtonBasicDungeon", "ModularDungeonTabs.ButtonBasicDungeon", buttonBasicDesc);
            buttonBasic.MouseClick += (s, o) =>
            {
                if (o.Buttons.HasFlag(MouseButtons.Left))
                {
                    Game.SetScene(new ModularDungeon.SceneModularDungeon(Game, false, "basicdungeon", null), SceneModes.DeferredLightning);
                }
            };
            modularDungeonTabs.TabPanels[basicIndex].AddChild(buttonBasic);

            var backButton = modularDungeonTabs.TabButtons[backIndex];
            backButton.MouseClick += (s, o) =>
            {
                if (o.Buttons.HasFlag(MouseButtons.Left))
                {
                    modularDungeonTabs.Hide(100);
                    ShowAllButButton(100);
                }
            };
        }
        private void LoadGameAssetsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            SetControlPositions();

            gameReady = true;
        }
        private void SetControlPositions()
        {
            long tweenTime = 1000;
            long tweenInc = 250;

            title.Text = $"Collada{Environment.NewLine}Loader Test";
            title.Show(2000);
            title.ScaleInScaleOut(1f, 1.05f, 10000);

            sceneDungeonWallButton.Caption.Text = "Dungeon Wall";
            sceneDungeonWallButton.TooltipText = "Shows a basic normal map scene demo.";
            sceneDungeonWallButton.MouseClick += SceneButtonClick;
            sceneDungeonWallButton.MouseOver += SceneButtonOver;
            sceneDungeonWallButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            sceneNavMeshTestButton.Caption.Text = "Navmesh Test";
            sceneNavMeshTestButton.TooltipText = "Shows a navigation mesh scene demo.";
            sceneNavMeshTestButton.MouseClick += SceneButtonClick;
            sceneNavMeshTestButton.MouseOver += SceneButtonOver;
            sceneNavMeshTestButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            sceneDungeonButton.Caption.Text = "Dungeon";
            sceneDungeonButton.TooltipText = "Shows a basic dungeon from a unique mesh scene demo.";
            sceneDungeonButton.MouseClick += SceneButtonClick;
            sceneDungeonButton.MouseOver += SceneButtonOver;
            sceneDungeonButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            sceneModularDungeonButton.Caption.Text = "Modular Dungeon";
            sceneModularDungeonButton.TooltipText = "Shows a modular dungeon scene demo.";
            sceneModularDungeonButton.MouseClick += SceneButtonClick;
            sceneModularDungeonButton.MouseOver += SceneButtonOver;
            sceneModularDungeonButton.Show(tweenTime, tweenTime);
            tweenTime += tweenInc;

            exitButton.Caption.Text = "Exit";
            exitButton.TooltipText = "Closes the application.";
            exitButton.MouseClick += ExitButtonClick;
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

            UpdateLayout();
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.TextHorizontalAlign = TextHorizontalAlign.Center;
            title.Top = Game.Form.RenderHeight * 0.25f;

            sceneDungeonWallButton.Left = ((Game.Form.RenderWidth / 6) * 1) - (sceneDungeonWallButton.Width / 2);
            sceneDungeonWallButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneDungeonWallButton.Height / 2);

            sceneNavMeshTestButton.Left = ((Game.Form.RenderWidth / 6) * 2) - (sceneNavMeshTestButton.Width / 2);
            sceneNavMeshTestButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneNavMeshTestButton.Height / 2);

            sceneDungeonButton.Left = ((Game.Form.RenderWidth / 6) * 3) - (sceneDungeonButton.Width / 2);
            sceneDungeonButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneDungeonButton.Height / 2);

            sceneModularDungeonButton.Left = ((Game.Form.RenderWidth / 6) * 4) - (sceneModularDungeonButton.Width / 2);
            sceneModularDungeonButton.Top = (Game.Form.RenderHeight / 4) * 3 - (sceneModularDungeonButton.Height / 2);

            exitButton.Left = (Game.Form.RenderWidth / 6) * 5 - (exitButton.Width / 2);
            exitButton.Top = (Game.Form.RenderHeight / 4) * 3 - (exitButton.Height / 2);

            modularDungeonTabs.Height = Game.Form.RenderHeight * 0.9f;
            modularDungeonTabs.Width = modularDungeonTabs.Height * 1.4f;
            modularDungeonTabs.Top = (Game.Form.RenderHeight - modularDungeonTabs.Height) * 0.5f;
            modularDungeonTabs.Left = (Game.Form.RenderWidth - modularDungeonTabs.Width) * 0.5f;
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

        private void SceneButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (sender is UIButton button)
            {
                var effect = AudioManager.CreateEffectInstance("push");
                long effectDuration = (long)(effect?.Duration.TotalMilliseconds ?? 100);

                HideAllButButton(button, effectDuration);

                Task.Run(async () =>
                {
                    effect.Play();

                    await Task.Delay(effect.Duration);

                    if (sender == sceneDungeonWallButton)
                    {
                        Game.SetScene<DungeonWall.SceneDungeonWall>();
                    }
                    else if (sender == sceneNavMeshTestButton)
                    {
                        Game.SetScene<NavmeshTest.SceneNavmeshTest>();
                    }
                    else if (sender == sceneDungeonButton)
                    {
                        Game.SetScene<Dungeon.SceneDungeon>();
                    }
                    else if (sender == sceneModularDungeonButton)
                    {
                        button.ClearTween();
                        button.Hide(effectDuration);
                        modularDungeonTabs.SetSelectedTab(0);
                        modularDungeonTabs.Show(100);
                    }
                });
            }
        }
        private void ExitButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            var effect = AudioManager.CreateEffectInstance("push");

            HideAllButButton(sender as UIButton, (long)effect.Duration.TotalMilliseconds);

            Task.Run(async () =>
            {
                effect.Play();

                await Task.Delay(effect.Duration);

                Game.Exit();
            });
        }
        private void ShowAllButButton(long milliseconds)
        {
            foreach (var but in sceneButtons)
            {
                but.ClearTween();
                but.Show(milliseconds);
            }
        }
        private void HideAllButButton(IUIControl button, long milliseconds)
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

        private void SceneButtonOver(IUIControl sender, MouseEventArgs e)
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
                currentMusic.PlayProgress += AudioManager_PlayProgress;
                currentMusic.Volume = 0;
                currentMusic.Play();
                currentMusic.TweenVolume(0, musicVolume, musicFadingMs, ScaleFuncs.Linear);
                musicFadingOff = false;
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
        private void AudioManager_PlayProgress(object sender, GameAudioProgressEventArgs e)
        {
            if (sender is IAudioEffect effect)
            {
                if (musicFadingOff)
                {
                    return;
                }

                if (e.TimeToEnd <= TimeSpan.FromMilliseconds(musicFadingMs))
                {
                    effect.TweenVolume(effect.Volume, 0, musicFadingMs, ScaleFuncs.Linear);
                    musicFadingOff = true;
                }
            }
        }
    }
}
