using System;
using System.IO;
using System.Collections.Generic;

namespace SeleniumParser
{
    public class Log
    {
        static string LogFileName = "Log.csv";
        static string ResultsFileName = "Results.csv";
        static string OutputFileDir = @"C:\Users\Trent\Desktop\TEmp\Outspoken Panda\";

        /// <summary>
        /// Create csv string from inputs
        /// </summary>
        /// <param name="inputs"></param>
        public static string ToCsv(params string[] inputs)
        {
            return string.Join(", ", inputs);
        }

        /// <summary>
        /// Write specified number of blank lines onto console
        /// </summary>
        /// <param name="number"></param>
        public static void BlankLines(int number = 1)
        {
            for (int i = 0; i < number; i++)
            {
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Log message to console and log file at information level
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            LogMessage(message, "Info");
        }

        /// <summary>
        /// Log message to console and log file at error level
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            LogMessage(message, "Error");
        }

        /// <summary>
        /// Write message at level to specified file and console
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        private static void LogMessage(string message, string logLevel)
        {
            // Create the log file message as csv with date stamp
            string logMessage = ToCsv(DateTime.Now.ToString(), logLevel, message);

            // Write to console and file
            Console.WriteLine(logMessage);
            WriteToLogFile(logMessage);
        }

        /// <summary>
        /// Writes the specified message to to file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        private static void WriteToLogFile(string message)
        {
            var outputFileName = OutputFileDir + LogFileName;
            
            var fileExists = File.Exists(outputFileName);

            using (var writer = File.AppendText(outputFileName))
            {
                if (!fileExists)
                {
                    // Create the log file with column headers if it has not been created before
                    var columnHeaders = ToCsv("DateStamp", "LogLevel", "Message");
                    writer.WriteLine(columnHeaders);
                }

                writer.WriteLine(message);
            }
        }

        /// <summary>
        /// Write the contents of the SuccessDescriptor to the console and file
        /// Output may contain more than one product category to url mapping
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="rankings"></param>
        public static void BsrRankings(IEnumerable<BsrRank> rankings)
        {
            var outputFileName = OutputFileDir + ResultsFileName;
            
            var fileExists = File.Exists(outputFileName);

            using (var writer = File.AppendText(outputFileName))
            {
                // If the file does not exists, write the column headings
                if (!fileExists)
                {
                    writer.WriteLine("Search Term, Title, Category, BSR, URL");
                }

                foreach(var ranking in rankings)
                {
                    writer.WriteLine(ranking.ToString());
                }
            }
        }
    }
}
