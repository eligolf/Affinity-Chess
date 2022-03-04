
using AffinityChess.General;
using AffinityChess.Board;

namespace AffinityChess.Moves
{
    public static class Bishops
    {
        public static ulong[] AttackMasks = new ulong[64];
        public static ulong[][] AttackTable = new ulong[64][];

        // Generate all attack moves from squares
        public static void GenerateAttackMask()
        {
            // Loop through all squares and add the correct value
            for (int square = 0; square < 64; square++)
            {
                // Set attack mask for current square
                AttackMasks[square] = GetAttacksFromSquare(square);

                // Set up attack table
                AttackTable[square] = new ulong[512];
                for (int i = 0; i < 512; i++) AttackTable[square][i] = 0ul;

                int relevantBitsCount = BitOperations.PopCount(AttackMasks[square]);
                int occupancyIndicies = (1 << relevantBitsCount);
                for (int index = 0; index < occupancyIndicies; index++)
                {
                    ulong occupancy = BitOperations.SetOccupancyBitboards(index, relevantBitsCount, AttackMasks[square]);

                    int magicIndex = (int)((occupancy * MagicNumbers.bishopMagics[square]) >> (64 - MagicNumbers.relevantBishopBits[square]));
                    AttackTable[square][magicIndex] = GenerateAttacksOnTheFly(occupancy, square);
                }
            }
        }        

        public static ulong GetAttacks(int square, ulong occupancy)
        {
            // Get attacks from current board occupancy
            occupancy &= AttackMasks[square];
            occupancy *= MagicNumbers.bishopMagics[square];
            occupancy >>= 64 - MagicNumbers.relevantBishopBits[square];

            return AttackTable[square][occupancy];
        }

        public static ulong GenerateAttacksOnTheFly(ulong blockerBoard, int square)
        {
            ulong attacks = 0ul;
            ulong bitboard = 0ul;
            bitboard = BitOperations.SetBit(bitboard, square);

            int targetRow = square / 8;
            int targetCol = square % 8;

            // Mask relevant occupancy bits for all 4 directions
            for (int row = targetRow + 1, col = targetCol + 1; row <= 7 && col <= 7; row++, col++)
            {
                attacks |= 1ul << (row * 8 + col);
                if ((1ul << (row * 8 + col) & blockerBoard) != 0) break;
            }                
            for (int row = targetRow - 1, col = targetCol + 1; row >= 0 && col <= 7; row--, col++)
            {
                attacks |= 1ul << (row * 8 + col);
                if ((1ul << (row * 8 + col) & blockerBoard) != 0) break;
            }                
            for (int row = targetRow + 1, col = targetCol - 1; row <= 7 && col >= 0; row++, col--)
            {
                attacks |= 1ul << (row * 8 + col);
                if ((1ul << (row * 8 + col) & blockerBoard) != 0) break;
            }                
            for (int row = targetRow - 1, col = targetCol - 1; row >= 0 && col >= 0; row--, col--)
            {
                attacks |= 1ul << (row * 8 + col);
                if ((1ul << (row * 8 + col) & blockerBoard) != 0) break;
            }                

            return attacks;
        }

        // Initialize the attack tables
        private static ulong GetAttacksFromSquare(int square)
        {
            ulong attacks = 0ul;
            ulong bitboard = 0ul;
            bitboard = BitOperations.SetBit(bitboard, square);

            int targetRow = square / 8;
            int targetCol = square % 8;

            // Mask relevant occupancy bits for all 4 directions
            for (int row = targetRow + 1, col = targetCol + 1; row <= 6 && col <= 6; row++, col++)
                attacks |= 1ul << (row * 8 + col);
            for (int row = targetRow - 1, col = targetCol + 1; row >= 1 && col <= 6; row--, col++)
                attacks |= 1ul << (row * 8 + col);
            for (int row = targetRow + 1, col = targetCol - 1; row <= 6 && col >= 1; row++, col--)
                attacks |= 1ul << (row * 8 + col);
            for (int row = targetRow - 1, col = targetCol - 1; row >= 1 && col >= 1; row--, col--)
                attacks |= 1ul << (row * 8 + col);

            return attacks;
        }
    }
}

