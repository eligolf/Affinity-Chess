using System.Collections.Generic;

namespace AffinityChess.General
{
    public class GameConstants
    {
        // Start position FEN string
        public static string startPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // FEN debug positions
        public static string emptyPosition = "8/8/8/8/8/8/8/8 w - - ";
        public static string kiwipetePosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        public static string complexPosition = "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
        public static string customPosition = "r2q1rk1/ppp2ppp/2n1bn2/2b1p3/3pP3/3P1NPP/PPP1NPB1/R1BQ1RK1 b - - 0 9";

        // Game variables
        public static int maxGameLength = 256;
        public static int maxMovesInAPosition = 256;

        // Board variables
        public static List<char> numbers = new List<char>()
        {
            '1', '2', '3', '4', '5', '6', '7', '8'
        };

        public static List<char> letters = new List<char>()
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'
        };

        public static string SquareIndexToString(int square) => $"{(char)('a' + square % 8)}{8 - square / 8}";
        public static int StringSquareToIndex(string square) 
        {
            int col = char.ToUpper(square[0]) - 'A';
            int row = '8' - square[1];
            return col + row * 8;
        }

        public static List<int> boardFlipped = new List<int>()
        {
            63, 62, 61, 60, 59, 58, 57, 56,
            55, 54, 53, 52, 51, 50, 49, 48,
            47, 46, 45, 44, 43, 42, 41, 40,
            39, 38, 37, 36, 35, 34, 33, 32,
            31, 30, 29, 28, 27, 26, 25, 24,
            23, 22, 21, 20, 19, 18, 17, 16,
            15, 14, 13, 12, 11, 10,  9,  8,
             7,  6,  5,  4,  3,  2,  1,  0
        };

        public static List<int> boardHorizontalFlip = new List<int>()
        {
            56, 57, 58, 59, 60, 61, 62, 63,
            48, 49, 50, 51, 52, 53, 54, 55, 
            40, 41, 42, 43, 44, 45, 46, 47,
            32, 33, 34, 35, 36, 37, 38, 39, 
            24, 25, 26, 27, 28, 29, 30, 31,
            16, 17, 18, 19, 20, 21, 22, 23,
             8,  9, 10, 11, 12, 13, 14, 15,
             0,  1,  2,  3,  4,  5,  6,  7
        };

        // Castling
        public static List<ulong> updateCastlingRights = new List<ulong>()
        {
             7, 15, 15, 15,  3, 15, 15, 11,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            13, 15, 15, 15, 12, 15, 15, 14
        };

        public static Dictionary<int, int[]> rookCastling = new Dictionary<int, int[]>()
        {
            { 62, new int[2]{ 63, 61} },
            { 58, new int[2]{ 56, 59} },
            { 6, new int[2]{ 7, 5} },
            { 2, new int[2]{ 0, 3} }
        };

        // Pieces
        public static Dictionary<string, int> pieceStringToNumber = new Dictionary<string, int>()
        {
            { "P", 0},
            { "N", 1},
            { "B", 2},
            { "R", 3},
            { "Q", 4},
            { "K", 5},

            { "p", 6},
            { "n", 7},
            { "b", 8},
            { "r", 9},
            { "q", 10},
            { "k", 11}
        };

        public static Dictionary<int, string> pieceNumberToString = new Dictionary<int, string>()
        {
            {0, "P"},
            {1, "N"},
            {2, "B"},
            {3, "R"},
            {4, "Q"},
            {5, "K"},

            {6, "p"},
            {7, "n"},
            {8, "b"},
            {9, "r"},
            {10, "q"},
            {11, "k"},
        };

        // Full or empty board
        public const ulong FullBitboard = ulong.MaxValue;
        public const ulong EmptyBitboard = 0;

        // Masks for each column and row
        public const ulong ColA = 0x0101010101010101;
        public const ulong ColB = 0x0202020202020202;
        public const ulong ColC = 0x0404040404040404;
        public const ulong ColD = 0x0808080808080808;
        public const ulong ColE = 0x1010101010101010;
        public const ulong ColF = 0x2020202020202020;
        public const ulong ColG = 0x4040404040404040;
        public const ulong ColH = 0x8080808080808080;

