using Discore;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DiscoreBotTest
{
    public class Program
    {
        static DiscordWebSocketApplication app;

        public static void Main(string[] args)
        {
            if (!File.Exists("token.txt"))
                throw new FileNotFoundException("Missing token.txt file!", "token.txt");

            string token = File.ReadAllText("token.txt").Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token from token.txt is invalid!");

            DiscoreLogger.OnLog += DiscoreLogger_OnLog;

            DiscordBotUserToken auth = new DiscordBotUserToken(token);
            app = new DiscordWebSocketApplication(auth);

            Shard shard = app.ShardManager.CreateSingleShard();
            if (shard.Start())
            {
                TestShard(shard);

                while (shard.IsRunning)
                    Thread.Sleep(1000);
            }
            else
                Console.WriteLine("Failed to start shard!");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static void TestShard(Shard shard)
        {
            
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
                case DiscoreLogType.Verbose:
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
