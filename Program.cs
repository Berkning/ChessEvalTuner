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
                PGNParser.LogGames();
                break;
            case "parse":
                PGNParser.ParseAll(args[1]);
                break;
            case "save":
                SaveData.Save(PositionPicker.positions);
                break;
            case "load":
                PositionPicker.positions = SaveData.Load();
                break;
            case "pickpositions":
                //PositionPicker.Pick(PGNParser.games);
                break;
            case "evaluate":
                StockfishInterface.EvaluateGames(PositionPicker.positions);
                break;
        }
    }
}