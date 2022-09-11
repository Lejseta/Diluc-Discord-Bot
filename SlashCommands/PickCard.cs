using Bot.Games.Cards;
using Discord;
using Discord.Extended.Models;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public class PickCard : SlashCommandBase
    {
        public override bool Disable => false;

        public override bool IsEphmeral => true;

        public override async Task ExecuteAsync(SocketSlashCommand interaction, SocketUser author)
        {
            try
            {
                var user = author;

                if (!CardGameController.Instance.GameStarted) return;
                if (!CardGameController.Instance.ContainsUser(user)) return;

                var en = interaction.Data.Options.GetEnumerator();
                int cardIndex = -1;
                int cardSlot = -1;

                while (en.MoveNext())
                {
                    int userOption = Convert.ToInt32(en.Current.Value);
                    switch (en.Current.Name)
                    {
                        case "index":
                            if (userOption < 0 || userOption > 14)
                            {
                                await interaction.RespondAsync(text: "Wrong card number.", ephemeral: IsEphmeral);
                                return;
                            }

                            cardIndex = userOption;
                            break;

                        case "slot":
                            if (userOption < 0 || userOption > 14)
                            {
                                await interaction.RespondAsync(text: "Wrong card position!", ephemeral: IsEphmeral);
                                return;
                            }

                            cardSlot = userOption;
                            break;
                    }
                }

                if (cardIndex == -1 || cardSlot == -1)
                {
                    await interaction.RespondAsync("Brakuje wartości!");
                    return;
                }

                Console.WriteLine($"Index {cardIndex}, slot {cardSlot}");
                CardGameController.Instance.CurrentGame.GetPlayer(user, out var player);
                if (CardGameController.Instance.CurrentGame.AnswersCount > cardSlot)
                {
                    if (player.SelectCard(cardIndex, cardSlot))
                        await interaction.RespondAsync("Przyjąłem do wiadomości", ephemeral: IsEphmeral);
                    else
                        await interaction.RespondAsync("Karta jest już zajęta.");
                }

                //debug
                var cards = player.SelectedCards;

                var results = cards.Select(card => card != -1);
                Console.WriteLine($"{user.Username}, {string.Join(", ", cards)}");
                //end
            }
            catch (Exception e) { Console.WriteLine("error *" + e); }
        }

        protected override SlashCommandBuilder GetCommand()
        {
            SlashCommandBuilder builder = new SlashCommandBuilder()
               .WithName("pickcard")
               .AddOption(name: "index", type: ApplicationCommandOptionType.Integer, description: "Wybiera kartę pod konkretnym indexem.", isRequired: true)
               .AddOption(name: "slot", type: ApplicationCommandOptionType.Integer, description: "Wybiera kartę pod konkretnym slotem.", isRequired: true)
               .WithDescription("Selects a card with a specific index");

            return builder;
        }
    }
}

