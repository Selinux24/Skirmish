using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Font map
    /// </summary>
    public class FontMap : IDisposable
    {
        /// <summary>
        /// Sample character
        /// </summary>
        private const char sampleChar = 'X';
        /// <summary>
        /// Space string
        /// </summary>
        private const string spaceString = " ";

        /// <summary>
        /// Creates a font map of the specified font file and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="generator">Keycode generator</param>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        public static FontMap FromFile(Game game, string contentPath, FontMapKeycodeGenerator generator, string fontFileName, float size, FontMapStyles style)
        {
            var fileNames = ContentManager.FindPaths(contentPath, fontFileName);
            if (!fileNames.Any())
            {
                Logger.WriteWarning(nameof(FontMap), $"Font resource not found: {fontFileName}");
                return null;
            }

            var fileName = fileNames.First();
            var fontName = Game.Fonts.GetFromFileFontName(fileName);

            var fMap = FontMapCache.Get(fontName, size, style);
            if (fMap != null)
            {
                return fMap;
            }

            var fontDesc = Game.Fonts.FromFile(generator, FontMapProcessParameters.Default, fileName, size, style);
            fMap = new FontMap(fontDesc);
            fMap.Initialize(game);

            //Add map to the font cache
            FontMapCache.Add(fMap);

            return fMap;
        }
        /// <summary>
        /// Creates a font map of the specified font mapping
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="fontMapping">Font mapping</param>
        /// <returns>Returns the created font map</returns>
        public static FontMap FromMap(Game game, string contentPath, FontMapping fontMapping)
        {
            string fontName = Path.Combine(contentPath, fontMapping.ImageFile);

            var fMap = FontMapCache.Get(fontName);
            if (fMap != null)
            {
                return fMap;
            }

            fMap = new FontMap
            {
                FontName = fontName,
                Texture = game.ResourceManager.RequestResource(fontName, false)
            };

            string fontMapName = Path.Combine(contentPath, fontMapping.MapFile);

            string[] charMaps = File.ReadAllLines(fontMapName);
            foreach (var charMap in charMaps)
            {
                if (string.IsNullOrWhiteSpace(charMap))
                {
                    continue;
                }

                if (charMap.StartsWith("size:", StringComparison.OrdinalIgnoreCase))
                {
                    Vector2 textureSize = FromMap(charMap[6..]);

                    fMap.TextureWidth = (int)textureSize.X;
                    fMap.TextureHeight = (int)textureSize.Y;

                    continue;
                }

                int leftTopIndex = charMap.IndexOf(':') + 1;
                int rightBottomIndex = charMap.IndexOf(';', leftTopIndex) + 1;

                char c = charMap[0];
                Vector2 topLeft = FromMap(charMap[leftTopIndex..rightBottomIndex]);
                Vector2 bottomRight = FromMap(charMap[rightBottomIndex..]);

                var chr = new FontMapChar()
                {
                    X = topLeft.X,
                    Y = topLeft.Y,
                    Width = bottomRight.X - topLeft.X,
                    Height = bottomRight.Y - topLeft.Y,
                };

                fMap.map.Add(c, chr);
            }
            fMap.Initialize(game);

            return fMap;
        }
        /// <summary>
        /// Creates a font map of the specified font family and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="generator">Keycode generator</param>
        /// <param name="fontFamilies">Comma separated font names</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        /// <remarks>The font family must exists in the FontFamily.Families collection</remarks>
        public static FontMap FromFamily(Game game, FontMapKeycodeGenerator generator, string fontFamilies, float size, FontMapStyles style)
        {
            var fontFamily = Game.Fonts.FindFonts(fontFamilies);
            if (string.IsNullOrEmpty(fontFamily))
            {
                Logger.WriteWarning(nameof(FontMap), $"Font familiy not found in the graphic context: {fontFamilies}");

                return null;
            }

            var fMap = FontMapCache.Get(fontFamily, size, style);
            if (fMap != null)
            {
                return fMap;
            }

            var fontDesc = Game.Fonts.FromFamilyName(generator, FontMapProcessParameters.Default, fontFamily, size, style);
            fMap = new FontMap(fontDesc);
            fMap.Initialize(game);

            //Add map to the font cache
            FontMapCache.Add(fMap);

            return fMap;
        }
        /// <summary>
        /// Reads a vector from a font-map line
        /// </summary>
        /// <param name="mapBitz">Map text bitz</param>
        /// <returns>Returns a vector</returns>
        private static Vector2 FromMap(string mapBitz)
        {
            string[] bitz = mapBitz?.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (bitz.Length < 2)
            {
                return Vector2.Zero;
            }

            //Clean ';'
            string xValue = bitz[0].Replace(";", "");
            string yValue = bitz[1].Replace(";", "");

            if (!float.TryParse(xValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            {
                Logger.WriteWarning(nameof(FontMap), $"Bad coordinate descriptor for X value. Single spected: {xValue}");
            }

            if (!float.TryParse(yValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                Logger.WriteWarning(nameof(FontMap), $"Bad coordinate descriptor for Y value. Single spected: {yValue}");
            }

            return new Vector2(x, y);
        }

        /// <summary>
        /// Map
        /// </summary>
        private Dictionary<char, FontMapChar> map = [];
        /// <summary>
        /// Bitmap stream
        /// </summary>
        private MemoryStream bitmapStream = null;
        /// <summary>
        /// Space size
        /// </summary>
        private Vector2 spaceSize = Vector2.Zero;

        /// <summary>
        /// Texure width
        /// </summary>
        protected int TextureWidth = 0;
        /// <summary>
        /// Texture height
        /// </summary>
        protected int TextureHeight = 0;

        /// <summary>
        /// Font name
        /// </summary>
        public string FontName { get; private set; }
        /// <summary>
        /// Font size
        /// </summary>
        public float FontSize { get; private set; }
        /// <summary>
        /// Font style
        /// </summary>
        public FontMapStyles FontStyle { get; private set; }
        /// <summary>
        /// Font texture
        /// </summary>
        public EngineShaderResourceView Texture { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FontMap()
            : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public FontMap(FontMapDescription fontDesc)
            : base()
        {
            FontName = fontDesc.FontName;
            FontSize = fontDesc.FontSize;
            FontStyle = fontDesc.FontStyle;

            map = fontDesc.Map;

            TextureWidth = fontDesc.TextureWidth;
            TextureHeight = fontDesc.TextureHeight;

            bitmapStream = fontDesc.ImageStream;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~FontMap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Texture?.Dispose();
                Texture = null;

                map?.Clear();
                map = null;

                bitmapStream?.Dispose();
                bitmapStream = null;
            }
        }

        /// <summary>
        /// Initializes the font resources
        /// </summary>
        /// <param name="game">Game</param>
        public void Initialize(Game game)
        {
            spaceSize = MapSpace();
            if (spaceSize == Vector2.Zero)
            {
                spaceSize = MapSampleChar();
            }

            if (bitmapStream != null)
            {
                Texture = game.ResourceManager.RequestResource(bitmapStream, false);
            }
        }

        /// <summary>
        /// Maps a space
        /// </summary>
        /// <returns>Returns the space size</returns>
        private Vector2 MapSpace()
        {
            bool firstPart = true;
            var tmpPos = Vector2.Zero;
            var desc = FontMapSentenceDescriptor.OneCharDescriptor;
            desc.Clear();
            MapPart(FontMapParsedPart.Space, false, float.MaxValue, ref firstPart, ref tmpPos, ref desc, out float height);

            return new(tmpPos.X, height);
        }
        /// <summary>
        /// Maps the sample character
        /// </summary>
        /// <returns>Returns the space size</returns>
        private Vector2 MapSampleChar()
        {
            bool firstPart = true;
            var tmpPos = Vector2.Zero;
            var desc = FontMapSentenceDescriptor.OneCharDescriptor;
            desc.Clear();
            MapPart(FontMapParsedPart.FromChar(GetSampleCharacter()), false, float.MaxValue, ref firstPart, ref tmpPos, ref desc, out float height);

            return new(tmpPos.X, height);
        }
        /// <summary>
        /// Gets the sample character
        /// </summary>
        /// <returns>Returns the sample character</returns>
        /// <remarks>Used for map the space if not specified</remarks>
        private char GetSampleCharacter()
        {
            var keys = map.Keys.ToArray();

            if (!Array.Exists(keys, c => c == sampleChar))
            {
                return keys.FirstOrDefault();
            }

            return sampleChar;
        }

        /// <summary>
        /// Maps a sentence
        /// </summary>
        /// <param name="sentenceDesc">Sentence</param>
        /// <param name="processShadows">Process text shadow</param>
        /// <param name="rect">Bounds rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a sentence descriptor</returns>
        public void MapSentence(
            FontMapParsedSentence sentenceDesc,
            bool processShadows,
            float width,
            ref FontMapSentenceDescriptor desc)
        {
            if (sentenceDesc.Count() <= 0)
            {
                return;
            }

            var pos = Vector2.Zero;
            bool firstPartInLine = true;

            for (int i = 0; i < sentenceDesc.Count(); i++)
            {
                var partDesc = sentenceDesc.GetPart(i);

                if (string.IsNullOrEmpty(partDesc.Text))
                {
                    //Discard empty parts
                    continue;
                }

                if (partDesc.Text == Environment.NewLine)
                {
                    //Move the position to the new line
                    pos.X = 0;
                    pos.Y -= (int)spaceSize.Y;

                    firstPartInLine = true;

                    continue;
                }

                if (partDesc.Text == spaceString)
                {
                    //Add a space
                    pos.X += (int)spaceSize.X;

                    continue;
                }

                //Map the part
                MapPart(partDesc, processShadows, width, ref firstPartInLine, ref pos, ref desc, out _);
            }
        }
        /// <summary>
        /// Maps a part
        /// </summary>
        /// <param name="partDesc">Part to map</param>
        /// <param name="processShadows">Process text shadow</param>
        /// <param name="width">Maximum width</param>
        /// <param name="firstPartInLine">It's the first part in the line</param>
        /// <param name="pos">Position</param>
        /// <param name="desc">Sentence descriptor</param>
        /// <param name="height">Returns the mapped part height</param>
        private void MapPart(
            FontMapParsedPart partDesc,
            bool processShadows,
            float width,
            ref bool firstPartInLine,
            ref Vector2 pos,
            ref FontMapSentenceDescriptor desc,
            out float height)
        {
            height = 0;

            //Store current character position and index
            int prevIndex = (int)desc.VertexCount;
            var prevPos = pos;

            for (int i = 0; i < partDesc.Count(); i++)
            {
                char c = partDesc.Text[i];

                if (!map.ContainsKey(c))
                {
                    //Discard unmapped characters
                    continue;
                }

                var chr = map[c];
                var chrColor = processShadows ? partDesc.GetShadowColor(i) : partDesc.GetColor(i);

                MapChar(chr, chrColor, ref pos, ref desc);

                //Store maximum height
                height = MathF.Max(height, chr.Height);
            }

            if (pos.X > width)
            {
                //Maximum width exceeded

                if (firstPartInLine)
                {
                    //The first part remains in the same position
                    firstPartInLine = false;

                    return;
                }

                //Move the position to the last character of the new line
                pos.X -= (int)prevPos.X;
                pos.Y -= (int)height;

                //Move the part to the next line
                var diff = new Vector3(prevPos.X, height, 0);
                for (int index = prevIndex; index < desc.VertexCount; index++)
                {
                    desc.Vertices[index].Position -= diff;
                }
            }

            firstPartInLine = false;
        }
        /// <summary>
        /// Maps a character
        /// </summary>
        /// <param name="chr">Character</param>
        /// <param name="color">Character color</param>
        /// <param name="pos">Position</param>
        /// <param name="vertList">Vertex list to fill</param>
        /// <param name="indexList">Index list to fill</param>
        private void MapChar(
            FontMapChar chr,
            Color4 color,
            ref Vector2 pos,
            ref FontMapSentenceDescriptor desc)
        {
            //Creates the texture UVMap
            var uv = GeometryUtil.CreateUVMap(
                chr.Width, chr.Height,
                chr.X, chr.Y,
                TextureWidth, TextureHeight);

            //Creates the sprite
            var s = GeometryUtil.CreateSprite(
                pos,
                chr.Width, chr.Height, 0, 0,
                uv);

            //Move the cursor position to the next character
            pos.X += chr.Width - MathF.Sqrt(chr.Width);

            //Store data
            desc.Add(s.Indices, s.Vertices, s.Uvs, color);
        }
        /// <summary>
        /// Gets the font's white space size
        /// </summary>
        public Vector2 GetSpaceSize()
        {
            return spaceSize;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{FontName} {FontSize} {FontStyle}";
        }
    }
}
