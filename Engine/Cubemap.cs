using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    public class Cubemap : ModelBase
    {
        private EffectCubemap effect = null;

        public Manipulator Manipulator { get; private set; }

        public Cubemap(Game game, Scene3D scene, ModelContent geometry)
            : base(game, scene, geometry)
        {
            this.effect = new EffectCubemap(game.Graphics.Device);
            this.LoadEffectLayouts(this.effect);

            this.Manipulator = new Manipulator();
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

            this.Manipulator.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                this.effect.FrameBuffer.WorldViewProjection = this.Scene.World * this.Manipulator.LocalTransform * this.Scene.ViewProjectionPerspective;
                this.effect.UpdatePerFrame();

                #endregion

                foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                {
                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = this.Materials[material];
                        string techniqueName = this.Techniques[mesh];

                        #region Per object update

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
    }
}
