using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;

namespace SceneTest
{
    public class TestScene : Scene
    {
        private float spaceSize = 40;

        private TextDrawer title = null;
        private TextDrawer runtime = null;

        private Model floorAsphalt = null;
        private ModelInstanced floorAsphaltI = null;

        private Model buildingObelisk = null;
        private ModelInstanced buildingObeliskI = null;

        private Model characterSoldier = null;
        private ModelInstanced characterSoldierI = null;

        private Model vehicleLeopard = null;
        private ModelInstanced vehicleLeopardI = null;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[0].CastShadow = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = true;

            this.Camera.NearPlaneDistance = 1;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-40, 10, -60f);
            this.Camera.LookTo(0, 0, 0);

            GameEnvironment.Background = Color.CornflowerBlue;

            this.InitializeTextBoxes();
            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeCharacterSoldier();
            this.InitializeVehiclesLeopard();

            this.SceneVolume = new BoundingSphere(Vector3.Zero, 150f);
        }

        private void InitializeTextBoxes()
        {
            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange));
            this.runtime = this.AddText(TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange));

            this.title.Text = "Scene Test";
            this.runtime.Text = "";

            this.title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, this.title.Top + this.title.Height + 3);
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
            mat.DiffuseTexture = "resources/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "resources/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "resources/floors/asphalt/d_road_asphalt_stripes_specular.dds";

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
                "resources/buildings/obelisk",
                "Obelisk.xml",
                new ModelDescription()
                {
                    CastShadow = true,
                    Static = true,
                });

            this.buildingObeliskI = this.AddInstancingModel(
                "resources/buildings/obelisk",
                "Obelisk.xml",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Static = true,
                    Instances = 4,
                });

            this.buildingObelisk.Manipulator.SetPosition(0, 0, 0);
            this.buildingObelisk.Manipulator.SetScale(10);

            this.buildingObeliskI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, 0, 0);
            this.buildingObeliskI.Instances[1].Manipulator.SetPosition(spaceSize * 2, 0, 0);
            this.buildingObeliskI.Instances[2].Manipulator.SetPosition(0, 0, -spaceSize * 2);
            this.buildingObeliskI.Instances[3].Manipulator.SetPosition(0, 0, spaceSize * 2);

            this.buildingObeliskI.Instances[0].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[1].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[2].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[3].Manipulator.SetScale(10);
        }
        private void InitializeCharacterSoldier()
        {
            this.characterSoldier = this.AddModel(
                @"Resources/character/soldier",
                @"soldier_anim2.xml",
                new ModelDescription()
                {
                    TextureIndex = 1,
                    CastShadow = true,
                    Static = false,
                });

            this.characterSoldierI = this.AddInstancingModel(
                @"Resources/character/soldier",
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
            this.characterSoldier.AnimationController.AddPath(p1);
            this.characterSoldier.AnimationController.Start(0);

            this.characterSoldierI.Instances[0].Manipulator.SetPosition(-spaceSize * 2 + s, 0, -s);
            this.characterSoldierI.Instances[1].Manipulator.SetPosition(spaceSize * 2 + s, 0, -s);
            this.characterSoldierI.Instances[2].Manipulator.SetPosition(s, 0, -spaceSize * 2 - s);
            this.characterSoldierI.Instances[3].Manipulator.SetPosition(s, 0, spaceSize * 2 - s);

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
                "resources/vehicles/leopard",
                "Leopard.xml",
                new ModelDescription()
                {
                    CastShadow = true,
                    Static = false,
                });

            this.vehicleLeopardI = this.AddInstancingModel(
                "resources/vehicles/leopard",
                "Leopard.xml",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                });

            float s = -spaceSize / 2f;

            this.vehicleLeopard.Manipulator.SetPosition(s, 0, 0);
            this.vehicleLeopard.Manipulator.SetScale(12);

            this.vehicleLeopardI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, 0, -spaceSize * 2);
            this.vehicleLeopardI.Instances[1].Manipulator.SetPosition(spaceSize * 2, 0, -spaceSize * 2);
            this.vehicleLeopardI.Instances[2].Manipulator.SetPosition(-spaceSize * 2, 0, spaceSize * 2);
            this.vehicleLeopardI.Instances[3].Manipulator.SetPosition(spaceSize * 2, 0, spaceSize * 2);

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
