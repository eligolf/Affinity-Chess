using System.Collections.Generic;

namespace AffinityChess.General
{
    public static class BitOperations
    {
        private static readonly int[] BitScanValues =
        {
            0,  1,  48,  2, 57, 49, 28,  3,
            61, 58, 50, 42, 38, 29, 17,  4,
            62, 55, 59, 36, 53, 51, 43, 22,
            45, 39, 33, 30, 24, 18, 12,  5,
            63, 47, 56, 27, 60, 41, 37, 16,
            54, 35, 52, 21, 44, 32, 23, 11,
            46, 26, 40, 15, 34, 20, 31, 10,
            25, 14, 19,  9, 13,  8,  7,  6
        };

        public static ulong SetBit(ulong bitboard, int square)
        {
            return (bitboard) |= (1ul << square);
        }

        public static ulong PopBit(ulong bitboard, int square)
        {            
            return GetBit(bitboard, square) != 0 ? (bitboard &= ~(1ul << (square))) : 0;
        }

        public static ulong GetBit(ulong bitboard, int square)
        {
            return (bitboard) & (1ul << square);
        }

        public static ulong GetLSB(ulong bitboard)
        {
            return (ulong)((long)bitboard & -(long)bitboard);
        }

        public static int GetLSBIndex(ulong bitboard)
        {
            return PopCount((ulong)((long)bitboard & -(long)bitboard) - 1);
        }

        public static ulong PopLSB(ulong bitboard)
        {
            return bitboard & (bitboard - 1);
        }

        public static int PopCount(ulong bitboard)
        {
            int count = 0;
            while (bitboard != GameConstants.EmptyBitboard)
            {
                bitboard &= (bitboard - 1);
                count++;
            }

            return count;
        }

        public static int BitScan(ulong bitboard)
        {
            return BitScanValues[((ulong)((long)bitboard & -(long)bitboard) * 0x03f79d71b4cb0a89) >> 58];
        }

        public static List<int> GetOccupiedSquares(ulong bitboard)
        {
            List<int> squares = new List<int>();
            while (bitboard != GameConstants.EmptyBitboard)
            {
                squares.Add(BitScan(bitboard));
                bitboard = PopLSB(bitboard);
            }
            return squares;
        }

        public static ulong SetOccupancyBitboards(int index, int bitsInMask, ulong attackMask)
        {
            ulong occupancy = 0ul;

            // Loop over range of bits withing attack mask
            for (int count = 0; count < bitsInMask; count++)
            {
                // Get LSB index of attacks mask
                int square = GetLSBIndex(attackMask);

                // Pop the LSB in attack mask
                attackMask = PopBit(attackMask, square);

                // Make sure occupancy is on board and populate occupancy mask
                if ((index & (1 << count)) != 0)
                {
                    occupancy |= (1ul << square);
                }
            }

            return occupancy;
        }
    }
}