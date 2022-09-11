using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Audio;

namespace Bot.Handlers
{
    public class CommandHandler
    {
        readonly DiscordSocketClient client;
        readonly CommandService commands;
        readonly IServiceProvider service;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider service)
        {
            this.client = client;
            this.commands = commands;
            this.service = service;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("Commands Collecting...");
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), service);
            client.MessageReceived += Message;
            Console.WriteLine(this + "  active");
        }

        int arg = 0;
        public async Task Message(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage message) || message.Source != Discord.MessageSource.User) return;
            var context = new SocketCommandContext(client, message);
            //do nothing
            if (message.Author.IsBot) return;

            if (message.HasCharPrefix('-', ref arg))
            {
                try
                {
                    var result = await commands.ExecuteAsync(context, arg, service);
                    Console.WriteLine(result.ErrorReason);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}