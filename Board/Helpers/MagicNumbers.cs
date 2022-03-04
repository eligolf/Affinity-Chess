using System;

using AffinityChess.General;
using AffinityChess.Moves;

namespace AffinityChess.Board
{

    public class MagicNumbers
    {
        public static int[] relevantBishopBits = new int[64]
        {
            6, 5, 5, 5, 5, 5, 5, 6,
            5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5,
            6, 5, 5, 5, 5, 5, 5, 6
        };

        public static int[] relevantRookBits = new int[64]
        {
            12, 11, 11, 11, 11, 11, 11, 12,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            12, 11, 11, 11, 11, 11, 11, 12
        };

        // Found from functions below
        public static ulong[] rookMagics = new ulong[64]
        {
            9979994641325359136, 90072129987412032, 180170925814149121, 72066458867205152, 144117387368072224, 216203568472981512, 9547631759814820096, 2341881152152807680, 140740040605696, 2316046545841029184, 72198468973629440, 81205565149155328, 146508277415412736, 703833479054336, 2450098939073003648, 576742228899270912, 36033470048378880, 72198881818984448, 1301692025185255936, 90217678106527746, 324684134750365696, 9265030608319430912, 4616194016369772546, 2199165886724, 72127964931719168, 2323857549994496000, 9323886521876609, 9024793588793472, 562992905192464, 2201179128832, 36038160048718082, 36029097666947201, 4629700967774814240, 306244980821723137, 1161084564161792, 110340390163316992, 5770254227613696, 2341876206435041792, 82199497949581313, 144120019947619460, 324329544062894112, 1152994210081882112, 13545987550281792, 17592739758089, 2306414759556218884, 144678687852232706, 9009398345171200, 2326183975409811457, 72339215047754240, 18155273440989312, 4613959945983951104, 145812974690501120, 281543763820800, 147495088967385216, 2969386217113789440, 19215066297569792, 180144054896435457, 2377928092116066437, 9277424307650174977, 4621827982418248737, 563158798583922, 5066618438763522, 144221860300195844, 281752018887682
        };

        // Found from functions below
        public static ulong[] bishopMagics = new ulong[64]
        {
            18018832060792964, 9011737055478280, 4531088509108738, 74316026439016464, 396616115700105744, 2382975967281807376, 1189093273034424848, 270357282336932352, 1131414716417028, 2267763835016, 2652629010991292674, 283717117543424, 4411067728898, 1127068172552192, 288591295206670341, 576743344005317120, 18016669532684544, 289358613125825024, 580966009790284034, 1126071732805635, 37440604846162944, 9295714164029260800, 4098996805584896, 9223937205167456514, 153157607757513217, 2310364244010471938, 95143507244753921, 9015995381846288, 4611967562677239808, 9223442680644702210, 64176571732267010, 7881574242656384, 9224533161443066400, 9521190163130089986, 2305913523989908488, 9675423050623352960, 9223945990515460104, 2310346920227311616, 7075155703941370880, 4755955152091910658, 146675410564812800, 4612821438196357120, 4789475436135424, 1747403296580175872, 40541197101432897, 144397831292092673, 1883076424731259008, 9228440811230794258, 360435373754810368, 108227545293391872, 4611688277597225028, 3458764677302190090, 577063357723574274, 9165942875553793, 6522483364660839184, 1127033795058692, 2815853729948160, 317861208064, 5765171576804257832, 9241386607448426752, 11258999336993284, 432345702206341696, 9878791228517523968, 4616190786973859872
        };

        public static void InitMagicNumbers()
        {
            rookMagics = new ulong[64];
            bishopMagics = new ulong[64];

            // Loop over all squares for the rooks
            for (int square = 0; square < 64; square++)
            {
                // Init rook and bishop magic numbers
                rookMagics[square] = FindMagicNumber(square, relevantRookBits[square], Piece.Rook);
            }
            // Loop over all squares for the bishops
            for (int square = 0; square < 64; square++)
            {
                // Init rook and bishop magic numbers
                bishopMagics[square] = FindMagicNumber(square, relevantBishopBits[square], Piece.Bishop);
            }
        }

        public static ulong GenerateMagicNumber()
        {
            return RandomNumberGenerator.GetRandomU64Number() & RandomNumberGenerator.GetRandomU64Number() & RandomNumberGenerator.GetRandomU64Number();
        }

        public static ulong FindMagicNumber(int square, int relevantBits, int piece)
        {
            // Init occupancies
            ulong[] occupancies = new ulong[4096];

            // Init attack tables
            ulong[] attacks = new ulong[4096];

            // Init used attacks
            ulong[] usedAttacks = new ulong[4096];

            // Init attack mask for piece
            ulong attackMask = piece == Piece.Bishop ? 
                Bishops.AttackMasks[square] : 
                Rooks.AttackMasks[square];

            // Init occupancy indicies
            int occupancyIndex = 1 << relevantBits;

            // Loop over occupancy indicies
            for (int index = 0; index < occupancyIndex; index++)
            {
                // Init occupancies
                occupancies[index] = BitOperations.SetOccupancyBitboards(index, relevantBits, attackMask);

                // Init attacks
                attacks[index] = piece == Piece.Bishop ?
                    Bishops.GenerateAttacksOnTheFly(occupancies[index], square) :
                    Rooks.GenerateAttacksOnTheFly(occupancies[index], square);
            }

            // Test magic numbers loop    
            for (int randomCount = 0; randomCount < 100000; randomCount++)
            {
                // Generate candidate magic number
                ulong magicNumber = GenerateMagicNumber();

                // Skip inappropriate magic numbers
                if (BitOperations.PopCount((attackMask * magicNumber) & 0xFF00000000000000) < 6)
                {
                   continue;
                }

                // Init used attacks
                for (int i = 0; i < 4096; i++) usedAttacks[i] = 0ul;

                // Test magic index
                int index, fail;
                for (index = 0, fail = 0; fail != 0 && index < occupancyIndex; index++)
                {
                    // Init magic index
                    int magicIndex = (int)((occupancies[index] * magicNumber) >> (64 - relevantBits));

                    // If magic index works
                    if (usedAttacks[magicIndex] == 0ul)
                    {
                        // Init used attacks
                        usedAttacks[magicIndex] = attacks[index];
                    }
                    else if (usedAttacks[magicIndex] != attacks[index])
                    {
                        // Magic index doesn't work
                        fail = 1;
                    }
                }

                // Magic number works
                if (fail != 1)
                {
                    return magicNumber;
                }
            }

            // Magic number failed
            Console.WriteLine("Failed to find magic number");
            return 0ul;        
        }

    }
}


