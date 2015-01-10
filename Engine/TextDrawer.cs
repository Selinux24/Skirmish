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
using Device = SharpDX.Direct3D11.Device;
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

    /// <summary>
    /// Text drawer
    /// </summary>
    public class TextDrawer : Drawable
    {
        /// <summary>
        /// Effect
        /// </summary>
        private EffectFont effect = null;
        /// <summary>
        /// Technique name
        /// </summary>
        private EffectTechnique technique = null;
        /// <summary>
        /// Input layout
        /// </summary>
        private InputLayout inputLayout = null;

        /// <summary>
        /// Vertex buffer
        /// </summary>
        private Buffer vertexBuffer = null;
        /// <summary>
        /// Vertex couunt
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Vertex stride
        /// </summary>
        private int vertexBufferStride = 0;
        /// <summary>
        /// Vertex offset
        /// </summary>
        private int vertexBufferOffset = 0;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding[] vertexBufferBinding = null;
        /// <summary>
        /// Index buffer
        /// </summary>
        private Buffer indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;

        /// <summary>
        /// Font map
        /// </summary>
        private FontMap fontMap = null;
        /// <summary>
        /// Text position in 2D screen
        /// </summary>
        private Vector2 position = Vector2.Zero;
        /// <summary>
        /// Text
        /// </summary>
        private string text = null;

        /// <summary>
        /// Scene
        /// </summary>
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
        /// <summary>
        /// Font name
        /// </summary>
        public readonly string Font = null;
        /// <summary>
        /// Gets or sets text to draw
        /// </summary>
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (!string.Equals(this.text, value))
                {
                    this.text = value;

                    this.MapText();
                }
            }
        }
        /// <summary>
        /// Gets character count
        /// </summary>
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
        /// <summary>
        /// Gets or sets text fore color
        /// </summary>
        public Color4 TextColor { get; set; }
        /// <summary>
        /// Gets or sets text shadow color
        /// </summary>
        public Color4 ShadowColor { get; set; }
        /// <summary>
        /// Gets or sets relative position of shadow
        /// </summary>
        public Vector2 ShadowRelative { get; set; }

        /// <summary>
        /// Gets or sest text position in 2D screen
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public int Left
        {
            get
            {
                return (int)this.position.X;
            }
            set
            {
                this.position.X = value;
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Top
        {
            get
            {
                return (int)this.position.Y;
            }
            set
            {
                this.position.Y = value;
            }
        }
        /// <summary>
        /// Gets text width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets text height
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="scene">Scene</param>
        /// <param name="font">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="textColor">Fore color</param>
        public TextDrawer(Game game, Scene3D scene, string font, int size, Color textColor)
            : this(game, scene, font, size, textColor, Color.Transparent)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="scene">Scene</param>
        /// <param name="font">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="textColor">Fore color</param>
        /// <param name="shadowColor">Shadow color</param>
        public TextDrawer(Game game, Scene3D scene, string font, int size, Color textColor, Color shadowColor)
            : base(game, scene)
        {
            this.Font = string.Format("{0} {1}", font, size);

            this.effect = new EffectFont(game.Graphics.Device);
            this.technique = this.effect.GetTechnique(VertexTypes.PositionTexture, DrawingStages.Drawing);
            this.inputLayout = this.effect.GetInputLayout(this.technique);

            this.fontMap = FontMap.MapFont(game.Graphics.Device, font, size);

            VertexPositionTexture[] vertices = new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4];

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(vertices);
            this.vertexBufferStride = vertices[0].Stride;
            this.vertexBufferOffset = 0;
            this.vertexCount = 0;
            this.vertexBufferBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(this.vertexBuffer, this.vertexBufferStride, this.vertexBufferOffset),
            };

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferWrite(new uint[FontMap.MAXTEXTLENGTH * 6]);
            this.indexCount = 0;

            this.TextColor = textColor;
            this.ShadowColor = shadowColor;
            this.ShadowRelative = Vector2.One;

            this.MapText();
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public override void Dispose()
        {
            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }
        }
        /// <summary>
        /// Update component state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
        {

        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (!string.IsNullOrWhiteSpace(this.text))
            {
                //this.Game.Graphics.SetBlendTransparent();

                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

                if (this.ShadowColor != Color.Transparent)
                {
                    //Draw shadow
                    this.DrawText(this.Position + this.ShadowRelative, this.ShadowColor);
                }

                //Draw text
                this.DrawText(this.Position, this.TextColor);
            }
        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="color">Color</param>
        private void DrawText(Vector2 position, Color4 color)
        {
            #region Per frame update

            Vector3 pos = new Vector3(
                position.X - this.Game.Form.RelativeCenter.X,
                -position.Y + this.Game.Form.RelativeCenter.Y,
                0f);

            Matrix world = this.Scene.World * Matrix.Translation(pos);
            Matrix worldViewProjection = world * this.Scene.ViewProjectionOrthogonal;

            this.effect.FrameBuffer.World = world;
            this.effect.FrameBuffer.WorldViewProjection = worldViewProjection;
            this.effect.FrameBuffer.Color = color;
            this.effect.UpdatePerFrame(this.fontMap.Texture);

            #endregion

            for (int p = 0; p < this.technique.Description.PassCount; p++)
            {
                this.technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

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
        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            VertexPositionTexture[] v;
            uint[] i;
            Vector2 size;
            this.fontMap.MapSentence(
                this.text,
                out v, out i, out size);

            this.Game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, v);
            this.Game.Graphics.DeviceContext.WriteBuffer(this.indexBuffer, i);

            this.vertexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 4;
            this.indexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 6;

            this.Width = (int)size.X;
            this.Height = (int)size.Y;
        }
    }

    /// <summary>
    /// Font map character
    /// </summary>
    public struct FontMapChar
    {
        /// <summary>
        /// X map coordinate
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y map coordinate
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Character map width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Character map height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets text representation of character map
        /// </summary>
        /// <returns>Returns text representation of character map</returns>
        public override string ToString()
        {
            return string.Format("X: {0}; Y: {1}; Width: {2}; Height: {3}", this.X, this.Y, this.Width, this.Height);
        }
    }

    /// <summary>
    /// Font map
    /// </summary>
    public class FontMap : Dictionary<char, FontMapChar>, IDisposable
    {
        /// <summary>
        /// Font cache
        /// </summary>
        private static List<FontMap> gCache = new List<FontMap>();

        /// <summary>
        /// Clears and dispose font cache
        /// </summary>
        internal static void ClearCache()
        {
            foreach (FontMap map in gCache)
            {
                map.Dispose();
            }

            gCache.Clear();
        }

        /// <summary>
        /// Maximum text length
        /// </summary>
        public const int MAXTEXTLENGTH = 1024;
        /// <summary>
        /// Texture size
        /// </summary>
        public const int TEXTURESIZE = 1024;
        /// <summary>
        /// Key codes
        /// </summary>
        public const uint KeyCodes = 512;

        /// <summary>
        /// Font name
        /// </summary>
        public string Font { get; private set; }
        /// <summary>
        /// Font size
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Font texture
        /// </summary>
        public ShaderResourceView Texture { get; private set; }
        /// <summary>
        /// Gets map keys
        /// </summary>
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
        /// <summary>
        /// Creates a font map of the specified font and size
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <returns>Returns created font map</returns>
        public static FontMap MapFont(Device device, string font, int size)
        {
            FontMap map = gCache.Find(f => f.Font == font && f.Size == size);
            if (map == null)
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

                            FontMapChar chr = new FontMapChar()
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

                    using (MemoryStream mstr = new MemoryStream())
                    {
                        bmp.Save(mstr, ImageFormat.Png);

                        map.Texture = device.LoadTexture(mstr.GetBuffer());
                    }
                }

                gCache.Add(map);
            }

            return map;
        }

        /// <summary>
        /// Maps a sentence
        /// </summary>
        /// <param name="text">Sentence text</param>
        /// <param name="vertices">Gets generated vertices</param>
        /// <param name="indices">Gets generated indices</param>
        /// <param name="size">Gets generated sentence total size</param>
        public void MapSentence(
            string text,
            out VertexPositionTexture[] vertices,
            out uint[] indices,
            out Vector2 size)
        {
            size = Vector2.Zero;

            Vector2 pos = Vector2.Zero;

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
                    FontMapChar chr = this[c];

                    VertexData[] cv;
                    uint[] ci;
                    ModelContent.CreateSprite(
                        pos,
                        chr.Width,
                        chr.Height,
                        0,
                        0,
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
                    Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPositionTexture(v)); });

                    pos.X += chr.Width - (int)(this.Size / 6);
                    if (chr.Height > height) height = chr.Height;
                }
            }

            pos.Y = height;

            vertices = vertList.ToArray();
            indices = indexList.ToArray();
            size = pos;
        }

        /// <summary>
        /// Dispose map resources
        /// </summary>
        public void Dispose()
        {
            if (this.Texture != null)
            {
                this.Texture.Dispose();
                this.Texture = null;
            }
        }
    }
}
