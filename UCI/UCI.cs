using System;
using System.Threading.Tasks;

using AffinityChess.AI;
using AffinityChess.Board;
using AffinityChess.General;
using AffinityChess.Moves;

namespace AffinityChess
{
    public static class UCI
    {
        private static bool isSilent = false;

        public static void Go(BoardState boardState, string[] inputString, Search search)
        {
            // Init input variables
            int movetime = -1;
            int depth = -1;
            int nodes = -1;
            int wtime = -1;
            int btime = -1;
            int winc = 0;
            int binc = 0;
            int movestogo = -1;

            // Check for the different input parameters and set them accordingly
            int number = 1;
            while (number < inputString.Length)
            {
                if (inputString[number] == "wtime")
                {
                    wtime = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "btime")
                {
                    btime = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "winc")
                {
                    winc = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "binc")
                {
                    binc = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "movetime")
                {
                    movetime = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "movestogo")
                {
                    movestogo = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "depth")
                {
                    depth = int.Parse(inputString[number + 1]);
                    number++;
                }
                else if (inputString[number] == "nodes")
                {
                    nodes = int.Parse(inputString[number + 1]);
                    number++;
                }
                else
                {
                    Log($"Error: Unknown input '{inputString[number]}'");
                }

                number++;
            }

            // Calculate the thinking time for a move, if one is given. If blitz, calculate this by formulas below.
            int thinkingTimeForAMove = SearchConstants.maxSearchTime;
            if (movetime != -1)
            {
                thinkingTimeForAMove = movetime;
            }
            else if (wtime != -1 && btime != -1)
            {
                // Check if movestogo wasn't given and if so make it over 0
                if (movestogo == -1)
                {
                    movestogo = 40 - boardState.movesCount;
                    while (movestogo < 1) movestogo += 40;
                }

                // Calc new thinking time per move
                int time = boardState.colorToMove == Color.White ? wtime : btime;
                int timeInc = boardState.colorToMove == Color.White ? winc : binc;

                // Later do better calculations..
                thinkingTimeForAMove = (time + (movestogo * timeInc)) / movestogo;

                // Limit duration if low
                int limitDuration = time / 15;
                if (thinkingTimeForAMove > limitDuration)
                {
                    thinkingTimeForAMove = limitDuration;
                }

                // Limit to always think for at least 10 ms
                thinkingTimeForAMove = Math.Max(thinkingTimeForAMove, 10);
            }

            // If depth wasn't given, set it to maximum search depth
            if (depth == -1)
            {
                depth = SearchConstants.maxDepth;
            }
            // If nodes wasn't given, set it to maximum nodes
            if (nodes == -1)
            {
                nodes = SearchConstants.maxNodes;
            }

            // Start a new thread to be able to take inputs when searching moves.
            // Then search the position with the given parameters.
            Task.Run(() => {
                Move move = search.FindBestMove(boardState, depth, nodes, thinkingTimeForAMove);

                // Convert move to the correct format
                string uciMove = GameConstants.SquareIndexToString(move.fromSquare) +
                                 GameConstants.SquareIndexToString(move.toSquare);

                // DEBUGGING
                Console.WriteLine($"Q-nodes %: {100f * search.qNodes / search.nodes}");

                // Check if move was correct
                if (move != Move.Empty)
                {
                    // Write best move to console and play it on the board
                    Console.WriteLine($"bestmove {uciMove}");
                    boardState.MakeMove(move);
                }
                // Else send illegal move
                else
                {
                    Console.WriteLine("bestmove a1a1");
                }
            });
        }

        public static void Log(string message)
        {
            if (!isSilent)
                Console.WriteLine($"info string {message}");
        }
    }
}
