using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    public class Sprite : ModelBase
    {
        private EffectBasic effect = null;

        private Matrix local = Matrix.Identity;

        private float baseWidth { get; set; }
        private float baseHeight { get; set; }

        public bool FitScreen { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public Vector2 Position { get; private set; }

        public Sprite(Game game, Scene3D scene, string texture, float width, float height)
            : base(game, scene, ModelContent.GenerateSprite(scene.ContentPath, texture))
        {
            this.effect = new EffectBasic(game.Graphics.Device);
            this.LoadEffectLayouts(this.effect);

            this.Width = width;
            this.Height = height;

            this.baseWidth = width / game.Form.RenderWidth;
            this.baseHeight = height / game.Form.RenderHeight;
        }
        public override void Dispose()
        {
            base.Dispose();

            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector3 pos = new Vector3(
                this.Position.X - this.Game.Form.RelativeCenter.X,
                this.Position.Y + this.Game.Form.RelativeCenter.Y, 
                0f);

            this.local =
                Matrix.Scaling(this.Width, this.Height, 1f) *
                Matrix.Translation(pos);
        }
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                Matrix world = this.Scene.World * this.local;
                Matrix worldInverse = Matrix.Invert(world);
                Matrix worldViewProjection = world * this.Scene.ViewProjectionOrthogonal;

                this.effect.FrameBuffer.World = world;
                this.effect.FrameBuffer.WorldInverse = worldInverse;
                this.effect.FrameBuffer.WorldViewProjection = worldViewProjection;
                this.effect.FrameBuffer.Lights = new BufferLights(this.Scene.Camera.Position, this.Scene.Lights);
                this.effect.UpdatePerFrame();

                #endregion

                #region Per skinning update

                if (this.SkinnedData != null)
                {
                    this.effect.SkinningBuffer.FinalTransforms = this.SkinnedData.FinalTransforms;
                    this.effect.UpdatePerSkinning();
                }

                #endregion

                foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                {
                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = material != NoMaterial ? this.Materials[material] : null;
                        string techniqueName = this.Techniques[mesh];

                        #region Per object update

                        if (mat != null)
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(mat.Material);
                            this.effect.UpdatePerObject(mat.DiffuseTexture);
                        }
                        else
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
                            this.effect.UpdatePerObject(null);
                        }

                        #endregion

                        mesh.SetInputAssembler(this.DeviceContext, this.effect.GetInputLayout(techniqueName));

                        EffectTechnique technique = this.effect.GetTechnique(techniqueName);

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                            mesh.Draw(gameTime, this.DeviceContext);
                        }
                    }
                }
            }
        }
        public override void HandleResizing()
        {
            base.HandleResizing();

            if (this.FitScreen)
            {
                this.Width = this.baseWidth * this.Game.Form.RenderWidth;
                this.Height = this.baseHeight * this.Game.Form.RenderHeight;
            }
        }

        public virtual void Move(GameTime gameTime, Vector2 d)
        {
            this.Position += d * gameTime.ElapsedSeconds;
        }
        public virtual void MoveLeft(GameTime gameTime, float d)
        {
            this.Position += Vector2.UnitX * -d * gameTime.ElapsedSeconds;
        }
        public virtual void MoveRight(GameTime gameTime, float d)
        {
            this.Position += Vector2.UnitX * d * gameTime.ElapsedSeconds;
        }
        public virtual void MoveUp(GameTime gameTime, float d)
        {
            this.Position += Vector2.UnitY * d * gameTime.ElapsedSeconds;
        }
        public virtual void MoveDown(GameTime gameTime, float d)
        {
            this.Position += Vector2.UnitY * -d * gameTime.ElapsedSeconds;
        }

        public virtual void SetPosition(float x, float y)
        {
            this.SetPosition(new Vector2(x, y));
        }
        public virtual void SetPosition(Vector2 position)
        {
            this.Position = position;
        }
    }
}
