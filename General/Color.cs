namespace AffinityChess.General
{
    public static class Color
    {
        public const int White = 0;
        public const int Black = 1;

        public static int Invert(int color)
        {
            return color == White ? Black : White;
        }
    }
}
