

public static class ModelInterface
{
    public static MLEvaluation evaluation = new MLEvaluation();
    public static Board board = new Board();

    public static void RecieveCommand(string[] args)
    {
        switch (args[1])
        {
            case "log":
                MLEvaluation.LogData();
                break;
            case "randomize":
                for (int i = 0; i < MLEvaluation.weights.Length; i++)
                {
                    MLEvaluation.weights[i] = (Random.Shared.NextSingle() - 0.5f) / 10f;
                }
                break;
            case "eval":
                Console.WriteLine(evaluation.GetEval(board));
                break;
            case "fen":
                LoadPosition(args);
                break;
            case "d":
                Console.WriteLine(FenUtility.GetCurrentFen(board));
                break;
        }
    }

    public static float Evaluate()
    {
        return evaluation.GetEval(board);
    }

    private static void LoadPosition(string[] args)
    {
        int moveStartIndex = -1;
        string fen = "";

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "moves")
            {
                moveStartIndex = i + 1;
                break;
            }

            fen += args[i] + ' ';
        }

        //Console.WriteLine("Loading fen: " + fen);

        FenUtility.LoadPositionFromFen(board, fen);

        if (moveStartIndex != -1)
        {
            for (int i = moveStartIndex; i < args.Length; i++)
            {
                if (args[i].IsWhiteSpace() || args[i] == "") break;

                Console.WriteLine("making move: " + args[i]);
                board.MakeMove(BoardHelper.GetMoveFromUCIName(board, args[i]));
            }
        }
    }
}