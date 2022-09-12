using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Extended.Models;
using Discord.WebSocket;
using Bot.Games.Cards;
using System.Linq;

namespace Bot.SlashCommands
{
    public class SelectCard : SlashCommandBase
    {
        public override async Task ExecuteAsync(SocketSlashCommand interaction, SocketUser author)
        {
            if (!CardGameController.Instance.GameStarted) return;
            if (!CardGameController.Instance.ContainsUser(author)) return;
            if (CardGameController.Instance.CurrentGame.CurrentRound.Reader.Id == author.Id) {
                await interaction.RespondAsync("Reader cannot select!!", ephemeral: true);
                return;
            }

            CardGameController.Instance.CurrentGame.GetPlayer(author, out PlayerInfo player);

            ComponentBuilder component = new ComponentBuilder();

            //Answers quantity
            for (int x = 0; x < CardGameController.Instance.CurrentGame.AnswersCount; x++) {
                //All Cards
                SelectMenuBuilder builder = new SelectMenuBuilder()
                    .WithCustomId("selectmenu" + x)
                    .WithMaxValues(1)
                    .WithMinValues(1);

                int counter = 0;
                foreach (var info in player.WhiteCards) {
                    builder.AddOption(info, counter.ToString());
                    counter++;
                }

                ActionRowBuilder row = new ActionRowBuilder()
                    .WithSelectMenu(builder);
                component.AddRow(row);
            }

            await interaction.RespondAsync(components: component.Build(), ephemeral: true);
        }

        public async Task SelectMenuRespond(SocketMessageComponent interaction)
        {
            if (!interaction.Data.CustomId.StartsWith("selectmenu")) return;

            CardGameController.Instance.CurrentGame.GetPlayer(interaction.User, out PlayerInfo player);

            var customId = interaction.Data.CustomId;
            int card = int.Parse(interaction.Data.Values.First());
            int pos = int.Parse(customId[customId.Length - 1].ToString());

            if (player.SelectCard(card, pos))
                await interaction.RespondAsync("Accepted.", ephemeral: true);
            else
                await interaction.RespondAsync("Rejected.", ephemeral: true);
        }

        protected override SlashCommandBuilder GetCommand()
        {
            BotProgram.GetInstance().client.SelectMenuExecuted += SelectMenuRespond;

            SlashCommandBuilder builder = new SlashCommandBuilder()
                .WithName("selectcard")
                .WithDescription("Select menu");

            return builder;
        }
    }
}
