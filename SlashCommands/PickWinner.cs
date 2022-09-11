using Bot.Games.Cards;
using Discord;
using Discord.Extended.Models;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public class PickWinner : SlashCommandBase
    {
        public override bool Disable => false;

        public override async Task ExecuteAsync(SocketSlashCommand interaction, SocketUser author)
        {
            CardGameController.Instance.CurrentGame.GetPlayer(author, out var player);
            if (player.User.Id != CardGameController.Instance.CurrentGame.CurrentRound.Reader.Id) return;
            if (!CardGameController.Instance.CurrentGame.AllAnswered()) return;

            try
            {
                var result = CardGameController.Instance.CurrentGame.EndCardChoosing();
                Console.WriteLine($"Length {result.Count}");

                var en = interaction.Data.Options.GetEnumerator();
                en.MoveNext();
                int userOption = Convert.ToInt32(en.Current.Value);

                if (userOption < 0 || userOption >= result.Count)
                {
                    await interaction.RespondAsync(text: "Wrong Card Number.", ephemeral: IsEphmeral);
                    return;
                }

                CardGameController.Instance.CurrentGame.SelectWinner(userOption);
            }
            catch (Exception e) { Console.WriteLine("Error " + e); }
        }

        protected override SlashCommandBuilder GetCommand()
        {
            SlashCommandBuilder builder = new SlashCommandBuilder()
               .WithName("picktwinner")
               .AddOption(name: "index", type: ApplicationCommandOptionType.Integer, description: "Selects a card with a specific index.", isRequired: true)
               .WithDescription("Selects a card with a specific index");
            return builder;
        }
    }
}
