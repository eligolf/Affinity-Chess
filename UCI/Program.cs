using System;
using System.Linq;
using System.Threading.Tasks;

using AffinityChess.AI;
using AffinityChess.Board;
using AffinityChess.FEN;
using AffinityChess.General;
using AffinityChess.Moves;

namespace AffinityChess
{
    public class Program
    {
        const string engineName = "Affinity Chess 1.0";
        const string authorName = "Elias Nilsson";

        static BoardState boardState = new BoardState();
        private static Search search;

        private static bool isRunning = true;

        private static async Task Main()
        {
            // Init parameters
            isRunning = true;
            InitAI.InitSetup();

            // Init search and the board state
            search = new Search();
            boardState = FENToBoard.GetBoardState(GameConstants.startPosition);

            // Check for input
            while (isRunning)
            {
                string input = await Task.Run(Console.ReadLine);
                GetUciInput(input);
            }
        }

        private static void GetUciInput(string input)
        {
            // Return if no input is given
            if (input == null) return;

            // Find the different commands given by the gui
            string[] inputString = input.Trim().Split();
            switch (inputString[0])
            {
                case "uci":
                    Console.WriteLine($"id name {engineName}");
                    Console.WriteLine($"id author {authorName}");                   
                    Console.WriteLine("uciok");
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "position":
                    UciPosition(inputString);
                    break;
                case "go":
                    UCI.Go(boardState, inputString, search);
                    break;
                case "ucinewgame":
                    Transpositions.Clear();
                    Transpositions.Resize(Transpositions.defaultSizeMB);
                    boardState = FENToBoard.GetBoardState(GameConstants.startPosition);
                    break;
                case "stop":
                    search.stopSearch = true;
                    break;
                case "quit":
                    search.stopSearch = true;
                    isRunning = false;
                    break;
                case "setoption":
                    // Later implement ability to set options here
                    break;
                default:
                    Console.WriteLine($"Error, unknown input: {input}");
                    return;
            }
        }

        private static void UciPosition(string[] inputString)
        {
            // Check what sort of position that was given
            if (inputString[1] == "startpos")
            {
                boardState = FENToBoard.GetBoardState(GameConstants.startPosition);
            }
            else if (inputString[1] == "fen")
            {
                // First 4 parts of the fen string
                string fen = $"{inputString[2]} {inputString[3]} {inputString[4]} {inputString[5]}";

                // Check if move counters are given in the fen
                if (inputString.Length > 6)
                {
                    // If move counters is in fen, add them
                    if (inputString[6] != "moves")
                    {
                        fen += $" {inputString[6]} {inputString[7]}";
                    }
                }
  
                // Set the given boardstate
                boardState = FENToBoard.GetBoardState(fen);
            }
            else
            {
                UCI.Log(" 'position' parameters missing or on the wrong format, assuming 'startpos'.");
                boardState = FENToBoard.GetBoardState(GameConstants.startPosition);
            }

            // Check for given moves after position
            if (inputString.Contains("moves"))
            {
                // Find where command starts
                int movesIndex = Array.IndexOf(inputString, "moves");
                string[] moveList = inputString.Skip(movesIndex + 1).ToArray();

                // Loop through the moves and play them on the boardstate
                foreach (string givenMove in moveList)
                {
                    // Find the move from the given move string
                    Move move = ParseMove(boardState, givenMove);

                    // Check if move exists, else print error message
                    if (move != Move.Empty)
                    {
                        boardState.MakeMove(move);
                    }
                    else
                    {
                        UCI.Log("Error: Given move in the move string was incorrect.");
                        break;
                    }
                }
            }
        }

        private static Move ParseMove(BoardState boardState, string moveString)
        {
            boardState.GetAllMoves();

            int fromSquare = GameConstants.StringSquareToIndex(moveString[0].ToString() + moveString[1].ToString());
            int toSquare = GameConstants.StringSquareToIndex(moveString[2].ToString() + moveString[3].ToString());

            // Loop through all possible moves and find the move corresponding to move string
            foreach (Move move in boardState.possibleMoves)
            {
                if (move.fromSquare == fromSquare &&
                    move.toSquare == toSquare)
                {
                    bool promotion = MoveFunctions.IsPromotion(move.flag) ? true : false;

                    // Special case if promotion
                    if (promotion)
                    {
                        // Check which promotion piece that was chosen
                        if ((move.flag == 11 || move.flag == 15) && moveString[4] == 'q') return move;
                        else if ((move.flag == 10 || move.flag == 14) && moveString[4] == 'r') return move;
                        else if ((move.flag == 9 || move.flag == 13) && moveString[4] == 'b') return move;
                        else if ((move.flag == 8 || move.flag == 12) && moveString[4] == 'n') return move;
                    }
                    // Else just return the move
                    else
                    {
                        return move;
                    }
                }
            }

            // Illegal move
            return Move.Empty;
        }

    }
}

