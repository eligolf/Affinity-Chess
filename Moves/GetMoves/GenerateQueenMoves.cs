
namespace AffinityChess.Moves
{
    public class GenerateQueenMoves
    {
        public static ulong GetMoves(ulong board, int square)
        {
            return Rooks.GetAttacks(square, board) | Bishops.GetAttacks(square, board);
        }
    }
}
