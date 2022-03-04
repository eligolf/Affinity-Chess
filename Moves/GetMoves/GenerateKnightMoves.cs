
namespace AffinityChess.Moves
{
    public class GenerateKnightMoves
    {
        public static ulong GetMoves(int square)
        {
            return Knights.AttackMasks[square];
        }
    }
}
