

public static class PositionPicker
{
    public static List<Position> positions = new List<Position>();

    public static void Pick(List<Game> games)
    {

    }
}

public class Position
{
    public string startFen;
    public string moves;
    public float stockfishEval;
}