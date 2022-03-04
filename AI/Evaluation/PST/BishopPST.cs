using AffinityChess.General;

namespace AffinityChess.AI
{
    public static class BishopPST
    {
        private static int[] openingTable = new int[]
        {
            -29,   4, -82, -37, -25, -42,   7,  -8,
            -26,  16, -18, -13,  30,  59,  18, -47,
            -16,  37,  43,  40,  35,  50,  37,  -2,
             -4,   5,  19,  50,  37,  37,   7,  -2,
             -6,  13,  13,  26,  34,  12,  10,   4,
              0,  15,  15,  15,  14,  27,  18,  10,
              4,  15,  16,   0,   7,  21,  33,   1,
            -33,  -3, -14, -21, -13, -12, -39, -21,
        };
        private static int[] endingTable = new int[]
        {
            -14, -21, -11,  -8, -7,  -9, -17, -24,
             -8,  -4,   7, -12, -3, -13,  -4, -14,
              2,  -8,   0,  -1, -2,   6,   0,   4,
             -3,   9,  12,   9, 14,  10,   3,   2,
             -6,   3,  13,  19,  7,  10,  -3,  -9,
            -12,  -3,   8,  10, 13,   3,  -7, -15,
            -14, -18,  -7,  -1,  4,  -9, -15, -27,
            -23,  -9, -23,  -5, -9, -16,  -5, -17,
        };

        public static int[][] BuildOpening()
        {
            int[][] pieceTable = new int[2][];
            for (int color = Color.White; color <= Color.Black; color++)
            {
                pieceTable[color] = new int[64];
                for (int square = 0; square < 64; square++)
                {
                    int realSquare = color == Color.White ? square : GameConstants.boardHorizontalFlip[square];
                    pieceTable[color][square] = openingTable[realSquare];
                }
            }
            return pieceTable;
        }

        public static int[][] BuildEnding()
        {
            int[][] pieceTable = new int[2][];
            for (int color = Color.White; color <= Color.Black; color++)
            {
                pieceTable[color] = new int[64];
                for (int square = 0; square < 64; square++)
                {
                    int realSquare = color == Color.White ? square : GameConstants.boardHorizontalFlip[square];
                    pieceTable[color][square] = endingTable[realSquare];
                }
            }
            return pieceTable;
        }
    }
}
