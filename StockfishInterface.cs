

using System.Collections.Concurrent;
using System.Diagnostics;

public static class StockfishInterface
{
    private const int NodeCap = 2000000;
    private const int HalfNodeCap = NodeCap / 2;

    private static StockfishThread[] stockfishInstances;
    private static Thread[] threads;
    private static ConcurrentQueue<Position> positionQueue = new ConcurrentQueue<Position>();
    private static ConcurrentBag<Position> evaluatedPositions = new ConcurrentBag<Position>();

    static StockfishInterface()
    {
        stockfishInstances = new StockfishThread[24];
        threads = new Thread[24];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(stockfishInstances[i].Run);
            threads[i].Start();
        }
    }

    public static void TestStockfish()
    {
        for (int i = 0; i < 64; i++)
        {
            Position position = new Position();
            position.startFen = "bench";
            position.stockfishEval = i;

            positionQueue.Enqueue(position);
        }
    }

    public static void EvaluateAll()
    {
        SaveData.Save(PositionPicker.positions);

        Console.WriteLine("Adding positions to evaluation queue...");

        for (int i = 0; i < PositionPicker.positions.Count; i++)
        {
            positionQueue.Enqueue(PositionPicker.positions[i]);
        }

        Console.WriteLine("Queue filled");

        int totalPositionCount = PositionPicker.positions.Count;
        int positionsEvaled = 0;
        Stopwatch stopwatch = new Stopwatch();

        PositionPicker.positions.Clear();

        Console.WriteLine("Waiting for evals to return...");

        stopwatch.Start();

        while (true)
        {
            if (stopwatch.ElapsedMilliseconds % 5000 == 0)
            {
                Console.WriteLine(positionsEvaled + '/' + totalPositionCount + " positions evaled - Time: " + stopwatch.ElapsedMilliseconds / 1000 + 's');
                SaveData.Save(PositionPicker.positions);
            }

            if (evaluatedPositions.TryTake(out Position position))
            {
                PositionPicker.positions.Add(position);
                positionsEvaled++;

                if (positionsEvaled == totalPositionCount) break;
            }
        }

        Console.WriteLine(positionsEvaled + '/' + totalPositionCount + " positions evaled - Time: " + stopwatch.ElapsedMilliseconds / 1000 + 's');

        Console.WriteLine("All evals done");
        SaveData.Save(PositionPicker.positions);
    }


    public struct StockfishThread
    {
        private Process stockfish;

        public void Run()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"./stockfish",
                Arguments = "",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            stockfish = Process.Start(startInfo);

            while (true)
            {
                if (positionQueue.TryDequeue(out Position position))
                {
                    if (position.startFen == "bench")
                    {
                        Console.WriteLine(position.stockfishEval);
                        Bench();
                    }
                    else
                    {
                        position.stockfishEval = Eval(position);
                    }
                }
            }
        }

        private void Bench()
        {
            stockfish.StandardInput.WriteLine("go nodes " + NodeCap);

            string o = "";

            while (!o.Contains("bestmove"))
            {
                o = stockfish.StandardOutput.ReadLine();
                //Console.WriteLine('#' + o);
            }

            Console.WriteLine("bench completed");
        }

        private float Eval(Position position)
        {
            stockfish.StandardInput.WriteLine("position fen " + position.startFen + " moves " + position.moves);
            stockfish.StandardInput.WriteLine("isready");

            if (stockfish.StandardOutput.ReadLine() != "readyok")
            {
                Console.WriteLine("did not recieve readyok response");
                return -10000f;
            }

            stockfish.StandardInput.WriteLine("go nodes " + NodeCap);

            string o = "";
            float eval = -6969f;

            while (!o.Contains("bestmove"))
            {
                if (o.Contains("mate"))
                {
                    eval = float.MaxValue;
                    stockfish.StandardInput.WriteLine("stop"); //If stockfish sees a forced mate we can exit early and not waste more time on evaluating
                }
                else
                {
                    eval = ParseEval(o);
                }

                o = stockfish.StandardOutput.ReadLine();
            }

            return eval;
        }

        private float ParseEval(string log)
        {
            int cpIndex = log.IndexOf("cp");
            int nodeIndex = log.IndexOf("nodes");

            if (cpIndex == -1 && nodeIndex == -1) return -67.67f; //Just a normal log prob
            if (cpIndex == -1) return float.MaxValue; //Position is mate

            return float.Parse(log.Substring(cpIndex + 3, nodeIndex - 2));
        }

        // public float GetEval(string _fen, string _moves)
        // {
        //     fen = _fen;
        //     moves = _moves;

        //     Thread thread = new Thread(RunStockfish)
        //     }

        // private float RunStockfish()
        // {

        // }
    }
}