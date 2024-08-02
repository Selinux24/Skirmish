
namespace AISamples.Common.Persistence
{
    class LevelFile
    {
        public float[] Inputs { get; set; }
        public float[] Outputs { get; set; }
        public float[] Biases { get; set; }
        public float[][] Weights { get; set; }
    }
}
