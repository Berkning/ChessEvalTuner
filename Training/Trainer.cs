

public static class Trainer
{
    public static List<Position> trainingData;
    private static float learningRate = 0.00005f;
    private static float lambda = 0.00001f;
    private static float accumulatedLoss = 0f;
    private const int BatchSize = 256;

    public static void BeginTraining()
    {
        if (trainingData == null || trainingData.Count == 0)
        {
            Console.WriteLine("Loading file...");
            trainingData = SaveData.Load();
        }
        else Console.WriteLine("Training data already present, skipping load");

        Console.WriteLine("Shuffling data...");
        trainingData = trainingData.Shuffle().ToList();

        Console.WriteLine("Random data check: " + trainingData[47].stockfishEval);

        Console.WriteLine("Translating moves...");
        StockfishInterface.TranslateMoves(ref trainingData);


        Console.WriteLine("Beginning training loop...");

        float[] gradients = new float[MLEvaluation.weights.Length];

        for (int i = 0; i < trainingData.Count; i++)
        {
            float target = Squash(trainingData[i].stockfishEval);

            ModelInterface.board = new Board(); //Just to be safe
            ModelInterface.RecieveCommand(("training fen " + trainingData[i].startFen + " moves " + trainingData[i].moves).Split(' ')); //Really janky and slow way to do this

            float rawEval = ModelInterface.Evaluate();
            float ourEval = Squash(rawEval);


            float diff = ourEval - target;

            accumulatedLoss += diff * diff;

            float dTanh = 1f - ourEval * ourEval;

            //Accumulate gradients
            for (int w = 0; w < MLEvaluation.weights.Length; w++)
            {
                gradients[w] += 2f * diff * dTanh * (MLEvaluation.features[w] / 6f); /// 4f;
                gradients[w] += 2f * lambda * w; //L2 Regularization
            }


            if (i != 0 && i % BatchSize == 0)
            {
                float norm = 0;


                for (int w = 0; w < MLEvaluation.weights.Length; w++)
                {
                    MLEvaluation.weights[w] -= learningRate * (gradients[w] / BatchSize);

                    norm += gradients[w] * gradients[w];
                    gradients[w] = 0;
                }

                //Console.WriteLine("Grad norm: " + Math.Sqrt(norm));
            }


            if (i % 10000 == 0)
            {
                Console.WriteLine(i + "/" + trainingData.Count + " Loss: " + accumulatedLoss / 10000f);
                accumulatedLoss = 0f;
            }
        }

        Console.WriteLine("Training Done.");
    }

    private static float Squash(float eval)
    {
        return (float)Math.Tanh(eval / 6f);
        return eval;
    }
}