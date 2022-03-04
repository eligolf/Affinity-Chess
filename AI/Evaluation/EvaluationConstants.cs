
namespace AffinityChess.AI
{

    public static class EvaluationConstants
    {
        // https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
        public static int[] piecesOpening = { 82, 337, 365, 477, 1025, 12000 };
        public static int[] piecesEnding = { 94, 281, 297, 512, 936, 12000 };
        public static int[] gamePhaseInc = { 0, 1, 1, 2, 4, 0, };
        public static int phaseResolution = 4*gamePhaseInc[1] + 4*gamePhaseInc[2] + 4*gamePhaseInc[3] + 2*gamePhaseInc[4];

        public static int endGameDefinition = phaseResolution / 4;  // When it is considered endgame

        // End of game constants
        public const int checkmate = 32000;
        public const int threefoldRepetition = 0;
        public const int insufficientMaterial = 0;

        // King related (not implemented yet)
        public static int[] manhattanDistance =
            {6, 5, 4, 3, 3, 4, 5, 6,
             5, 4, 3, 2, 2, 3, 4, 5,
             4, 3, 2, 1, 1, 2, 3, 4,
             3, 2, 1, 0, 0, 1, 2, 3,
             3, 2, 1, 0, 0, 1, 2, 3,
             4, 3, 2, 1, 1, 2, 3, 4,
             5, 4, 3, 2, 2, 3, 4, 5,
             6, 5, 4, 3, 3, 4, 5, 6
            };

    }
}
