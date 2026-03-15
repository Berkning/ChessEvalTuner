

public static class Trainer
{
    public static List<Position> trainingData;
    private static float learningRate = 0.01f;
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

        //Console.WriteLine("Random data check: " + trainingData[47].stockfishEval);

        //Console.WriteLine("Translating moves...");
        //StockfishInterface.TranslateMoves(ref trainingData);


        Console.WriteLine("Beginning training loop...");

        float[] gradients = new float[MLEvaluation.weights.Length + 1];
        int biasGradientIndex = MLEvaluation.weights.Length;

        for (int i = 0; i < trainingData.Count; i++)
        {
            float target = trainingData[i].result;

            ModelInterface.board = new Board(); //Just to be safe
            ModelInterface.LoadPosition(("training fen " + trainingData[i].fen).Split(' ')); //Kinda janky

            float rawEval = ModelInterface.Evaluate();
            float ourPrediction = Sigmoid(rawEval);

            //Console.WriteLine($"rawEval {rawEval} target {target} ourEval {ourEval}");

            float diff = ourPrediction - target;

            accumulatedLoss += diff * diff;

            //Accumulate gradients
            for (int w = 0; w < MLEvaluation.weights.Length; w++)
            {
                //gradients[w] += 2f * diff * dTanh * MLEvaluation.features[w]; /// 4f;

                gradients[w] += -(target - ourPrediction) * SigmoidDiff(ourPrediction) * MLEvaluation.features[w];
            }

            gradients[biasGradientIndex] += -(target - ourPrediction) * SigmoidDiff(ourPrediction);

            //gradients[biasGradientIndex] += 2f * diff * dTanh * 1f; //TODO:


            if ((i + 1) % BatchSize == 0) //Because index starts at 0
            {
                //float norm = 0;

                for (int w = 0; w < MLEvaluation.weights.Length; w++)
                {
                    gradients[w] /= BatchSize;
                    //gradients[w] += 2f * lambda * MLEvaluation.weights[w]; //L2 Regularization

                    //gradients[w] = Math.Clamp(gradients[w], -1f, 1f); //Gradient clipping
                }

                gradients[biasGradientIndex] /= BatchSize;


                for (int w = 0; w < MLEvaluation.weights.Length; w++)
                {
                    MLEvaluation.weights[w] -= learningRate * gradients[w];

                    //norm += gradients[w] * gradients[w];
                    gradients[w] = 0;
                }

                MLEvaluation.bias -= learningRate * gradients[biasGradientIndex];
                gradients[biasGradientIndex] = 0;

                //Console.WriteLine("Grad norm: " + Math.Sqrt(norm));
            }


            if (i % 10000 == 0)
            {
                Console.WriteLine("CheckEval: " + rawEval);
                Console.WriteLine(i + "/" + trainingData.Count + " Loss: " + accumulatedLoss / 10000f);
                accumulatedLoss = 0f;
            }
        }

        Console.WriteLine("Training Done.");
    }

    public static float GetAverageEvaluationError() //Used for tuning K for a specific dataset
    {
        if (trainingData == null || trainingData.Count == 0)
        {
            Console.WriteLine("Loading file...");
            trainingData = SaveData.Load();
        }

        float errorSum = 0f;

        for (int i = 0; i < trainingData.Count; i++)
        {
            ModelInterface.board = new Board(); //Just to be safe
            ModelInterface.LoadPosition(("training fen " + trainingData[i].fen).Split(' '));

            float rawEval = ModelInterface.Evaluate();

            float ourPrediction = Sigmoid(rawEval);

            float error = (float)Math.Pow(trainingData[i].result - ourPrediction, 2d);

            errorSum += error;
        }

        return errorSum / trainingData.Count;
    }


    public static float K = 1f;

    private static float Sigmoid(float eval)
    {
        return (float)(1d / (1d + Math.Pow(10d, -K * eval / 4d)));
    }

    private static float SigmoidDiff(float prediction)
    {
        return 2.302585093f * K / 4f * (prediction * (1f - prediction));
    }
}