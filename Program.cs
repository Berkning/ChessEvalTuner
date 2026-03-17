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
                Trainer.trainingData = PositionPicker.positions;
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
                if (args.Length == 1) Trainer.BeginTraining(1);
                else
                {
                    int iterations = int.Parse(args[1]);

                    Trainer.BeginTraining(iterations);
                }
                break;
            case "findK":
                Trainer.FindK(int.Parse(args[1]), float.Parse(args[2]));
                break;
            case "setLR":
                Trainer.initialLearningRate = float.Parse(args[1]);
                Console.WriteLine("Learning Rate is now " + Trainer.initialLearningRate);
                break;
            case "setDecay":
                Trainer.decayRate = float.Parse(args[1]);
                Console.WriteLine("Decay Rate is now " + Trainer.decayRate);
                break;
            case "setLambda":
                Trainer.lambda = float.Parse(args[1]);
                Console.WriteLine("Lambda is now " + Trainer.lambda);
                break;
            case "filterChecks":
                CheckFilter.FilterChecks(Trainer.trainingData);
                break;
        }
    }
}