using AffinityChess.General;

namespace AffinityChess.AI
{
    public static class QueenPST
    {
        private static int[] openingTable = new int[]
        {
            -28,   0,  29,  12,  59,  44,  43,  45,
            -24, -39,  -5,   1, -16,  57,  28,  54,
            -13, -17,   7,   8,  29,  56,  47,  57,
            -27, -27, -16, -16,  -1,  17,  -2,   1,
             -9, -26,  -9, -10,  -2,  -4,   3,  -3,
            -14,   2, -11,  -2,  -5,   2,  14,   5,
            -35,  -8,  11,   2,   8,  15,  -3,   1,
             -1, -18,  -9,  10, -15, -25, -31, -50,
        };
        private static int[] endingTable = new int[]
        {
            -9,  22,  22,  27,  27,  19,  10,  20,
           -17,  20,  32,  41,  58,  25,  30,   0,
           -20,   6,   9,  49,  47,  35,  19,   9,
             3,  22,  24,  45,  57,  40,  57,  36,
           -18,  28,  19,  47,  31,  34,  39,  23,
           -16, -27,  15,   6,   9,  17,  10,   5,
           -22, -23, -30, -16, -16, -23, -36, -32,
           -33, -28, -22, -43,  -5, -32, -20, -41,
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
