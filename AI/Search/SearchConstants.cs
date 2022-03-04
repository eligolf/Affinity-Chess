using System.Collections.Generic;

using AffinityChess.AI;
using AffinityChess.General;

namespace AffinityChess.AI
{
    public static class SearchConstants
    {
        // Mate related
        public const int mateScore = 28000;
        public const int mateValue = 29000;

        // Alpha and beta min and max value, must be more than mate value and mate score
        public const short minValue = -30000;
        public const short maxValue = 30000;

        // Limit search
        public const int maxPly = 99;
        public const int maxDepth = 99;
        public const int maxNodes = int.MaxValue - 10000; // Close to highest int value
        public const int maxSearchTime = int.MaxValue - 10000;

        // Delta pruning
        public const int bigDelta = 1025; // Value of a queen
        public const int deltaPruningMargin = 82; // Value of a pawn

        // Late move reductions
        public const int lmrMinDepth = 4;
        public const int lmrMinLegalMoves = 3;

        // Razoring
        public const int razoringMinDepth = 1;
        public const int razoringMaxDepth = 3;
        public const int razoringMargin = 70;
        public const int razoringMarginMultiplier = 150;

        // Static nullmove
        public const int staticNullMoveMaxDepth = 3;
        public const int staticNullMoveMargin = 200;

        // Null move
        public const int nullMoveMinDepth = 3;
        public const int nullMoveMin = nullMoveMinDepth - 1;
        public const int nullMoveMax = nullMoveMinDepth;

        // Move ordering
        public const int promotionScoreBonus = 500;
        public const int hashScore = 20000;
        public const int pvScore = 15000;
        public const int MVV_LVA = 10000;
        public const int firstKillerMove = 9000;
        public const int secondKillerMove = 8000;

        public static Dictionary<int, int> MVV_LVA_Values = new Dictionary<int, int>()
        {
            { 0, 1},
            { 1, 3},
            { 2, 3},
            { 3, 4},
            { 4, 5},
            { 5, 6},
        };
    }
}
