
namespace Engine.PathFinding.RecastNavigation
{
    static class DirectionUtils
    {
        public static int GetDirOffsetX(int dir)
        {
            int[] offset = new[] { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            int[] offset = new[] { 0, 1, 0, -1 };
            return offset[dir & 0x03];
        }
        public static int RotateCW(int dir)
        {
            return (dir + 1) & 0x3;
        }
        public static int RotateCCW(int dir)
        {
            return (dir + 3) & 0x3;
        }
    }
}
