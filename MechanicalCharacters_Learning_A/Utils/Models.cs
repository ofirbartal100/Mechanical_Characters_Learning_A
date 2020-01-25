using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MechanicalCharacters_Learning_A.Utils
{
    public enum CurveSimilarity
    {
        Similar,
        ProbablySimilar,
        ProbablyDissimilar,
        Dissimilar
    }

    public class Curve
    {
        public List<double> Features;
        public List<List<double>> Points;
        private readonly List<List<double>> keyValuePairs;
        public Curve()
        {
            Points = new List<List<double>>();
            Features = new List<double>(6);
        }

        public Curve(Dictionary<string, dynamic> keyValuePairs)
        {
            Points = JsonConvert.DeserializeObject<List<List<double>>>(keyValuePairs["points"].ToString());
            Features = JsonConvert.DeserializeObject<List<double>>(keyValuePairs["features"].ToString());
        }

        public Vector<double> GetCenterOfMass()
        {
            Vector<double> Xcom = new DenseVector(Points[0].Count);
            return Points.Aggregate(Xcom, (seed, vector1) => seed += Vector<double>.Build.DenseOfEnumerable(vector1)) / Points.Count;
        }

        public List<Vector<double>> GetCurvatures()
        {
            List<Vector<double>> Edges = new List<Vector<double>>(Points.Count);
            for (int i = 0; i < Points.Count; i++)
            {
                Edges.Add(Vector<double>.Build.DenseOfEnumerable(Points[i]) - Vector<double>.Build.DenseOfEnumerable(Points[(i + 1) % Points.Count]));
            }

            List<Vector<double>> EdgesDirections = Edges.Select(vector => vector / vector.L2Norm()).ToList();

            List<Vector<double>> Curvatures = new List<Vector<double>>(Points.Count);

            for (int i = 0; i < Points.Count; i++)
            {
                var tj_1 = EdgesDirections[(i - 1 + Points.Count) % Points.Count];
                var tj = EdgesDirections[i];

                var cross = new DenseVector(3);
                if (tj_1.Count == 3)
                {
                    cross[0] = tj_1[1] * tj[2] - tj_1[2] * tj[1];
                    cross[1] = tj_1[2] * tj[0] - tj_1[0] * tj[2];
                    cross[2] = tj_1[0] * tj[1] - tj_1[1] * tj[0];
                }

                Curvatures.Add((2 / (1 + (tj * tj_1))) * cross);
            }

            return Curvatures;
        }

        public List<Vector<double>> Project2D()
        {
            
            Vector<double> Xcom = new DenseVector(Points[0].Count);
            Xcom = Points.Aggregate(Xcom, (vector, p) => vector + Vector<double>.Build.DenseOfEnumerable(p)) / Points.Count;

            var centeredData = new double[Points.Count, Points[0].Count];
            for (int i = 0; i < Points.Count; i++)
            {
                for (int j = 0; j < Points[0].Count; j++)
                {
                    centeredData[i,j]= Points[i][j] - Xcom[j];
                }
            }

            double[] s2 = new double[2];
            var v = new double[Points[0].Count, 2];
            alglib.pcatruncatedsubspace(centeredData, Points.Count, Points[0].Count, 2, 0.001, 500, out s2, out v);

            var projectedData = new List<Vector<double>>();
            for (int i = 0; i < Points.Count; i++)
            {
                double[] projection = new double[2]{0,0};
                for (int j = 0; j < Points[0].Count; j++)
                {
                    projection[0] += v[j, 0] * centeredData[i, j];
                    projection[1] += v[j, 1] * centeredData[i, j];
                }
                projectedData.Add(Vector<double>.Build.DenseOfEnumerable(projection));
            }

            var maxValue = Math.Abs(projectedData.Max(vector => vector.AbsoluteMaximum())) / 3;
            return projectedData.Select(vector => vector.Divide(maxValue)).ToList();
        }


    }
    public class CurvesPair
    {
        public List<double> Features;
        public CurvesPair()
        {
            CurveA = new Curve();
            CurveB = new Curve();
            SimilarityType = CurveSimilarity.Dissimilar;
        }

        public CurvesPair(Curve curveA, Curve curveB)
        {
            CurveA = curveA;
            CurveB = curveB;
            CalculateFeatures();
            Vector<double> a = new DenseVector(SQP.Instance.A.ToArray());
            Vector<double> f = new DenseVector(Features.ToArray());

            Distance = a.PointwiseMultiply(f).DotProduct(f);
            SimilarityTypeCalculator(Distance);
        }

        public CurvesPair(bool isSimilar)
        {
            CurveA = new Curve();
            CurveB = new Curve();
            SimilarityType = isSimilar ? CurveSimilarity.Similar : CurveSimilarity.Dissimilar;
        }

        public Curve CurveA { get; set; }
        public Curve CurveB { get; set; }
        public double Distance { get; set; }
        public CurveSimilarity SimilarityType { get; set; }
        public static CurvesPair PairOfRandomCurves()
        {
            var randomCurves = new Curve[2];
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < randomCurves.Length; i++)
            {
                tasks.Add(F(randomCurves, i));
            }
            Task.WaitAll(tasks.ToArray());
            var pair = new CurvesPair(randomCurves[0], randomCurves[1]);
            return pair;
        }

        /// <summary>
        /// calculate features (fi-fj) by substructing, and adding features 7 and 8
        /// </summary>
        public void CalculateFeatures()
        {
            double f7 = Double.MaxValue;
            var ci = CurveA.Points;
            var cj = CurveB.Points;
            var np = ci.Count;
            for (int l = 0; l < np; l++)
            {
                double distance = 0;
                for (int k = 0; k < np; k++)
                {
                    var norm = (Vector<double>.Build.DenseOfEnumerable(ci[k]) - Vector<double>.Build.DenseOfEnumerable(cj[(k + l + np) % np])).L2Norm();
                    distance += norm * norm;
                }

                if (distance < f7)
                {
                    f7 = distance;
                }
            }

            f7 = Math.Sqrt(f7 / np);

            double f8 = Double.MaxValue;
            var ki = CurveA.GetCurvatures();
            var kj = CurveB.GetCurvatures();

            for (int l = 0; l < np; l++)
            {
                double distance = 0;
                for (int k = 0; k < np; k++)
                {
                    var norm = (ki[k] - kj[(k + l + np) % np]).L2Norm();
                    distance += norm * norm;
                }

                if (distance < f8)
                {
                    f8 = distance;
                }
            }
            f8 = Math.Sqrt(f8);

            var diff = Vector<double>.Build.DenseOfEnumerable(CurveA.Features) - Vector<double>.Build.DenseOfEnumerable(CurveB.Features);

            Features = diff.ToArray().Concat(new double[] { f7, f8 }).ToList();
        }

        private static Task F(Curve[] a, int b)
        {
            return Task.Factory.StartNew(() =>
            {
                a[b] = CurveGenerator.GenerateRandomCurve();
            });
        }

        private void SimilarityTypeCalculator(double distance)
        {
            if (distance > SQP.Instance.Gamma)
            {
                SimilarityType = CurveSimilarity.Dissimilar;
            }
            else if (distance > SQP.Instance.Gamma * 0.5)
            {
                SimilarityType = CurveSimilarity.ProbablyDissimilar;
            }
            else if (distance > SQP.Instance.Gamma * 0.25)
            {
                SimilarityType = CurveSimilarity.ProbablySimilar;
            }
            else
            {
                SimilarityType = CurveSimilarity.Similar;
            }
        }
    }

    public class SimilarityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(CurveSimilarity), value?.ToString() ?? "Dissimilar");
        }
    }
}