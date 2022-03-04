using System;

using AffinityChess.General;

namespace AffinityChess.Board
{
    public static class Zobrist
    {
        private static readonly ulong[][][] fieldHashes;
        private static readonly ulong[] castlingHashes;
        private static readonly ulong[] enPassantHashes;
        private static readonly ulong blackSideHash;
        private static readonly Random random;

        static Zobrist()
        {
            // Init arrays
            fieldHashes = new ulong[2][][];
            castlingHashes = new ulong[16];
            enPassantHashes = new ulong[8];
            random = new Random((int)RandomNumberGenerator.stateNumber);

            // Fill each piece for all squares with keys
            fieldHashes[Color.White] = new ulong[6][];
            fieldHashes[Color.Black] = new ulong[6][];

            for (int piece = 0; piece < 6; piece++)
            {
                fieldHashes[0][piece] = new ulong[64];
                fieldHashes[1][piece] = new ulong[64];

                PopulateHashArrays(fieldHashes[Color.White][piece]);
                PopulateHashArrays(fieldHashes[Color.Black][piece]);
            }

            // Fill other arrays with values
            PopulateHashArrays(castlingHashes);
            PopulateHashArrays(enPassantHashes);
            blackSideHash = NextLong(random);
        }

        public static ulong CalculateHash(BoardState board)
        {
            // Go through both colors and all pieces
            ulong result = 0ul;
            for (int color = 0; color < 2; color++)
            {
                for (int piece = 0; piece < 6; piece++)
                {
                    ulong pieceBitboard = board.pieces[color][piece];
                    while (pieceBitboard != 0)
                    {
                        ulong lsb = BitOperations.GetLSB(pieceBitboard);
                        pieceBitboard = BitOperations.PopLSB(pieceBitboard);

                        int square = BitOperations.BitScan(lsb);
                        result ^= fieldHashes[color][piece][square];
                    }
                }
            }

            // Check for castling rights
            if ((board.castling & 1) != 0)
            {
                result ^= castlingHashes[0];
            }
            if ((board.castling & 2) != 0)
            {
                result ^= castlingHashes[1];
            }
            if ((board.castling & 4) != 0)
            {
                result ^= castlingHashes[3];
            }
            if ((board.castling & 8) != 0)
            {
                result ^= castlingHashes[7];
            }

            // Check for enpassant
            if (board.enPassant != 0)
            {
                int epSquare = BitOperations.BitScan(board.enPassant);
                result ^= enPassantHashes[epSquare % 8];
            }

            // Check for color to move
            if (board.colorToMove == Color.Black)
            {
                result ^= blackSideHash;
            }

            return result;
        }

        public static ulong MovePiece(ulong hash, int color, int piece, int from, int to)
        {
            return hash ^ fieldHashes[color][piece][from] ^ fieldHashes[color][piece][to];
        }

        public static ulong AddOrRemovePiece(ulong hash, int color, int piece, int from)
        {
            return hash ^ fieldHashes[color][piece][from];
        }

        public static ulong AddOrRemoveCastlingRights(ulong hash, ulong castlingRights)
        {
            return hash ^ castlingHashes[castlingRights];
        }

        public static ulong ToggleEnPassant(ulong hash, int enPassantRank)
        {
            return hash ^ enPassantHashes[enPassantRank];
        }

        public static ulong ChangeSide(ulong hash)
        {
            return hash ^ blackSideHash;
        }

        private static void PopulateHashArrays(ulong[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = NextLong(random);
            }
        }

        private static ulong NextLong(Random _random)
        {
            byte[] bytes = new byte[8];
            _random.NextBytes(bytes);

            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}