public static class Program
{
    public static void Main(string[] args)
    {
        string? message = string.Empty;

        while (message != "quit")
        {
            message = Console.ReadLine();

            if (message == null) continue;

            RecieveCommand(message);
        }
    }

    private static void RecieveCommand(string command)
    {
        string[] args = command.Split(' ');

        switch (args[0])
        {
            case "logall":
                PGNParser.LogPositions();
                break;
            case "parse":
                PGNParser.ParseAll(args[1]);
                break;
            case "save":
                SaveData.Save(PGNParser.positions);
                break;
            case "load":
                PositionPicker.positions = SaveData.Load();
                break;
            case "pickpositions":
                //PositionPicker.Pick(PGNParser.games);
                break;
            //TODO: Add option after pickpositions to pick out some quiet positions from the current ones to train on.
            case "testfish":
                //StockfishInterface.TestStockfish();
                break;
            case "evaluatepositions":
                //StockfishInterface.EvaluateAll();
                break;
            case "getAEE":
                Console.WriteLine(Trainer.GetAverageEvaluationError());
                break;
            case "setK":
                Trainer.K = float.Parse(args[1]);
                Console.WriteLine("K is now " + Trainer.K);
                break;
            case "training":
                ModelInterface.RecieveCommand(args);
                break;
            case "begin":
                if (args.Length == 1) Trainer.BeginTraining();
                else
                {
                    int iterations = int.Parse(args[1]);

                    for (int i = 0; i < iterations; i++)
                    {
                        Trainer.BeginTraining();
                        Console.WriteLine("Epoch #" + (i + 1) + " Done");
                    }
                }
                break;
            case "findK":
                Trainer.FindK(int.Parse(args[1]), float.Parse(args[2]));
                break;
            case "setLR":
                Trainer.learningRate = float.Parse(args[1]);
                Console.WriteLine("Learning Rate is now " + Trainer.learningRate);
                break;
            case "setLambda":
                Trainer.lambda = float.Parse(args[1]);
                Console.WriteLine("Lambda is now " + Trainer.lambda);
                break;
        }
    }
}