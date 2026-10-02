using System.Runtime.CompilerServices;
using Newtonsoft.Json;

public class MLEvaluation
{
    //TODO: Mobility? Rooks on open files? Mopup score? 
    public static readonly double[] weights = new double[768 + 6 + 9 + 7 + 6]; //TODO: We can easily eliminate bounds checks when using these arrays
    public static double[] features = new double[weights.Length]; //TODO: We can easily eliminate bounds checks when using these arrays
    public static double bias;

    public double GetEval(Board board)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            features[i] = 0;
        }

        CalculatePhase(board);

        CalculateFeatures(board);



        double result = bias;

        for (int i = 0; i < weights.Length; i++)
        {
            result += features[i] * weights[i];
        }

        int perspective = board.colorToMove == Piece.White ? 1 : -1; //TODO: Remove i think

        //Console.WriteLine("Phase: " + phase);

        return result /** perspective*/;
    }

    private void CalculateFeatures(Board board)
    {
        //LogData();
        CalculatePieceSquareTables(board);
        //LogData();
        CalculateMaterial(board);
        //LogData();
        CalculatePawnStructure(board);
        //LogData();
        CalculateKingSafety(board);

        CalculateMobility(board);
    }

    public static void InitializeWeights()
    {
        //Initialize material weights by hand to give a good starting point
        weights[768] = 0.390625f; //Pawn weight
        weights[769] = 1.171875f; //Knight weight
        weights[770] = 1.171875f; //Bishop weight
        weights[771] = 1.953125f; //Rook weight
        weights[772] = 3.515625f; //Queen weight
    }

    public static void LogData()
    {
        Console.WriteLine("Weights: " + JsonConvert.SerializeObject(weights));
        Console.WriteLine("Features: " + JsonConvert.SerializeObject(features));
        Console.WriteLine("Bias: " + bias);
    }

    public static void LogDataAsInt(int shift)
    {
        double multiplier = (double)Math.Pow(2f, shift);

        int[] intWeights = new int[weights.Length];
        //int[] intFeatures = new int[weights.Length];

        for (int i = 0; i < weights.Length; i++)
        {
            intWeights[i] = (int)Math.Round(weights[i] * multiplier);
            //intFeatures[i] = (int)Math.Round(features[i] * multiplier);
        }

        int intBias = (int)Math.Round(bias * multiplier);

        Console.WriteLine("IntWeights: " + JsonConvert.SerializeObject(intWeights));
        //Console.WriteLine("IntFeatures: " + JsonConvert.SerializeObject(intFeatures));
        Console.WriteLine("IntBias: " + intBias);
    }

    #region Phase

    private const double KnightPhase = 1f;
    private const double BishopPhase = 1f;
    private const double RookPhase = 2f;
    private const double QueenPhase = 4f;

    private const double MaxPhase = KnightPhase * 4f + BishopPhase * 4f + RookPhase * 4f + QueenPhase * 2f;
    private double phase; //Phase is between 0 (MG) and 100 (EwhiteBishopListG)

    private void CalculatePhase(Board board)
    {
        phase = MaxPhase;

        phase -= board.GetPieceList(Piece.Knight, 0).Count * KnightPhase;
        phase -= board.GetPieceList(Piece.Knight, 1).Count * KnightPhase;
        phase -= board.GetPieceList(Piece.Bishop, 0).Count * BishopPhase;
        phase -= board.GetPieceList(Piece.Bishop, 1).Count * BishopPhase;
        phase -= board.GetPieceList(Piece.Rook, 0).Count * RookPhase;
        phase -= board.GetPieceList(Piece.Rook, 1).Count * RookPhase;
        phase -= board.GetPieceList(Piece.Queen, 0).Count * QueenPhase;
        phase -= board.GetPieceList(Piece.Queen, 1).Count * QueenPhase;

        //TODO:           Remove this when working with ints vvvvvv
        phase = (phase * 100f + (MaxPhase / 2f)) / MaxPhase - 0.5f;
    }

    #endregion


    #region Features

    #region PSQT
    //6 pieces * 64 squares * 2 game stages = 768 features
    private void CalculatePieceSquareTables(Board board)
    {
        //TODO: find more efficient way to do this?

        //TODO: Use the allPieceList array instead of calling GetPiecelist

        //SetPSQTFeaturesWhite(board.GetPieceList(Piece.King, 0), 64 * 0);
        features[board.whiteKingSquare] += 1f - (phase / 100f);
        features[board.whiteKingSquare + 384] += phase / 100f;

        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Pawn, 0), 64 * 1);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Knight, 0), 64 * 2);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Bishop, 0), 64 * 3);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Rook, 0), 64 * 4);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Queen, 0), 64 * 5);


        //SetPSQTFeaturesBlack(board.GetPieceList(Piece.King, 1), 64 * 0);
        features[BoardHelper.FlipIndex(board.blackKingSquare)] -= 1f - (phase / 100f);
        features[BoardHelper.FlipIndex(board.blackKingSquare) + 384] -= phase / 100f;

        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Pawn, 1), 64 * 1);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Knight, 1), 64 * 2);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Bishop, 1), 64 * 3);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Rook, 1), 64 * 4);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Queen, 1), 64 * 5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPSQTFeaturesWhite(PieceList list, int PSQTOffset)
    {
        for (int i = 0; i < list.Count; i++)
        {
            features[list[i] + PSQTOffset] += 1f - (phase / 100f); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame

            features[list[i] + PSQTOffset + 384] += phase / 100f; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPSQTFeaturesBlack(PieceList list, int PSQTOffset)
    {
        for (int i = 0; i < list.Count; i++)
        {
            //TODO: Rename "flipindex" to "mirrorindex"
            int mirroredSquare = BoardHelper.FlipIndex(list[i]);

            features[mirroredSquare + PSQTOffset] -= 1f - (phase / 100f); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame

            features[mirroredSquare + PSQTOffset + 384] -= phase / 100f; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }
    }
    #endregion


    #region Material
    //5 material differences + 1 bishop pair difference = 6 features
    private void CalculateMaterial(Board board)
    {
        //TODO: Use the allPieceList array instead of calling GetPiecelist
        features[768] = board.GetPieceList(Piece.Pawn, 0).Count - board.GetPieceList(Piece.Pawn, 1).Count;
        features[769] = board.GetPieceList(Piece.Knight, 0).Count - board.GetPieceList(Piece.Knight, 1).Count;
        features[770] = board.GetPieceList(Piece.Bishop, 0).Count - board.GetPieceList(Piece.Bishop, 1).Count;
        features[771] = board.GetPieceList(Piece.Rook, 0).Count - board.GetPieceList(Piece.Rook, 1).Count;
        features[772] = board.GetPieceList(Piece.Queen, 0).Count - board.GetPieceList(Piece.Queen, 1).Count;

        double whiteBishopPair = board.GetPieceList(Piece.Bishop, 0).Count > 1 ? 1f : 0f; //TODO: Obv don't call this again
        double blackBishopPair = board.GetPieceList(Piece.Bishop, 1).Count > 1 ? 1f : 0f; //TODO: Obv don't call this again

        features[773] = whiteBishopPair - blackBishopPair;
    }
    #endregion

    #region Pawn Structure
    //1 doubled pawn difference + 1 isolated pawn difference + (0) backward pawn difference + 6 passed pawn buckets + 1 connected passed pawn difference = 9 features
    private void CalculatePawnStructure(Board board)
    {
        ulong whitePawnBoard = board.GetPieceList(Piece.Pawn, 0).bitboard;
        ulong blackPawnBoard = board.GetPieceList(Piece.Pawn, 1).bitboard;

        int doubledPawnDifference = 0;

        //Doubled/Tripled Pawns
        for (int file = 0; file < 8; file++)
        {
            ulong fileMask = PrecomputedData.fileMasks[file];

            doubledPawnDifference += Math.Max(0, BitBoardHelper.BitCount(whitePawnBoard & fileMask) - 1);
            doubledPawnDifference -= Math.Max(0, BitBoardHelper.BitCount(blackPawnBoard & fileMask) - 1);
        }

        features[774] = doubledPawnDifference;
        //Console.WriteLine("doubledPawnDifference: " + doubledPawnDifference);


        //Isolated Pawns
        int isolatedPawnDifference = 0;

        ulong whitePawns = whitePawnBoard;

        while (whitePawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref whitePawns);

            //TODO: Calculate in PrecomputedData
            ulong isolationMask = 0;
            int file = BoardHelper.IndexToFile(pawnSquare);

            if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
            if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

            if ((whitePawnBoard & isolationMask) == 0) isolatedPawnDifference++;
        }

        ulong blackPawns = blackPawnBoard;

        while (blackPawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref blackPawns);

            //TODO: Calculate in PrecomputedData
            ulong isolationMask = 0;
            int file = BoardHelper.IndexToFile(pawnSquare);

            if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
            if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

            if ((blackPawnBoard & isolationMask) == 0) isolatedPawnDifference--;
        }

        features[775] = isolatedPawnDifference;
        //Console.WriteLine("isolatedPawnDifference: " + isolatedPawnDifference);


        //TODO: Try backward pawns


        //TODO: Combine with isolated pawn check
        //(Connected) Passed Pawns
        int connectedPassedPawnDifference = 0;

        whitePawns = whitePawnBoard;

        while (whitePawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref whitePawns);

            ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[pawnSquare] & blackPawnBoard;

            int opposingPawnCount = BitBoardHelper.BitCount(opposingPawnBoard);

            //Is passed pawn
            if (opposingPawnCount == 0)
            {
                //Console.WriteLine("white has passed pawn");

                int rank = BoardHelper.IndexToRank(pawnSquare);
                features[776 - 1 + rank] = features[776 - 1 + rank] + 1;

                ulong isolationMask = 0;
                int file = BoardHelper.IndexToFile(pawnSquare);

                if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
                if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

                if ((whitePawnBoard & isolationMask) != 0) connectedPassedPawnDifference++;
            }
        }

        blackPawns = blackPawnBoard;

        while (blackPawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref blackPawns);

            ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[pawnSquare + 64] & whitePawnBoard;

            int opposingPawnCount = BitBoardHelper.BitCount(opposingPawnBoard);

            //Is passed pawn
            if (opposingPawnCount == 0)
            {
                //Console.WriteLine("Black has passed pawn");

                int rank = 7 - BoardHelper.IndexToRank(pawnSquare);
                features[776 - 1 + rank] = features[776 - 1 + rank] - 1;

                ulong isolationMask = 0;
                int file = BoardHelper.IndexToFile(pawnSquare);

                if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
                if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

                if ((blackPawnBoard & isolationMask) != 0) connectedPassedPawnDifference--;
            }
        }

        features[782] = connectedPassedPawnDifference;
        //Console.WriteLine("connectedPassedPawnDifference: " + connectedPassedPawnDifference);
    }
    #endregion

    #region King Safety
    //TODO: Add way more features
    //1 missing pawns on top of king difference + 1 hole in kings pawnshield difference + 1 complete open file above king difference + 4 pawn storm rank differences above king = 7 feature
    private void CalculateKingSafety(Board board)
    {
        //TODO: Calculate in PrecomputedData
        int missingPawnDefenseDifference = 0;

        int kingFile = BoardHelper.IndexToFile(board.whiteKingSquare);

        if (kingFile > 0 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpLeft)) missingPawnDefenseDifference++;

        if (kingFile < 7 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpRight)) missingPawnDefenseDifference++;

        if (!BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.Up)) missingPawnDefenseDifference++;



        kingFile = BoardHelper.IndexToFile(board.blackKingSquare);

        if (kingFile > 0 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.DownLeft)) missingPawnDefenseDifference--;

        if (kingFile < 7 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.DownRight)) missingPawnDefenseDifference--;

        if (!BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.Down)) missingPawnDefenseDifference--;

        features[783] = missingPawnDefenseDifference * (1f - phase / 100f); //TODO: Try () around the division






        int missingPawnShieldDifference = 0;

        kingFile = BoardHelper.IndexToFile(board.whiteKingSquare);

        if (kingFile > 0 && !(BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpLeft) || BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpLeft + PrecomputedData.Up))) missingPawnShieldDifference++;

        if (kingFile < 7 && !(BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpRight) || BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpRight + PrecomputedData.Up))) missingPawnShieldDifference++;

        if (!(BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.Up) || BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.Up + PrecomputedData.Up))) missingPawnShieldDifference++;



        kingFile = BoardHelper.IndexToFile(board.blackKingSquare);

        if (kingFile > 0 && !(BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.DownLeft) || BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.DownLeft + PrecomputedData.Down))) missingPawnShieldDifference--;

        if (kingFile < 7 && !(BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.DownRight) || BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.DownRight + PrecomputedData.Down))) missingPawnShieldDifference--;

        if (!(BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.Down) || BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.Down + PrecomputedData.Down))) missingPawnShieldDifference--;

        features[784] = missingPawnShieldDifference * (1f - phase / 100f); //TODO: Try () around the division





        int openFileAboveKingDifference = 0;

        kingFile = BoardHelper.IndexToFile(board.whiteKingSquare);

        if (kingFile > 0 && ((PrecomputedData.fileMasks[kingFile - 1] & board.GetPieceList(Piece.Pawn, 0).bitboard) == 0)) openFileAboveKingDifference++;

        if (kingFile < 7 && ((PrecomputedData.fileMasks[kingFile + 1] & board.GetPieceList(Piece.Pawn, 0).bitboard) == 0)) openFileAboveKingDifference++;

        if ((PrecomputedData.fileMasks[kingFile] & board.GetPieceList(Piece.Pawn, 0).bitboard) == 0) openFileAboveKingDifference++;



        kingFile = BoardHelper.IndexToFile(board.blackKingSquare);

        if (kingFile > 0 && ((PrecomputedData.fileMasks[kingFile - 1] & board.GetPieceList(Piece.Pawn, 1).bitboard) == 0)) openFileAboveKingDifference--;

        if (kingFile < 7 && ((PrecomputedData.fileMasks[kingFile + 1] & board.GetPieceList(Piece.Pawn, 1).bitboard) == 0)) openFileAboveKingDifference--;

        if ((PrecomputedData.fileMasks[kingFile] & board.GetPieceList(Piece.Pawn, 1).bitboard) == 0) openFileAboveKingDifference--;

        features[785] = openFileAboveKingDifference * (1f - phase / 100f); //TODO: Try () around the division





        ulong whitePawns = board.GetPieceList(Piece.Pawn, 0).bitboard;
        ulong blackPawns = board.GetPieceList(Piece.Pawn, 1).bitboard;

        for (int i = 0; i < 4; i++)
        {
            int pawnStormRankDifference = BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare + PrecomputedData.Up * i] & blackPawns) - BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64 + PrecomputedData.Down * i] & whitePawns); ;


            features[786 + i] = pawnStormRankDifference * (1f - phase / 100f); //TODO: Try () around the division
        }
    }
    #endregion



    #region Mobility

    //Mobility for 3(*2) piece types in both game phases = 6 features
    private void CalculateMobility(Board board)
    {
        PieceList whiteBishopList = board.GetPieceList(Piece.Bishop, 0);
        PieceList blackBishopList = board.GetPieceList(Piece.Bishop, 1);
        PieceList whiteRookList = board.GetPieceList(Piece.Rook, 0);
        PieceList blackRookList = board.GetPieceList(Piece.Rook, 1);
        PieceList whiteQueenList = board.GetPieceList(Piece.Queen, 0);
        PieceList blackQueenList = board.GetPieceList(Piece.Queen, 1);


        ulong otherPieces = board.GetPieceList(Piece.Pawn, 0).bitboard | board.GetPieceList(Piece.Knight, 0).bitboard | board.GetPieceList(Piece.Pawn, 1).bitboard | board.GetPieceList(Piece.Knight, 1).bitboard;

        ulong allPiecesNoKings = whiteBishopList.bitboard | blackBishopList.bitboard | whiteRookList.bitboard | blackRookList.bitboard | whiteQueenList.bitboard | blackQueenList.bitboard | otherPieces;

        //We exclude the opponent king bc he can't be on check rays - will be irrelevant when we account for checks in quiescence
        ulong whiteAllPieces = allPiecesNoKings | (1UL << board.whiteKingSquare);
        ulong blackAllPieces = allPiecesNoKings | (1UL << board.blackKingSquare);



        int bishopDifference = 0;

        for (int i = 0; i < whiteBishopList.Count; i++)
        {
            bishopDifference += BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(whiteAllPieces, whiteBishopList[i])) >> 1;
        }

        for (int i = 0; i < blackBishopList.Count; i++)
        {
            bishopDifference -= BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(blackAllPieces, blackBishopList[i])) >> 1;
        }

        features[790] = bishopDifference * (1f - (phase / 100f));
        features[791] = bishopDifference * (phase / 100f);



        int rookDifference = 0;

        for (int i = 0; i < whiteRookList.Count; i++)
        {
            rookDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteRookList[i])) >> 1;
        }

        for (int i = 0; i < blackRookList.Count; i++)
        {
            rookDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackRookList[i])) >> 1;
        }

        features[792] = rookDifference * (1f - (phase / 100f));
        features[793] = rookDifference * (phase / 100f);



        int queenDifference = 0;

        for (int i = 0; i < whiteQueenList.Count; i++)
        {
            queenDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteQueenList[i]) | MagicData.GetBishopMoveBoard(whiteAllPieces, whiteQueenList[i])) >> 1;
        }

        for (int i = 0; i < blackQueenList.Count; i++)
        {
            queenDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackQueenList[i]) | MagicData.GetBishopMoveBoard(blackAllPieces, blackQueenList[i])) >> 1;
        }

        features[794] = queenDifference * (1f - (phase / 100f));
        features[795] = queenDifference * (phase / 100f);
    }

    #endregion

    #endregion
}