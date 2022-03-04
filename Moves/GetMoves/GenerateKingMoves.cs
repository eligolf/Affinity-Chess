
namespace AffinityChess.Moves
{
    public class GenerateKingMoves
    {
        public static ulong GetMoves(int square)
        {
            return Kings.AttackMasks[square];
        }
    }
}
