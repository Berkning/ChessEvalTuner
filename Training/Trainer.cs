

public static class Trainer //TODO: Q-Search
{
    public static List<Position> trainingData;
    public static double initialLearningRate = 0.01f;
    private static double currentLearningRate = initialLearningRate;
    public static double decayRate = 0.01f; //Learning rate decay rate
    public static double lambda = 0.00001f;
    private static double accumulatedLoss = 0f;
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

            double[] gradients = new double[MLEvaluation.weights.Length + 1];
            int biasGradientIndex = MLEvaluation.weights.Length;

            for (int i = 0; i < trainingData.Count; i++)
            {
                double target = trainingData[i].result;

                ModelInterface.board = new Board(); //Just to be safe
                ModelInterface.LoadPosition(("training fen " + trainingData[i].fen).Split(' ')); //Kinda janky

                double rawEval = ModelInterface.Evaluate();
                double ourPrediction = Sigmoid(rawEval);

                //Console.WriteLine($"rawEval {rawEval} target {target} ourEval {ourEval}");

                double diff = ourPrediction - target;

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
                    //double norm = 0;

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

    public static double GetAverageEvaluationError() //Used for tuning K for a specific dataset
    {
        if (trainingData == null || trainingData.Count == 0)
        {
            Console.WriteLine("Loading file...");
            trainingData = SaveData.Load();
        }

        double errorSum = 0f;

        for (int i = 0; i < trainingData.Count; i++)
        {
            ModelInterface.board = new Board(); //Just to be safe
            ModelInterface.LoadPosition(("training fen " + trainingData[i].fen).Split(' '));

            double rawEval = ModelInterface.Evaluate();

            double ourPrediction = Sigmoid(rawEval);

            double error = Loss(ourPrediction, trainingData[i].result);

            errorSum += error;
        }

        return errorSum / trainingData.Count;
    }

    public static void FindK(int iterations, double range)
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

        double BestAEE = GetAverageEvaluationError();
        double BestK = K;

        double direction = range / 4f;


        for (int i = 0; i < iterations; i++)
        {
            K += direction;
            double ForwardAEE = GetAverageEvaluationError();

            K -= direction * 2f; //Go back twice so we are |direction| away from the starting point
            double BackAEE = GetAverageEvaluationError();

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


    public static double K = 1f;

    private static double Sigmoid(double eval)
    {
        double result = (double)(1d / (1d + Math.Pow(Math.E, -K * eval / 4d * Math.Log(10))));
        if (result > 1f) result = 1f;

        return result;
    }

    private static double Loss(double prediction, double target)
    {
        if (prediction >= 1f && target >= 1f)
        {
            Console.WriteLine("Would have been nan?: " + -(double)(target * Math.Log(prediction) + (1d - target) * Math.Log(1d - prediction)));
            Console.WriteLine("Prediction: " + prediction + " : Target: " + target);
            return 0f;
        }

        if (prediction > 1f || prediction < 0f) Console.WriteLine("ERROR: prediction = " + prediction);
        if (double.IsNaN(-(double)(target * Math.Log(prediction) + (1d - target) * Math.Log(1d - prediction)))) Console.WriteLine("Nan: " + prediction + "   " + target);
        return -(double)(target * Math.Log(prediction) + (1d - target) * Math.Log(1d - prediction));
    }
}