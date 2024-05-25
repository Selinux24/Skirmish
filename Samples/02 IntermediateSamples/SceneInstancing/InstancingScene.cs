using Engine;
using Engine.Animation;
using Engine.BuiltIn.PostProcess;
using Engine.Common;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneInstancing
{
    class InstancingScene : Scene
    {
        private const string resourceFolder = "Common/";

        private const string resourceSkyboxesFolder = resourceFolder + "skyboxes/";
        private readonly string[] resourceSkyboxesFiles =
        [
            resourceSkyboxesFolder + "Sky_horiz_1_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_3_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_4_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_5_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_7_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_9_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_10_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_14_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_15_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_18_4096.jpg",
            resourceSkyboxesFolder + "Sky_horiz_22_4096.jpg",
        ];

        private const string resourceGroundFolder = resourceFolder + "gravel/";
        private const string resourceGroundDiffuseTexture = resourceGroundFolder + "gravel_01_diffuse.jpg";
        private const string resourceGroundNormalMapTexture = resourceGroundFolder + "gravel_01_normal.jpg";

        private const string resourceTreesFolder = resourceFolder + "Trees/";
        private const string resourceTreeFile = "tree.json";

        private const string resourceSoldierFolder = resourceFolder + "Soldier/";
        private const string resourceSoldierFile = "soldier_anim2.json";

        private const string resourceWallFolder = resourceFolder + "Wall/";
        private const string resourceWallFile = "wall.json";

        private BuiltInPostProcessStateTweener postProcessingTweener;

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;

        private Sprite helpPanel = null;
        private UITextArea help = null;

        private Skydom skydom = null;
        private uint skyboxesCount = 0;

        private ModelInstanced troops = null;

        private readonly int instanceBlock = 10;
        private readonly BuiltInPostProcessState postProcessState = BuiltInPostProcessState.Empty;

        private bool gameReady = false;

        public InstancingScene(Game game) : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            GameEnvironment.Background = Color.Black;
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTweener,
                    InitializeTexts,
                    InitializeSky,
                    InitializeFloor,
                    InitializeTrees,
                    InitializeTroops,
                    InitializeWall,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            postProcessingTweener = this.AddBuiltInPostProcessStateTweener();
        }
        private async Task InitializeTexts()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Arial", 18);
            var defaultFont11 = TextDrawerDescription.FromFamily("Arial", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });

            title.Text = "Instancing test";
            runtimeText.Text = "";
            info.Text = "Press F1 for Help.";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);

            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            help.Visible = false;
            helpPanel = await AddComponentUI<Sprite, SpriteDescription>("HelpPanel", "Help panel", spDesc, LayerUI - 1);
            helpPanel.Visible = false;

            Color d = Color.Gray;
            Color h = Color.White;
            help.Text = $"{d}Camera: {h}W{d}-{h}A{d}-{h}S{d}-{h}D{d}.{Environment.NewLine}" +
                $"{d}Change tone mapping using {h}Tab{d} ({h}Shift{d} reverse).{Environment.NewLine}" +
                $"{d}Change instance count using {h}Left{d} and {h}Right{d} arrows ({h}Shift{d} moves by {instanceBlock} to {instanceBlock}).{Environment.NewLine}" +
                $"{d}Change sky box using {h}Top{d} and {h}Down{d} arrows";
        }
        private async Task InitializeSky()
        {
            skyboxesCount = (uint)resourceSkyboxesFiles.Length;

            var desc = SkydomDescription.Hemispheric(resourceSkyboxesFiles, Camera.FarPlaneDistance);

            skydom = await AddComponentSky<Skydom, SkydomDescription>("Sky", "Sky", desc);
        }
        private async Task InitializeFloor()
        {
            float l = 12f;
            float h = 0f;
            int side = 5;

            var geo = GeometryUtil.CreatePlane(l * 2, h, Vector3.Up);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourceGroundDiffuseTexture;
            mat.NormalMapTexture = resourceGroundNormalMapTexture;

            var desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Instances = side * side,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            var floor = await AddComponent<ModelInstanced, ModelInstancedDescription>("Floor", "Floor", desc);

            Vector3 delta = new Vector3(l * side, 0, l * side) - new Vector3(l, 0, l);
            int x = 0;
            int y = 0;
            for (int i = 0; i < floor.InstanceCount; i++)
            {
                var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta;

                floor[i].Manipulator.SetPosition(iPos);

                if (++x < side)
                {
                    continue;
                }

                x = 0;
                y++;
            }
        }
        private async Task InitializeTrees()
        {
            int instances = 40;

            var treeDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = instances,
                BlendMode = BlendModes.OpaqueTransparent,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(resourceTreesFolder, resourceTreeFile),
            };
            var trees = await AddComponent<ModelInstanced, ModelInstancedDescription>("Trees", "Trees", treeDesc);

            int side = instances / 4;
            float groundSide = 55f;

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                Vector3 iPos;
                if (i < side)
                {
                    iPos = new Vector3((i - ((side * 0) + (side * 0.5f))) * side, 0, +groundSide);
                }
                else if (i >= side && i < side * 2)
                {
                    iPos = new Vector3(+groundSide, 0, (i - ((side * 1) + (side * 0.5f))) * side);
                }
                else if (i >= side * 2 && i < side * 3)
                {
                    iPos = new Vector3((i - ((side * 2) + (side * 0.5f))) * side, 0, -groundSide);
                }
                else if (i >= side * 3 && i < side * 4)
                {
                    iPos = new Vector3(-groundSide, 0, (i - ((side * 3) + (side * 0.5f))) * side);
                }
                else
                {
                    iPos = Vector3.Zero;
                }

                trees[i].Manipulator.SetPosition(iPos);
                trees[i].Manipulator.SetRotation(iPos.Z + iPos.X, 0, 0);
                trees[i].Manipulator.SetScaling(2 + (i % 3 * 0.2f));
                trees[i].TextureIndex = (uint)(i % 2);
            }
        }
        private async Task InitializeTroops()
        {
            var tDesc = new ModelInstancedDescription()
            {
                Instances = 100,
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(resourceSoldierFolder, resourceSoldierFile),
            };
            troops = await AddComponentAgent<ModelInstanced, ModelInstancedDescription>("Troops", "Troops", tDesc);
            troops.MaximumCount = -1;

            Dictionary<string, AnimationPlan> animations = [];

            var sp1 = new AnimationPath();
            sp1.AddLoop("idle1");
            animations.Add("soldier_idle1", new AnimationPlan(sp1));

            var sp2 = new AnimationPath();
            sp2.AddLoop("idle2");
            animations.Add("soldier_idle2", new AnimationPlan(sp2));

            string[] anim = ["soldier_idle1", "soldier_idle2"];

            var rnd = Helper.NewGenerator(1);
            float l = 5;
            var vMax = new Vector3(l - 1, 0, l - 1);
            var vMin = -vMax;
            int side = 10;
            var delta = new Vector3(l * side, 0, l * side) - new Vector3(l, 0, l);

            int x = 0;
            int y = 0;
            for (int i = 0; i < troops.InstanceCount; i++)
            {
                var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta + rnd.NextVector3(vMin, vMax);

                troops[i].Manipulator.SetPosition(iPos);
                troops[i].Manipulator.SetRotation(iPos.Z, 0, 0);
                troops[i].TextureIndex = (uint)(i % 3);

                troops[i].AnimationController.TimeDelta = 0.4f + (0.1f * (i % 2));
                troops[i].AnimationController.AppendPlan(animations[anim[i % anim.Length]]);
                troops[i].AnimationController.Start(rnd.NextFloat(0f, 8f));

                if (++x < side)
                {
                    continue;
                }

                x = 0;
                y++;
            }
        }
        private async Task InitializeWall()
        {
            var wall = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Wall",
                "Wall",
                new ModelInstancedDescription()
                {
                    Instances = 40,
                    CastShadow = ShadowCastingAlgorihtms.All,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(resourceWallFolder, resourceWallFile),
                });

            BoundingBox bbox = wall[0].GetBoundingBox();

            float x = bbox.Width * (10f / 11f);

            for (int i = 0; i < 10; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3((i - 5) * x, 0.01f, 60));
            }

            for (int i = 10; i < 20; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3(60, 0, (i - 9 - 5) * x));
                wall[i].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            }

            for (int i = 20; i < 30; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3((i - 19 - 5) * x, 0.01f, -60));
                wall[i].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            }

            for (int i = 30; i < 40; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3(-60, 0, (i - 30 - 5) * x));
                wall[i].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);
            }
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            Camera.Goto(new Vector3(-48, 8, -30));
            Camera.LookTo(Vector3.Zero);
            Camera.FarPlaneDistance = 250;

            SetPostProcessingEffects();

            postProcessingTweener.Tween(postProcessState, (s, value) => s.SepiaIntensity = value, 1, 0, 15000, ScaleFuncs.CubicEaseOut);
            postProcessingTweener.Tween(postProcessState, (s, value) => s.BlurIntensity = value, 1, 0, 30000, ScaleFuncs.CubicEaseOut);

            gameReady = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);

                SetPostProcessingEffects();
            }

            if (!gameReady)
            {
                return;
            }

            UpdateCamera(gameTime);

            UpdateInstances();

            UpdateToneMapping();

            UpdateSkybox();

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                bool helpVisible = !helpPanel.Visible;

                help.Visible = helpVisible;
                helpPanel.Visible = helpVisible;
            }

            runtimeText.Text = $"{Game.RuntimeText}. Instances: {troops.MaximumCount}; Tone: {postProcessState.ToneMappingTone}.";
        }
        private void UpdateCamera(IGameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                Game.GameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                var fwd = new Vector3(Camera.Forward.X, 0, Camera.Forward.Z);
                fwd.Normalize();
                Camera.Move(gameTime, fwd, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                var bwd = new Vector3(Camera.Backward.X, 0, Camera.Backward.Z);
                bwd.Normalize();
                Camera.Move(gameTime, bwd, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInstances()
        {
            int increment = Game.Input.ShiftPressed ? instanceBlock : 1;

            if (Game.Input.KeyJustReleased(Keys.Left))
            {
                troops.MaximumCount = Math.Max(-1, troops.MaximumCount - increment);
            }

            if (Game.Input.KeyJustReleased(Keys.Right))
            {
                troops.MaximumCount = Math.Min(troops.InstanceCount, troops.MaximumCount + increment);
            }
        }
        private void UpdateToneMapping()
        {
            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                int inc = (int)postProcessState.ToneMappingTone + (Game.Input.ShiftPressed ? -1 : 1);
                uint tone = (uint)inc;
                tone %= 8;

                postProcessState.ToneMappingTone = (BuiltInToneMappingTones)tone;
            }
        }
        private void UpdateSkybox()
        {
            if (Game.Input.KeyJustReleased(Keys.Up))
            {
                skydom.TextureIndex++;
            }

            if (Game.Input.KeyJustReleased(Keys.Down))
            {
                skydom.TextureIndex--;
            }

            skydom.TextureIndex %= skyboxesCount;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            runtimeText.SetPosition(new Vector2(5, title.Top + title.Height + 3));
            info.SetPosition(new Vector2(5, runtimeText.Top + runtimeText.Height + 3));

            panel.Width = Game.Form.RenderWidth;
            panel.Height = info.Top + info.Height + 3;

            help.SetPosition(5, Game.Form.RenderHeight - help.Height - 10);
            helpPanel.SetPosition(0, Game.Form.RenderHeight - help.Height - 15);
            helpPanel.Width = Game.Form.RenderWidth;
            helpPanel.Height = help.Height + 15;
        }

        private void SetPostProcessingEffects()
        {
            postProcessState.AddSepia();
            postProcessState.AddBlurStrong();
            postProcessState.AddBloom();
            postProcessState.AddToneMapping(BuiltInToneMappingTones.LumaBasedReinhard);

            Renderer.PostProcessingObjectsEffects = postProcessState;
        }
    }
}
