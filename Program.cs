using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace pc_keys
{
    class Program
    {
        private static void Main(string[] args)
        {
            //Read in json file
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            {
                using (var sr = new StreamReader(fs))
                {
                    json = sr.ReadToEnd();
                }
            }

            //Convert from json to a BotConfig object
            var config = JsonConvert.DeserializeObject<BotConfig>(json);

            //Run the bot
            var bot = new Bot(config);
            Console.WriteLine("Starting bot.");
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
