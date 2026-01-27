

public static class StockfishInterface
{
    private const int NodeCap = 300000;
    private const int HalfNodeCap = NodeCap / 2;

    private static List<Position> positions;

    public static void EvaluateGames(List<Game> games)
    {
        Console.WriteLine("Evaluating " + games.Count + " games");

        positions = new List<Position>();

        for (int i = 0; i < games.Count; i++)
        {
            ExtractPositions(games[i]);
        }

        Console.WriteLine("All games evaluated");
    }

    private static void StartStockfish()
    {
        Process.Start("notepad", "readme.txt");

        string winpath = Environment.GetEnvironmentVariable("windir");
        string path = System.IO.Path.GetDirectoryName(
                      System.Windows.Forms.Application.ExecutablePath);

        Process.Start(winpath + @"\Microsoft.NET\Framework\v1.0.3705\Installutil.exe",
        path + "\\MyService.exe");

    }

    private static void ExtractPositions(Game game)
    {

    }


    private static float GetEval(string fen)
    {
        return ((float)Random.Shared.Next() / int.MaxValue - 0.5f) * 18f;
    }
}

public class Position
{
    public string fen;
    public float stockfishEval;
}