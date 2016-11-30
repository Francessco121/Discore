using Discore;
using Discore.Http.Net;
using Discore.WebSocket;
using Discore.WebSocket.Net;
using System;
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
            shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;
        }

        private static void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
        {
            DiscordMessage message = e.Message;

            if (message.Author != e.Shard.User && message.Mentions.Contains(e.Shard.User))
            {
                //if (message.Content.Contains("give me some porn"))
                //{
                    //var webHooks = app.HttpApi.Webhooks.GetWebhooks(message.Channel);

                    //string[] porn = new string[] {
                    //    "69jym8z.jpg",
                    //    "c3a7776e573d939077ee5e0946c5e377.jpg",
                    //    "c45357b59ad1130957ad2a6e533b5699.jpg",
                    //    "da9f0203bfdca3a8f464847b9e764a7b.png",
                    //    "e0bd9c45b1b1970c6af11bfde17651f8.jpg"
                    //};

                    //Random r = new Random();
                    //int index = r.Next(0, porn.Length - 1);

                    //FileInfo image = new FileInfo(porn[index]);

                    //app.HttpApi.Webhooks.ExecuteWebhook(
                    //    webHooks[0], 
                    //    File.ReadAllBytes(image.ToString()), image.ToString(), 
                    //    avatar: new Uri("https://i.imgur.com/bgrnW3m.jpg"), 
                    //    username: "Porn Supply");

                //}

                //message.AddReaction("ancap", new Snowflake(250034455006281728));         

                //ITextChannel channel = (ITextChannel)message.Channel;
                //DiscordMessage heckoff = channel.SendMessage($"<@!{message.Author.Id}> kys");
                //heckoff.AddReaction("👌");
                //heckoff.AddReaction("🇩");
                //heckoff.AddReaction("🇮");
                //heckoff.AddReaction("🇪");

                //message.AddReaction("wutface", new Snowflake(245312095791480834));

                //ITextChannel channel = (ITextChannel)message.Channel;

                //channel.SendMessage("heck off");

                //if (message.Channel.ChannelType == DiscordChannelType.DirectMessage)
                //{
                //    DiscordDMChannel dm = (DiscordDMChannel)message.Channel;
                //    dm.SendMessage("heck you");
                //}
                //else if (message.Channel.ChannelType == DiscordChannelType.Guild && message.Mentions.Contains(e.Shard.User))
                //{
                //    DiscordGuildTextChannel channel = (DiscordGuildTextChannel)message.Channel;
                //    channel.SendMessage($"<@!{message.Author.Id}> heck off");
                //}
            }
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
