using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using EffectTechniqueDescription = SharpDX.Direct3D11.EffectTechniqueDescription;

namespace Common
{
    using Common.Utils;

    public class TextControl : Sprite
    {
        public const int TEXTURESIZE = 512;
        public const int MAXTEXTLENGTH = 1024;

        private Dictionary<char, FontChar> fontMap = null;

        public string Text { get; private set; }
        public int CharacterCount
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Text))
                {
                    return this.Text.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        public TextControl(Game game, Scene3D scene, string font, int size)
            : base(game, scene)
        {
            this.Effect = new FontEffect(game.Graphics.Device);
            this.Texture = FontMapper.MapFont(game.Graphics.Device, font, size, TEXTURESIZE, out this.fontMap);

            this.SetText(0, 0, string.Empty.PadRight(MAXTEXTLENGTH));
        }
        public override void Draw()
        {
            base.Draw();

            this.DeviceContext.InputAssembler.InputLayout = Effect.Layout;

            EffectTechniqueDescription effectDescription = Effect.SelectedTechnique.Description;
            for (int p = 0; p < effectDescription.PassCount; p++)
            {
                this.Geometry.Set(this.DeviceContext);

                Effect.UpdatePerObject(
                    this.Scene.GetOrthoMatrizes(this.Geometry.Material, Matrix.Identity),
                    this.Texture);

                Effect.SelectedTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.Geometry.Draw(this.DeviceContext, this.CharacterCount * 6);
            }
        }
        public void SetText(int x, int y, string text)
        {
            this.Text = text;

            List<Vertex> vertexes = new List<Vertex>();
            List<uint> indices = new List<uint>();

            if (!string.IsNullOrEmpty(text))
            {
                Vector2 pos = new Vector2(x, y);

                foreach (char c in text)
                {
                    if (this.fontMap.ContainsKey(c))
                    {
                        FontChar chr = this.fontMap[c];

                        Vertex[] v;
                        uint[] i;
                        ResourceUtils.CreateSprite(
                            pos,
                            chr.Width,
                            chr.Height,
                            this.Game.Graphics.Width,
                            this.Game.Graphics.Height,
                            out v,
                            out i);

                        //Remap texture
                        float u0 = chr.X / TEXTURESIZE;
                        float v0 = chr.Y / TEXTURESIZE;
                        float u1 = (chr.X + chr.Width) / TEXTURESIZE;
                        float v1 = (chr.Y + chr.Height) / TEXTURESIZE;

                        v[0].Texture = new Vector2(u0, v0);
                        v[1].Texture = new Vector2(u1, v1);
                        v[2].Texture = new Vector2(u0, v1);
                        v[3].Texture = new Vector2(u1, v0);

                        //Update indices
                        for (int iv = 0; iv < i.Length; iv++)
                        {
                            i[iv] += (uint)vertexes.Count;
                        }

                        vertexes.AddRange(v);
                        indices.AddRange(i);

                        pos.X += chr.Width;
                    }
                }
            }

            if (this.Geometry != null)
            {
                this.Geometry.WriteVertexData(
                    this.Game.Graphics.DeviceContext,
                    VertexPositionTexture.Create(vertexes.ToArray()));

                this.Geometry.WriteIndexData(
                    this.Game.Graphics.DeviceContext,
                    indices.ToArray());
            }
            else
            {
                this.Geometry = this.Game.Graphics.Device.CreateGeometry(
                    Material.Default,
                    VertexPositionTexture.Create(vertexes.ToArray()),
                    PrimitiveTopology.TriangleList,
                    indices.ToArray());
            }
        }
    }
}
