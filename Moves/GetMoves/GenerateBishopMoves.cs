
namespace AffinityChess.Moves
{
    public class GenerateBishopMoves
    {
        public static ulong GetMoves(ulong board, int square)
        {
            return Bishops.GetAttacks(square, board);
        }
    }
}