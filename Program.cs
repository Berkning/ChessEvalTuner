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
            case "GetAEE":
                Console.WriteLine(Trainer.GetAverageEvaluationError());
                break;
            case "SetK":
                Trainer.K = float.Parse(args[1]);
                Console.WriteLine("K is now " + Trainer.K);
                break;
            case "training":
                ModelInterface.RecieveCommand(args);
                break;
            case "begin":
                Trainer.BeginTraining();
                break;
        }
    }
}