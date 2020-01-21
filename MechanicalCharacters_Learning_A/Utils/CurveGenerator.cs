using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MechanicalCharacters_Learning_A.Utils
{
    public static class CurveGenerator
    {
        private static readonly bool _shownMessageBox = false;
        private static readonly object key = new object();
        private static int count = 0;
        static CurveGenerator()
        {
            lock (key)
            {
                Services.Tracker.Track(Paths);
                if (string.IsNullOrEmpty(Paths.PythonPath))
                {
                    Paths.PythonPath = "No Path Given";
                    Services.Tracker.Persist(Paths);
                }
                if (string.IsNullOrEmpty(Paths.PythonScriptPath))
                {
                    Paths.PythonScriptPath = "No Path Given";
                    Services.Tracker.Persist(Paths);
                }

                if (!File.Exists(Paths.PythonPath))
                {
                    MessageBox.Show("No valid PythonPath found at: \"" + Paths.PythonPath + "\" \nPlease Change In The Configuration File At: " + Environment.CurrentDirectory);
                    Environment.Exit(1);
                }
                else if (!File.Exists(Paths.PythonScriptPath))
                {
                    MessageBox.Show("No valid PythonScriptPath found at: \"" + Paths.PythonScriptPath + "\" \nPlease Change In The Configuration File At: " + Environment.CurrentDirectory);
                    Environment.Exit(1);
                }
            }
        }

        public static Services.PythonPaths Paths { get; set; } = new Services.PythonPaths();
        public static Curve GenerateRandomCurve()
        {
            int localCount = count++;
            //run python and get json
            Debug.Print($"Generating Random Curve #{localCount}");
            string json = RunPythonScript(Paths.PythonScriptPath);
            Debug.Print($"Finished Generating Random Curve #{localCount}");

            //parse json into a Curve
            Dictionary<string, dynamic> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
            Curve curve = new Curve(keyValuePairs);

            return curve;
        }

        private static string RunPythonScript(string cmd)
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo
            {
                FileName = Paths.PythonPath,

                //StackOverflow:
                //When standard output it was being redirected,
                //the event in C# wasn't being raised when a line was written on console because there were no calls to stdout.flush;
                //Putting a stdout.flush() statement after each print statement made the events fire as they should and C# now captures the output as it comes.
                //Or you could just use -u switch.
                Arguments = "-u \"" + cmd + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(StartInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                    return result;
                }
            }
        }

        private static string Serializer(Curve curve1)
        {
            throw new NotImplementedException();
        }
    }
}