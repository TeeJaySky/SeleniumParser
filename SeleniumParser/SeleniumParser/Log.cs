using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class Log
    {
        static string LogFileName = "Log.csv";
        static string ResultsFileName = "Results.csv";
        static string OutputFileDir = @"C:\Users\Trent\Desktop\TEmp\Outspoken Panda\";

        // Incoming queue for messages to put into the log
        static ConcurrentQueue<string> logMessageQueue = new ConcurrentQueue<string>();

        // Incoming queue for bsr rankings to put into the results file
        static ConcurrentQueue<IEnumerable<BsrRank>> bsrRankQueue = new ConcurrentQueue<IEnumerable<BsrRank>>();

        static ManualResetEvent bsrThreadDone = new ManualResetEvent(false);
        static ManualResetEvent logThreadDone = new ManualResetEvent(false);
        static ManualResetEvent loggerDone = new ManualResetEvent(false);

        private static void LogThread()
        {
            bool logThreadFinished = false;
            while(!logThreadFinished)
            {
                // While there are things in the queue
                while(!logMessageQueue.IsEmpty)
                {
                    // Try to get one from the queue
                    string logMessage;
                    if (logMessageQueue.TryDequeue(out logMessage))
                    {
                        // If we got it from the queue, log it!
                        LogMessage(logMessage);
                    }
                }

                logThreadFinished = logThreadDone.WaitOne(1000);
            }
        }

        private static void BsrThread()
        {
            bool bsrThreadFinished = false;
            while (!bsrThreadFinished)
            {
                // While there are things in the queue
                while(!bsrRankQueue.IsEmpty)
                {
                    // Try to get one from the queue
                    IEnumerable<BsrRank> bsrRankings;
                    if(bsrRankQueue.TryDequeue(out bsrRankings))
                    {
                        // Log the rankings
                        LogBsrRankings(bsrRankings);
                    }
                }

                bsrThreadFinished = bsrThreadDone.WaitOne(1000);
            }
        }

        public static void ThreadMain()
        {
            var logThread = new Thread(LogThread);
            logThread.Start();

            var bsrThread = new Thread(BsrThread);
            bsrThread.Start();

            // Wait for the termination message
            loggerDone.WaitOne();

            // Signal the other threads to clean up but its probably too late at this point
            logThreadDone.Set();
            bsrThreadDone.Set();
        }

        public static void ShutDown()
        {
            loggerDone.Set();
        }

        /// <summary>
        /// Create csv string from inputs
        /// </summary>
        /// <param name="inputs"></param>
        public static string ToCsv(params string[] inputs)
        {
            return string.Join(", ", inputs);
        }

        /// <summary>
        /// Make a log message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        private static string MakeLogMessage(string message, string logLevel)
        {
            string logMessage = ToCsv(Thread.CurrentThread.ManagedThreadId.ToString(), DateTime.Now.ToString(), logLevel, message);
            return logMessage;
        }

        /// <summary>
        /// Queue a log message to be written to a file at information level
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            logMessageQueue.Enqueue(MakeLogMessage(message, "Info"));
        }

        /// <summary>
        /// Queue a log messsage to be written to a file at error level
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            logMessageQueue.Enqueue(MakeLogMessage(message, "Error"));
        }

        /// <summary>
        /// Write message at level to specified file and console
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        private static void LogMessage(string message)
        {
            // Write to console and file
            Console.WriteLine(message);
            WriteToLogFile(message);
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
                    var columnHeaders = ToCsv("ThreadID", "DateStamp", "LogLevel", "Message");
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
            bsrRankQueue.Enqueue(rankings);
        }

        private static void LogBsrRankings(IEnumerable<BsrRank> rankings)
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

                foreach (var ranking in rankings)
                {
                    writer.WriteLine(ranking.ToString());
                }
            }
        }
    }
}
