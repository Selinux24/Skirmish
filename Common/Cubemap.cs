using System;
using SharpDX;
using EffectTechniqueDescription = SharpDX.Direct3D11.EffectTechniqueDescription;

namespace Common
{
    using Common.Utils;

    public class Cubemap : Model
    {
        public Cubemap(Game game, Scene3D scene, Geometry[] geometry)
            : base(game, scene, geometry, true)
        {

        }

        protected override void ProcessGeometry(bool effectFramework)
        {
            for (int i = 0; i < this.Geometry.Length; i++)
            {
                VertexTypes type = this.Geometry[i].VertextType;
                if (!this.Drawers.ContainsKey(type))
                {
                    this.Drawers.Add(
                        this.Geometry[i].VertextType,
                        new CubemapEffect(this.Game.Graphics.Device));
                }
            }
        }
        protected override void DrawEffect(EffectBase effect, VertexTypes type)
        {
            Matrix w = Matrix.Translation(this.Scene.Camera.Position);

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
                        this.Scene.GetMatrizes(mat, w),
                        mat.Textured ? this.Textures[mat.Texture.Name] : null);

                    effect.SelectedTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                    this.Geometry[i].Draw(this.DeviceContext);
                }
            }
        }
        protected override void DrawShader(ShaderBase shader, VertexTypes type)
        {

        }
    }
}
