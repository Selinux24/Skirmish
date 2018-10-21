using System;

namespace GameLogic.Rules.Enum
{
    [Flags]
    public enum SoldierClasses
    {
        None = 0,
        Line = 1,
        Support = 2,
        Heavy = 4,
        Medic = 8,
    }
}
