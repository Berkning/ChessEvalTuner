public static class PGNParser
{
    public static List<Game> games = new List<Game>();

    public static void ParseAll(string folder)
    {
        string[] files = Directory.GetFiles(folder);

        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Contains(".pgn"))
            {
                Console.WriteLine("File " + files[i] + " is not a .pgn file");
                continue;
            }

            ParseAndAdd(files[i]);
        }

    }

    private static void ParseAndAdd(string pgnPath)
    {
        Console.WriteLine("Parsing .pgn file at '" + pgnPath + '\'');

        if (!File.Exists(pgnPath))
        {
            Console.WriteLine("Specified .pgn file doesn't exist");
            return;
        }

        using (StreamReader stream = new StreamReader(pgnPath))
        {
            Game currentGame = new Game();
            int linesSinceFen = 246642;

            while (!stream.EndOfStream)
            {
                string? line = stream.ReadLine();
                linesSinceFen++;

                if (line == null) continue;


                if (line.Contains("FEN"))
                {
                    string fen = line.Substring(6, line.Length - 8);

                    currentGame.fen = fen;
                    linesSinceFen = 0;
                }
                else if (linesSinceFen == 4)
                {
                    string[] moves = line.Split(' ');

                    currentGame.moves = new List<string>();

                    for (int i = 0; i < moves.Length - 1; i += 2)
                    {
                        currentGame.moves.Add(moves[i]);
                    }

                    games.Add(currentGame);
                }
            }

            Console.WriteLine("Parsed all games. " + games.Count + " games are now loaded");
        }
    }

    public static void LogGames()
    {
        for (int i = 0; i < games.Count; i++)
        {
            string moveString = "";
            foreach (string move in games[i].moves) moveString += move + ' ';

            Console.WriteLine("Game #" + (i + 1) + " Fen: " + games[i].fen + "   Moves: " + moveString);
        }
    }
}

public struct Game
{
    public string fen; //Starting position for the game
    public List<string> moves; //Moves played in the game
    public float stockfishEval;
}