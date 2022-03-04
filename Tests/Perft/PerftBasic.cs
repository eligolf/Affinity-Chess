using System;
using System.Diagnostics;

using AffinityChess.Board;
using AffinityChess.Moves;

namespace AffinityChess.Tests.Perft
{
    public static class PerftBasic
    {

        public static void Run(BoardState boardState, int depth)
        {
            // Start a new timer
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Look at the node count for the given boardstate
            int nodeCount = Perft(boardState, depth);

            // Stop the timer
            stopwatch.Stop();

            // Print things
            float totalSeconds = (float)stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine("Perft completed in " + Math.Round(totalSeconds, 2) + " s.");
            Console.WriteLine("Total node count: " + nodeCount);
            Console.WriteLine("Nodes per second: " + Math.Round(nodeCount / totalSeconds, 0));
        }

        private static int Perft(BoardState boardState, int depth)
        {
            // Return one if we find a leaf node
            if (depth <= 0) return 1;

            boardState.GetAllMoves();
            int nodes = 0;
            foreach (Move move in boardState.possibleMoves)
            {
                if (move == Move.Empty) break;
                if (boardState.MakeMove(move))
                {
                    nodes += Perft(boardState, depth - 1);
                    boardState.UnmakeMove(move);
                }
            }

            return nodes;
        }
    }
}

