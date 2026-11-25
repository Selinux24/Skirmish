// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Engine.Mathematics
{
    /// <summary>
    /// List of predefined <see cref="Color"/>.
    /// </summary>
    public partial struct Color
    {
        /// <summary>
        /// Zero color.
        /// </summary>
        public static readonly Color Zero = FromRgba(0x00000000);
        /// <summary>
        /// Transparent color.
        /// </summary>
        public static readonly Color Transparent = FromRgba(0x00000000);

        /// <summary>
        /// AliceBlue color.
        /// </summary>
        public static readonly Color AliceBlue = FromRgba(0xF0F8FFFF);
        /// <summary>
        /// AntiqueWhite color.
        /// </summary>
        public static readonly Color AntiqueWhite = FromRgba(0xFAEBD7FF);
        /// <summary>
        /// Aqua color.
        /// </summary>
        public static readonly Color Aqua = FromRgba(0x00FFFFFF);
        /// <summary>
        /// Aquamarine color.
        /// </summary>
        public static readonly Color Aquamarine = FromRgba(0x7FFFD4FF);
        /// <summary>
        /// Azure color.
        /// </summary>
        public static readonly Color Azure = FromRgba(0xF0FFFFFF);
        /// <summary>
        /// Beige color.
        /// </summary>
        public static readonly Color Beige = FromRgba(0xF5F5DCFF);
        /// <summary>
        /// Bisque color.
        /// </summary>
        public static readonly Color Bisque = FromRgba(0xFFE4C4FF);
        /// <summary>
        /// Black color.
        /// </summary>
        public static readonly Color Black = FromRgba(0x000000FF);
        /// <summary>
        /// BlanchedAlmond color.
        /// </summary>
        public static readonly Color BlanchedAlmond = FromRgba(0xFFEBCDFF);
        /// <summary>
        /// Blue color.
        /// </summary>
        public static readonly Color Blue = FromRgba(0x0000FFFF);
        /// <summary>
        /// BlueViolet color.
        /// </summary>
        public static readonly Color BlueViolet = FromRgba(0x8A2BE2FF);
        /// <summary>
        /// Brown color.
        /// </summary>
        public static readonly Color Brown = FromRgba(0xA52A2AFF);
        /// <summary>
        /// BurlyWood color.
        /// </summary>
        public static readonly Color BurlyWood = FromRgba(0xDEB887FF);
        /// <summary>
        /// CadetBlue color.
        /// </summary>
        public static readonly Color CadetBlue = FromRgba(0x5F9EA0FF);
        /// <summary>
        /// Chartreuse color.
        /// </summary>
        public static readonly Color Chartreuse = FromRgba(0x7FFF00FF);
        /// <summary>
        /// Chocolate color.
        /// </summary>
        public static readonly Color Chocolate = FromRgba(0xD2691EFF);
        /// <summary>
        /// Coral color.
        /// </summary>
        public static readonly Color Coral = FromRgba(0xFF7F50FF);
        /// <summary>
        /// CornflowerBlue color.
        /// </summary>
        public static readonly Color CornflowerBlue = FromRgba(0x6495EDFF);
        /// <summary>
        /// Cornsilk color.
        /// </summary>
        public static readonly Color Cornsilk = FromRgba(0xFFF8DCFF);
        /// <summary>
        /// Crimson color.
        /// </summary>
        public static readonly Color Crimson = FromRgba(0xDC143CFF);
        /// <summary>
        /// Cyan color.
        /// </summary>
        public static readonly Color Cyan = FromRgba(0x00FFFFFF);
        /// <summary>
        /// DarkBlue color.
        /// </summary>
        public static readonly Color DarkBlue = FromRgba(0x00008BFF);
        /// <summary>
        /// DarkCyan color.
        /// </summary>
        public static readonly Color DarkCyan = FromRgba(0x008B8BFF);
        /// <summary>
        /// DarkGoldenrod color.
        /// </summary>
        public static readonly Color DarkGoldenrod = FromRgba(0xB8860BFF);
        /// <summary>
        /// DarkGray color.
        /// </summary>
        public static readonly Color DarkGray = FromRgba(0xA9A9A9FF);
        /// <summary>
        /// DarkGreen color.
        /// </summary>
        public static readonly Color DarkGreen = FromRgba(0x006400FF);
        /// <summary>
        /// DarkKhaki color.
        /// </summary>
        public static readonly Color DarkKhaki = FromRgba(0xBDB76BFF);
        /// <summary>
        /// DarkMagenta color.
        /// </summary>
        public static readonly Color DarkMagenta = FromRgba(0x8B008BFF);
        /// <summary>
        /// DarkOliveGreen color.
        /// </summary>
        public static readonly Color DarkOliveGreen = FromRgba(0x556B2FFF);
        /// <summary>
        /// DarkOrange color.
        /// </summary>
        public static readonly Color DarkOrange = FromRgba(0xFF8C00FF);
        /// <summary>
        /// DarkOrchid color.
        /// </summary>
        public static readonly Color DarkOrchid = FromRgba(0x9932CCFF);
        /// <summary>
        /// DarkRed color.
        /// </summary>
        public static readonly Color DarkRed = FromRgba(0x8B0000FF);
        /// <summary>
        /// DarkSalmon color.
        /// </summary>
        public static readonly Color DarkSalmon = FromRgba(0xE9967AFF);
        /// <summary>
        /// DarkSeaGreen color.
        /// </summary>
        public static readonly Color DarkSeaGreen = FromRgba(0x8FBC8BFF);
        /// <summary>
        /// DarkSlateBlue color.
        /// </summary>
        public static readonly Color DarkSlateBlue = FromRgba(0x483D8BFF);
        /// <summary>
        /// DarkSlateGray color.
        /// </summary>
        public static readonly Color DarkSlateGray = FromRgba(0x2F4F4FFF);
        /// <summary>
        /// DarkTurquoise color.
        /// </summary>
        public static readonly Color DarkTurquoise = FromRgba(0x00CED1FF);
        /// <summary>
        /// DarkViolet color.
        /// </summary>
        public static readonly Color DarkViolet = FromRgba(0x9400D3FF);
        /// <summary>
        /// DeepPink color.
        /// </summary>
        public static readonly Color DeepPink = FromRgba(0xFF1493FF);
        /// <summary>
        /// DeepSkyBlue color.
        /// </summary>
        public static readonly Color DeepSkyBlue = FromRgba(0x00BFFFFF);
        /// <summary>
        /// DimGray color.
        /// </summary>
        public static readonly Color DimGray = FromRgba(0x696969FF);
        /// <summary>
        /// DodgerBlue color.
        /// </summary>
        public static readonly Color DodgerBlue = FromRgba(0x1E90FFFF);
        /// <summary>
        /// Firebrick color.
        /// </summary>
        public static readonly Color Firebrick = FromRgba(0xB22222FF);
        /// <summary>
        /// FloralWhite color.
        /// </summary>
        public static readonly Color FloralWhite = FromRgba(0xFFFAF0FF);
        /// <summary>
        /// ForestGreen color.
        /// </summary>
        public static readonly Color ForestGreen = FromRgba(0x228B22FF);
        /// <summary>
        /// Fuchsia color.
        /// </summary>
        public static readonly Color Fuchsia = FromRgba(0xFF00FFFF);
        /// <summary>
        /// Gainsboro color.
        /// </summary>
        public static readonly Color Gainsboro = FromRgba(0xDCDCDCFF);
        /// <summary>
        /// GhostWhite color.
        /// </summary>
        public static readonly Color GhostWhite = FromRgba(0xF8F8FFFF);
        /// <summary>
        /// Gold color.
        /// </summary>
        public static readonly Color Gold = FromRgba(0xFFD700FF);
        /// <summary>
        /// Goldenrod color.
        /// </summary>
        public static readonly Color Goldenrod = FromRgba(0xDAA520FF);
        /// <summary>
        /// Gray color.
        /// </summary>
        public static readonly Color Gray = FromRgba(0x808080FF);
        /// <summary>
        /// Green color.
        /// </summary>
        public static readonly Color Green = FromRgba(0x008000FF);
        /// <summary>
        /// GreenYellow color.
        /// </summary>
        public static readonly Color GreenYellow = FromRgba(0xADFF2FFF);
        /// <summary>
        /// Honeydew color.
        /// </summary>
        public static readonly Color Honeydew = FromRgba(0xF0FFF0FF);
        /// <summary>
        /// HotPink color.
        /// </summary>
        public static readonly Color HotPink = FromRgba(0xFF69B4FF);
        /// <summary>
        /// IndianRed color.
        /// </summary>
        public static readonly Color IndianRed = FromRgba(0xCD5C5CFF);
        /// <summary>
        /// Indigo color.
        /// </summary>
        public static readonly Color Indigo = FromRgba(0x4B0082FF);
        /// <summary>
        /// Ivory color.
        /// </summary>
        public static readonly Color Ivory = FromRgba(0xFFFFF0FF);
        /// <summary>
        /// Khaki color.
        /// </summary>
        public static readonly Color Khaki = FromRgba(0xF0E68CFF);
        /// <summary>
        /// Lavender color.
        /// </summary>
        public static readonly Color Lavender = FromRgba(0xE6E6FAFF);
        /// <summary>
        /// LavenderBlush color.
        /// </summary>
        public static readonly Color LavenderBlush = FromRgba(0xFFF0F5FF);
        /// <summary>
        /// LawnGreen color.
        /// </summary>
        public static readonly Color LawnGreen = FromRgba(0x7CFC00FF);
        /// <summary>
        /// LemonChiffon color.
        /// </summary>
        public static readonly Color LemonChiffon = FromRgba(0xFFFACDFF);
        /// <summary>
        /// LightBlue color.
        /// </summary>
        public static readonly Color LightBlue = FromRgba(0xADD8E6FF);
        /// <summary>
        /// LightCoral color.
        /// </summary>
        public static readonly Color LightCoral = FromRgba(0xF08080FF);
        /// <summary>
        /// LightCyan color.
        /// </summary>
        public static readonly Color LightCyan = FromRgba(0xE0FFFFFF);
        /// <summary>
        /// LightGoldenrodYellow color.
        /// </summary>
        public static readonly Color LightGoldenrodYellow = FromRgba(0xFAFAD2FF);
        /// <summary>
        /// LightGray color.
        /// </summary>
        public static readonly Color LightGray = FromRgba(0xD3D3D3FF);
        /// <summary>
        /// LightGreen color.
        /// </summary>
        public static readonly Color LightGreen = FromRgba(0x90EE90FF);
        /// <summary>
        /// LightPink color.
        /// </summary>
        public static readonly Color LightPink = FromRgba(0xFFB6C1FF);
        /// <summary>
        /// LightSalmon color.
        /// </summary>
        public static readonly Color LightSalmon = FromRgba(0xFFA07AFF);
        /// <summary>
        /// LightSeaGreen color.
        /// </summary>
        public static readonly Color LightSeaGreen = FromRgba(0x20B2AAFF);
        /// <summary>
        /// LightSkyBlue color.
        /// </summary>
        public static readonly Color LightSkyBlue = FromRgba(0x87CEFAFF);
        /// <summary>
        /// LightSlateGray color.
        /// </summary>
        public static readonly Color LightSlateGray = FromRgba(0x778899FF);
        /// <summary>
        /// LightSteelBlue color.
        /// </summary>
        public static readonly Color LightSteelBlue = FromRgba(0xB0C4DEFF);
        /// <summary>
        /// LightYellow color.
        /// </summary>
        public static readonly Color LightYellow = FromRgba(0xFFFFE0FF);
        /// <summary>
        /// Lime color.
        /// </summary>
        public static readonly Color Lime = FromRgba(0x00FF00FF);
        /// <summary>
        /// LimeGreen color.
        /// </summary>
        public static readonly Color LimeGreen = FromRgba(0x32CD32FF);
        /// <summary>
        /// Linen color.
        /// </summary>
        public static readonly Color Linen = FromRgba(0xFAF0E6FF);
        /// <summary>
        /// Magenta color.
        /// </summary>
        public static readonly Color Magenta = FromRgba(0xFF00FFFF);
        /// <summary>
        /// Maroon color.
        /// </summary>
        public static readonly Color Maroon = FromRgba(0x800000FF);
        /// <summary>
        /// MediumAquamarine color.
        /// </summary>
        public static readonly Color MediumAquamarine = FromRgba(0x66CDAAFF);
        /// <summary>
        /// MediumBlue color.
        /// </summary>
        public static readonly Color MediumBlue = FromRgba(0x0000CDFF);
        /// <summary>
        /// MediumOrchid color.
        /// </summary>
        public static readonly Color MediumOrchid = FromRgba(0xBA55D3FF);
        /// <summary>
        /// MediumPurple color.
        /// </summary>
        public static readonly Color MediumPurple = FromRgba(0x9370DBFF);
        /// <summary>
        /// MediumSeaGreen color.
        /// </summary>
        public static readonly Color MediumSeaGreen = FromRgba(0x3CB371FF);
        /// <summary>
        /// MediumSlateBlue color.
        /// </summary>
        public static readonly Color MediumSlateBlue = FromRgba(0x7B68EEFF);
        /// <summary>
        /// MediumSpringGreen color.
        /// </summary>
        public static readonly Color MediumSpringGreen = FromRgba(0x00FA9AFF);
        /// <summary>
        /// MediumTurquoise color.
        /// </summary>
        public static readonly Color MediumTurquoise = FromRgba(0x48D1CCFF);
        /// <summary>
        /// MediumVioletRed color.
        /// </summary>
        public static readonly Color MediumVioletRed = FromRgba(0xC71585FF);
        /// <summary>
        /// MidnightBlue color.
        /// </summary>
        public static readonly Color MidnightBlue = FromRgba(0x191970FF);
        /// <summary>
        /// MintCream color.
        /// </summary>
        public static readonly Color MintCream = FromRgba(0xF5FFFAFF);
        /// <summary>
        /// MistyRose color.
        /// </summary>
        public static readonly Color MistyRose = FromRgba(0xFFE4E1FF);
        /// <summary>
        /// Moccasin color.
        /// </summary>
        public static readonly Color Moccasin = FromRgba(0xFFE4B5FF);
        /// <summary>
        /// NavajoWhite color.
        /// </summary>
        public static readonly Color NavajoWhite = FromRgba(0xFFDEADFF);
        /// <summary>
        /// Navy color.
        /// </summary>
        public static readonly Color Navy = FromRgba(0x000080FF);
        /// <summary>
        /// OldLace color.
        /// </summary>
        public static readonly Color OldLace = FromRgba(0xFDF5E6FF);
        /// <summary>
        /// Olive color.
        /// </summary>
        public static readonly Color Olive = FromRgba(0x808000FF);
        /// <summary>
        /// OliveDrab color.
        /// </summary>
        public static readonly Color OliveDrab = FromRgba(0x6B8E23FF);
        /// <summary>
        /// Orange color.
        /// </summary>
        public static readonly Color Orange = FromRgba(0xFFA500FF);
        /// <summary>
        /// OrangeRed color.
        /// </summary>
        public static readonly Color OrangeRed = FromRgba(0xFF4500FF);
        /// <summary>
        /// Orchid color.
        /// </summary>
        public static readonly Color Orchid = FromRgba(0xDA70D6FF);
        /// <summary>
        /// PaleGoldenrod color.
        /// </summary>
        public static readonly Color PaleGoldenrod = FromRgba(0xEEE8AAFF);
        /// <summary>
        /// PaleGreen color.
        /// </summary>
        public static readonly Color PaleGreen = FromRgba(0x98FB98FF);
        /// <summary>
        /// PaleTurquoise color.
        /// </summary>
        public static readonly Color PaleTurquoise = FromRgba(0xAFEEEEFF);
        /// <summary>
        /// PaleVioletRed color.
        /// </summary>
        public static readonly Color PaleVioletRed = FromRgba(0xDB7093FF);
        /// <summary>
        /// PapayaWhip color.
        /// </summary>
        public static readonly Color PapayaWhip = FromRgba(0xFFEFD5FF);
        /// <summary>
        /// PeachPuff color.
        /// </summary>
        public static readonly Color PeachPuff = FromRgba(0xFFDAB9FF);
        /// <summary>
        /// Peru color.
        /// </summary>
        public static readonly Color Peru = FromRgba(0xCD853FFF);
        /// <summary>
        /// Pink color.
        /// </summary>
        public static readonly Color Pink = FromRgba(0xFFC0CBFF);
        /// <summary>
        /// Plum color.
        /// </summary>
        public static readonly Color Plum = FromRgba(0xDDA0DDFF);
        /// <summary>
        /// PowderBlue color.
        /// </summary>
        public static readonly Color PowderBlue = FromRgba(0xB0E0E6FF);
        /// <summary>
        /// Purple color.
        /// </summary>
        public static readonly Color Purple = FromRgba(0x800080FF);
        /// <summary>
        /// Red color.
        /// </summary>
        public static readonly Color Red = FromRgba(0xFF0000FF);
        /// <summary>
        /// RosyBrown color.
        /// </summary>
        public static readonly Color RosyBrown = FromRgba(0xBC8F8FFF);
        /// <summary>
        /// RoyalBlue color.
        /// </summary>
        public static readonly Color RoyalBlue = FromRgba(0x4169E1FF);
        /// <summary>
        /// SaddleBrown color.
        /// </summary>
        public static readonly Color SaddleBrown = FromRgba(0x8B4513FF);
        /// <summary>
        /// Salmon color.
        /// </summary>
        public static readonly Color Salmon = FromRgba(0xFA8072FF);
        /// <summary>
        /// SandyBrown color.
        /// </summary>
        public static readonly Color SandyBrown = FromRgba(0xF4A460FF);
        /// <summary>
        /// SeaGreen color.
        /// </summary>
        public static readonly Color SeaGreen = FromRgba(0x2E8B57FF);
        /// <summary>
        /// SeaShell color.
        /// </summary>
        public static readonly Color SeaShell = FromRgba(0xFFF5EEFF);
        /// <summary>
        /// Sienna color.
        /// </summary>
        public static readonly Color Sienna = FromRgba(0xA0522DFF);
        /// <summary>
        /// Silver color.
        /// </summary>
        public static readonly Color Silver = FromRgba(0xC0C0C0FF);
        /// <summary>
        /// SkyBlue color.
        /// </summary>
        public static readonly Color SkyBlue = FromRgba(0x87CEEBFF);
        /// <summary>
        /// SlateBlue color.
        /// </summary>
        public static readonly Color SlateBlue = FromRgba(0x6A5ACDFF);
        /// <summary>
        /// SlateGray color.
        /// </summary>
        public static readonly Color SlateGray = FromRgba(0x708090FF);
        /// <summary>
        /// Snow color.
        /// </summary>
        public static readonly Color Snow = FromRgba(0xFFFAFAFF);
        /// <summary>
        /// SpringGreen color.
        /// </summary>
        public static readonly Color SpringGreen = FromRgba(0x00FF7FFF);
        /// <summary>
        /// SteelBlue color.
        /// </summary>
        public static readonly Color SteelBlue = FromRgba(0x4682B4FF);
        /// <summary>
        /// Tan color.
        /// </summary>
        public static readonly Color Tan = FromRgba(0xD2B48CFF);
        /// <summary>
        /// Teal color.
        /// </summary>
        public static readonly Color Teal = FromRgba(0x008080FF);
        /// <summary>
        /// Thistle color.
        /// </summary>
        public static readonly Color Thistle = FromRgba(0xD8BFD8FF);
        /// <summary>
        /// Tomato color.
        /// </summary>
        public static readonly Color Tomato = FromRgba(0xFF6347FF);
        /// <summary>
        /// Turquoise color.
        /// </summary>
        public static readonly Color Turquoise = FromRgba(0x40E0D0FF);
        /// <summary>
        /// Violet color.
        /// </summary>
        public static readonly Color Violet = FromRgba(0xEE82EEFF);
        /// <summary>
        /// Wheat color.
        /// </summary>
        public static readonly Color Wheat = FromRgba(0xF5DEB3FF);
        /// <summary>
        /// White color.
        /// </summary>
        public static readonly Color White = FromRgba(0xFFFFFFFF);
        /// <summary>
        /// WhiteSmoke color.
        /// </summary>
        public static readonly Color WhiteSmoke = FromRgba(0xF5F5F5FF);
        /// <summary>
        /// Yellow color.
        /// </summary>
        public static readonly Color Yellow = FromRgba(0xFFFF00FF);
        /// <summary>
        /// YellowGreen color.
        /// </summary>
        public static readonly Color YellowGreen = FromRgba(0x9ACD32FF);
    }
}
