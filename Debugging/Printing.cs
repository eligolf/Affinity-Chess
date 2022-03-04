using System;

using AffinityChess.General;
using AffinityChess.Moves;
using AffinityChess.Board;

namespace AffinityChess.Debugging
{
    public static class Printing
    {
        public static void PrintBitboard(ulong bitboard)
        {
            string zero = "0\t";
            string one = "1\t";

            string printString = "";

            // Loop through all squares and print a 1 if piece and 0 if not a piece on the square
            for (int row = 0; row < 8; row++)
            {
                // Add numbering on the left side
                printString += (8 - row) + "\t";

                for (int col = 0; col < 8; col++)
                {
                    int currentSquare = row * 8 + col;
                    printString += BitOperations.GetBit(bitboard, currentSquare) != 0 ? one : zero;
                }

                // Change to new row
                printString += "\n";
            }

            // Print bottom letters
            printString += "\n" + "\ta\tb\tc\td\te\tf\tg\th";

            // Print the bitboard number representation below board
            printString += "\n\n\tBitboard: " + bitboard + "\n"; 

            // Send message to the console
            Console.WriteLine(printString);
        }

        public static void PrintArray(int[] boardArray)
        {
            string printString = "";

            // Loop through all squares and print a 1 if piece and 0 if not a piece on the square
            for (int row = 0; row < 8; row++)
            {
                // Add numbering on the left side
                printString += "   " + (8 - row) + "  ";

                for (int col = 0; col < 8; col++)
                {
                    int currentSquare = row * 8 + col;
                    printString += boardArray[currentSquare] == -1 ? " " + boardArray[currentSquare] + " " :
                                                                     "  " + boardArray[currentSquare] + " ";
                }

                // Change to new row
                printString += "\n";
            }

            // Print bottom letters
            printString += "\n" + "        a   b   c   d   e   f   g   h";

            // Send message to the console
            Console.WriteLine(printString);
        }

        public static void PrintIsSquareAttacked(BoardState board, int color)
        {
            string zero = "0\t";
            string one = "1\t";

            string printString = "";

            // Loop through all squares and print a 1 if piece and 0 if not a piece on the square
            for (int row = 0; row < 8; row++)
            {
                // Add numbering on the left side
                printString += (8 - row) + "\t";

                for (int col = 0; col < 8; col++)
                {
                    int square = row * 8 + col;
                    printString += board.IsSquareAttacked(color, square) ? one : zero;
                }

                // Change to new row
                printString += "\n";
            }

            // Print bottom letters
            printString += "\n" + "\ta\tb\tc\td\te\tf\tg\th" + "\n";

            // Send message to the console
            Console.WriteLine(printString);
        }

        public static string GetPVLine(Move[][] pvTable, int[] pvLength)
        {
            string pvString = "";

            for (int index = 0; index < pvLength[0]; index++)
            {
                Move move = pvTable[0][index];

                int moveFrom = move.fromSquare;
                string moveFromSquare = GameConstants.SquareIndexToString(moveFrom);

                int moveTo = move.toSquare;
                string moveToSquare = GameConstants.SquareIndexToString(moveTo);

                int pieceMoved = move.pieceMoved;
                string pieceMovedChar = pieceMoved == 0 ? "" : GameConstants.pieceNumberToString[pieceMoved];

                pvString += pieceMovedChar + moveFromSquare + "-" + moveToSquare + ", ";
            }

            return pvString;
        }
    }
}


