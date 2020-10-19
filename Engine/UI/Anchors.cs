using System;

namespace Engine.UI
{
    [Flags]
    public enum Anchors
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
        HorizontalCenter = 16,
        VerticalCenter = 32,
        Center = HorizontalCenter | VerticalCenter,
    }
}
