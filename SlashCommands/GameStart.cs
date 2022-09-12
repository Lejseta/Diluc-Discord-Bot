using Bot.Games.Cards;
using Discord;
using Discord.Extended.Models;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public class GameStart : SlashCommandBase
    {
        public override bool Disable => false;

        public override async Task ExecuteAsync(SocketSlashCommand interaction, SocketUser author)
        {
            if (!CardGameController.Instance.ContainsUser(author))
            {
                await interaction.RespondAsync("You did not join the game. Join the game using ``-joingame`` command");
                return;
            }

            if (CardGameController.Instance.UsersCount < 2)
            {
                await interaction.RespondAsync("To start the game min. 2 users must join.");
                return;
            }

            int roundsCount = 5;
            var data = interaction.Data.Options.First().Value;
            if (data != null)
                roundsCount = Convert.ToInt32(data);
                
            if (!CardGameController.Instance.StartGame(interaction, roundsCount))
            {
                await interaction.RespondAsync("Game is already running or players are not ready.");
                return;
            }
            await interaction.RespondAsync("Game is starting...");
        }

        protected override SlashCommandBuilder GetCommand()
        {
            SlashCommandBuilder builder = new SlashCommandBuilder()
               .WithName("startgame")
               .WithDescription("Start Game")
               .AddOption("roundscount", ApplicationCommandOptionType.Integer, "Number of rounds, by default 5.", minValue: 5, maxValue: 50);
            return builder;
        }
    }
}
