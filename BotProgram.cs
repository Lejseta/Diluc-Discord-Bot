using Bot.Handlers;
using Discord;
using Discord.Commands;
using Discord.Extended;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
namespace Bot
{
    public class BotProgram
    {
        public static BotProgram GetInstance() => instance;
        static BotProgram instance;

        public DiscordSocketClient client;
        public CommandService commands;
        public IServiceProvider service;
        public ApplicationCommandService application;

        bool update = false;

        public BotProgram()
        {
            instance = this;
        }

        public async Task Bot()
        {
            DiscordSocketConfig socketConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.Guilds | GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true
            };

            Console.WriteLine("Bot is running...");
            // Tworzysz klasę bota i logujesz
            client = new DiscordSocketClient(socketConfig);
            await client.LoginAsync(TokenType.Bot, "TOKEN", true);
            await client.StartAsync();
            Console.WriteLine("Bot loaded...");

            try
            {
                Console.WriteLine("Update application commands? T/*");
                var key = Console.ReadKey();
                update = key.Key == ConsoleKey.T;

                Console.WriteLine("Created CommandService.");
                application = new ApplicationCommandService(client);
                application.CollectSlashCommands();
                application.CollectUserCommands();
                application.CollectMessageCommands();

                commands = new CommandService();
                service = instance.BuildServiceProvider();
                client.Ready += InitializeServices;

                Console.WriteLine("Ready");
                await Task.Delay(-1);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
        public async Task InitializeServices()
        {
            await (service.GetService(typeof(CommandHandler)) as CommandHandler).InitializeAsync();
            application.RegisterCommands(update, true);
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(client)
            .AddSingleton(commands)
            .AddSingleton(application)
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }
}
