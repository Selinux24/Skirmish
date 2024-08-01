using SharpDX;

namespace AISamples.Common
{
    static class Constants
    {
        public const string CommonResourcesFolder = "Common/Resources/";
        public const string MarkingsTexture = CommonResourcesFolder + "markings.png";
        public const string TreesResourcesFolder = CommonResourcesFolder + "Tree/";
        public const string TreesModel = "Tree1.json";

        const float Width = 2000f;
        const float Height = 1600f;

        const float CrossingLeft = 0f / Width;
        const float CrossingTop = 0f / Height;
        const float CrossingRight = 1323f / Width;
        const float CrossingBottom = 594f / Height;

        const float ParkingLeft = 1333f / Width;
        const float ParkingTop = 657f / Height;
        const float ParkingRight = 2000f / Width;
        const float ParkingBottom = 1239f / Height;

        const float StopLeft = 0f / Width;
        const float StopTop = 649f / Height;
        const float StopRight = 733f / Width;
        const float StopBottom = 1199f / Height;

        const float YieldLeft = 759f / Width;
        const float YieldTop = 652f / Height;
        const float YieldRight = 1318f / Width;
        const float YieldBottom = 1244f / Height;

        const float StartLeft = 1361f / Width;
        const float StartTop = 21f / Height;
        const float StartRight = 1635f / Width;
        const float StartBottom = 586f / Height;

        const float TargetLeft = 1699f / Width;
        const float TargetTop = 30f / Height;
        const float TargetRight = 1972f / Width;
        const float TargetBottom = 303f / Height;

        public static readonly Vector2 Black = new(1500f / Width, 180f / Height);
        public static readonly Vector2 White = new(150f / Width, 50f / Height);
        public static readonly Vector2 DarkGreen = new(45f / Width, 1260f / Height);
        public static readonly Vector2 Gray = new(110f / Width, 1260f / Height);
        public static readonly Vector2 Red = new(80f / Width, 1340f / Height);
        public static readonly Vector2 Yellow = new(80f / Width, 1420f / Height);
        public static readonly Vector2 Green = new(80f / Width, 1495f / Height);

        public static readonly Vector2[] CrossingUVs = [new(CrossingLeft, CrossingBottom), new(CrossingLeft, CrossingTop), new(CrossingRight, CrossingTop), new(CrossingRight, CrossingBottom)];
        public static readonly Vector2[] ParkingUVs = [new(ParkingRight, ParkingBottom), new(ParkingRight, ParkingTop), new(ParkingLeft, ParkingTop), new(ParkingLeft, ParkingBottom)];
        public static readonly Vector2[] StopUVs = [new(StopRight, StopBottom), new(StopRight, StopTop), new(StopLeft, StopTop), new(StopLeft, StopBottom)];
        public static readonly Vector2[] YieldUVs = [new(YieldLeft, YieldBottom), new(YieldLeft, YieldTop), new(YieldRight, YieldTop), new(YieldRight, YieldBottom)];
        public static readonly Vector2[] CarUVs = [new(StartLeft, StartBottom), new(StartLeft, StartTop), new(StartRight, StartTop), new(StartRight, StartBottom)];
        public static readonly Vector2[] TargetUVs = [new(TargetLeft, TargetBottom), new(TargetLeft, TargetTop), new(TargetRight, TargetTop), new(TargetRight, TargetBottom)];
    }
}
