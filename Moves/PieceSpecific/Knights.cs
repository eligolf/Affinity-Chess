using AffinityChess.General;

namespace AffinityChess.Moves
{
    public static class Knights
    {
        public static ulong[] AttackMasks = new ulong[64];

        // Generate all attack moves from squares
        public static void GenerateAttackMask()
        {
            // Loop through all squares and add the correct value
            for (int currentSquare = 0; currentSquare < 64; currentSquare++)
            {
                AttackMasks[currentSquare] = GetAttacksFromSquare(currentSquare);
            }
        }

        // Initialize the attack tables
        private static ulong GetAttacksFromSquare(int square)
        {
            ulong attacks = 0ul;
            ulong bitboard = 0ul;
            bitboard = BitOperations.SetBit(bitboard, square);

            // Go in all knight directions but make sure to not go around the A or H file.
            ulong m1 = ~(GameConstants.ColA | GameConstants.ColB);
            ulong m2 = ~GameConstants.ColA;
            ulong m3 = ~GameConstants.ColH;
            ulong m4 = ~(GameConstants.ColH | GameConstants.ColG);

            // Add the 8 directions and remove the ones corresponding to m1-m4
            attacks |= (bitboard & m1) << 6;
            attacks |= (bitboard & m2) << 15;
            attacks |= (bitboard & m3) << 17;
            attacks |= (bitboard & m4) << 10;
            attacks |= (bitboard & m4) >> 6;
            attacks |= (bitboard & m3) >> 15;
            attacks |= (bitboard & m2) >> 17;
            attacks |= (bitboard & m1) >> 10;

            return attacks;
        }
    }
}

