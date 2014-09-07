using SharpDX.Direct3D;

namespace Common
{
    using Common.Utils;

    public class BasicSprite : Sprite
    {
        public BasicSprite(Game game, Scene3D scene, string textureFilename, int width, int height)
            : base(game, scene)
        {
            this.Effect = new BasicEffect(this.Graphics.Device, VertexTypes.PositionTexture);

            Vertex[] vertexes;
            uint[] indexes;
            ResourceUtils.CreateSprite(
                this.Position,
                width, height,
                this.Graphics.Width, this.Graphics.Height,
                out vertexes,
                out indexes);

            this.Geometry = this.Graphics.Device.CreateGeometry<VertexPositionTexture>(
                Material.CreateTextured(textureFilename),
                Vertex.Convert<VertexPositionTexture>(vertexes),
                PrimitiveTopology.TriangleList,
                indexes);

            this.Texture = game.Graphics.Device.LoadTexture(textureFilename);
        }
        public override void Draw()
        {
            base.Draw();

            this.DeviceContext.InputAssembler.InputLayout = Effect.Layout;

            Effect.UpdatePerFrame(this.Scene.GetLights());

            for (int p = 0; p < Effect.SelectedTechnique.Description.PassCount; p++)
            {
                this.Geometry.Set(this.DeviceContext);

                this.Effect.UpdatePerObject(
                    this.Scene.GetOrthoMatrizes(this.Geometry.Material, this.LocalTransform),
                    this.Texture);

                Effect.SelectedTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.Geometry.Draw(this.DeviceContext);
            }
        }
    }
}
