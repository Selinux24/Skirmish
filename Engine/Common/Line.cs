using SharpDX;

namespace Engine.Common
{
    public struct Line
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public float Length
        {
            get
            {
                return Vector3.Distance(this.Point1, this.Point2);
            }
        }
    }
}
