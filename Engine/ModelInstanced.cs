using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    public class ModelInstanced : ModelBase
    {
        private EffectInstancing effect;
        private VertexInstancingData[] instancingData = null;

        private Manipulator[] instances = null;
        private int currentInstance = 0;
        public Manipulator this[int index]
        {
            get
            {
                return this.instances[index];
            }
        }
        public Manipulator Manipulator
        {
            get
            {
                return this.instances[currentInstance];
            }
        }
        public int Count
        {
            get
            {
                return this.instances.Length;
            }
        }

        public ModelInstanced(Game game, Scene3D scene, ModelContent model, int instances, bool debug = false)
            : base(game, scene, model, true, instances)
        {
            this.effect = new EffectInstancing(game.Graphics.Device);
            this.LoadEffectLayouts(this.effect);

            this.instancingData = new VertexInstancingData[instances];

            this.instances = new Manipulator[instances];
            for (int i = 0; i < instances; i++)
            {
                this.instances[i] = new Manipulator();
            }
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

            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    Manipulator man = this.instances[i];

                    man.Update(gameTime);

                    this.instancingData[i].Local = man.LocalTransform;
                }
            }
        }
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                this.effect.FrameBuffer.World = this.Scene.World;
                this.effect.FrameBuffer.WorldInverse = this.Scene.WorldInverse;
                this.effect.FrameBuffer.WorldViewProjection = this.Scene.World * this.Scene.ViewProjectionPerspective;
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
                        MeshInstanced mesh = (MeshInstanced)dictionary[material];
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

                        mesh.WriteInstancingData(this.DeviceContext, this.instancingData);

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
        public void Next()
        {
            this.currentInstance++;

            if (this.currentInstance >= this.instances.Length)
            {
                this.currentInstance = 0;
            }
        }
        public void Previous()
        {
            this.currentInstance--;

            if (this.currentInstance < 0)
            {
                this.currentInstance = this.instances.Length - 1;
            }
        }
    }
}
