
public static class CheckFilter
{
    public static void FilterChecks(List<Position> positions)
    {
        int checksFound = 0;

        Board board = new Board();
        MoveGenerator moveGenerator = new MoveGenerator(board);

        for (int i = 0; i < positions.Count; i++)
        {
            FenUtility.LoadPositionFromFen(board, positions[i].fen);
            _ = moveGenerator.GenerateMovesSlow(); //Really slow and janky way to detect checks but quick to implement

            if (moveGenerator.inCheck)
            {
                checksFound++;
                //Console.WriteLine("Check found in fen: " + positions[i].fen);

                positions.RemoveAt(i);
                i--; //Go back one index bc we just removed an entry in the list
            }
        }

        Console.WriteLine("Removed " + checksFound + " positions with check");
    }
}