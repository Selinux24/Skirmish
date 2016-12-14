using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;

namespace SceneTest
{
    public class SceneTextures : Scene
    {
        private float spaceSize = 40;

        private TextDrawer title = null;
        private TextDrawer runtime = null;

        private LensFlare lensFlare = null;

        private Model floorAsphalt = null;
        private ModelInstanced floorAsphaltI = null;

        private Model buildingObelisk = null;
        private ModelInstanced buildingObeliskI = null;

        private Model characterSoldier = null;
        private ModelInstanced characterSoldierI = null;

        private Model vehicleLeopard = null;
        private ModelInstanced vehicleLeopardI = null;

        private Sun sun = null;

        public SceneTextures(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-20, 10, -40f);
            this.Camera.LookTo(0, 0, 0);

            this.Lights.DirectionalLights[0].CastShadow = true;

            GameEnvironment.Background = Color.CornflowerBlue;

            this.InitializeTextBoxes();
            this.InitializeSkyEffects();
            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeCharacterSoldier();
            this.InitializeVehiclesLeopard();

            this.SceneVolume = new BoundingSphere(Vector3.Zero, 150f);

            this.sun = new Sun(this.Game);
            this.sun.Light = this.Lights.DirectionalLights[0];
            this.sun.TimeOfDayController.BeginAnimation(360, 10);
        }

