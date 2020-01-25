using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechanicalCharacters_Learning_A.Utils
{
    public class SQP
    {
        public readonly double Gamma = 1;
        public List<double> A = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<CurvesPair> D = new List<CurvesPair>();
        public List<CurvesPair> S = new List<CurvesPair>();
        private static readonly object padlock = new object();
        private static SQP instance = null;
        private readonly double _kreg = 0.5;

        private SQP()
        {
        }

        public static SQP Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SQP();
                    }
                    return instance;
                }
            }
        }

        public CurvesPair FindPairWithinDistanceBounds(Curve randomCurve, double min, double max)
        {
            var result = S
                .Concat(D)
                .SelectMany(pair => new[] { pair.CurveA, pair.CurveB })
                .OrderBy(curve => Guid.NewGuid())
                .FirstOrDefault(curve =>
                {
                    var d = new CurvesPair(curve, randomCurve).Distance;
                    return (d >= min) && (d <= max);
                });
            if (result is null)
            {
                result = S
                    .Concat(D)
                    .SelectMany(pair => new[] { pair.CurveA, pair.CurveB })
                    .OrderBy(curve => Guid.NewGuid())
                    .First();
            }
            return new CurvesPair(randomCurve, result);
        }

        public List<double> Optimize(List<CurvesPair> simillarCurvesList, List<CurvesPair> dissimillarCurvesList)
        {
            S.AddRange(simillarCurvesList);
            D.AddRange(dissimillarCurvesList);

            //Solve for A given S and D

            //calculate coefficients Sum((Fi-Fj)^2) + _kreg for each n in len(A)
            Vector<double> coefficients = CalculateMinimizationCoeffitients(S, _kreg);

            (double[] x, alglib.minlpreport rep) = AlglibMinimization(coefficients);

            A = x.ToList();
            return A;
        }

        private (double[],alglib.minlpreport) AlglibMinimization(Vector<double> coefficients)
        {
            //
            // This example demonstrates how to minimize
            //
            //     F(x0,x1) = -0.1*x0 - x1
            //
            // subject to box constraints
            //
            //     -1 <= x0,x1 <= +1 
            //
            // and general linear constraints
            //
            //     x0 - x1 >= -1
            //     x0 + x1 <=  1
            //
            // We use dual simplex solver provided by ALGLIB for this task. Box
            // constraints are specified by means of constraint vectors bndl and
            // bndu (we have bndl<=x<=bndu). General linear constraints are
            // specified as AL<=A*x<=AU, with AL/AU being 2x1 vectors and A being
            // 2x2 matrix.
            //
            // NOTE: some/all components of AL/AU can be +-INF, same applies to
            //       bndl/bndu. You can also have AL[I]=AU[i] (as well as
            //       BndL[i]=BndU[i]).
            //

            
            alglib.minlpstate state;
            alglib.minlpreport rep;

            alglib.minlpcreate(8, out state);

            //
            // Set cost vector, box constraints, general linear constraints.
            //
            // Box constraints can be set in one call to minlpsetbc() or minlpsetbcall()
            // (latter sets same constraints for all variables and accepts two scalars
            // instead of two vectors).
            //
            // General linear constraints can be specified in several ways:
            // * minlpsetlc2dense() - accepts dense 2D array as input; sometimes this
            //   approach is more convenient, although less memory-efficient.
            // * minlpsetlc2() - accepts sparse matrix as input
            // * minlpaddlc2dense() - appends one row to the current set of constraints;
            //   row being appended is specified as dense vector
            // * minlpaddlc2() - appends one row to the current set of constraints;
            //   row being appended is specified as sparse set of elements
            // Independently from specific function being used, LP solver uses sparse
            // storage format for internal representation of constraints.
            //
            alglib.minlpsetcost(state, coefficients.AsArray());

            double[] bndl = new double[8];
            double[] bndu = new double[8];
            for (int iii = 0; iii < 8; iii++)
            {
                bndl[iii] = -System.Double.PositiveInfinity;
                bndu[iii] = System.Double.PositiveInfinity;
            }
            alglib.minlpsetbc(state, bndl, bndu);


            double[,] a;
            double[] al;
            double[] au;

            CalculateLinearConstaints(D, Gamma, out a, out al, out au);
            alglib.minlpsetlc2dense(state, a, al, au, D.Count + 8);

            //
            // Set scale of the parameters.
            //
            // It is strongly recommended that you set scale of your variables.
            // Knowing their scales is essential for evaluation of stopping criteria
            // and for preconditioning of the algorithm steps.
            // You can find more information on scaling at http://www.alglib.net/optimization/scaling.php
            //
            double[] s = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
            alglib.minlpsetscale(state, s);

            // Solve
            double[] x;
            alglib.minlpoptimize(state);
            alglib.minlpresults(state, out x, out rep);
            return (x,rep);
        }

        private void CalculateLinearConstaints(List<CurvesPair> d, double gamma,out double[,] a, out double[] al , out double[] au)
        {
            a = new double[d.Count + 8, 8];
            al = new double[d.Count + 8];
            au = new double[d.Count + 8];
            int counter = 0;
            //every pair norm is >= _gamma
            foreach (var curvesPair in D)
            {
                Vector<double> f = Vector<double>.Build.DenseOfEnumerable(curvesPair.Features);
                var squares = f.PointwiseMultiply(f);
                for (int ii = 0; ii < 8; ii++)
                {
                    a[counter, ii] = squares[ii];
                }

                al[counter] = gamma*gamma;
                au[counter] = System.Double.PositiveInfinity;
                counter++;
            }

            //// all Ai are >= 0
            for (int i = 0; i < A.Count; i++)
            {
                for (int j = 0; j < A.Count; j++)
                {
                    if (i == j)
                    {
                        a[counter, j] = 1;
                    }
                    else
                    {
                        a[counter, j] = 0;
                    }
                }

                al[counter] = 0;
                au[counter] = System.Double.PositiveInfinity;
                counter++;
            }
        }

        private Vector<double> CalculateMinimizationCoeffitients(List<CurvesPair> s, double kreg)
        {
            Vector<double> coefficients = new DenseVector(8);
            foreach (var curvesPair in s)
            {
                Vector<double> f = Vector<double>.Build.DenseOfEnumerable(curvesPair.Features);
                var coefs = f.PointwiseMultiply(f);
                coefficients += coefs;
            }

            //normalize so the regularization will be relevant?
            coefficients /= S.Count;

            coefficients += _kreg;

            return coefficients;
        }
    }
}