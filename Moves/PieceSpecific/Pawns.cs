using AffinityChess.General;

namespace AffinityChess.Moves
{
    public static class Pawns
    {
        public static ulong[][] AttackMasks = new ulong[2][];
        public static ulong[][] QuietMasksOneStep = new ulong[2][];
        public static ulong[][] QuietMasksTwoStep = new ulong[2][];

        // Generate all single and double pawn pushes
        public static void GenerateQuietMasks()
        {
            // Loop through colors
            for (int color = 0; color < 2; color++)
            {
                // Add all 64 squares for current color
                QuietMasksOneStep[color] = new ulong[64];
                QuietMasksTwoStep[color] = new ulong[64];

                // Loop through the board and add attack squares for the pawns
                for (int currentSquare = 0; currentSquare < 64; currentSquare++)
                {
                    QuietMasksOneStep[color][currentSquare] = GetQuietsFromSquare(color, currentSquare, true);
                    QuietMasksTwoStep[color][currentSquare] = GetQuietsFromSquare(color, currentSquare, false);
                }
            }
        }

        // Initialize the quiet tables
        private static ulong GetQuietsFromSquare(int color, int square, bool getOnlyOneStep)
        {
            ulong quiets = 0ul;
            ulong bitboard = 0ul;
            bitboard = BitOperations.SetBit(bitboard, square);

            if (color == Color.White)
            {
                if (getOnlyOneStep) quiets |= bitboard >> 8;
                else quiets |= (bitboard & GameConstants.Row2) >> 16;
            }
            else
            {
                if (getOnlyOneStep) quiets |= bitboard << 8;
                else quiets |= (bitboard & GameConstants.Row7) << 16;
            }

            return quiets;
        }

        // Generate all attack moves from squares
        public static void GenerateAttackMask()
        {
            // Loop through colors
            for (int color = 0; color < 2; color++)
            {
                // Add all 64 squares for current color
                AttackMasks[color] = new ulong[64];

                // Loop through the board and add attack squares for the pawns
                for (int currentSquare = 0; currentSquare < 64; currentSquare++)
                {
                    AttackMasks[color][currentSquare] = GetAttacksFromSquare(color, currentSquare);
                }
            }
        }

        // Initialize the attack tables
        private static ulong GetAttacksFromSquare(int color, int square)
        {
            ulong attacks = 0ul;
            ulong bitboard = 0ul;
            bitboard = BitOperations.SetBit(bitboard, square);

            if (color == Color.White)
            {
                attacks |= (bitboard & ~GameConstants.ColH) >> 7;
                attacks |= (bitboard & ~GameConstants.ColA) >> 9;
            }
            else
            {
                attacks |= (bitboard & ~GameConstants.ColH) << 9;
                attacks |= (bitboard & ~GameConstants.ColA) << 7;
            }

            return attacks;
        }
    }
}

