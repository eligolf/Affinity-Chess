using System;
using System.Diagnostics;
using System.Collections.Generic;

using AffinityChess.Board;
using AffinityChess.FEN;
using AffinityChess.Moves;

namespace AffinityChess.Tests.Perft
{
    public class Perft
    {
        private static List<string> testFile;
        private static List<string> basicTest;

        private static bool testFailed = false;

        private static Stopwatch stopWatch;

        public static void RunPerft()
        {
            // Init variables
            InitTestFiles();
            testFile = basicTest;

            Console.WriteLine("Perft test started.");
            stopWatch = Stopwatch.StartNew();
            int totalNodes = 0;

            for (int j = 0; j < testFile.Count; j++)
            {
                // Init FEN and the answers
                string[] separatedFile = testFile[j].Split(',');
                string fen = separatedFile[0];
                List<int> answers = new List<int>();
                for (int answer = 1; answer < separatedFile.Length; answer++)
                {
                    answers.Add(int.Parse(separatedFile[answer]));
                }

                // Set up the gamestate
                BoardState boardState = FENToBoard.GetBoardState(fen);

                // Run perft test with iterative deepening to test all depths
                for (int i = 0; i < answers.Count; i++)
                {
                    // Count all nodes that we search
                    totalNodes += answers[i];

                    // Calc number of nodes
                    int nodes = PerftRun(boardState, i + 1);

                    // Check if answer is different from nodes and print what went wrong
                    if (answers[i] != nodes && answers[i] != 0)
                    {
                        testFailed = true;
                        Console.WriteLine(" ");
                        Console.WriteLine($"Test{j + 1} failed: {testFile[j]}");
                        Console.WriteLine($"Nodes searched: {nodes}");
                        Console.WriteLine($"Answer: {answers[i]}");
                        Console.WriteLine("______________________");
                    }
                }

                // Test case finished, break if we failed
                if (testFailed) break;
                else
                {
                    Console.WriteLine($"Test case {j+1}/{testFile.Count} completed successfully.");
                }
            }

            // Perft finished
            if (testFailed)
            {
                Console.WriteLine("Test failed.");
            }
            else
            {
                Console.WriteLine($"Perft completed successfully in {(stopWatch.ElapsedMilliseconds / 1000)} seconds.");
                Console.WriteLine($"Nodes/s: {totalNodes / (stopWatch.ElapsedMilliseconds / 1000)}");
            }
        }

        private static int PerftRun(BoardState boardState, int depth)
        {
            if (depth == 0) return 1;

            boardState.GetAllMoves();

            int _nodes = 0;
            foreach (Move move in boardState.possibleMoves)
            {
                if (move == Move.Empty) break;
                if (boardState.MakeMove(move))
                {
                    _nodes += PerftRun(boardState, depth - 1);
                    boardState.UnmakeMove(move);
                }
            }

            return _nodes;
        }


        private static void InitTestFiles()
        {
            basicTest = new List<string>()
            {
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -,20,400,8902,197281,4865609",
            "8/8/8/8/8/p7/8/k1K5 b - -,2,6,13,63,382,2217",
            "K1k5/8/P7/8/8/8/8/8 w - -,2,6,13,63,382,2217",
            "r6r/1bp1knpp/1N4P1/pP3p2/1b1p2nP/BP1P1pqR/1QP1P1B1/4RKN1 w - -,31,1603,45495,2263658",
            "8/1R6/rn1b4/Pk2p2p/3P3p/2PbP3/1K6/BB3N2 w - -,23,565,12698,327247,7504855",
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -,20,400,8902,197281,4865609",
            "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -,14,191,2812,43238,674624",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -,48,2039,97862,4085603",
            "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq -,6,264,9467,422333,15833292",
            "r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ -,6,264,9467,422333,15833292",
            "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ -,44,1486,62379,2103487",
            "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - -,46,2079,89890,3894594",
            "8/8/8/8/8/k7/p1K5/8 b - -,0,0,0,0,0,92683",
            "8/P1k5/K7/8/8/8/8/8 w - -,0,0,0,0,0,92683",
            "8/8/8/8/1k6/8/K1p5/8 b - -,0,0,0,0,0,0,567584",
            "8/k1P5/8/1K6/8/8/8/8 w - -,0,0,0,0,0,0,567584",
            "8/5k2/8/5N2/5Q2/2K5/8/8 w - -,0,0,0,23527",
            "8/8/2k5/5q2/5n2/8/5K2/8 b - -,0,0,0,23527",
            "r3k2r/8/5Q2/8/8/3q4/8/R3K2R w KQkq -,0,0,0,1720476",
            "r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq -,0,0,0,1720476",
            "2K2r2/4P3/8/8/8/8/8/3k4 w - -,0,0,0,0,0,3821001",
            "3K4/8/8/8/8/8/4p3/2k2R2 b - -,0,0,0,0,0,3821001",
            "4k2r/8/8/7r/8/8/1B6/1K6 w k -,0,0,0,0,1063513",
            "1k6/1b6/8/8/7R/8/8/4K2R b K -,0,0,0,0,1063513",
            "8/k7/8/8/8/8/1p6/4K3 b - -,0,0,0,0,0,217342",
            "4k3/1P6/8/8/8/8/K7/8 w - -,0,0,0,0,0,217342",
            "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - -,0,0,0,0,3605103",
            "1k6/8/8/8/R7/1n6/8/R3K3 b Q -,0,0,0,0,346695",
            "r3k3/8/1N6/r7/8/8/8/1K6 w q -,0,0,0,0,346695",
            "1k6/1b6/8/8/7R/8/8/4K2R b K -,0,0,0,0,1063513",
            "4k2r/8/8/7r/8/8/1B6/1K6 w k -,0,0,0,0,1063513",
            "2K2r2/4P3/8/8/8/8/8/3k4 w - -,0,0,0,0,0,3821001",
            "3K4/8/8/8/8/8/4p3/2k2R2 b - -,0,0,0,0,0,3821001",
            "8/k1P5/8/1K6/8/8/8/8 w - -,0,0,0,0,0,0,567584"
            };            
        }
    }
}

