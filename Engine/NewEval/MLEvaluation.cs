using System.Runtime.CompilerServices;
using Newtonsoft.Json;

public class MLEvaluation
{
    //TODO: Mobility? Rooks on open files? Mopup score? 
    public static readonly float[] weights = new float[768 + 6 + 9 + 1]; //TODO: We can easily eliminate bounds checks when using these arrays
    public static float[] features = new float[768 + 6 + 9 + 1]; //TODO: We can easily eliminate bounds checks when using these arrays

    public float GetEval(Board board)
    {
        for (int i = 0; i < 768 + 6 + 9 + 1; i++)
        {
            features[i] = 0;
        }

        CalculatePhase(board);

        CalculateFeatures(board);



        float result = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            result += features[i] * weights[i];
        }

        int perspective = board.colorToMove == Piece.White ? 1 : -1;

        //Console.WriteLine("Phase: " + phase);

        return result;//* perspective;
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
    }

    public static void LogData()
    {
        Console.WriteLine("Weights: " + JsonConvert.SerializeObject(weights));
        Console.WriteLine("Features: " + JsonConvert.SerializeObject(features));
    }

    #region Phase

    private const float KnightPhase = 1f;
    private const float BishopPhase = 1f;
    private const float RookPhase = 2f;
    private const float QueenPhase = 4f;

    private const float MaxPhase = KnightPhase * 4f + BishopPhase * 4f + RookPhase * 4f + QueenPhase * 2f;
    private float phase; //Phase is between 0 (MG) and 100 (EG)

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
        features[board.whiteKingSquare] = 1f - (phase / 100f);
        features[board.whiteKingSquare + 384] = phase / 100f;

        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Pawn, 0), 64 * 1);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Knight, 0), 64 * 2);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Bishop, 0), 64 * 3);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Rook, 0), 64 * 4);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Queen, 0), 64 * 5);


        //SetPSQTFeaturesBlack(board.GetPieceList(Piece.King, 1), 64 * 0);
        features[BoardHelper.FlipIndex(board.blackKingSquare)] = 1f - (phase / 100f);
        features[BoardHelper.FlipIndex(board.blackKingSquare) + 384] = phase / 100f;

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
            features[list[i] + PSQTOffset] = 1f - (phase / 100f); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame

            features[list[i] + PSQTOffset + 384] = phase / 100f; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPSQTFeaturesBlack(PieceList list, int PSQTOffset)
    {
        for (int i = 0; i < list.Count; i++)
        {
            //TODO: Rename "flipindex" to "mirrorindex"
            int mirroredSquare = BoardHelper.FlipIndex(list[i]);

            features[mirroredSquare + PSQTOffset] = 1f - (phase / 100f); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame

            features[mirroredSquare + PSQTOffset + 384] = phase / 100f; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
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

        float whiteBishopPair = board.GetPieceList(Piece.Bishop, 0).Count > 1 ? 1f : 0f; //TODO: Obv don't call this again
        float blackBishopPair = board.GetPieceList(Piece.Bishop, 1).Count > 1 ? 1f : 0f; //TODO: Obv don't call this again

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
    //1 missing pawns on top of king difference = 1 feature
    private void CalculateKingSafety(Board board)
    {
        //TODO: Calculate in PrecomputedData
        int missingPawnDefenseDifference = 0;

        int kingFile = BoardHelper.IndexToFile(board.whiteKingSquare);

        if (kingFile > 0 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpLeft)) missingPawnDefenseDifference++;

        if (kingFile < 7 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpRight)) missingPawnDefenseDifference++;

        if (!BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.Up)) missingPawnDefenseDifference++;



        kingFile = BoardHelper.IndexToFile(board.blackKingSquare);

        if (kingFile > 0 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.UpLeft)) missingPawnDefenseDifference--;

        if (kingFile < 7 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.UpRight)) missingPawnDefenseDifference--;

        if (!BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.Up)) missingPawnDefenseDifference--;

        features[783] = missingPawnDefenseDifference * (1f - phase);
    }
    #endregion

    #endregion
}