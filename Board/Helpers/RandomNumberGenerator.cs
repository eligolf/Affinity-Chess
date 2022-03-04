
namespace AffinityChess.Board
{
    public static class RandomNumberGenerator
    {
        // Random number state to get same random numbers and hashes
        public static uint stateNumber = 1804289383;

        public static uint GetRandomU32Number()
        {
            // Current state
            uint number = stateNumber;

            // Shift algorithm
            number ^= number << 13;
            number ^= number >> 17;
            number ^= number << 5;

            // Update random number state
            stateNumber = number;

            return number;
        }

        public static ulong GetRandomU64Number()
        {
            // Current state
            ulong n1, n2, n3, n4;

            // Init random numbers and slicing 16 bits from end
            n1 = ((ulong)GetRandomU32Number()) & 0xFFFF;
            n2 = ((ulong)GetRandomU32Number()) & 0xFFFF;
            n3 = ((ulong)GetRandomU32Number()) & 0xFFFF;
            n4 = ((ulong)GetRandomU32Number()) & 0xFFFF;

            return n1 | (n2 << 16) | (n3 << 32) | (n4 << 48);
        }
    }
}


