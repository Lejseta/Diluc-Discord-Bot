using Bot.Games.Cards;
using Discord;
using Discord.Extended.Models;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public class ShowEmbed : SlashCommandBase
    {
        public override bool Disable => true;

        public override async Task ExecuteAsync(SocketSlashCommand interaction, SocketUser author)
        {
            if(!CardGameController.Instance.StartGame)return;
            if (CardGameController.Instance.CurrentGame.AllAnswered())
            {
                var results = CardGameController.Instance.CurrentGame.EndCardChoosing();
                var embed = new EmbedBuilder()
                    .WithTitle("Users Choices")
                    .WithColor(CardGameController.Instance.CurrentGame.CurrentRound.ColorTheme);
                
                int counter = 0;
                foreach (var str in results.Values)
                {
                    embed.AddField($"[{counter}]", str);
                    
                    counter++;
                }
                await interaction.RespondAsync(embed: embed.Build(), ephemeral: false);
            }
        }

        protected override SlashCommandBuilder GetCommand()
        {
            SlashCommandBuilder builder = new SlashCommandBuilder()
               .WithName("showanswers")
               .WithDescription("Show Answers");
            return builder;
        }
    }
}
