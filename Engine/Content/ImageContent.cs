using System.IO;

namespace Engine.Content
{
    public class ImageContent
    {
        private string[] paths = null;

        public MemoryStream Stream { get; set; }
        public string Path
        {
            get
            {
                return this.paths != null && this.paths.Length == 1 ? this.paths[0] : null;
            }
            set
            {
                this.paths = new[] { value };
            }
        }
        public string[] Paths
        {
            get
            {
                return this.paths != null && this.paths.Length > 1 ? this.paths : null;
            }
            set
            {
                this.paths = value;
            }
        }
        public bool IsArray
        {
            get
            {
                return this.paths != null && this.paths.Length > 1;
            }
        }
        public bool IsCubic { get; set; }
        public int CubicFaceSize { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Path))
            {
                return string.Format("Path: {0}; ", this.Path);
            }
            else if (this.Stream != null)
            {
                return string.Format("Stream: {0} bytes; ", this.Stream.Length);
            }
            else
            {
                return "Empty;";
            }
        }
    }
}
