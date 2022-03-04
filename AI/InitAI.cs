using AffinityChess.Moves;

namespace AffinityChess.AI
{
    public static class InitAI
    {
        public static void InitSetup()
        {
            // Init PST tables
            PSTData.BuildPieceSquareTables();

            // Pawn, knight and king attack tables
            InitLeaperAttacks();
            InitLeaperQuiets();

            // Bishop, rook and queen attack tables
            InitSliderAttacks();
        }

        private static void InitLeaperQuiets()
        {
            Pawns.GenerateQuietMasks();
        }

        private static void InitLeaperAttacks()
        {
            Pawns.GenerateAttackMask();
            Knights.GenerateAttackMask();
            Kings.GenerateAttackMask();
        }

        private static void InitSliderAttacks()
        {
            Bishops.GenerateAttackMask();
            Rooks.GenerateAttackMask();
        }
    }
}

