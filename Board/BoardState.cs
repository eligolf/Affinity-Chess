using System;
using System.Collections.Generic;

using AffinityChess.General;
using AffinityChess.Moves;
using AffinityChess.AI;

namespace AffinityChess.Board
{

    public sealed class BoardState
    {
        public ulong[][] pieces;
        public ulong[] occupancy;
        public ulong occupancyAll;
        public ulong enPassant;
        public ulong castling;
        public int colorToMove;
        public int movesCount;
        public int halfMoveClock;
        public int nullMoves;

        public int[] PSTValuesOpening;
        public int[] PSTValuesEnding;

        public int currentGamePhase;

        public List<int> capturedPieces;
        public List<int> promotedPieces;
        public List<ulong> castlings;
        public List<ulong> zobristKeys;
        public List<ulong> enPassants;
        public List<int> halfMoveClockList;

        public ulong zobristKey;

        public Move[] possibleMoves;
        public int movesAdded;

        public string stalemateReason;

        public int[] kingPositions;

        public BoardState()
        {
            // Init bitboards
            pieces = new ulong[2][];
            pieces[Color.White] = new ulong[6];
            pieces[Color.Black] = new ulong[6];
            occupancy = new ulong[2];

            // Init lists
            PSTValuesOpening = new int[2];
            PSTValuesEnding = new int[2];

            capturedPieces = new List<int>();
            promotedPieces = new List<int>();
            castlings = new List<ulong>();
            zobristKeys = new List<ulong>();
            enPassants = new List<ulong>();
            halfMoveClockList = new List<int>();

            // Init other variables
            currentGamePhase = 0;

            kingPositions = new int[2];
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                                GENERATE MOVES
        // ###################################################################################
        // -----------------------------------------------------------------------------------
        public Move[] GetAllMoves()
        {
            // Clear list of moves
            possibleMoves = new Move[GameConstants.maxMovesInAPosition];
            movesAdded = 0;

            GetQuietMoves(false);
            GetCaptureMoves(false);

            return possibleMoves;
        }

        public Move[] GetQuietMoves(bool justQuiets)
        {
            // Clear list of moves if we should just get the quiet moves
            if (justQuiets)
            {
                possibleMoves = new Move[GameConstants.maxMovesInAPosition];
                movesAdded = 0;
            }
            ulong possibleSquares;

            // Loop through all piece bitboards
            for (int piece = Piece.Pawn; piece <= Piece.King; piece++)
            {
                // Loop through all occupied squares for the given bitboard
                ulong pieceBitboard = pieces[colorToMove][piece];
                foreach (byte fromSquare in BitOperations.GetOccupiedSquares(pieceBitboard))
                {

                    // ##################################################################
                    //                             PAWNS
                    // ##################################################################
                    if (piece == Piece.Pawn)
                    {  
                        // One step quiet moves
                        possibleSquares = Pawns.QuietMasksOneStep[colorToMove][fromSquare] & ~occupancyAll;
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            // Check if toSquare is on a promoting row
                            if ((GameConstants.promotionRows[colorToMove] & (1ul << toSquare)) != 0)
                            {
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionKnight);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionBishop);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionRook);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionQueen);
                            }
                            // Else a normal one step move
                            else
                            {
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.Quiet);
                            }
                        }

