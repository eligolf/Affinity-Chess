using System;
using System.Collections.Generic;
using System.Diagnostics;

using AffinityChess.Board;
using AffinityChess.AI;
using AffinityChess.General;
using AffinityChess.Moves;
using AffinityChess.Debugging;

namespace AffinityChess.AI
{
    public class Search
    {
        // PV tables
        public static int[] pvLength;
        public static Move[][] pvTable;

        // Sorting
        public static int[][] historyMoves;
        public static Move[][] killerMoves;

        // Time keeping
        private static Stopwatch stopWatch;

        // References
        private static BoardState boardState;

        // Stop search variables
        public bool stopSearch;
        private static int maxNodes;
        private static int maxTime;
        private static int stopSearchCheckInterval = 4095;

        // Information variables
        public int nodes;
        public int qNodes;

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                             ITERATIVE DEEPENING
        // ###################################################################################
        // -----------------------------------------------------------------------------------
        public Move FindBestMove(BoardState _boardState, int _maxDepth, int _maxNodes, int _maxTime)
        {
            // ------------------------------------------------------
            //           Init variables for the search
            // ------------------------------------------------------
            boardState = _boardState;
            maxNodes = _maxNodes;
            maxTime = _maxTime;
            stopSearch = false;

            int alpha = SearchConstants.minValue;
            int beta = SearchConstants.maxValue;
            int ply = 0;

            Move currentBestMove = Move.Empty;

            // Start with nodes = -1 to get the correct node count
            nodes = -1;
            qNodes = -1;

            // Reset tables
            ResetKillerAndHistoryTables();
            ResetPVTables();

            // Start the clock
            stopWatch = Stopwatch.StartNew();

            // ------------------------------------------------------
            //               Iterative deepening
            // ------------------------------------------------------
            for (int depth = 1; depth <= _maxDepth; depth++)
            {
                // Search position with negamax framework
                int score = Negamax(depth, ply, alpha, beta, true);

                // Timing for current depth
                float searchTimer = stopWatch.ElapsedMilliseconds;

                // Later check if previous search took much longer than time left then abort already here to save time for next move
                // ...

                // If we don't stop the search, save some variables for next iteration and write data to GUI            
                if (!stopSearch)
                {            
                    // Update best move and best score so far
                    currentBestMove = pvTable[0][0];

                    // Write data to GUI
                    Console.WriteLine($"info depth {depth} " +
                                      $"score {ScoreToString(score)} " +
                                      $"nodes {nodes} " +
                                      $"nps {Math.Round(nodes / ((searchTimer + 1) / 1000))} " +  // +1 is in case searchTimer is 0 which prevent division with zero
                                      $"time {searchTimer} " +
                                      $"pv {string.Join(' ', Printing.GetPVLine(pvTable, pvLength))}");
                }
                // Break loop if search is stopped
                else break;
            }

            // Return best move when done
            return currentBestMove;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                               NEGAMAX LOOP
        // ###################################################################################
        // -----------------------------------------------------------------------------------
        private int Negamax(int depth, int ply, int alpha, int beta, bool nullMoveAllowed)
        {
            // Init variables
            int score;            
            bool isPV = (beta - alpha) > 1;

            // -------------------------------------------------------------
            //             Check if search should be stopped
            //
            //    This is done every X nodes to not slow down too much
            //    but also be good enough for time management. 
            // -------------------------------------------------------------
            if (nodes % stopSearchCheckInterval == 0) CheckForStopSearch();
            if (stopSearch) return 0;

            // -------------------------------------------------------------
            //                  Things to not check in root
            //
            //    Here is code to only check if we are not in root (ply != 0)
            // ------------------------------------------------------------- 
            if (ply != 0)
            {

                // -------------------------------------------------------------
                //                       Check for draws
                //
                //    If we have reached a draw we return 0 as result. This
                //    could be improved to return different results based on
                //    if we want to play for win.
                // -------------------------------------------------------------
                if (boardState.CheckForDraw()) return 0;

                // -------------------------------------------------------------
                //                        Catch overflows
                //
                //    If we get too deep, stop before running out of space in 
                //    our predefined lists.
                // -------------------------------------------------------------
                if (ply > SearchConstants.maxPly - 1)
                {
                    stopSearch = true;
                    return Evaluate.Evaluation(boardState);
                }

                // -------------------------------------------------------------
                //                   Mate distance pruning
                //
                //    Makes the code faster when finding mate and it will not
                //    look for mates longer than we have already found.
                // -------------------------------------------------------------
                int mateValue = SearchConstants.mateValue - ply;
                if (alpha < -mateValue) alpha = -mateValue;
                if (beta > mateValue - 1) beta = mateValue - 1;
                if (alpha >= beta) return alpha;
            }

            // -------------------------------------------------------------
            //                      Check related
            //
            //    Find out if we are currently in check. If we are, extend
            //    the depth with 1 (check extension). 
            // -------------------------------------------------------------            
            bool isInCheck = boardState.IsSquareAttacked(boardState.colorToMove, boardState.kingPositions[boardState.colorToMove]);

            if (isInCheck) depth++;

            // -------------------------------------------------------------
            //                      Reached depth 0
            //
            //    When reaching depth 0 we go into quiescence search. 
            //    Check extension makes sure that we don't go into 
            //    quiescence when being in check. 
            // -------------------------------------------------------------
            if (depth == 0)
            {
                score = QuiescenceSearch(ply, alpha, beta);
                return score;
            }

            // Increase nodes counter 
            nodes++;

            // -------------------------------------------------------------
            //                   Init PV related stuff
            //
            //    PV table is used to save the PV line into a table. This
            //    could later be improved to instead obtain PV from TT.
            // -------------------------------------------------------------
            pvTable[ply][ply] = Move.Empty;
            pvLength[ply] = 0;

            // -------------------------------------------------------------
            //                      TT lookup
            //
            //    Here we check if the node has already been searched. If
            //    we have a match, if we are not a PV node, and if ply is
            //    not 0 then we can return the score immediately.
            // -------------------------------------------------------------
            // TT lookup (not in PV nodes, not when ply == 0 and not when we don't have an entry)
            if (Transpositions.ReadTT(boardState.zobristKey, depth, ply, alpha, beta, out Move hashMove, out int _score))
            {
                if (!isPV && ply != 0)
                {
                    // Give history bonus to TT move if giving beta cutoff and if 
                    // it is not a capturing move.
                    if (_score >= beta && !MoveFunctions.IsCapture(hashMove.flag))
                    {
                        historyMoves[hashMove.pieceMoved][hashMove.toSquare] += depth;
                    }

                    return _score;
                }
            }
            
            // -------------------------------------------------------------
            //                      Null move pruning
            //
            //    Null move means allowing opponent to do two moves in a row.
            //    If this cannot make our position worse then there is no
            //    point in searching further. We can't make two nullmoves in
            //    a row, hence the flag nullMoveAllowed which is set to false
            //    here. Null moves are not used in endgames due to risk
            //    of zugzwang.
            // -------------------------------------------------------------
            if (!isPV && 
                nullMoveAllowed && 
                depth >= SearchConstants.nullMoveMinDepth &&
                boardState.currentGamePhase > EvaluationConstants.endGameDefinition && 
                !isInCheck)
            {
                // Calculate how much to reduce                
                int R = SearchConstants.nullMoveMin;
                if (depth > 6) R = SearchConstants.nullMoveMax;

                // Make nullmove, check score, and unmake the nullmove
                boardState.MakeNullMove();
                int nullScore = -Negamax(depth - 1 - R, ply + 1, -beta, -beta + 1, false);
                boardState.UnmakeNullMove();

                // If score is larger than beta, return the null move score
                if (nullScore >= beta) return nullScore;
            }
            // -------------------------------------------------------------
            //                         Razoring
            //
            //    Inspiration taken from https://www.chessprogramming.org/Razoring.
            //    Razoring is applied if we are not in PV node, if depth is
            //    within the allowed range, when not in check, and not
            //    when we are close to checkmate.
            // -------------------------------------------------------------
            if (!isPV && 
                depth >= SearchConstants.razoringMinDepth && 
                depth <= SearchConstants.razoringMaxDepth && 
                !isInCheck &&
                !IsScoreNearCheckmate(alpha))
            {
                // Calculate static evaluation and the evaluation margin.
                int staticEval = Evaluate.Evaluation(boardState);
                int margin = SearchConstants.razoringMargin + (depth - 1) * SearchConstants.razoringMarginMultiplier;
                int futileAlpha = alpha - margin;

                // If static eval is less than futile alpha (current alpha - margin), then
                // go straight into quiescence search. 
                if (staticEval < futileAlpha)
                {
                    int result = QuiescenceSearch(ply, futileAlpha, futileAlpha + 1);

                    // If quiescence returns a result lower than futile alpha then we can 
                    // return it immediately.
                    if (result <= futileAlpha)
                    {
                        return futileAlpha;
                    }
                }
            }

            // -------------------------------------------------------------
            //                         Move generation
            //
            //    Affinity chess uses a pseudo legal move generator which
            //    means it generates moves without considering checks. 
            //    The moves are then sorted based on highest score first.
            // -------------------------------------------------------------
            Move[] moves = boardState.GetAllMoves();
            SortMoves(ply, moves, hashMove);

            // -------------------------------------------------------------
            //                     Loop through moves
            //
            //    Here we loop through all moves and see if we find any
            //    legal moves. The moves are then played and we go into
            //    next Negamax function again, but with a lower depth. 
            // -------------------------------------------------------------
            int legalMoves = 0;
            foreach (Move move in moves)
            {
                // -------------------------------------------------------------
                //                   Check for move legality
                //
                //    Try if move is legal and not makes us end up in check. 
                //    If it is not legal we continue and check the next move instead.
                // -------------------------------------------------------------
                if (move == Move.Empty) break;
                if (boardState.MakeMove(move))  
                {
                    // Increase legal moves count
                    legalMoves++;

                    // -------------------------------------------------------------
                    //                   Late move reductions (LMR)
                    //
                    //    Inspiration from https://www.chessprogramming.org/Late_Move_Reductions
                    // -------------------------------------------------------------
                    if (!isPV &&
                        depth > SearchConstants.lmrMinDepth &&
                        legalMoves > SearchConstants.lmrMinLegalMoves &&
                        !isInCheck &&
                        !MoveFunctions.IsCapture(move.flag) &&
                        !MoveFunctions.IsPromotion(move.flag))
                    {
                        if (!boardState.IsSquareAttackedByPiece(boardState.colorToMove, boardState.kingPositions[boardState.colorToMove], move.pieceMoved))
                        {
                            int reductionDepth = 1;
                            if (legalMoves > 6) 
                            {
                                reductionDepth++;
                            }
                            depth -= reductionDepth;
                        }
                    }
                    // -------------------------------------------------------------
                    //                         PVS search
                    //
                    //    If we are in a PV node we check with a smaller window. 
                    //    If score is outside of window during these conditions
                    //    we make a full research with -beta, -alpha.
                    //    If we are not in a PV node we search as normal.
                    // -------------------------------------------------------------
                    if (isPV)
                    {
                        score = -Negamax(depth - 1, ply + 1, -alpha - 1, -alpha, true);
                        if ((score > alpha) && (score < beta))
                        {
                            score = -Negamax(depth - 1, ply + 1, -beta, -alpha, true);
                        }
                    }
                    else
                    {
                        score = -Negamax(depth - 1, ply + 1, -beta, -alpha, true);
                    }
                    // -------------------------------------------------------------

                    // Unmake the move
                    boardState.UnmakeMove(move);

                    // Return if we are given the stop command
                    if (stopSearch) return 0;

                    // -------------------------------------------------------------
                    //                       Found better move
                    //
                    //    If we find a move that can raise alpha it is better than
                    //    the previous best move.
                    // -------------------------------------------------------------
                    if (score > alpha)
                    {
                        // Update variables
                        alpha = score;
                        hashMove = move;

                        // Store history moves which are used in move sorting later.
                        // These are only stored if it is a non capturing move.
                        if (!MoveFunctions.IsCapture(move.flag))
                        {
                            historyMoves[move.pieceMoved][move.toSquare] += depth;
                        }

                        // -------------------------------------------------------------
                        //                       Save PV line
                        //
                        //    The PV line is saved in the PV table as below. 
                        // -------------------------------------------------------------
                        // Write PV move to PV table for the given ply
                        pvTable[ply][ply] = move;

                        // Loop over the next ply and update PV table accordingly
                        for (int j = 0; j < pvLength[ply + 1]; j++)
                        {
                            // Copy move from deeper ply into current ply's line
                            pvTable[ply][ply + 1 + j] = pvTable[ply + 1][ply + 1 + j];
                        }

                        // Adjust PV lenght
                        pvLength[ply] = 1 + pvLength[ply + 1];
                        // -------------------------------------------------------------

                        // Fail hard beta cutoff (node fails high)
                        if (score >= beta)
                        {
                            // Store transposition tabel entry with score = beta
                            Transpositions.WriteTT(boardState.zobristKey, depth, ply, alpha, beta, beta, hashMove);

                            // Store killer moves which are used in move sorting later.
                            // These are only stored if it is a non capturing move.
                            if (!MoveFunctions.IsCapture(move.flag))
                            {
                                killerMoves[1][ply] = killerMoves[0][ply];
                                killerMoves[0][ply] = move;
                            }

                            return beta;
                        }
                    }
                }              
            }

            // -------------------------------------------------------------
            //                       Check legal moves
            //
            //    If we don't have any legal moves in the position, check 
            //    if it is mate or stalemate. Score returned when stalemate
            //    is 0 here but can as before be another value if we want
            //    to play for a win instead.
            //    Ply is added to the checkmate value to ensure that we 
            //    find the shortest way to mate.
            // -------------------------------------------------------------
            if (legalMoves == 0)
            {
                if (isInCheck)
                {
                    return -SearchConstants.mateValue + ply;
                }
                else
                {
                    return 0;
                }
            }
            // -------------------------------------------------------------

            // Write to transposition table wih score = alpha
            Transpositions.WriteTT(boardState.zobristKey, depth, ply, alpha, beta, alpha, hashMove);

            // Node fails low
            return alpha;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                               QUIESCENCE
        // ###################################################################################
        // -----------------------------------------------------------------------------------

        private int QuiescenceSearch(int ply, int alpha, int beta)
        {
            // Check if we should stop the search every x nodes
            if (nodes % stopSearchCheckInterval == 0) CheckForStopSearch();
            if (stopSearch) return 0;

            // Increase node count
            nodes ++;
            qNodes++;

            // Evaluate the position
            int eval = Evaluate.Evaluation(boardState);

            // Break to not risk any overflow
            if (ply > SearchConstants.maxPly - 1)
            {
                stopSearch = true;
                return eval;
            }

            // Fail hard beta cutoff (node fails high)
            if (eval >= beta) return beta;

            // Delta pruning
            // Later need to check for promotion captures
            int capturedPieces = boardState.capturedPieces.Count;
            if (capturedPieces > 0)
            {
                int capturedPieceValue = EvaluationConstants.piecesOpening[boardState.capturedPieces[capturedPieces - 1]];
                if (eval < alpha - capturedPieceValue - SearchConstants.deltaPruningMargin) return alpha;
            }

            // Found a better move (PV-node)
            if (eval > alpha) alpha = eval;

            // Get capture moves and sort them
            Move[] children = boardState.GetCaptureMoves(true);
            SortCaptureMoves(children);

            // Quiescence loop
            foreach (Move move in children)
            {
                // Break if finding illegal move
                if (move == Move.Empty) break;

                // Make move if legal
                if (boardState.MakeMove(move))
                {
                    // Search new gamestate
                    int score = -QuiescenceSearch(ply + 1, -beta, -alpha);

                    // Unmake the move
                    boardState.UnmakeMove(move);

                    // Found a better move
                    if (score > alpha)
                    {
                        alpha = score;

                        // Fail hard beta cutoff (node fails high)
                        if (score >= beta)
                        {
                            return beta;
                        }
                    }
                }
            }

            // Node fails low
            return alpha;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                               SORTING
        // ###################################################################################
        // -----------------------------------------------------------------------------------
        private static void SortCaptureMoves(Move[] moves)
        {
            // Assign a score to each move
            int[] scoreList = new int[boardState.movesAdded];
            for (int i = 0; i < boardState.movesAdded; i++)
            {
                scoreList[i] = ScoreCaptureMove(moves[i]);
            }

            // Sort the moves based on their score
            Array.Sort(scoreList, moves, Comparer<int>.Create((a, b) => b.CompareTo(a)));
        }

        private static int ScoreCaptureMove(Move move)
        {
            int _pieceCaptured;
            if (MoveFunctions.IsEnPassant(move.flag))
            {
                _pieceCaptured = Piece.Pawn;
            }
            else
            {
                // Loop over other sides bitboards to find the captured piece
                _pieceCaptured = MoveFunctions.GetCapturedPiece(boardState.pieces[Color.Invert(boardState.colorToMove)], move.toSquare);
            }

            return SearchConstants.MVV_LVA +
                    SearchConstants.MVV_LVA_Values[_pieceCaptured] -
                    SearchConstants.MVV_LVA_Values[move.pieceMoved];
        }

        private static void SortMoves(int ply, Move[] moves, Move _bestMove = new Move())
        {
            // Assign a score to each move
            int[] scoreList = new int[boardState.movesAdded];
            for (int i = 0; i < boardState.movesAdded; i++)
            {
                Move move = moves[i];

                // Hash move
                if (move == _bestMove)
                {
                    scoreList[i] = SearchConstants.hashScore;
                }
                // Else a normal move
                else
                {
                    scoreList[i] = ScoreMove(ply, moves[i]);
                }
            }

            // Sort the moves based on their score
            Array.Sort(scoreList, moves, Comparer<int>.Create((a, b) => b.CompareTo(a)));
        }

        private static int ScoreMove(int ply, Move move)
        {
            // Capture moves (including enpassants)
            if (MoveFunctions.IsCapture(move.flag))
            {
                int _pieceCaptured;
                if (MoveFunctions.IsEnPassant(move.flag))
                {
                    _pieceCaptured = Piece.Pawn;
                }
                else
                {
                    // Loop over other sides bitboards to find the captured piece
                    _pieceCaptured = MoveFunctions.GetCapturedPiece(boardState.pieces[Color.Invert(boardState.colorToMove)], move.toSquare);
                }

                return SearchConstants.MVV_LVA + 
                       SearchConstants.MVV_LVA_Values[_pieceCaptured] -
                       SearchConstants.MVV_LVA_Values[move.pieceMoved];
            }
            // Quiet moves
            else
            {
                // Killer move 1 or 2, else return the history move
                if (killerMoves[0][ply] == move) return SearchConstants.firstKillerMove;
                else if (killerMoves[1][ply] == move) return SearchConstants.secondKillerMove;
                else return historyMoves[move.pieceMoved][move.toSquare];

            }
        }

        // Score all moves and not sort them, currently not used
        private static void ScoreMoves(int ply, Move[] moves, Move _bestMove)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                Move move = moves[i];
                if (move == Move.Empty) break;

                // Hash move
                if (move == _bestMove)
                {
                    moves[i].score = SearchConstants.hashScore;
                }

                // Capture moves (including enpassants)
                if (MoveFunctions.IsCapture(move.flag))
                {
                    int _pieceCaptured;
                    if (MoveFunctions.IsEnPassant(move.flag))
                    {
                        _pieceCaptured = Piece.Pawn;
                    }
                    else
                    {
                        // Loop over other sides bitboards to find the captured piece
                        _pieceCaptured = MoveFunctions.GetCapturedPiece(boardState.pieces[Color.Invert(boardState.colorToMove)], move.toSquare);
                    }

                    moves[i].score = SearchConstants.MVV_LVA +
                           SearchConstants.MVV_LVA_Values[_pieceCaptured] -
                           SearchConstants.MVV_LVA_Values[move.pieceMoved];
                }
                // Quiet moves
                else
                {
                    // Killer move 1 or 2, else return the history move
                    if (killerMoves[0][ply] == move) moves[i].score = SearchConstants.firstKillerMove;
                    else if (killerMoves[1][ply] == move) moves[i].score = SearchConstants.secondKillerMove;
                    else moves[i].score = historyMoves[move.pieceMoved][move.toSquare];
                }
            }
        }

        // Sort the list and puts the best move first
        private static void SortNextMove(Move[] moves, int movesCount, int currentIndex)
        {
            // Don't sort if list is 1 or less items
            if (movesCount <= 1)
            {
                return;
            }

            int currentMaxScore = -30001;
            int currentMaxScoreIndex = -1;

            for (int i = currentIndex; i < movesCount; i++)
            {
                int moveScore = moves[i].score;
                if (moveScore > currentMaxScore)
                {
                    currentMaxScore = moveScore;
                    currentMaxScoreIndex = i;
                }
            }

            Move tempMove = moves[currentIndex];
            moves[currentIndex] = moves[currentMaxScoreIndex];
            moves[currentMaxScoreIndex] = tempMove;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                               HELPERS
        // ###################################################################################
        // -----------------------------------------------------------------------------------
        private static bool IsScoreNearCheckmate(int score)
        {
            var scoreAbs = Math.Abs(score);
            return scoreAbs >= EvaluationConstants.checkmate - SearchConstants.maxDepth &&
                   scoreAbs <= EvaluationConstants.checkmate + SearchConstants.maxDepth;
        }

        private void CheckForStopSearch()
        {
            // Check for time up
            if (stopWatch.ElapsedMilliseconds > maxTime) stopSearch = true;

            // Check for nodes searched
            else if (nodes > maxNodes) stopSearch = true;
        }

        private static string ScoreToString(float score)
        {
            // If we have found mate we return e.g. "-M4", else return the score in centipawns
            if (Math.Abs(score) > SearchConstants.mateScore)
            {
                return $"mate {Math.Sign(score) * GetMateDistance(score)}";
            }
            return $"cp {score}";
        }

        private static int GetMateDistance(float score)
        {
            // Find how close we are to mate
            int plies = SearchConstants.mateValue - (int)Math.Abs(score);
            int moves = (plies + 1) / 2;
            return moves;
        }

        private static void ResetPVTables()
        {
            pvLength = new int[SearchConstants.maxPly];
            pvTable = new Move[SearchConstants.maxPly][];
            for (int i = 0; i < SearchConstants.maxPly; i++)
            {
                pvTable[i] = new Move[SearchConstants.maxPly];
                for (int j = 0; j < SearchConstants.maxPly; j++)
                {
                    pvTable[i][j] = Move.Empty;
                }
            }
        }
        
        private static void ResetKillerAndHistoryTables()
        {
            killerMoves = new Move[2][];
            for (int i = 0; i < 2; i++)
            {
                killerMoves[i] = new Move[SearchConstants.maxPly];
                for (int j = 0; j < SearchConstants.maxPly; j++)
                {
                    killerMoves[i][j] = Move.Empty;
                }
            }
            historyMoves = new int[12][];
            for (int i = 0; i < 12; i++)
            {
                historyMoves[i] = new int[64];
                for (int j = 0; j < 64; j++)
                {
                    historyMoves[i][j] = 0;
                }
            }
        }
    }
}
