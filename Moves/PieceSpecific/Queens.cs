using AffinityChess.General;

namespace AffinityChess.Moves
{
    public static class Queens
    {
        // Generate all attack moves, it is the same as bishop and rook attacks combined
        public static ulong GetAttacks(int square, ulong occupancy)
        {
            return Bishops.GetAttacks(square, occupancy) | Rooks.GetAttacks(square, occupancy);
        }
    }
}

