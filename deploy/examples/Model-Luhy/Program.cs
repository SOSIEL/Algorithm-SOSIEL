using System;
using System.Diagnostics;
using System.IO;
using ModelLuhy.Configuration;

namespace ModelLuhy
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Console.WriteLine("Reading configuration");

            string algorithmConfigurationFileName = "configuration.json";
            string algorithmConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), algorithmConfigurationFileName);

            if (File.Exists(algorithmConfigurationFilePath) == false)
            {
                throw new FileNotFoundException($"{algorithmConfigurationFileName} not found at {Directory.GetCurrentDirectory()}");
            }

            string outputDirectory = @"Output\";

            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, true);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            ConfigurationModel configuration = ConfigurationParser.ParseConfiguration(algorithmConfigurationFilePath);
            AlgorithmModel model = new AlgorithmModel() { OutputFolder = outputDirectory };

            Algorithm algorithm = new Algorithm(configuration);

            Console.WriteLine($"{algorithm.Name} algorithm is running....");
            algorithm.Run(model);
            Console.WriteLine("Algorithm has completed");

            WaitKeyPress();

            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = true,
                    FileName = Path.Combine(Directory.GetCurrentDirectory(), outputDirectory)
                }
            };
            process.Start();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception;

            if (e.ExceptionObject is AggregateException)
            {
                exception = (e.ExceptionObject as AggregateException).InnerException;
            }
            else
            {
                exception = e.ExceptionObject as Exception;
            }


            Console.WriteLine($"ERROR! {exception.Message}");

            WaitKeyPress();

            Environment.Exit(1);
        }

        private static void WaitKeyPress()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
