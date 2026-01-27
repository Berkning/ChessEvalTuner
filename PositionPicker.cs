

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

        for (int i = 0; i < games.Count; i++)
        {
            string moves = "";


            for (int j = 0; j < games[i].moves.Count; j++)
            {
                moves += games[i].moves[j];

                //We only sample after move 10 to avoid too much opening theory
                //After 10 moves we sample every 6 ply to get a wide variety of different positions
                if (j >= 10 && (j - 10) % 6 == 0)
                {
                    Position position = new Position();
                    position.startFen = games[i].fen;
                    position.moves = moves;

                    positions.Add(position);
                }
            }
        }

        Console.WriteLine("Picked " + positions.Count + " positions");
    }
}

public class Position
{
    public string startFen;
    public string moves;
    public float stockfishEval;
}