using SharpDX;

namespace Common
{
    using Common.Utils;

    public class SceneTransforms
    {
        public Matrix World { get; set; }
        public Matrix WorldInverse { get; set; }
        public Matrix WorldViewProjection { get; set; }
        public Material Material { get; set; }
    }
}
