using Newtonsoft.Json;


public static class SaveData
{
    private const string SaveFile = "./Data.save";

    public static void Save(List<Position> positions)
    {
        string data = JsonConvert.SerializeObject(positions, Formatting.Indented);

        File.WriteAllText(SaveFile, data);

        Console.WriteLine("Saved");
    }

    public static List<Position> Load()
    {
        string data = File.ReadAllText(SaveFile);

        List<Position>? positions = JsonConvert.DeserializeObject<List<Position>>(data);

        if (positions == null)
        {
            Console.WriteLine("Couldn't load savefile");
            return new List<Position>();
        }

        Console.WriteLine("Loaded " + positions.Count + " positions");

        return positions;
    }
}