
namespace AffinityChess.Moves
{
    public class GenerateRookMoves
    {
        public static ulong GetMoves(ulong board, int square)
        {
            return Rooks.GetAttacks(square, board);
        }
    }
}
