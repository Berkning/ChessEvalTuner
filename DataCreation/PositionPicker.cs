

public static class PositionPicker
{
    public static List<Position> positions = new List<Position>();

    public static void Pick(List<Game> games)
    {
        if (positions.Count != 0)
        {
            Console.WriteLine("Position list is not empty");
            return;
        }

        Dictionary<string, bool> duplicateCheck = new Dictionary<string, bool>();
        int duplicates = 0;

        for (int i = 0; i < games.Count; i++)
        {
            string moves = "";

            int spacing = 6;//Random.Shared.Next(3, 8);

            for (int j = 0; j < games[i].moves.Count; j++)
            {
                moves += games[i].moves[j] + " ";

                //We only sample after move 10 to avoid too much opening theory
                //After 10 moves we sample every 6 ply to get a wide variety of different positions

                if (j >= 10 && (j - 10) % spacing == 0)
                {
                    Position position = new Position(games[i].fen, moves, -6969);

                    // if (positions.Contains(position))
                    // {
                    //     Console.WriteLine("Duplicate");
                    // }
                    // else positions.Add(position);

                    if (duplicateCheck.ContainsKey(position.moves))
                    //if (positions.Contains(position))
                    {
                        duplicates++;
                    }
                    else
                    {
                        positions.Add(position);
                        duplicateCheck.Add(position.moves, true);
                    }

                }
            }
        }

        Console.WriteLine("Got " + duplicates + " duplicates");
        Console.WriteLine("Picked " + positions.Count + " positions");
    }
}

public class Position
{
    public string startFen;
    public string moves;
    public float stockfishEval;

    public Position(string _fen, string _moves, float _eval)
    {
        startFen = _fen;
        moves = _moves;
        stockfishEval = _eval;
    }
}