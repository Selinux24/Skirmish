
namespace AISamples.SceneCodingWithRadu
{
    class CarControls
    {
        public bool Forward { get; set; } = false;
        public bool Reverse { get; set; } = false;
        public bool Left { get; set; } = false;
        public bool Right { get; set; } = false;


        public CarControls(ControlTypes controlType)
        {
            switch (controlType)
            {
                case ControlTypes.Dummy:
                    Forward = true;
                    break;
                case ControlTypes.Player:
                default:
                    break;
            }
        }
    }

    enum ControlTypes
    {
        Player,
        Dummy,
    }
}
