
using AffinityChess.Board;
using AffinityChess.General;

namespace AffinityChess.AI
{
    public static class PSTData
    {
        public static int[][][] openingTables;
        public static int[][][] endingTables;

        public static void BuildPieceSquareTables()
        {
            // Create main PST tables
            openingTables = new int[6][][]
            {
                PawnPST.BuildOpening(),
                KnightPST.BuildOpening(),
                BishopPST.BuildOpening(),
                RookPST.BuildOpening(),
                QueenPST.BuildOpening(),
                KingPST.BuildOpening()
            };
            endingTables = new int[6][][]
            {
                PawnPST.BuildEnding(),
                KnightPST.BuildEnding(),
                BishopPST.BuildEnding(),
                RookPST.BuildEnding(),
                QueenPST.BuildEnding(),
                KingPST.BuildEnding()
            };

            // Add piece base values to the main tables
            for (int color = 0; color < 2; color++)
            {
                for (int piece = Piece.Pawn; piece <= Piece.King; piece++)
                {
                    for (int square = 0; square < 64; square++)
                    {
                        openingTables[piece][color][square] += EvaluationConstants.piecesOpening[piece];
                        endingTables[piece][color][square] += EvaluationConstants.piecesEnding[piece];
                    }
                }
            }
        }
    }
}