        public const ulong Row8 = 0x00000000000000FF;
        public const ulong Row7 = 0x000000000000FF00;
        public const ulong Row6 = 0x0000000000FF0000;
        public const ulong Row5 = 0x00000000FF000000;
        public const ulong Row4 = 0x000000FF00000000;
        public const ulong Row3 = 0x0000FF0000000000;
        public const ulong Row2 = 0x00FF000000000000;
        public const ulong Row1 = 0xFF00000000000000;

        // Masks for special cases
        public const ulong Edges = ColA | ColH | Row1 | Row8;
        public const ulong BoardWithoutEdges = FullBitboard & ~Edges;
        public const ulong RightLeftEdge = ColA | ColH;
        public const ulong TopBottomEdge = Row1 | Row8;
        public const ulong Corners = 0x8100000000000081;

        public const ulong NearPromotionAreaWhite = 0xffff00;
        public const ulong NearPromotionAreaBlack = 0xffff0000000000;

        public static ulong[] promotionRows = new ulong[] { Row8, Row1 };
        public static ulong[] oneStepPawnMoveRows = new ulong[] { Row3, Row6 };

        public static List<int> PromotionSquaresWhite = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7};
        public static List<int> PromotionSquaresBlack = new List<int>() { 56, 57, 58, 59, 60, 61, 62, 63 };

        // Distance from center map
        public static int[] DistanceFromCenter =
        {
            3, 3, 3, 3, 3, 3, 3, 3,
            3, 2, 2, 2, 2, 2, 2, 3,
            3, 2, 1, 1, 1, 1, 2, 3,
            3, 2, 1, 0, 0, 1, 2, 3,
            3, 2, 1, 0, 0, 1, 2, 3,
            3, 2, 1, 1, 1, 1, 2, 3,
            3, 2, 2, 2, 2, 2, 2, 3,
            3, 3, 3, 3, 3, 3, 3, 3
        };

        // Get the corresponding index from a given square
        public static Dictionary<string, int> squareToIndex = new Dictionary<string, int>()
        {
            { "a1", 56 },
            { "b1", 57 },
            { "c1", 58 },
            { "d1", 59 },
            { "e1", 60 },
            { "f1", 61 },
            { "g1", 62 },
            { "h1", 63 },

            { "a2", 48 },
            { "b2", 49 },
            { "c2", 50 },
            { "d2", 51 },
            { "e2", 52 },
            { "f2", 53 },
            { "g2", 54 },
            { "h2", 55 },

            { "a3", 40 },
            { "b3", 41 },
            { "c3", 42 },
            { "d3", 43 },
            { "e3", 44 },
            { "f3", 45 },
            { "g3", 46 },
            { "h3", 47 },

            { "a4", 32 },
            { "b4", 33 },
            { "c4", 34 },
            { "d4", 35 },
            { "e4", 36 },
            { "f4", 37 },
            { "g4", 38 },
            { "h4", 39 },

            { "a5", 24 },
            { "b5", 25 },
            { "c5", 26 },
            { "d5", 27 },
            { "e5", 28 },
            { "f5", 29 },
            { "g5", 30 },
            { "h5", 31 },

            { "a6", 16 },
            { "b6", 17 },
            { "c6", 18 },
            { "d6", 19 },
            { "e6", 20 },
            { "f6", 21 },
            { "g6", 22 },
            { "h6", 23 },

            { "a7", 8 },
            { "b7", 9 },
            { "c7", 10 },
            { "d7", 11 },
            { "e7", 12 },
            { "f7", 13 },
            { "g7", 14 },
            { "h7", 15 },

            { "a8", 0 },
            { "b8", 1 },
            { "c8", 2 },
            { "d8", 3 },
            { "e8", 4 },
            { "f8", 5 },
            { "g8", 6 },
            { "h8", 7 },
        };
    }
}
