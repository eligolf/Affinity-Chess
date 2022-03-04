

//            Binary moves           Flag          Hexidecimal 
//    
//    0000 0000 0000 0011 1111    fromSquare          0x3f
//    0000 0000 1111 1100 0000    toSquare            0xfc0
//    0000 1111 0000 0000 0000    piece               0xf000
//    1111 0000 0000 0000 0000    move flags          0x100000


using AffinityChess.Board;
using AffinityChess.General;

namespace AffinityChess.Moves
{
    public struct Move
    {
        public byte fromSquare;
        public byte toSquare;
        public byte pieceMoved;
        public byte flag;
        public int score;

        public static Move Empty = new Move();        

        public Move(byte _from, byte _to, byte _pieceMoved, byte _flag, int _score = 0)
        {
            fromSquare = _from;
            toSquare = _to;
            pieceMoved = _pieceMoved;
            flag = _flag;
            score = _score;
        }

        public static bool operator ==(Move lhs, Move rhs) => lhs.Equals(rhs);
        public static bool operator !=(Move lhs, Move rhs) => !lhs.Equals(rhs);
    }

    public enum MoveFlags : byte
    {
        Quiet = 0,
        TwoStepPawn = 1,
        Castle = 2,
        Capture = 4,
        EnPassant = 5,
        PromotionKnight = 8,
        PromotionBishop = 9,
        PromotionRook = 10,
        PromotionQueen = 11,
        PromotionKnightCapture = 12,
        PromotionBishopCapture = 13,
        PromotionRookCapture = 14,
        PromotionQueenCapture = 15
    }

    public static class MoveFlagFields
    {
        public const byte Special0 = 1;
        public const byte Special1 = 2;
        public const byte Capture = 4;
        public const byte Promotion = 8;

    }

    public static class MoveFunctions
    {
        public static bool IsQuiet(byte moveFlag)
        {
            return moveFlag == (byte)MoveFlags.Quiet || moveFlag == (byte)MoveFlags.TwoStepPawn;
        }

        public static bool IsQuietNotTwoStep(byte moveFlag)
        {
            return moveFlag == (byte)MoveFlags.Quiet;
        }

        public static bool IsDoublePush(byte moveFlag)
        {
            return moveFlag == (byte)MoveFlags.TwoStepPawn;
        }

        public static bool IsEnPassant(byte moveFlag)
        {
            return moveFlag == (byte)MoveFlags.EnPassant;
        }

        public static bool IsCapture(byte moveFlag)
        {
            return (moveFlag & MoveFlagFields.Capture) != 0;
        }

        public static bool IsCastling(byte moveFlag)
        {
            return moveFlag == (byte)MoveFlags.Castle;
        }

        public static bool IsPromotion(byte moveFlag)
        {
            return (moveFlag & MoveFlagFields.Promotion) != 0;
        }

        public static int GetPromotionPiece(byte moveFlag, BoardState boardState)
        {
            if (moveFlag == 8 || moveFlag == 12) return boardState.colorToMove == Color.White ? 1 : 7;
            if (moveFlag == 9 || moveFlag == 13) return boardState.colorToMove == Color.White ? 2 : 8;
            if (moveFlag == 10 || moveFlag == 14) return boardState.colorToMove == Color.White ? 3 : 9;
            if (moveFlag == 11 || moveFlag == 15) return boardState.colorToMove == Color.White ? 4 : 10;

            return -1;
        }

        public static int GetCapturedPiece(ulong[] pieces, int captureSquare)
        {
            for (int piece = Piece.Pawn; piece <= Piece.King; piece++)
            {
                if ((pieces[piece] & (1ul << captureSquare)) != 0)
                {
                    return piece;
                }
            }
            return -1;
        }
    }    
}