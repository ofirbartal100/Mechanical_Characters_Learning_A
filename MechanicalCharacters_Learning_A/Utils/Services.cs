using Jot;
using Jot.Storage;
using System;

namespace MechanicalCharacters_Learning_A.Utils
{
    // Expose services as static class to keep the example simple
    public static class Services
    {
        // expose the tracker instance
        public static Tracker Tracker = new Tracker(new JsonFileStore(Environment.CurrentDirectory));

        static Services()
        {
            // tell Jot how to track Window objects
            Tracker.Configure<SQP>()
                .Id(sqp => "SQP")
                .Properties(sqp => new { sqp.A, sqp.S, sqp.D });

            Tracker.Configure<CurvesPair>()
                .Id(cp => "CurvesPair")
                .Properties(cp => new { cp.CurveA, cp.CurveB, cp.Features, cp.Distance, cp.SimilarityType });

            var h = Tracker.Configure<Curve>()
                .Id(c => "Curve")
                .Properties(c => new { c.Points, c.Features })
                .TrackedProperties;

            Tracker.Configure<PythonPaths>()
                .Id(d => "PythonPaths").Properties(d => new { d.PythonPath, d.PythonScriptPath });
        }

        public class PythonPaths
        {
            public PythonPaths()
            {
            }

            public string PythonPath { get; set; }
            public string PythonScriptPath { get; set; }
        }
    }
}