using AffinityChess.Board;
using AffinityChess.General;

namespace AffinityChess.Moves
{
    public static class Kings
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

            // Go in all directions and make sure the king doesn't "go around" the A or H file.
            attacks |= (bitboard & ~GameConstants.ColA) << 7;
            attacks |= bitboard << 8;
            attacks |= (bitboard & ~GameConstants.ColH) << 9;
            attacks |= (bitboard & ~GameConstants.ColH) << 1;
            attacks |= (bitboard & ~GameConstants.ColH) >> 7;
            attacks |= bitboard >> 8;
            attacks |= (bitboard & ~GameConstants.ColA) >> 9;
            attacks |= (bitboard & ~GameConstants.ColA) >> 1;

            return attacks;
        }

        // ##################################################################
        //                              CASTLING
        //
        //    Not checking for checks on destination square since this will
        //    be handled in the makeMove function later on.
        // ##################################################################

        // White king side
        public static bool WhiteCanCastleKingSide(BoardState boardState, int color)
        {
            // Check if we can castle short and that no pieces are in the way
            if ((boardState.castling & 1) != 0 && (boardState.occupancyAll & 6917529027641081856) == 0)
            {
                // Check that no square is attacked
                if (!boardState.IsSquareAttacked(color, 60) && !boardState.IsSquareAttacked(color, 61))
                {
                    return true;
                }
            }
            return false;
        }

        // White queen side
        public static bool WhiteCanCastleQueenSide(BoardState boardState, int color)
        {
            if ((boardState.castling & 2) != 0 && (boardState.occupancyAll & 1008806316530991104) == 0)
            {
                if (!boardState.IsSquareAttacked(color, 59) && !boardState.IsSquareAttacked(color, 60))
                {
                    return true;
                }
            }
            return false;
        }

        // Black king side
        public static bool BlackCanCastleKingSide(BoardState boardState, int color)
        {
            if ((boardState.castling & 4) != 0 && (boardState.occupancyAll & 96) == 0)
            {
                if (!boardState.IsSquareAttacked(color, 4) && !boardState.IsSquareAttacked(color, 5))
                {
                    return true;
                }
            }
            return false;
        }

        // Black queen side
        public static bool BlackCanCastleQueenSide(BoardState boardState, int color)
        {
            if ((boardState.castling & 8) != 0 && (boardState.occupancyAll & 14) == 0)
            {
                if (!boardState.IsSquareAttacked(color, 3) && !boardState.IsSquareAttacked(color, 4))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

