

public static class Trainer //TODO: Q-Search
{
    public static List<Position> trainingData;
    public static float initialLearningRate = 0.01f;
    private static float currentLearningRate = initialLearningRate;
    public static float decayRate = 0.01f; //Learning rate decay rate
    public static float lambda = 0.00001f;
    private static float accumulatedLoss = 0f;
    private const int BatchSize = 1024;

    public static void BeginTraining(int epochs)
    {
        if (trainingData == null || trainingData.Count == 0)
        {
            Console.WriteLine("Loading file...");
            trainingData = SaveData.Load();
        }
        else Console.WriteLine("Training data already present, skipping load");

        for (int e = 0; e < epochs; e++)
        {
            currentLearningRate = initialLearningRate / (1f + decayRate * e);
            Console.WriteLine("Learning rate decayed to " + currentLearningRate);

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

                accumulatedLoss += Loss(ourPrediction, target);

                //Accumulate gradients
                for (int w = 0; w < MLEvaluation.weights.Length; w++)
                {
                    //gradients[w] += 2f * diff * dTanh * MLEvaluation.features[w]; /// 4f;

                    //gradients[w] += -(target - ourPrediction) * SigmoidDiff(ourPrediction) * MLEvaluation.features[w];
                    gradients[w] += K * (ourPrediction - target) * MLEvaluation.features[w];
                }

                gradients[biasGradientIndex] += K * (ourPrediction - target);


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
                        MLEvaluation.weights[w] -= currentLearningRate * gradients[w];

                        //norm += gradients[w] * gradients[w];
                        gradients[w] = 0;
                    }

                    MLEvaluation.bias -= currentLearningRate * gradients[biasGradientIndex];
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

            Console.WriteLine("Epoch #" + e + " finished");
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

            float error = Loss(ourPrediction, trainingData[i].result);

            errorSum += error;
        }

        return errorSum / trainingData.Count;
    }

    public static void FindK(int iterations, float range)
    {
        if (trainingData == null || trainingData.Count == 0)
        {
            Console.WriteLine("Loading file...");
            trainingData = SaveData.Load();
        }

        Console.WriteLine("Filtering for checks across " + trainingData.Count + " positions...");
        CheckFilter.FilterChecks(trainingData);
        Console.WriteLine("Filtering Done. " + trainingData.Count + " positions remaining");

        K = range / 2f;

        float BestAEE = GetAverageEvaluationError();
        float BestK = K;

        float direction = range / 4f;


        for (int i = 0; i < iterations; i++)
        {
            K += direction;
            float ForwardAEE = GetAverageEvaluationError();

            K -= direction * 2f; //Go back twice so we are |direction| away from the starting point
            float BackAEE = GetAverageEvaluationError();

            if (BackAEE > BestAEE && ForwardAEE > BestAEE)
            {
                //K is set to what it was before, since that guess is still our best
                K = BestK;
            }
            else if (BackAEE <= ForwardAEE) //If stepping backward gave better results
            {
                //Do nothing to K bc it is already at the best spot
                BestK = K;
                BestAEE = BackAEE;
            }
            else //If stepping forward gave better results
            {
                K += direction * 2f; //We stepped back previously, so we step two forward now
                BestAEE = ForwardAEE;
                BestK = K;
            }

            Console.WriteLine("K is " + BestK);
            direction /= 1.9f;
        }

        Console.WriteLine("K is now: " + K);
    }


    public static float K = 1f;

    private static float Sigmoid(float eval)
    {
        float result = (float)(1d / (1d + Math.Pow(Math.E, -K * eval / 4d * Math.Log(10))));
        if (result > 1f) result = 1f;

        return result;
    }

    private static float Loss(float prediction, float target)
    {
        if (prediction >= 1f && target >= 1f)
        {
            Console.WriteLine("Would have been nan?: " + -(float)(target * Math.Log(prediction) + (1d - target) * Math.Log(1d - prediction)));
            return 0f;
        }

        if (prediction > 1f || prediction < 0f) Console.WriteLine("ERROR: prediction = " + prediction);
        if (float.IsNaN(-(float)(target * Math.Log(prediction) + (1d - target) * Math.Log(1d - prediction)))) Console.WriteLine("Nan: " + prediction + "   " + target);
        return -(float)(target * Math.Log(prediction) + (1d - target) * Math.Log(1d - prediction));
    }
}