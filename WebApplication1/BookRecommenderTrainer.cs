using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System.Data;

namespace WebApplication1;

public static class BookRecommenderTrainer
{
    public static void RunExperiments(string csvPath = "book-ratings.csv")
    {
        var ml = new MLContext(seed: 1);
        IDataView data = ml.Data.LoadFromTextFile<BookRating>(
            csvPath, hasHeader: true, separatorChar: ',');
        var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);

        (int iter, int rank)[] configs =
        [
            (5, 4),
            (20, 4),
            (20, 16),
            (50, 16),
            (50, 64)
        ];

        foreach (var (iter, rank) in configs)
        {
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserKey",
                MatrixRowIndexColumnName = "BookKey",
                LabelColumnName = "Label",
                NumberOfIterations = iter,
                ApproximationRank = rank
            };

            var pipeline = ml.Transforms.Conversion.MapValueToKey("UserKey", "UserId")
                .Append(ml.Transforms.Conversion.MapValueToKey("BookKey", "BookId"))
                .Append(ml.Recommendation().Trainers.MatrixFactorization(options));

            ITransformer model = pipeline.Fit(split.TrainSet);
            var m = ml.Regression.Evaluate(model.Transform(split.TestSet));

            Console.WriteLine($"Iter={iter,2}, Rank={rank,2} -> RMSE={m.RootMeanSquaredError:0.00}, R²={m.RSquared:0.00}");
        }
    }

    public static void TrainAndSave(
        string csvPath = "book-ratings.csv",
        string outputZipPath = "BookRecommender.zip",
        int numberOfIterations = 20,
        int approximationRank = 16)
    {
        var ml = new MLContext(seed: 1);
        IDataView data = ml.Data.LoadFromTextFile<BookRating>(
            csvPath, hasHeader: true, separatorChar: ',');
        var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "UserKey",
            MatrixRowIndexColumnName = "BookKey",
            LabelColumnName = "Label",
            NumberOfIterations = numberOfIterations,
            ApproximationRank = approximationRank
        };

        var pipeline = ml.Transforms.Conversion.MapValueToKey("UserKey", "UserId")
            .Append(ml.Transforms.Conversion.MapValueToKey("BookKey", "BookId"))
            .Append(ml.Recommendation().Trainers.MatrixFactorization(options));

        ITransformer model = pipeline.Fit(split.TrainSet);
        var m = ml.Regression.Evaluate(model.Transform(split.TestSet));
        Console.WriteLine($"Saved model -> RMSE={m.RootMeanSquaredError:0.00}, R²={m.RSquared:0.00}");

        ml.Model.Save(model, data.Schema, outputZipPath);
    }
}

public class BookRating
{
    [LoadColumn(0)] public float UserId { get; set; }
    [LoadColumn(1)] public float BookId { get; set; }
    [LoadColumn(2), ColumnName("Label")] public float Rating { get; set; }
}

public class BookRatingPrediction
{
    public float Score { get; set; }
}