        private void InitializeTextBoxes()
        {
            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange));
            this.runtime = this.AddText(TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange));

            this.title.Text = "Scene Test - Textures";
            this.runtime.Text = "";

            this.title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, this.title.Top + this.title.Height + 3);
        }
        private void InitializeSkyEffects()
        {
            this.lensFlare = this.AddLensFlare(new LensFlareDescription()
            {
                ContentPath = @"Common/lensFlare",
                GlowTexture = "lfGlow.png",
                Flares = new[]
                {
                    new LensFlareDescription.Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 0.3f, 0.4f, new Color(100, 255, 200), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.2f, 1.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "lfFlare1.png"),

                    new LensFlareDescription.Flare(-0.3f, 0.7f, new Color(200,  50,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "lfFlare2.png"),

                    new LensFlareDescription.Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            });

            this.lensFlare.Light = this.Lights.DirectionalLights[0];
        }
        private void InitializeFloorAsphalt()
        {
            float l = spaceSize;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
                {
                    new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 1.0f) },
                    new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 1.0f) },
                };

            uint[] indices = new uint[]
                {
                    0, 1, 2,
                    1, 3, 2,
                };

            MaterialContent mat = MaterialContent.Default;
            mat.DiffuseTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Static = true,
                CastShadow = true,
                AlwaysVisible = false,
                DeferredEnabled = true,
                EnableDepthStencil = true,
                EnableAlphaBlending = false,
            };

            var descI = new ModelInstancedDescription()
            {
                Static = true,
                CastShadow = true,
                AlwaysVisible = false,
                DeferredEnabled = true,
                EnableDepthStencil = true,
                EnableAlphaBlending = false,
                Instances = 8,
            };

            this.floorAsphalt = this.AddModel(content, desc);

            this.floorAsphaltI = this.AddInstancingModel(content, descI);

            this.floorAsphaltI.Instances[0].Manipulator.SetPosition(-l * 2, 0, 0);
            this.floorAsphaltI.Instances[1].Manipulator.SetPosition(l * 2, 0, 0);
            this.floorAsphaltI.Instances[2].Manipulator.SetPosition(0, 0, -l * 2);
            this.floorAsphaltI.Instances[3].Manipulator.SetPosition(0, 0, l * 2);

            this.floorAsphaltI.Instances[4].Manipulator.SetPosition(-l * 2, 0, -l * 2);
            this.floorAsphaltI.Instances[5].Manipulator.SetPosition(l * 2, 0, -l * 2);
            this.floorAsphaltI.Instances[6].Manipulator.SetPosition(-l * 2, 0, l * 2);
            this.floorAsphaltI.Instances[7].Manipulator.SetPosition(l * 2, 0, l * 2);
        }
        private void InitializeBuildingObelisk()
        {
            this.buildingObelisk = this.AddModel(
                "SceneTextures/buildings/obelisk",
                "Obelisk.xml",
                new ModelDescription()
                {
                    CastShadow = true,
                    Static = true,
                });

            this.buildingObeliskI = this.AddInstancingModel(
                "SceneTextures/buildings/obelisk",
                "Obelisk.xml",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Static = true,
                    Instances = 4,
                });

            this.buildingObelisk.Manipulator.SetPosition(0, 0, 0);
            this.buildingObelisk.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObelisk.Manipulator.SetScale(10);

            this.buildingObeliskI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, 0, 0);
            this.buildingObeliskI.Instances[1].Manipulator.SetPosition(spaceSize * 2, 0, 0);
            this.buildingObeliskI.Instances[2].Manipulator.SetPosition(0, 0, -spaceSize * 2);
            this.buildingObeliskI.Instances[3].Manipulator.SetPosition(0, 0, spaceSize * 2);

            this.buildingObeliskI.Instances[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.buildingObeliskI.Instances[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObeliskI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.buildingObeliskI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.buildingObeliskI.Instances[0].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[1].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[2].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[3].Manipulator.SetScale(10);
        }
        private void InitializeCharacterSoldier()
        {
            this.characterSoldier = this.AddModel(
                @"SceneTextures/character/soldier",
                @"soldier_anim2.xml",
                new ModelDescription()
                {
                    TextureIndex = 1,
                    CastShadow = true,
                    Static = false,
                });

            this.characterSoldierI = this.AddInstancingModel(
                @"SceneTextures/character/soldier",
                @"soldier_anim2.xml",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                });

            float s = spaceSize / 2f;

            AnimationPath p1 = new AnimationPath();
            p1.AddLoop("idle1");

            this.characterSoldier.Manipulator.SetPosition(s, 0, -s);
            this.characterSoldier.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldier.AnimationController.AddPath(p1);
            this.characterSoldier.AnimationController.Start(0);

            this.characterSoldierI.Instances[0].Manipulator.SetPosition(-spaceSize * 2 + s, 0, -s);
            this.characterSoldierI.Instances[1].Manipulator.SetPosition(spaceSize * 2 + s, 0, -s);
            this.characterSoldierI.Instances[2].Manipulator.SetPosition(s, 0, -spaceSize * 2 - s);
            this.characterSoldierI.Instances[3].Manipulator.SetPosition(s, 0, spaceSize * 2 - s);

            this.characterSoldierI.Instances[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.characterSoldierI.Instances[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldierI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.characterSoldierI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.characterSoldierI.Instances[0].AnimationController.AddPath(p1);
            this.characterSoldierI.Instances[1].AnimationController.AddPath(p1);
            this.characterSoldierI.Instances[2].AnimationController.AddPath(p1);
            this.characterSoldierI.Instances[3].AnimationController.AddPath(p1);

            this.characterSoldierI.Instances[0].AnimationController.Start(1);
            this.characterSoldierI.Instances[1].AnimationController.Start(2);
            this.characterSoldierI.Instances[2].AnimationController.Start(3);
            this.characterSoldierI.Instances[3].AnimationController.Start(4);
        }
        private void InitializeVehiclesLeopard()
        {
            this.vehicleLeopard = this.AddModel(
                "SceneTextures/vehicles/leopard",
                "Leopard.xml",
                new ModelDescription()
                {
                    CastShadow = true,
                    Static = false,
                });

            this.vehicleLeopardI = this.AddInstancingModel(
                "SceneTextures/vehicles/leopard",
                "Leopard.xml",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                });

            float s = -spaceSize / 2f;

            this.vehicleLeopard.Manipulator.SetPosition(s, 0, 0);
            this.vehicleLeopard.Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.vehicleLeopard.Manipulator.SetScale(12);

            this.vehicleLeopardI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, 0, -spaceSize * 2);
            this.vehicleLeopardI.Instances[1].Manipulator.SetPosition(spaceSize * 2, 0, -spaceSize * 2);
            this.vehicleLeopardI.Instances[2].Manipulator.SetPosition(-spaceSize * 2, 0, spaceSize * 2);
            this.vehicleLeopardI.Instances[3].Manipulator.SetPosition(spaceSize * 2, 0, spaceSize * 2);

            this.vehicleLeopardI.Instances[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.vehicleLeopardI.Instances[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.vehicleLeopardI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.vehicleLeopardI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.vehicleLeopardI.Instances[0].Manipulator.SetScale(12);
            this.vehicleLeopardI.Instances[1].Manipulator.SetScale(12);
            this.vehicleLeopardI.Instances[2].Manipulator.SetScale(12);
            this.vehicleLeopardI.Instances[3].Manipulator.SetScale(12);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            this.sun.Update(gameTime);

            GameEnvironment.Background = this.sun.TimeOfDayController.SunBandColor;

            base.Update(gameTime);

            this.runtime.Text = this.Game.RuntimeText;
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }
    }
}
