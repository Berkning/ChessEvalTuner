using Newtonsoft.Json;

public static class PGNParser
{
    public static List<Position> positions = new List<Position>();

    public static void ParseAll(string folder)
    {
        string[] files = Directory.GetFiles(folder);

        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Contains(".epd"))
            {
                Console.WriteLine("File " + files[i] + " is not a .epd file");
                continue;
            }

            ParseAndAdd(files[i]);
        }

    }

    private static void ParseAndAdd(string epdPath)
    {
        Console.WriteLine("Parsing .epd file at '" + epdPath + '\'');

        if (!File.Exists(epdPath))
        {
            Console.WriteLine("Specified .epd file doesn't exist");
            return;
        }

        using (StreamReader stream = new StreamReader(epdPath))
        {
            while (!stream.EndOfStream)
            {
                string? line = stream.ReadLine();

                if (line == null) continue;

                string[] parts = line.Split(" | ");

                string fen = parts[0];

                float result = -1f;

                if (parts[1].Contains("1-0")) result = 1f;
                else if (parts[1].Contains("1/2-1/2")) result = 0.5f;
                else if (parts[1].Contains("0-1")) result = 0f;

                if (result == -1f)
                {
                    Console.WriteLine("Couldn't parse result string: " + parts[1]);
                    return;
                }

                positions.Add(new Position(fen, result));
            }

            Console.WriteLine("Parsed all positions. " + positions.Count + " positions are now loaded");
        }
    }

    public static void LogPositions()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            //string moveString = "";
            //foreach (string move in positions[i].moves) moveString += move + ' ';

            Console.WriteLine("Position #" + (i + 1) + " Fen: " + positions[i].fen + " Result: " + positions[i].result/*+ "   Moves: " + moveString*/);
        }
    }
}