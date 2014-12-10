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

        public Vector2 Position { get; private set; }

        public Sprite(Game game, Scene3D scene, ModelContent model)
            : base(game, scene, model)
        {
            this.effect = new EffectBasic(game.Graphics.Device);
            this.LoadEffectLayouts(this.effect);
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
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                Matrix local = Matrix.Translation(this.Position.X, this.Position.Y, 0f);
                Matrix world = this.Scene.World * local;
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
                        MeshMaterial mat = this.Materials[material];
                        string techniqueName = this.Techniques[mesh];

                        #region Per object update

                        this.effect.ObjectBuffer.Material = new BufferMaterials(mat.Material);
                        this.effect.UpdatePerObject(mat.DiffuseTexture);

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

        public virtual void Move(Vector2 d)
        {
            this.Position += d;
        }
        public virtual void MoveLeft(float d)
        {
            this.Position += Vector2.UnitX * -d;
        }
        public virtual void MoveRight(float d)
        {
            this.Position += Vector2.UnitX * d;
        }
        public virtual void MoveUp(float d)
        {
            this.Position += Vector2.UnitY * d;
        }
        public virtual void MoveDown(float d)
        {
            this.Position += Vector2.UnitY * -d;
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
