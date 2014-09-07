using System;
using EffectTechniqueDescription = SharpDX.Direct3D11.EffectTechniqueDescription;

namespace Common
{
    using Common.Utils;

    public class BasicModel : Model
    {
        public Controller Transform { get; protected set; }

        public BasicModel(Game game, Scene3D scene, Geometry[] geometry, bool effectFramework = true)
            : base(game, scene, geometry, effectFramework)
        {
            this.Transform = new Controller();
        }
        public override void Update()
        {
            base.Update();

            this.Transform.Update();
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
                            new BasicEffect(this.Game.Graphics.Device, type));
                    }
                    else
                    {
                        this.Drawers.Add(
                            this.Geometry[i].VertextType,
                            new BasicShader(this.Game.Graphics.Device, type));
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
                    this.Geometry[i].Set(this.DeviceContext);

                    Material mat = this.Geometry[i].Material;

                    effect.UpdatePerObject(
                        this.Scene.GetMatrizes(mat, this.Transform.LocalTransform),
                        mat.Textured ? this.Textures[mat.Texture.Name] : null);

                    effect.SelectedTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                    this.Geometry[i].Draw(this.DeviceContext);
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
                this.Geometry[i].Set(this.DeviceContext);

                Material mat = this.Geometry[i].Material;

                shader.UpdatePerObject(
                    this.Scene.GetMatrizes(mat, this.Transform.LocalTransform),
                    mat.Textured ? this.Textures[mat.Texture.Name] : null);

                this.Geometry[i].Draw(this.DeviceContext);
            }
        }
    }
}
