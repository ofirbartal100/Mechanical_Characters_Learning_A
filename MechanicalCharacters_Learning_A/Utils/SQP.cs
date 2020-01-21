using CenterSpace.NMath.Core;
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

            var problem = new LinearProgrammingProblem(new DoubleVector(coefficients.ToArray()));
            List<LinearConstraint> constraints = CalculateLinearConstaints(D, Gamma);
            foreach (var c in constraints)
            {
                problem.AddConstraint(c);
            }

            var solver = new PrimalSimplexSolver();
            var solverParams = new PrimalSimplexSolverParams()
            {
                // Do not perform more than 1000 pivots
                MaxPivotCount = 1000,

                // Minimize, rather than maximize, the objective function
                Minimize = true
            };

            solver.Solve(problem, solverParams);

            Console.WriteLine(Enum.GetName(typeof(ConstrainedOptimizer.SolveResult), solver.Result));
            Console.WriteLine("X = " + solver.OptimalX);
            Console.WriteLine("f(x) = " + solver.OptimalObjectiveFunctionValue);
            A = solver.OptimalX.ToList();
            return A;
        }

        private List<LinearConstraint> CalculateLinearConstaints(List<CurvesPair> d, double gamma)
        {
            var constraints = new List<LinearConstraint>();

            //every pair norm is >= _gamma
            foreach (var curvesPair in D)
            {
                Vector<double> f = Vector<double>.Build.DenseOfEnumerable(curvesPair.Features);
                var squares = f.PointwiseMultiply(f);
                var constraint = new LinearConstraint(new DoubleVector(squares.ToArray()), gamma, ConstraintType.GreaterThanOrEqualTo);
                constraints.Add(constraint);
            }

            //// all Ai are >= 0
            for (int i = 0; i < A.Count; i++)
            {
                double[] coef = new double[A.Count];
                for (int j = 0; j < A.Count; j++)
                {
                    if (i == j)
                    {
                        coef[j] = 1;
                    }
                    else
                    {
                        coef[j] = 0;
                    }
                }
                var constraint = new LinearConstraint(new DoubleVector(coef), 0, ConstraintType.GreaterThanOrEqualTo);
                constraints.Add(constraint);
            }

            return constraints;
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