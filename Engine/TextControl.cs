using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Bitmap = System.Drawing.Bitmap;
using Brushes = System.Drawing.Brushes;
using Buffer = SharpDX.Direct3D11.Buffer;
using CompositingMode = System.Drawing.Drawing2D.CompositingMode;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Graph = System.Drawing.Graphics;
using GraphicsUnit = System.Drawing.GraphicsUnit;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using RectangleF = System.Drawing.RectangleF;
using Region = System.Drawing.Region;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using SizeF = System.Drawing.SizeF;
using StringFormat = System.Drawing.StringFormat;
using TextRenderingHint = System.Drawing.Text.TextRenderingHint;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    public class TextControl : Drawable
    {
        private EffectFont effect = null;
        private string technique = null;
        private InputLayout inputLayout = null;

        private Buffer vertexBuffer = null;
        private int vertexCount = 0;
        private int vertexBufferStride = 0;
        private int vertexBufferOffset = 0;
        private Buffer indexBuffer = null;
        private int indexCount = 0;

        private ShaderResourceView texture = null;
        private FontMap fontMap = null;
        private Vector2 position = Vector2.Zero;
        private string text = null;

        public new Scene3D Scene
        {
            get
            {
                return base.Scene as Scene3D;
            }
            set
            {
                base.Scene = value;
            }
        }
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;

                this.MapText();
            }
        }
        public int CharacterCount
        {
            get
            {
                if (!string.IsNullOrEmpty(this.text))
                {
                    return this.text.Length;
                }
                else
                {
                    return 0;
                }
            }
        }
        public Color4 ForeColor { get; set; }
        public Color4 BackColor { get; set; }
        public readonly string Font = null;

        public Vector2 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;

                this.MapText();
            }
        }
        public int Left
        {
            get
            {
                return (int)this.position.X;
            }
            set
            {
                this.position.X = value;

                this.MapText();
            }
        }
        public int Top
        {
            get
            {
                return (int)this.position.Y;
            }
            set
            {
                this.position.Y = value;

                this.MapText();
            }
        }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TextControl(Game game, Scene3D scene, string font, int size, Color color)
            : this(game, scene, font, size, color, Color.Transparent)
        {
            
        }
        public TextControl(Game game, Scene3D scene, string font, int size, Color color, Color backColor)
            : base(game, scene)
        {
            this.Font = string.Format("{0} {1}", font, size);

            this.effect = new EffectFont(game.Graphics.Device);
            this.technique = this.effect.AddInputLayout(VertexTypes.PositionTexture);
            this.inputLayout = this.effect.GetInputLayout(this.technique);

            using (MemoryStream mstr = FontMap.MapFont(font, size, out this.fontMap))
            {
                this.texture = game.Graphics.Device.LoadTexture(mstr.GetBuffer());
            }

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4]);
            this.vertexBufferStride = VertexPositionTexture.SizeInBytes;
            this.vertexBufferOffset = 0;
            this.vertexCount = 0;

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferWrite(new uint[FontMap.MAXTEXTLENGTH * 6]);
            this.indexCount = 0;

            this.ForeColor = color;
            this.BackColor = backColor;
        }
        public override void Dispose()
        {
            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }

            if (this.texture != null)
            {
                this.texture.Dispose();
                this.texture = null;
            }
        }
        public override void Update(GameTime gameTime)
        {
            
        }
        public override void Draw(GameTime gameTime)
        {
            if (!string.IsNullOrWhiteSpace(this.text))
            {
                #region Per frame update

                this.effect.FrameBuffer.World = this.Scene.World;
                this.effect.FrameBuffer.WorldViewProjection = this.Scene.World * this.Scene.ViewProjectionOrthogonal;
                this.effect.FrameBuffer.Color = this.ForeColor;
                this.effect.UpdatePerFrame(this.texture);

                #endregion

                this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;

                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(
                    0,
                    new VertexBufferBinding[]
                    {
                        new VertexBufferBinding(this.vertexBuffer, this.vertexBufferStride, this.vertexBufferOffset),
                    });

                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

                EffectTechnique technique = this.effect.GetTechnique("FontDrawer");

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    if (this.indexBuffer != null)
                    {
                        this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);
                    }
                    else
                    {
                        this.Game.Graphics.DeviceContext.Draw(this.vertexCount, 0);
                    }

                    Counters.DrawCallsPerFrame++;
                }
            }
        }
        public override void HandleResizing()
        {
            this.MapText();
        }
        private void MapText()
        {
            VertexPositionTexture[] v;
            uint[] i;
            Vector2 size;
            this.fontMap.MapSentence(
                this.text,
                this.position,
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                out v, out i, out size);

            this.Game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, v);
            this.Game.Graphics.DeviceContext.WriteBuffer(this.indexBuffer, i);

            this.vertexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 4;
            this.indexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 6;

            this.Width = (int)size.X;
            this.Height = (int)size.Y;
        }
    }

    public struct FontChar
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return string.Format("X: {0}; Y: {1}; Width: {2}; Height: {3}", this.X, this.Y, this.Width, this.Height);
        }
    }

    public class FontMap : Dictionary<char, FontChar>
    {
        public const int MAXTEXTLENGTH = 1024;
        public const int TEXTURESIZE = 1024;
        public const uint KeyCodes = 512;

        public string Font { get; private set; }
        public int Size { get; private set; }

        public static char[] ValidKeys
        {
            get
            {
                List<char> cList = new List<char>((int)KeyCodes);

                for (uint i = 1; i < KeyCodes; i++)
                {
                    char c = (char)i;
                    if (!char.IsControl(c))
                    {
                        cList.Add(c);
                    }
                }

                return cList.ToArray();
            }
        }

        public static MemoryStream MapFont(string font, int size, out FontMap map)
        {
            map = new FontMap()
            {
                Font = font,
                Size = size,
            };

            using (Bitmap bmp = new Bitmap(TEXTURESIZE, TEXTURESIZE))
            using (Graph gra = Graph.FromImage(bmp))
            {
                gra.TextContrast = 12;
                gra.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                gra.CompositingMode = CompositingMode.SourceCopy;

                gra.FillRegion(
                    Brushes.Transparent,
                    new Region(new RectangleF(0, 0, TEXTURESIZE, TEXTURESIZE)));

                using (StringFormat fmt = StringFormat.GenericDefault)
                using (Font fnt = new Font(font, size, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    float left = 0f;
                    float top = 0f;

                    char[] keys = FontMap.ValidKeys;

                    for (int i = 0; i < keys.Length; i++)
                    {
                        char c = keys[i];

                        SizeF s = gra.MeasureString(
                            c.ToString(),
                            fnt,
                            int.MaxValue,
                            fmt);

                        if (c == ' ')
                        {
                            s.Width = fnt.SizeInPoints;
                        }

                        if (left + s.Width >= TEXTURESIZE)
                        {
                            left = 0f;
                            top += s.Height;
                        }

                        gra.DrawString(
                            c.ToString(),
                            fnt,
                            Brushes.White,
                            left,
                            top,
                            fmt);

                        FontChar chr = new FontChar()
                        {
                            X = (int)left,
                            Y = (int)top,
                            Width = (int)Math.Round(s.Width),
                            Height = (int)Math.Round(s.Height),
                        };

                        map.Add(c, chr);

                        left += s.Width;
                    }
                }

                MemoryStream mstr = new MemoryStream();

                bmp.Save(mstr, ImageFormat.Png);

                return mstr;
            }
        }

        public void MapSentence(
            string text, 
            Vector2 pos, 
            float formWidth, 
            float formHeight, 
            out VertexPositionTexture[] vertices, 
            out uint[] indices,
            out Vector2 size)
        {
            size = Vector2.Zero;

            List<VertexPositionTexture> vertList = new List<VertexPositionTexture>();
            List<uint> indexList = new List<uint>();

            if (string.IsNullOrWhiteSpace(text))
            {
                text = string.Empty.PadRight(MAXTEXTLENGTH);
            }
            else if (text.Length > MAXTEXTLENGTH)
            {
                text = text.Substring(0, MAXTEXTLENGTH);
            }
            else
            {
                text += string.Empty.PadRight(MAXTEXTLENGTH - text.Length);
            }

            int height = 0;

            foreach (char c in text)
            {
                if (this.ContainsKey(c))
                {
                    FontChar chr = this[c];

                    Vertex[] cv;
                    uint[] ci;
                    ModelContent.CreateSprite(
                        pos,
                        chr.Width,
                        chr.Height,
                        formWidth,
                        formHeight,
                        out cv,
                        out ci);

                    //Remap texture
                    float u0 = (chr.X) / (float)FontMap.TEXTURESIZE;
                    float v0 = (chr.Y) / (float)FontMap.TEXTURESIZE;
                    float u1 = (chr.X + chr.Width) / (float)FontMap.TEXTURESIZE;
                    float v1 = (chr.Y + chr.Height) / (float)FontMap.TEXTURESIZE;

                    cv[0].Texture = new Vector2(u0, v0);
                    cv[1].Texture = new Vector2(u1, v1);
                    cv[2].Texture = new Vector2(u0, v1);
                    cv[3].Texture = new Vector2(u1, v0);

                    Array.ForEach(ci, (i) => { indexList.Add(i + (uint)vertList.Count); });
                    Array.ForEach(cv, (v) => { vertList.Add(VertexPositionTexture.Create(v)); });

                    pos.X += chr.Width - (int)(this.Size / 6);
                    if (chr.Height > height) height = chr.Height;
                }
            }

            pos.Y = height;

            vertices = vertList.ToArray();
            indices = indexList.ToArray();
            size = pos;
        }
    }
}
