using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    public class Model : ModelBase
    {
        private EffectBasic effect;
        private Volume volume;
        OrientedBoundingBox a;

        public Manipulator Manipulator { get; private set; }

        public Model(Game game, Scene3D scene, ModelContent model)
            : base(game, scene, model)
        {
            this.effect = new EffectBasic(game.Graphics.Device);
            this.LoadEffectLayouts(this.effect);

            this.Manipulator = new Manipulator();

            ModelContent modelVolume = ModelContent.GenerateBoundingBox(Color.Red);
            this.volume = new Volume(game, scene, modelVolume);
        }
        public override void Dispose()
        {
            base.Dispose();

            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }

            if (this.volume != null)
            {
                this.volume.Dispose();
                this.volume = null;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.Manipulator.Update(gameTime);

            Vector3 scale = this.BoundingBox.Maximum - this.BoundingBox.Minimum;
            Vector3 position = (this.BoundingBox.Maximum + this.BoundingBox.Minimum) * 0.5f;

            this.volume.Manipulator.SetScale(this.Manipulator.Scaling * scale);
            this.volume.Manipulator.SetRotation(this.Manipulator.Rotation);
            this.volume.Manipulator.SetPosition(this.Manipulator.Position + position);

            this.volume.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                Matrix local = this.Manipulator.LocalTransform;
                Matrix world = this.Scene.World * local;
                Matrix worldInverse = Matrix.Invert(world);
                Matrix worldViewProjection = world * this.Scene.ViewProjectionPerspective;

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

            this.volume.Draw(gameTime);
        }
    }
}
