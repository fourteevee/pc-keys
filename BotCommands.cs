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
    /// Basic commands.
    /// </summary>
    [Description("Basic bot commands.")]
    public class BasicCommands
    {
        [Command("addkey"), Description("Add a key to a user, updating role as necessary")]
        public async Task AddKey(CommandContext ctx, [Description("User to add a key to")] DiscordMember member)
        {
            await AddKeys(ctx, 1, member);
        }

        [Command("addkeys"), Description("Add multiple keys to a user, updating role as necessary")]
        public async Task AddKeys(CommandContext ctx, [Description("Number of keys to add")] int keys,
            [Description("User to add a key to")] DiscordMember member)
        {
            //Restrict the use of this command to only specified roles
            bool permitted = false;
            foreach (ulong checkRole in Bot.Config.PermittedRoles)
            {
                if (ctx.Member.Roles.Contains(ctx.Guild.GetRole(checkRole)))
                {
                    permitted = true;
                    break;
                }
            }
            if (!permitted) return;
            var json = LoadFile();

            //Parse the JSON. If the parse fails (usually due to empty string), create a new dictionary
            var dict = JsonConvert.DeserializeObject<Dictionary<ulong, int>>(json) ?? new Dictionary<ulong, int>();
            //If user isn't in the dictionary, add them with no keys.
            if (!dict.ContainsKey(member.Id))
                dict.Add(member.Id, 0);
            
            //Update their key count, grant them a new role if necessary
            var value = dict[member.Id] + keys;
            ulong role = 0;
            foreach (int k in Bot.Config.Roles.Keys)
            {
                if (value >= k)
                {
                    role = Bot.Config.Roles[k];
                }
            }

            if (role != 0)
            {
                await ctx.Guild.GrantRoleAsync(member, ctx.Guild.GetRole(role), "Gained enough keys.");
            }
            
            //Save the JSON back to the file
            dict[member.Id] = value;
            string toSave = JsonConvert.SerializeObject(dict);
            await File.WriteAllTextAsync(Bot.Config.FileName, toSave);

            //Send an update message
            string keyStr = value == 1 ? "key!" : "keys!";
            await ctx.RespondAsync($"{member.Nickname ?? member.Username} now has {value} {keyStr}");
        }
        
        [Command("getkeys"), Description("Gets the number of keys a user has")]
        public async Task GetKeys(CommandContext ctx, [Description("User to get keys from")] DiscordMember member)
        {
            var json = LoadFile();
            
            //Parse the JSON. If the parse fails (usually due to empty string), create a new dictionary
            var dict = JsonConvert.DeserializeObject<Dictionary<ulong, int>>(json) ?? new Dictionary<ulong, int>();
            if (dict.ContainsKey(member.Id))
            {
                string keys = dict[member.Id] == 1 ? "key!" : "keys!";
                await ctx.RespondAsync($"{member.Nickname ?? member.Username} has {dict[member.Id]} {keys}");
            }
            else
            {
                await ctx.RespondAsync($"{member.Nickname ?? member.Username} does not have any keys.");
            }
        }

        private string LoadFile()
        {
            //Open the JSON file, or create a new one if it doesn't exist already
            using var fs = File.Open(Bot.Config.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using var sr = new StreamReader(fs);
            var json = sr.ReadToEnd();
            sr.Close();
            return json;
        }
    }
}
