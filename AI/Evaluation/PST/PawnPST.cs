using AffinityChess.General;

namespace AffinityChess.AI
{
    public static class PawnPST
    {
        private static int[] openingTable = new int[]
        {
            0,   0,   0,   0,   0,   0,  0,   0,
           98, 134,  61,  95,  68, 126, 34, -11,
           -6,   7,  26,  31,  65,  56, 25, -20,
          -14,  13,   6,  21,  23,  12, 17, -23,
          -27,  -2,  -5,  12,  17,   6, 10, -25,
          -26,  -4,  -4, -10,   3,   3, 33, -12,
          -35,  -1, -20, -23, -15,  24, 38, -22,
            0,   0,   0,   0,   0,   0,  0,   0,
        };
        private static int[] endingTable = new int[]
        {
            0,   0,   0,   0,   0,   0,   0,   0,
          178, 173, 158, 134, 147, 132, 165, 187,
           94, 100,  85,  67,  56,  53,  82,  84,
           32,  24,  13,   5,  -2,   4,  17,  17,
           13,   9,  -3,  -7,  -7,  -8,   3,  -1,
            4,   7,  -6,   1,   0,  -5,  -1,  -8,
           13,   8,   8,  10,  13,   0,   2,  -7,
            0,   0,   0,   0,   0,   0,   0,   0,
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



