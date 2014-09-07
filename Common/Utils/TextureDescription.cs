
namespace Common.Utils
{
    public class TextureDescription
    {
        public string Name { get; set; }
        public string Texture
        {
            get
            {
                if (this.TextureArray != null && this.TextureArray.Length == 1)
                {
                    return this.TextureArray[0];
                }

                return null;
            }
        }
        public string[] TextureArray { get; set; }
        public bool IsArray
        {
            get
            {
                return (this.TextureArray != null && this.TextureArray.Length > 1);
            }
        }
    }
}
