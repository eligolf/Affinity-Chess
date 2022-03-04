using System;
using System.Diagnostics;

using AffinityChess.Board;
using AffinityChess.FEN;
using AffinityChess.General;
using AffinityChess.Moves;

namespace AffinityChess.Tests
{
    
    public class Timings
    {
        private static BoardState boardState;

        private static int timesToRunTest = 10000000;

        public static void RunTest()
        {
            string startPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            boardState = FENToBoard.GetBoardState(startPosition);

            Stopwatch stopwatchA = Stopwatch.StartNew();
            stopwatchA.Start();

            for (int i = 0; i < timesToRunTest; i++)
            {
                FunctionA();
            }

            Console.WriteLine("Test A time: " + stopwatchA.ElapsedMilliseconds);

            Stopwatch stopwatchB = Stopwatch.StartNew();
            stopwatchB.Start();

            for (int i = 0; i < timesToRunTest; i++)
            {
                FunctionB();
            }
            Console.WriteLine("Test B time: " + stopwatchB.ElapsedMilliseconds);

        }

        private static void FunctionA()
        {
            boardState.GetCaptureMoves(true);
        }

        private static void FunctionB()
        {
            boardState.GetAllMoves();
        }
    }
}
