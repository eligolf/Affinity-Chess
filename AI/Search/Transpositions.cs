// ----------------------------------------------------------------------------------
// Huge thanks to Thomas, author of Leorik engine, for this section. I wouldn't 
// be able to do it without the support from you and people at TalkChess forum :) 
// Leorik: https://github.com/lithander/Leorik
// ----------------------------------------------------------------------------------

using System;

using AffinityChess.Moves;

namespace AffinityChess.AI
{
    public class Transpositions
    {
        // Type of hash entry
        public enum Flag : byte
        {
            Invalid,
            Exact,
            Beta,
            Alpha
        }

        // A hash entry with its content
        public struct HashEntry
        {
            public ulong Key;        // 8 Bytes
            public short Score;      // 2 Bytes
            public byte Depth;       // 1 Byte
            public byte Age;         // 1 Byte
            public byte Flag;        // 1 Byte
            public Move BestMove;    // 3 Bytes
        }

        // Size of hash table and a hash entry
        public const int defaultSizeMB = 50;
        const int entrySize = 16;

        public static HashEntry[] hashTable;

        static Transpositions()
        {
            Resize(defaultSizeMB);
        }

        public static bool ReadTT(ulong zobristHash, int depth, int ply, int alpha, int beta, out Move bestMove, out int score)
        {
            // Init move and score
            bestMove = Move.Empty;
            score = 0;

            // Return if not finding a match
            if (!Index(zobristHash, out int index)) return false;

            // Find entry and stored best move
            ref HashEntry entry = ref hashTable[index];
            bestMove = entry.BestMove;

            // We must be at a higher depth than the entry
            if (entry.Depth < depth) return false;

            // Adjust score to take into account how far we are from mate
            score = AdjustMateDistance(entry.Score, -ply);

            // If any of the flags match we can return a matching entry
            if (entry.Flag == (byte)Flag.Exact) return true;
            if (entry.Flag == (byte)Flag.Alpha && score <= alpha) return true;
            if (entry.Flag == (byte)Flag.Beta && score >= beta) return true;

            // Else return false
            return false;
        }

        public static void WriteTT(ulong zobristHash, int depth, int ply, int alpha, int beta, int score, Move bestMove)
        {
            // Create entry
            ref HashEntry entry = ref hashTable[Index(zobristHash)];

            // Don't overwrite a bestmove with empty move unless it's a new position
            if (entry.Key != zobristHash || bestMove != Move.Empty)
            {
                entry.BestMove = bestMove;
            }
                
            // Store values
            entry.Key = zobristHash;
            entry.Depth = depth < 0 ? default : (byte)depth;
            entry.Age = 0;

            // Store flag and score depending on how score compares to alpha and beta
            if (score >= beta)
            {
                entry.Flag = (byte)Flag.Beta;
                entry.Score = AdjustMateDistance(beta, ply);
            }
            else if (score <= alpha)
            {
                entry.Flag = (byte)Flag.Alpha;
                entry.Score = AdjustMateDistance(alpha, ply);
            }
            else
            {
                entry.Flag = (byte)Flag.Exact;
                entry.Score = AdjustMateDistance(score, ply);
            }
        }

        public static short AdjustMateDistance(int score, int ply)
        {
            // If close to mate score, return adjusted with ply. Else just return score.
            return Math.Abs(score) > SearchConstants.mateScore ?
                (short)(score + Math.Sign(score) * ply) :
                (short)score;
        }

        private static bool Index(in ulong key, out int index)
        {
            // Index is the zobrist key modded with hash table size
            index = (int)(key % (ulong)hashTable.Length);

            // Try if we get a hit, if not return false
            if (hashTable[index].Key != key)
            {
                index ^= 1;
            }                
            if (hashTable[index].Key != key)
            {
                return false;
            }                

            // If we get a hit we reset age of entry
            hashTable[index].Age = 0;

            return true;
        }

        private static int Index(in ulong key)
        {
            // Index is the zobrist key modded with hash table size
            int index = (int)(key % (ulong)hashTable.Length);
            ref HashEntry entry1 = ref hashTable[index];
            ref HashEntry entry2 = ref hashTable[index ^ 1];

            // Try if entry 1 is matching with given hash
            if (entry1.Key == key)
            {
                return index;
            }

            // If not try next position in table
            if (entry2.Key == key)
            {
                return index ^ 1;
            }

            // Raise age of both entries and choose shallower entry
            return (++entry1.Age - entry1.Depth) > (++entry2.Age - entry2.Depth) ? index : index ^ 1;
        }

        public static void Resize(int hashSizeMBytes)
        {
            int length = (hashSizeMBytes * 1024 * 1024) / entrySize;
            hashTable = new HashEntry[length];
        }

        public static void Clear()
        {
            Array.Clear(hashTable, 0, hashTable.Length);
        }
    }
}
