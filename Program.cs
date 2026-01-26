public static class Program
{
    public static void Main(string[] args)
    {
        string message = string.Empty;

        while (message != "quit")
        {
            message = Console.ReadLine();
            Console.WriteLine("poo");
        }
    }
}