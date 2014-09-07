
namespace Common
{
    public struct FontChar
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public override string ToString()
        {
            return string.Format("X: {0}; Y: {1}; Width: {2}; Height: {3}", this.X, this.Y, this.Width, this.Height);
        }
    }
}
