using Bot.Games.Cards;
using Discord;
using Discord.Extended.Models;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public class JoinGame : SlashCommandBase
    {
        public override bool Disable => false;

        public override async Task ExecuteAsync(SocketSlashCommand interaction, SocketUser author)
        {
            if (CardGameController.Instance.AddUser(author, interaction))
                await interaction.RespondAsync(author + " joined the game");
            else
                await interaction.RespondAsync("You already joined the game");
        }

        protected override SlashCommandBuilder GetCommand()
        {
            SlashCommandBuilder builder = new SlashCommandBuilder()
               .WithName("joingame")
               .WithDescription("Join Game");
            return builder;
        }
    }
}


