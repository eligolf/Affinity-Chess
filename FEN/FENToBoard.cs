using System;

using AffinityChess.Board;
using AffinityChess.General;

namespace AffinityChess.FEN
{
    public static class FENToBoard
    {

        public static BoardState GetBoardState(string fen)
        {
            // Split all parts of the fen string
            string[] splitFenString = fen.Split(' ');
            string boardState = splitFenString[0];
            string colorToMove = splitFenString[1];
            string castlingRights = splitFenString[2];
            string epSquare = splitFenString[3];

            var halfmoveClock = 0;
            var movesCount = 0;

            // Check if a half move clock and move count was provided
            if (splitFenString.Length > 4) int.TryParse(splitFenString[4], out halfmoveClock);
            if (splitFenString.Length > 5) int.TryParse(splitFenString[5], out movesCount);

            // Init a new boardstate
            BoardState newBoardState = new BoardState();

            // Convert all parts of fen string to boardState
            int currentColor = GetColorToMove(colorToMove);
            GetBoard(boardState, newBoardState);
            GetCastlingRights(castlingRights, newBoardState);
            GetEpSquare(epSquare, newBoardState);

            // Calculate new values for evaluation
            newBoardState.CalculateEvaluationValues();

            // Set other parameters
            newBoardState.movesCount = movesCount;
            newBoardState.halfMoveClock = halfmoveClock;
            newBoardState.colorToMove = currentColor;
            newBoardState.nullMoves = 0;

            // Calculate Zobrist keys
            newBoardState.zobristKey = Zobrist.CalculateHash(newBoardState);

            return newBoardState;
        }

        private static void GetBoard(string boardState, BoardState _board)
        {
            var rows = boardState.Split('/');
            int square = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < rows[row].Length; col++)
                {
                    char fenItem = rows[row][col];
                    if (char.IsLetter(fenItem))
                    {
                        int piece = ConvertToPiece(fenItem);
                        int color = ConvertToColor(fenItem);

                        // If king, update king position
                        if (piece == Piece.King)
                        {
                            _board.kingPositions[color] = square;
                        }

                        // Add piece to square
                        _board.AddPiece(color, piece, square);
                        square++;
                    }
                    else if (char.IsDigit(fenItem))
                    {
                        square += (int)Char.GetNumericValue(fenItem);
                    }
                }
            }
        }

        private static int GetColorToMove(string currentColor)
        {
            return currentColor == "w" ? Color.White : Color.Black;
        }

        private static void GetCastlingRights(string castlingRights, BoardState boardState)
        {
            // Add castling rights to the boardstate depening on fen
            if (castlingRights.Contains('K')) boardState.castling |= 1;
            if (castlingRights.Contains('Q')) boardState.castling |= 2;
            if (castlingRights.Contains('k')) boardState.castling |= 4;
            if (castlingRights.Contains('q')) boardState.castling |= 8;
        }

        private static void GetEpSquare(string enPassantSquare, BoardState boardState)
        {
            if (enPassantSquare != "-")
            {
                boardState.enPassant = 1ul << GameConstants.StringSquareToIndex(enPassantSquare);
            }
        }

        private static int ConvertToPiece(char item)
        {
            switch (char.ToLower(item))
            {
                case 'p': return Piece.Pawn;
                case 'r': return Piece.Rook;
                case 'n': return Piece.Knight;
                case 'b': return Piece.Bishop;
                case 'q': return Piece.Queen;
                case 'k': return Piece.King;
            }

            throw new InvalidOperationException();
        }

        private static int ConvertToColor(char color)
        {
            return char.IsUpper(color) ? Color.White : Color.Black;
        }
    }
}
