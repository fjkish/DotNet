// *************************
// Solution: ArcGisDotNetSync2
// Project: ArcGisDotNetSync2
// File: Logger.cs
// Year: 2014
// Vendor: ESRI
// *************************

using System;
using System.IO;

namespace WpfApplication1
{
    public static class Logger
    {
        public static void Report(IProgress<string> prog, string replicaStarted)
        {
            prog.Report(replicaStarted);
            Logger.WriteToFile(replicaStarted);
        }
        public static void Report(string replicaStarted, IProgress<string> prog)
        {
            prog.Report(replicaStarted);
            Logger.WriteToFile(replicaStarted);
        }

        private static void WriteToFile(string output)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("./Sync.txt", FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }

            Console.SetOut(writer);
            output = DateTime.Now + ": " + output;
            Console.WriteLine(output);
            Console.SetOut(oldOut);

            writer.Close();
            ostrm.Close();
        }
    }
}