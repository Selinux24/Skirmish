
namespace AISamples.SceneCWRSelfDrivingCar
{
    class CarControls
    {
        public const int InputCount = 4;

        private readonly CarControlTypes controlType;

        public bool Forward { get; set; } = false;
        public bool Reverse { get; set; } = false;
        public bool Left { get; set; } = false;
        public bool Right { get; set; } = false;


        public CarControls(CarControlTypes controlType)
        {
            this.controlType = controlType;

            Reset();
        }

        public void SetControls(float[] outputs)
        {
            Forward = outputs[0] > 0f;
            Reverse = outputs[1] > 0f;
            Left = outputs[2] > 0f;
            Right = outputs[3] > 0f;

            if (Forward && Reverse)
            {
                Forward = false;
                Reverse = false;
            }

            if (Left && Right)
            {
                Left = false;
                Right = false;
            }
        }
        public void Reset()
        {
            Forward = false;
            Reverse = false;
            Left = false;
            Right = false;

            switch (controlType)
            {
                case CarControlTypes.Dummy:
                    Forward = true;
                    break;
                case CarControlTypes.Player:
                case CarControlTypes.AI:
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return $"FW:{ToInt(Forward)} | LF:{ToInt(Left)} | RF:{ToInt(Right)} | BK:{ToInt(Reverse)}";
        }
        private static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }
    }
}
