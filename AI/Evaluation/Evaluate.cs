using System;

using AffinityChess.Board;
using AffinityChess.General;

namespace AffinityChess.AI
{
    public static class Evaluate
    {

        // Only PeSTO with tapered eval
        public static int Evaluation(BoardState boardState)
        {
            // Calculate the phase we are currently in
            int gamePhaseOpening = Math.Min(boardState.currentGamePhase, EvaluationConstants.phaseResolution);
            int gamePhaseEnding = EvaluationConstants.phaseResolution - gamePhaseOpening;

            // Calculate the scores from PST tables (white minus black score)
            int scoreOpening = boardState.PSTValuesOpening[Color.White] - boardState.PSTValuesOpening[Color.Black];
            int scoreEnding = boardState.PSTValuesEnding[Color.White] - boardState.PSTValuesEnding[Color.Black];

            // Tapered evaluation (https://www.chessprogramming.org/Tapered_Eval)
            int score = ((scoreOpening * gamePhaseOpening) + (scoreEnding * gamePhaseEnding)) / EvaluationConstants.phaseResolution;

            // Return based on turn to move
            return boardState.colorToMove == Color.White ? score : -score;           
        }
    }
}

