using Discore;
using System;
using System.IO;
using System.Threading;

namespace DiscoreBotTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!File.Exists("token.txt"))
                throw new FileNotFoundException("Missing token.txt file!", "token.txt");

            string token = File.ReadAllText("token.txt").Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token from token.txt is invalid!");

            DiscoreLogger.OnLog += DiscoreLogger_OnLog;

            DiscordApplication app = new DiscordApplication(token);

            app.ShardManager.CreateShards(1);

            while (app.ShardManager.Shards[0].IsActive)
                Thread.Sleep(1000);

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static void DiscoreLogger_OnLog(object sender, DiscoreLogEventArgs e)
        {
            switch (e.Line.Type)
            {
                case DiscoreLogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case DiscoreLogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case DiscoreLogType.Important:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case DiscoreLogType.Verbose:
                case DiscoreLogType.Heartbeat:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.WriteLine($"[{e.Line.Timestamp}] {e.Line.Message}");
        }
    }
}
