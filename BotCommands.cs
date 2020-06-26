using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace pc_keys
{
    /// <summary>
    /// Basic commands that don't have to do with parsing. Rules, ping, etc.
    /// </summary>
    [Description("Basic bot commands.")]
    public class BasicCommands
    {
        [Command("addkey"), Description("Add a key to a user, updating role as necessary")]
        public async Task AddKeys(CommandContext ctx, [Description("User to add a key to")] DiscordMember member)
        {
            string json;
            using var fs = File.Open(Bot.Config.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using var sr = new StreamReader(fs);
            json = sr.ReadToEnd() ?? "";
            sr.Close();

            var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            if (!dict.ContainsKey(member.Username))
                dict.Add(member.Username, 0);

            var value = dict[member.Username] + 1;
            if (Bot.Config.Roles.ContainsKey(value))
                await member.GrantRoleAsync(ctx.Guild.GetRole(Bot.Config.Roles[value]));

            dict[member.Username] = value;
            string toSave = JsonConvert.SerializeObject(dict);
            File.WriteAllText(Bot.Config.FileName, toSave);

            string keys = dict[member.Username] == 1 ? "key!" : "keys!";
            await ctx.RespondAsync($"{member.Username} now has {value} {keys}");
        }

        [Command("getkeys"), Description("Gets the number of keys a user has")]
        public async Task GetKeys(CommandContext ctx, [Description("User to get keys from")] DiscordMember member)
        {
            string json;
            using var fs = File.OpenRead(Bot.Config.FileName);
            using var sr = new StreamReader(fs);
            json = sr.ReadToEnd();
            
            var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
            if (!dict.ContainsKey(member.Username))
            {
                await ctx.RespondAsync($"{member.Username} does not have any keys.");
            }
            else
            {
                string keys = dict[member.Username] == 1 ? "key!" : "keys!";
                await ctx.RespondAsync($"{member.Username} has {dict[member.Username]} {keys}");
            }
        }
    }
}