                        // Two step (quiet moves)
                        int direction = colorToMove == Color.White ? 8 : -8;
                        possibleSquares = Pawns.QuietMasksTwoStep[colorToMove][fromSquare] & ~occupancyAll;
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            // Need to check that the one step square is empty to be able to do 2 step move
                            if (((1ul << (toSquare + direction)) & (GameConstants.oneStepPawnMoveRows[colorToMove] & occupancyAll)) == 0)
                            {
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.TwoStepPawn);
                            }                            
                        }
                    }

                    // ##################################################################
                    //                              KING
                    // ##################################################################
                    else if (piece == Piece.King)
                    {
                        // Quiet moves
                        possibleSquares = Kings.AttackMasks[fromSquare] & ~occupancyAll;
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.King, (byte)MoveFlags.Quiet);
                        }                      

                        // CASTLING

                        // White side
                        if (colorToMove == Color.White)
                        {
                            // King side
                            if (Kings.WhiteCanCastleKingSide(this, colorToMove))
                            {
                                AddMoveToPossibleMoves(fromSquare, (byte)(fromSquare + 2), Piece.King, (byte)MoveFlags.Castle);
                            }
                            // Queen side
                            if (Kings.WhiteCanCastleQueenSide(this, colorToMove))
                            {
                                AddMoveToPossibleMoves(fromSquare, (byte)(fromSquare - 2), Piece.King, (byte)MoveFlags.Castle);
                            }
                        }
                        // Black side
                        else
                        {
                            // King side
                            if (Kings.BlackCanCastleKingSide(this, colorToMove))
                            {
                                AddMoveToPossibleMoves(fromSquare, (byte)(fromSquare + 2), Piece.King, (byte)MoveFlags.Castle);
                            }
                            // Queen side
                            if (Kings.BlackCanCastleQueenSide(this, colorToMove))
                            {
                                AddMoveToPossibleMoves(fromSquare, (byte)(fromSquare - 2), Piece.King, (byte)MoveFlags.Castle);
                            }
                        }
                    }

                    // ##################################################################
                    //                       ALL OTHER PIECES
                    // ##################################################################

                    // Knight
                    else if (piece == Piece.Knight)
                    {
                        // Quiet moves
                        possibleSquares = Knights.AttackMasks[fromSquare] & ~occupancyAll;
                        foreach (int toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, (byte)(toSquare), Piece.Knight, (byte)MoveFlags.Quiet);
                        }
                    }

                    // Bishop
                    else if (piece == Piece.Bishop)
                    {
                        // Quiet moves
                        possibleSquares = Bishops.GetAttacks(fromSquare, occupancyAll) & ~occupancyAll;
                        foreach (int toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, (byte)(toSquare), Piece.Bishop, (byte)MoveFlags.Quiet);
                        }
                    }

                    // Rook
                    else if (piece == Piece.Rook)
                    {
                        // Quiet moves
                        possibleSquares = Rooks.GetAttacks(fromSquare, occupancyAll) & ~occupancyAll;
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Rook, (byte)MoveFlags.Quiet);
                        }
                    }

                    // Queen
                    else if (piece == Piece.Queen)
                    {
                        // Quiet moves
                        possibleSquares = Queens.GetAttacks(fromSquare, occupancyAll) & ~occupancyAll;
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, (toSquare), Piece.Queen, (byte)MoveFlags.Quiet);
                        }
                    }
                }
            }

            return possibleMoves;
        }

        public Move[] GetCaptureMoves(bool justCaptures)
        {
            // Clear list of moves if we should just get capturing moves
            if (justCaptures)
            {
                possibleMoves = new Move[GameConstants.maxMovesInAPosition];
                movesAdded = 0;
            }
            ulong possibleSquares;

            // Loop through all piece bitboards
            for (int piece = Piece.Pawn; piece <= Piece.King; piece++)
            {
                // Loop through all occupied squares for the given bitboard
                ulong pieceBitboard = pieces[colorToMove][piece];
                foreach (byte fromSquare in BitOperations.GetOccupiedSquares(pieceBitboard))
                {
                    // ##################################################################
                    //                             PAWNS
                    // ##################################################################
                    if (piece == Piece.Pawn)
                    {
                        // Find if we are on a promotion square
                        List<int> promotionSquares = colorToMove == Color.White ?
                            GameConstants.PromotionSquaresWhite : GameConstants.PromotionSquaresBlack;

                        // Captures
                        possibleSquares = Pawns.AttackMasks[colorToMove][fromSquare] & occupancy[Color.Invert(colorToMove)];
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            // No promotions 
                            if (!promotionSquares.Contains(toSquare))
                            {
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.Capture);
                            }
                            // Promotions
                            else
                            {
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionKnightCapture);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionBishopCapture);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionRookCapture);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.PromotionQueenCapture);
                            }
                        }

                        // Enpassant
                        if (enPassant != GameConstants.EmptyBitboard)
                        {
                            ulong epAttacks = Pawns.AttackMasks[colorToMove][fromSquare] & enPassant;
                            if (epAttacks != GameConstants.EmptyBitboard)
                            {
                                byte toSquare = (byte)BitOperations.GetLSBIndex(epAttacks);
                                AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Pawn, (byte)MoveFlags.EnPassant);
                            }
                        }
                    }

                    // ##################################################################
                    //                              KING
                    // ##################################################################
                    else if (piece == Piece.King)
                    {
                        // Capture moves
                        possibleSquares = Kings.AttackMasks[fromSquare] & occupancy[Color.Invert(colorToMove)];
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.King, (byte)MoveFlags.Capture);
                        }                       
                    }

                    // ##################################################################
                    //                       ALL OTHER PIECES
                    // ##################################################################

                    // Knight
                    else if (piece == Piece.Knight)
                    {
                        // Capture moves
                        possibleSquares = Knights.AttackMasks[fromSquare] & occupancy[Color.Invert(colorToMove)];
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Knight, (byte)MoveFlags.Capture);
                        }
                    }

                    // Bishop
                    else if (piece == Piece.Bishop)
                    {
                        // Capture moves
                        possibleSquares = Bishops.GetAttacks(fromSquare, occupancyAll) & occupancy[Color.Invert(colorToMove)];
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Bishop, (byte)MoveFlags.Capture);
                        }
                    }

                    // Rook
                    else if (piece == Piece.Rook)
                    {
                        // Capture moves
                        possibleSquares = Rooks.GetAttacks(fromSquare, occupancyAll) & occupancy[Color.Invert(colorToMove)];
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Rook, (byte)MoveFlags.Capture);
                        }
                    }

                    // Queen
                    else if (piece == Piece.Queen)
                    {
                        // Capture moves
                        possibleSquares = Queens.GetAttacks(fromSquare, occupancyAll) & occupancy[Color.Invert(colorToMove)];
                        foreach (byte toSquare in BitOperations.GetOccupiedSquares(possibleSquares))
                        {
                            AddMoveToPossibleMoves(fromSquare, toSquare, Piece.Queen, (byte)MoveFlags.Capture);
                        }
                    }
                }
            }

            return possibleMoves;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                            MAKE AND UNMAKE MOVES
        // ###################################################################################
        // -----------------------------------------------------------------------------------

        public bool MakeMove(Move move)
        {
            // Extract variables from given move
            byte fromSquare = move.fromSquare;
            byte toSquare = move.toSquare;
            byte pieceMoved = move.pieceMoved;
            byte moveFlag = move.flag;

            int enemyColor = Color.Invert(colorToMove);

            // Increase counters
            if (colorToMove == Color.White) movesCount++;

            // Add variables that can't be tracked in any other way to lists
            castlings.Add(castling);
            zobristKeys.Add(zobristKey);
            enPassants.Add(enPassant);
            halfMoveClockList.Add(halfMoveClock);

            // Update king position
            if (pieceMoved == Piece.King)
            {
                kingPositions[colorToMove] = toSquare;
            }

            // Increase halfmove clock, resets to 0 further down if applicable
            halfMoveClock++;
            if (pieceMoved == Piece.Pawn || MoveFunctions.IsCapture(moveFlag) || MoveFunctions.IsCastling(moveFlag))
            {
                halfMoveClock = 0;
            }
            else halfMoveClock++;

            // Check if enpassant is possible and if so disable it
            if (enPassant != 0)
            {
                int enPassantRank = BitOperations.BitScan(enPassant) % 8;
                zobristKey = Zobrist.ToggleEnPassant(zobristKey, enPassantRank);
                enPassant = 0; 
            }

            // ------------------------------------------------------
            //            Check for different move types
            // ------------------------------------------------------

            // Quiet moves that are not double pushes
            if (MoveFunctions.IsQuietNotTwoStep(moveFlag))
            {
                MovePiece(colorToMove, pieceMoved, fromSquare, toSquare);
                zobristKey = Zobrist.MovePiece(zobristKey, colorToMove, pieceMoved, fromSquare, toSquare);

                // Reset halfmove clock if pawn move
                if (pieceMoved == Piece.Pawn) halfMoveClock = 0;

            }
            // Double pawn pushes
            else if (MoveFunctions.IsDoublePush(moveFlag))
            {
                // Reset halfmove clock
                halfMoveClock = 0;

                // Move piece
                MovePiece(colorToMove, pieceMoved, fromSquare, toSquare);
                zobristKey = Zobrist.MovePiece(zobristKey, colorToMove, pieceMoved, fromSquare, toSquare);

                // Enable enpassant
                ulong epSquare = colorToMove == Color.White ? 1ul << toSquare + 8 : 1ul << toSquare - 8;
                enPassant |= epSquare;
                zobristKey = Zobrist.ToggleEnPassant(zobristKey, BitOperations.BitScan(epSquare) % 8);

            }
            // Enpassant moves
            else if (MoveFunctions.IsEnPassant(moveFlag))
            {
                // Find enemy pawn (which is not on the toSquare)
                int enemyPieceSquare = colorToMove == Color.White ? toSquare + 8 : toSquare - 8;
                int capturedPiece = Piece.Pawn;
                capturedPieces.Add(capturedPiece);

                // Move own pawn and remove enemy pawn
                MovePiece(colorToMove, pieceMoved, fromSquare, toSquare);
                zobristKey = Zobrist.MovePiece(zobristKey, colorToMove, pieceMoved, fromSquare, toSquare);
                RemovePiece(enemyColor, capturedPiece, enemyPieceSquare);
                zobristKey = Zobrist.AddOrRemovePiece(zobristKey, enemyColor, capturedPiece, enemyPieceSquare);
            }
            // Capture moves
            else if (MoveFunctions.IsCapture(moveFlag))
            {
                // Reset halfmove clock
                halfMoveClock = 0;

                // Remove captured piece from its square and add it to list of captured pieces
                int capturedPiece = MoveFunctions.GetCapturedPiece(pieces[enemyColor], toSquare);
                RemovePiece(enemyColor, capturedPiece, toSquare);
                zobristKey = Zobrist.AddOrRemovePiece(zobristKey, enemyColor, capturedPiece, toSquare);
                capturedPieces.Add(capturedPiece);

                // Check for promotion captures and if so move in special way
                if (MoveFunctions.IsPromotion(moveFlag))
                {
                    // Find the piece that we should promote to
                    int promotionPiece = GetPromotionPiece(moveFlag);

                    // Remove pawn from square and add the promoted piece instead
                    RemovePiece(colorToMove, pieceMoved, fromSquare);
                    zobristKey = Zobrist.AddOrRemovePiece(zobristKey, colorToMove, pieceMoved, fromSquare);
                    AddPiece(colorToMove, promotionPiece, toSquare);
                    zobristKey = Zobrist.AddOrRemovePiece(zobristKey, colorToMove, promotionPiece, toSquare);

                    // Add to list of promoted pieces
                    promotedPieces.Add(promotionPiece);
                }
                // If not it is a normal capture
                else
                {
                    // Move the piece as normal
                    MovePiece(colorToMove, pieceMoved, fromSquare, toSquare);
                    zobristKey = Zobrist.MovePiece(zobristKey, colorToMove, pieceMoved, fromSquare, toSquare);
                }
            }
            // Castling moves
            else if (MoveFunctions.IsCastling(moveFlag))
            {
                // Reset halfmove clock
                halfMoveClock = 0;

                // Get from and to square for the rook
                int rookFromSquare = GameConstants.rookCastling[toSquare][0];
                int rookToSquare = GameConstants.rookCastling[toSquare][1];

                // Move rook and king to the correct places
                MovePiece(colorToMove, Piece.King, fromSquare, toSquare);
                MovePiece(colorToMove, Piece.Rook, rookFromSquare, rookToSquare);

                zobristKey = Zobrist.MovePiece(zobristKey, colorToMove, Piece.King, fromSquare, toSquare);
                zobristKey = Zobrist.MovePiece(zobristKey, colorToMove, Piece.Rook, rookFromSquare, rookToSquare);
            
            }
            // Normal promotion moves that are not captures
            else if (MoveFunctions.IsPromotion(moveFlag))
            {
                // Get what piece was promoted
                int promotionPiece = GetPromotionPiece(moveFlag);

                // Remove pawn from square and add promoted piece on toSquare
                RemovePiece(colorToMove, Piece.Pawn, fromSquare);
                zobristKey = Zobrist.AddOrRemovePiece(zobristKey, colorToMove, pieceMoved, fromSquare);
                AddPiece(colorToMove, promotionPiece, toSquare);
                zobristKey = Zobrist.AddOrRemovePiece(zobristKey, colorToMove, pieceMoved, toSquare);

                // Add piece to list
                promotedPieces.Add(promotionPiece);
            }

            // If castling rights, check and remove the correct ones
            if (castling != 0)
            {
                zobristKey = Zobrist.AddOrRemoveCastlingRights(zobristKey, castling);
                castling &= GameConstants.updateCastlingRights[fromSquare];
                castling &= GameConstants.updateCastlingRights[toSquare];
                zobristKey = Zobrist.AddOrRemoveCastlingRights(zobristKey, castling);
            }

            // Check if move is legal (king is not under attack after move is done)
            if (!IsSquareAttacked(colorToMove, kingPositions[colorToMove]))
            {
                // Change turns
                colorToMove = enemyColor;
                zobristKey = Zobrist.ChangeSide(zobristKey);

                return true;
            }
            // If not, then unmake the move and return false
            else
            {
                // Change turns
                colorToMove = enemyColor;
                zobristKey = Zobrist.ChangeSide(zobristKey);

                UnmakeMove(move);
                return false;
            }
        }

        public void UnmakeMove(Move move)
        {
            // Extract variables from move
            byte fromSquare = move.fromSquare;
            byte toSquare = move.toSquare;
            byte pieceMoved = move.pieceMoved;
            byte moveFlag = move.flag;

            // Change turn
            colorToMove = Color.Invert(colorToMove);

            // Update king position
            if (pieceMoved == Piece.King)
            {
                kingPositions[colorToMove] = fromSquare;
            }

            // ------------------------------------------------------
            //            Check for different move types
            // ------------------------------------------------------

            // Single or double pawn pushes
            if (MoveFunctions.IsQuietNotTwoStep(moveFlag) || MoveFunctions.IsDoublePush(moveFlag))
            {
                MovePiece(colorToMove, pieceMoved, toSquare, fromSquare);
            }
            // Enpassants
            else if (MoveFunctions.IsEnPassant(moveFlag))
            {
                int enemyColor = Color.Invert(colorToMove);
                int enemyPieceField = colorToMove == Color.White ? (byte)(toSquare + 8) : (byte)(toSquare - 8);
                int capturedPiece = capturedPieces[capturedPieces.Count - 1];
                capturedPieces.RemoveAt(capturedPieces.Count - 1);

                MovePiece(colorToMove, Piece.Pawn, toSquare, fromSquare);
                AddPiece(enemyColor, capturedPiece, enemyPieceField);
            }
            // Capturing moves
            else if (MoveFunctions.IsCapture(moveFlag))
            {
                int enemyColor = Color.Invert(colorToMove);
                int capturedPiece = capturedPieces[capturedPieces.Count - 1];
                capturedPieces.RemoveAt(capturedPieces.Count - 1);

                // Capturing promotion
                if (MoveFunctions.IsPromotion(moveFlag))
                {
                    int promotedPiece = promotedPieces[promotedPieces.Count - 1];
                    promotedPieces.RemoveAt(promotedPieces.Count - 1);
                    RemovePiece(colorToMove, promotedPiece, toSquare);
                    AddPiece(colorToMove, Piece.Pawn, fromSquare);
                }
                else
                {
                    MovePiece(colorToMove, pieceMoved, toSquare, fromSquare);
                }

                AddPiece(enemyColor, capturedPiece, toSquare);
            }
            // Castling
            else if (MoveFunctions.IsCastling(moveFlag))
            {
                // Get from and to square for the rook
                int rookFromSquare = GameConstants.rookCastling[toSquare][0];
                int rookToSquare = GameConstants.rookCastling[toSquare][1];

                // Move rook and king to the correct places
                MovePiece(colorToMove, Piece.King, toSquare, fromSquare);
                MovePiece(colorToMove, Piece.Rook, rookToSquare, rookFromSquare);
            }
            // Promotions
            else if (MoveFunctions.IsPromotion(moveFlag))
            {
                int promotedPiece = promotedPieces[promotedPieces.Count - 1];
                promotedPieces.RemoveAt(promotedPieces.Count - 1);
                RemovePiece(colorToMove, promotedPiece, toSquare);
                AddPiece(colorToMove, Piece.Pawn, fromSquare);
            }

            // Update with latest move variables and remove from lists
            int endIndex = halfMoveClockList.Count - 1;
            halfMoveClock = halfMoveClockList[endIndex];
            halfMoveClockList.RemoveAt(endIndex);

            endIndex = zobristKeys.Count - 1;
            zobristKey = zobristKeys[endIndex];
            zobristKeys.RemoveAt(endIndex);

            endIndex = castlings.Count - 1;
            castling = castlings[endIndex];
            castlings.RemoveAt(endIndex);

            endIndex = enPassants.Count - 1;
            enPassant = enPassants[endIndex];
            enPassants.RemoveAt(endIndex);

            // Decrease moves count if white to move
            if (colorToMove == Color.White) movesCount--;
        }

        public void MakeNullMove()
        {
            // Add null move
            nullMoves++;

            // Increase moves count if side to move is white
            if (colorToMove == Color.White) movesCount++;

            // Add enpassant and zobrist to lists
            enPassants.Add(enPassant);
            zobristKeys.Add(zobristKey);

            // Check if there was an enpassant possible and if so reset its state
            if (enPassant != 0)
            {
                int enPassantRank = BitOperations.BitScan(enPassant) % 8;
                zobristKey = Zobrist.ToggleEnPassant(zobristKey, enPassantRank);
                enPassant = 0;
            }

            // Change color to move
            colorToMove = Color.Invert(colorToMove);
            zobristKey = Zobrist.ChangeSide(zobristKey);
        }

        public void UnmakeNullMove()
        {
            // Remove null move
            nullMoves--;

            // Change color to move
            colorToMove = Color.Invert(colorToMove);

            // Retrieve Zobrist key and enpassant from lists
            int endIndex = zobristKeys.Count - 1;
            zobristKey = zobristKeys[endIndex];
            zobristKeys.RemoveAt(endIndex);

            endIndex = enPassants.Count - 1;
            enPassant = enPassants[endIndex];
            enPassants.RemoveAt(endIndex);

            // Decrease moves count if side to move is white
            if (colorToMove == Color.White) movesCount--;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                                EVALUATION
        // ###################################################################################
        // -----------------------------------------------------------------------------------

        public void CalculateEvaluationValues()
        {
            // Calculate PST values for opening and endgame for white and black
            PSTValuesOpening[Color.White] = CalculatePST(Color.White, true);
            PSTValuesEnding[Color.White] = CalculatePST(Color.White, false);
            PSTValuesOpening[Color.Black] = CalculatePST(Color.Black, true);
            PSTValuesEnding[Color.Black] = CalculatePST(Color.Black, false);

            // Calculate the current game phase
            CalculateGamePhase();
        }

        public int CalculatePST(int color, bool isOpeningPhase)
        {
            int result = 0;
            for (int piece = 0; piece < 6; piece++)
            {
                // Get piece occupancies
                ulong pieceBitboard = pieces[color][piece];
                List<int> occupiedSquares = BitOperations.GetOccupiedSquares(pieceBitboard);

                // Loop through all piece occupancies and add PST data for each one.
                foreach (int square in occupiedSquares)
                {
                    if (isOpeningPhase) result += PSTData.openingTables[piece][color][square];
                    else result += PSTData.endingTables[piece][color][square];
                }
            }

            return result;
        }

        private void CalculateGamePhase()
        {
            currentGamePhase = 0;
            for (int color = Color.White; color <= Color.Black; color++)
            {
                for (int piece = Piece.Pawn; piece <= Piece.King; piece++)
                {
                    // Get piece occupancies
                    ulong pieceBitboard = pieces[color][piece];
                    List<int> occupiedSquares = BitOperations.GetOccupiedSquares(pieceBitboard);

                    // Loop through all piece occupancies and add game phase value
                    foreach (int square in occupiedSquares)
                    {
                        currentGamePhase += EvaluationConstants.gamePhaseInc[piece];
                    }
                }
            }
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                                   SQUARE  
        // ###################################################################################
        // -----------------------------------------------------------------------------------
        public bool IsSquareAttackedByPiece(int color, int square, int piece)
        {
            // Invert color to check for enemy pieces attacking
            int enemyColor = Color.Invert(color);

            switch (piece)
            {
                case Piece.Pawn:
                    ulong field = 1ul << square;
                    ulong potentialPawns = GenerateKingMoves.GetMoves(square) & pieces[enemyColor][Piece.Pawn];
                    ulong attackingPawns = color == Color.White ?
                        field & ((potentialPawns << 7) | (potentialPawns << 9)) :
                        field & ((potentialPawns >> 7) | (potentialPawns >> 9));
                    if (attackingPawns != 0) return true;
                    else return false;
                case Piece.Knight:
                    ulong attackingKnights = GenerateKnightMoves.GetMoves(square) & pieces[enemyColor][Piece.Knight];
                    if (attackingKnights != 0) return true;
                    else return false;
                case Piece.Bishop:
                    ulong diagonalAttacks = GenerateBishopMoves.GetMoves(occupancyAll, square) & occupancy[enemyColor];
                    ulong attackingBishops = diagonalAttacks & (pieces[enemyColor][Piece.Bishop] | pieces[enemyColor][Piece.Queen]);
                    if (attackingBishops != 0) return true;
                    else return false;
                case Piece.Rook:
                    ulong colRowAttacks = GenerateRookMoves.GetMoves(occupancyAll, square) & occupancy[enemyColor];
                    ulong attackingRooks = colRowAttacks & (pieces[enemyColor][Piece.Rook] | pieces[enemyColor][Piece.Queen]);
                    if (attackingRooks != 0) return true;
                    else return false;
                case Piece.Queen:
                    colRowAttacks = GenerateRookMoves.GetMoves(occupancyAll, square) & occupancy[enemyColor];
                    attackingRooks = colRowAttacks & (pieces[enemyColor][Piece.Rook] | pieces[enemyColor][Piece.Queen]);
                    diagonalAttacks = GenerateBishopMoves.GetMoves(occupancyAll, square) & occupancy[enemyColor];
                    attackingBishops = diagonalAttacks & (pieces[enemyColor][Piece.Bishop] | pieces[enemyColor][Piece.Queen]);
                    if (attackingRooks != 0 || attackingBishops != 0) return true;
                    else return false;
                case Piece.King:
                    ulong kingAttacks = GenerateKingMoves.GetMoves(square);
                    ulong attackingKings = kingAttacks & pieces[enemyColor][Piece.King];
                    if (attackingKings != 0) return true;
                    else return false;
                default:
                    return true;
            }
        }

        public bool IsSquareAttacked(int color, int square)
        {
            // Invert color to check for enemy pieces attacking
            int enemyColor = Color.Invert(color);

            // Horizontal and vertical attacks (rook or queen)
            ulong colRowAttacks = GenerateRookMoves.GetMoves(occupancyAll, square) & occupancy[enemyColor];
            ulong attackingRooks = colRowAttacks & (pieces[enemyColor][Piece.Rook] | pieces[enemyColor][Piece.Queen]);
            if (attackingRooks != 0) return true;

            // Diagonal attacks (bishop or queen)
            ulong diagonalAttacks = GenerateBishopMoves.GetMoves(occupancyAll, square) & occupancy[enemyColor];
            ulong attackingBishops = diagonalAttacks & (pieces[enemyColor][Piece.Bishop] | pieces[enemyColor][Piece.Queen]);
            if (attackingBishops != 0) return true;

            // Knight attacks
            ulong attackingKnights = GenerateKnightMoves.GetMoves(square) & pieces[enemyColor][Piece.Knight];
            if (attackingKnights != 0) return true;

            // King attacks
            ulong kingAttacks = GenerateKingMoves.GetMoves(square);
            ulong attackingKings = kingAttacks & pieces[enemyColor][Piece.King];
            if (attackingKings != 0) return true;

            // Pawn attacks
            ulong field = 1ul << square;
            ulong potentialPawns = kingAttacks & pieces[enemyColor][Piece.Pawn];
            ulong attackingPawns = color == Color.White ?
                field & ((potentialPawns << 7) | (potentialPawns << 9)) :
                field & ((potentialPawns >> 7) | (potentialPawns >> 9));
            if (attackingPawns != 0) return true;

            // Return false if not finding any attackers
            return false;
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                            ADD OR REMOVE PIECES
        // ###################################################################################
        // -----------------------------------------------------------------------------------

        private void AddMoveToPossibleMoves(byte _fromSquare, byte _toSquare, byte _pieceMoved, byte _flag)
        {
            Move entry = new Move(_fromSquare, _toSquare, _pieceMoved, _flag);
            possibleMoves[movesAdded] = entry;
            movesAdded++;
        }

        public void AddPiece(int color, int piece, int square)
        {
            // Add piece to corresponding place on bitboards
            pieces[color][piece] ^= (1ul << square);
            occupancy[color] ^= (1ul << square);
            occupancyAll ^= (1ul << square);

            // Update PST values
            PSTValuesOpening[color] += PSTData.openingTables[piece][color][square];
            PSTValuesEnding[color] += PSTData.endingTables[piece][color][square];

            // Add piece to game phase
            currentGamePhase += EvaluationConstants.gamePhaseInc[piece];
        }

        public void MovePiece(int color, int piece, int fromSquare, int toSquare)
        {            
            // Make a bitboard with the move from and to square as 1:s
            ulong move = (1ul << fromSquare) | (1ul << toSquare);

            // Removes and adds the piece on the correct places
            pieces[color][piece] ^= move;
            occupancy[color] ^= move;
            occupancyAll ^= move;

            // Update the PST values
            PSTValuesOpening[color] -= PSTData.openingTables[piece][color][fromSquare];
            PSTValuesOpening[color] += PSTData.openingTables[piece][color][toSquare];

            PSTValuesEnding[color] -= PSTData.endingTables[piece][color][fromSquare];
            PSTValuesEnding[color] += PSTData.endingTables[piece][color][toSquare];

        }

        public void RemovePiece(int color, int piece, int square)
        {
            // Remove piece from corresponding place on bitboard
            pieces[color][piece] ^= (1ul << square);
            occupancy[color] ^= (1ul << square);
            occupancyAll ^= (1ul << square);

            // Update PST values
            PSTValuesOpening[color] -= PSTData.openingTables[piece][color][square];
            PSTValuesEnding[color] -= PSTData.endingTables[piece][color][square];

            // Remove piece from game phase
            currentGamePhase -= EvaluationConstants.gamePhaseInc[piece];
        }

        // -----------------------------------------------------------------------------------
        // ###################################################################################
        //                              HELPER FUNCTIONS
        // ###################################################################################
        // -----------------------------------------------------------------------------------

        private int GetPromotionPiece(int moveFlag)
        {
            // Check for the move flag and return the correct piece
            if (moveFlag == (int)MoveFlags.PromotionKnight || moveFlag == (int)MoveFlags.PromotionKnightCapture) return Piece.Knight;
            else if (moveFlag == (int)MoveFlags.PromotionBishop || moveFlag == (int)MoveFlags.PromotionBishopCapture) return Piece.Bishop;
            else if (moveFlag == (int)MoveFlags.PromotionRook || moveFlag == (int)MoveFlags.PromotionRookCapture) return Piece.Rook;
            else if (moveFlag == (int)MoveFlags.PromotionQueen || moveFlag == (int)MoveFlags.PromotionQueenCapture) return Piece.Queen;
            else throw new InvalidOperationException();
        }

        public bool CheckForDraw()
        {
            // 3 fold repetition
            int lengthOfZobrist = zobristKeys.Count;
            int positionsToCheck = Math.Min(lengthOfZobrist, halfMoveClock + 1);
            if (nullMoves == 0 && positionsToCheck >= 8)
            {
                int repetitions = 1;
                for (int i = lengthOfZobrist - 2; i >= lengthOfZobrist - positionsToCheck; i -= 2)
                {
                    if (zobristKeys[i] == zobristKey)
                    {
                        repetitions++;
                        if (repetitions >= 3)
                        {
                            stalemateReason = "3 fold repetition.";
                            return true;
                        }
                    }
                }
            }            

            // Check if halfmove counter reached 100 (50 full moves)
            if (nullMoves == 0 && halfMoveClock >= 100)
            {
                stalemateReason = "50 move rule.";
                return true;
            }

            // No draw found
            return false;
        }
    }
}