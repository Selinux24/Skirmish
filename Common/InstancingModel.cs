using System;
using SharpDX;
using EffectTechniqueDescription = SharpDX.Direct3D11.EffectTechniqueDescription;

namespace Common
{
    using Common.Utils;

    public class InstancingModel : Model
    {
        private Controller[] instanceControllers = null;
        private BufferInstancingData[] instanceData = null;
        private int selectedIndex = 0;

        public virtual Controller Transform
        {
            get
            {
                return this.instanceControllers[this.selectedIndex];
            }
        }
        public Controller this[int index]
        {
            get
            {
                return this.instanceControllers[index];
            }
        }
        public int Count
        {
            get
            {
                return this.instanceControllers != null ? this.instanceControllers.Length : 0;
            }
        }

        public InstancingModel(Game game, Scene3D scene, Geometry[] geometry, int instanceCount, bool effectFramework = true)
            : base(game, scene, geometry, effectFramework)
        {
            this.instanceControllers = new Controller[instanceCount];
            this.instanceData = new BufferInstancingData[instanceCount];

            for (int i = 0; i < instanceCount; i++)
            {
                this.instanceControllers[i] = new Controller();
                this.instanceData[i] = new BufferInstancingData(Matrix.Identity);
            }
        }
        public override void Update()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this.instanceControllers[i].Update();

                instanceData[i].Local = this.instanceControllers[i].LocalTransform;
            }

            for (int i = 0; i < this.Geometry.Length; i++)
            {
                this.Geometry[i].WriteInstancingData(this.Game.Graphics.DeviceContext, instanceData);
            }
        }
        public void Select(int index)
        {
            this.selectedIndex = index;
        }
        public void Next()
        {
            this.selectedIndex++;

            if (this.selectedIndex >= this.instanceControllers.Length) this.selectedIndex = 0;
        }
        public void Previous()
        {
            this.selectedIndex--;

            if (this.selectedIndex < 0) this.selectedIndex = this.instanceControllers.Length - 1;
        }

        protected override void ProcessGeometry(bool effectFramework)
        {
            for (int i = 0; i < this.Geometry.Length; i++)
            {
                VertexTypes type = this.Geometry[i].VertextType;
                if (!this.Drawers.ContainsKey(type))
                {
                    if (effectFramework)
                    {
                        this.Drawers.Add(
                            this.Geometry[i].VertextType,
                            new InstancingEffect(this.Game.Graphics.Device, type));
                    }
                    else
                    {
                        this.Drawers.Add(
                            this.Geometry[i].VertextType,
                            new InstancingShader(this.Game.Graphics.Device, type));
                    }
                }
            }
        }
        protected override void DrawEffect(EffectBase effect, VertexTypes type)
        {
            this.DeviceContext.InputAssembler.InputLayout = effect.Layout;

            effect.UpdatePerFrame(this.Scene.GetLights());

            EffectTechniqueDescription effectDescription = effect.SelectedTechnique.Description;
            for (int p = 0; p < effectDescription.PassCount; p++)
            {
                Geometry[] geomByType = Array.FindAll(this.Geometry, g => g.VertextType == type);
                for (int i = 0; i < geomByType.Length; i++)
                {
                    this.Geometry[i].SetInstancing(this.DeviceContext);

                    Material mat = this.Geometry[i].Material;

                    effect.UpdatePerObject(
                        this.Scene.GetMatrizes(mat, Matrix.Identity),
                        mat.Textured ? this.Textures[mat.Texture.Name] : null);

                    effect.SelectedTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                    this.Geometry[i].DrawInstancing(this.DeviceContext);
                }
            }
        }
        protected override void DrawShader(ShaderBase shader, VertexTypes type)
        {
            this.DeviceContext.InputAssembler.InputLayout = shader.Layout;
            this.DeviceContext.VertexShader.Set(shader.VertexShader);
            this.DeviceContext.PixelShader.Set(shader.PixelShader);

            shader.UpdatePerFrame(this.Scene.GetLights());

            Geometry[] geomByType = Array.FindAll(this.Geometry, g => g.VertextType == type);
            for (int i = 0; i < geomByType.Length; i++)
            {
                this.Geometry[i].SetInstancing(this.DeviceContext);

                Material mat = this.Geometry[i].Material;

                shader.UpdatePerObject(
                    this.Scene.GetMatrizes(mat, Matrix.Identity),
                    mat.Textured ? this.Textures[mat.Texture.Name] : null);

                this.Geometry[i].DrawInstancing(this.DeviceContext);
            }
        }
    }
}
