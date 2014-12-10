using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    public class Billboard : ModelBase
    {
        private EffectBillboard effect;

        public Manipulator Manipulator { get; private set; }

        public Billboard(Game game, Scene3D scene, ModelContent geometry)
            : base(game, scene, geometry)
        {
            this.effect = new EffectBillboard(game.Graphics.Device);
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
                this.effect.FrameBuffer.Lights = new BufferLights(this.Scene.Camera.Position - this.Manipulator.Position, this.Scene.Lights);
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
    }
}
