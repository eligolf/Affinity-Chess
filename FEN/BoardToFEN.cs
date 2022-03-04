using AffinityChess.Board;
using AffinityChess.General;
using System;

namespace AffinityChess.FEN
{
    public static class BoardToFEN
    {
        public static string GetFenString(BoardState boardState)
        {
            return string.Join(" ",
                GetBoardRepresentation(boardState),
                GetColorToMove(boardState),
                GetCastlingRights(boardState),
                GetEnpassantSquare(boardState),
                GetHalfMoveCounter(boardState),
                GetMoveCounter(boardState)
            );
        }

        private static string GetBoardRepresentation(BoardState boardState)
        {
            string boardString = "";

            // Loop over squares to see if we have a piece on it or not
            int rowNumber = 0;
            int colNumber = 0;
            bool foundPiece;
            for (int square = 0; square < 64; square++)
            {
                // Increase total number of squares searched
                rowNumber++;

                // Reset found piece bool
                foundPiece = false;

                // Look through all piece bitboards
                for (int color = Color.White; color <= Color.Black; color++)
                {
                    for (int piece = Piece.Pawn; piece <= Piece.King; piece++)
                    {
                        // Check if there is a piece on the square
                        if (BitOperations.GetOccupiedSquares(boardState.pieces[color][piece]).Contains(square))
                        {
                            foundPiece = true;

                            // Check if we have counted any non-piece squares before and if so add them to the front
                            if (colNumber != 0)
                            {
                                boardString += colNumber;
                                colNumber = 0;
                            }
                            boardString += ConvertToPiece(piece, color);
                            break;
                        }
                    }                   
                }

                // Increase number if we didn't find a piece
                if (!foundPiece) colNumber++;

                // Check if we got to a new row
                if (rowNumber == 8)
                {
                    // If we found empty squares at end of row, add the number before '/'.
                    if (colNumber != 0) boardString += colNumber;
                    boardString += '/';
                    rowNumber = colNumber = 0;
                }
            }

            // Remove last '/' from string
            boardString = boardString.Remove(boardString.Length - 1);

            // Return the resulting string
            return boardString;
        }

        private static string GetColorToMove(BoardState boardState)
        {
            return (boardState.colorToMove == Color.White) ? "w" : "b";
        }

        private static string GetCastlingRights(BoardState boardState)
        {
            string castlingString = "";
            if ((boardState.castling & 1) != 0) castlingString += "K";
            if ((boardState.castling & 2) != 0) castlingString += "Q";
            if ((boardState.castling & 4) != 0) castlingString += "k";
            if ((boardState.castling & 8) != 0) castlingString += "q";
            if (boardState.castling == 0) castlingString += "-";

            return castlingString;
        }

        private static string GetEnpassantSquare(BoardState boardState)
        {
            if (boardState.enPassant == 0) return "-";
            else
            {
                int epSquare = BitOperations.BitScan(boardState.enPassant);
                return GameConstants.SquareIndexToString(epSquare);
            }
        }

        private static string GetHalfMoveCounter(BoardState boardState)
        {
            return boardState.halfMoveClock.ToString();
        }

        private static string GetMoveCounter(BoardState boardState)
        {
            return boardState.movesCount.ToString();
        }

        private static char ConvertToPiece(int piece, int color)
        {
            char pieceChar;
            if (piece == Piece.Pawn) pieceChar = 'P';
            else if (piece == Piece.Knight) pieceChar = 'N';
            else if (piece == Piece.Bishop) pieceChar = 'B';
            else if (piece == Piece.Rook) pieceChar = 'R';
            else if (piece == Piece.Queen) pieceChar = 'Q';
            else if (piece == Piece.King) pieceChar = 'K';
            else throw new InvalidOperationException();

            // Return lower case for black pieces
            return color == Color.Black ? char.ToLower(pieceChar) : pieceChar;
        }

    }
}
