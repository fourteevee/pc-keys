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
        [Hidden]
        public async Task AddKeys(CommandContext ctx, [Description("User to add a key to")] DiscordMember member)
        {
            //Restrict the use of this command to only specified roles
            bool permitted = false;
            foreach (ulong l in Bot.Config.PermittedRoles)
            {
                if (ctx.Member.Roles.Contains(ctx.Guild.GetRole(l)))
                {
                    permitted = true;
                    break;
                }
            }
            if (!permitted) return;

            var json = LoadFile();

            //Parse the JSON. If the parse fails (usually due to empty string), create a new dictionary
            var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            //If user isn't in the dictionary, add them with no keys.
            if (!dict.ContainsKey(member.Username))
                dict.Add(member.Username, 0);
            
            //Update their key count, grant them a new role if necessary
            var value = dict[member.Username] + 1;
            if (Bot.Config.Roles.ContainsKey(value))
                await member.GrantRoleAsync(ctx.Guild.GetRole(Bot.Config.Roles[value]));

            //Save the JSON back to the file
            dict[member.Username] = value;
            string toSave = JsonConvert.SerializeObject(dict);
            File.WriteAllText(Bot.Config.FileName, toSave);

            //Send an update message
            string keys = dict[member.Username] == 1 ? "key!" : "keys!";
            await ctx.RespondAsync($"{member.Username} now has {value} {keys}");
        }

        [Command("getkeys"), Description("Gets the number of keys a user has")]
        public async Task GetKeys(CommandContext ctx, [Description("User to get keys from")] DiscordMember member)
        {
            var json = LoadFile();
            
            //Parse the JSON. If the parse fails (usually due to empty string), create a new dictionary
            var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            if (dict.ContainsKey(member.Username))
            {
                string keys = dict[member.Username] == 1 ? "key!" : "keys!";
                await ctx.RespondAsync($"{member.Username} has {dict[member.Username]} {keys}");
            }
            else
            {
                await ctx.RespondAsync($"{member.Username} does not have any keys.");
            }
        }

        private string LoadFile()
        {
            //Open the JSON file, or create a new one if it doesn't exist already
            using var fs = File.Open(Bot.Config.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using var sr = new StreamReader(fs);
            //If ReadToEnd returns null (empty file), set json to an empty string
            var json = sr.ReadToEnd() ?? "";
            sr.Close();
            return json;
        }
    }
}