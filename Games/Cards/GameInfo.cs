using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Games.Cards
{
    public class GameInfo
    {
        public int MaxNoRounds { get; private set; }
        public int AnswersCount => CurrentRound.BlackCard.WhiteCardsSpaces;
        public string GetBlackCardText => string.Join(@"\_\_\_\_", CurrentRound.BlackCard.BlackCardText);
        //It becomes true when it sends an embed and becomes false at every round end
        public bool EndRoundMessage { get; private set; } = false;
        public ISocketMessageChannel GameChannel;
        public RoundInfo CurrentRound;

        BlackCard[] blackCards;
        string[] whiteCards;
        List<BlackCard> blackCardsPool;
        List<string> whiteCardsPool;

        List<PlayerInfo> players;
        Random random;


        public GameInfo(List<UserInfo> users, string[] whiteCards, BlackCard[] blackCards, int maxRound, SocketSlashCommand channel)
        {
            try
            {
                // Variables initialization
                this.blackCards = blackCards;
                this.whiteCards = whiteCards;
                blackCardsPool = new List<BlackCard>(blackCards);
                whiteCardsPool = new List<string>(whiteCards);

                MaxNoRounds = maxRound;
                CurrentRound = new RoundInfo(0);

                random = new Random();
                players = new List<PlayerInfo>();

                GameChannel = channel.Channel;

                // Players filling with white cards
                foreach (var user in users)
                {
                    var ply = new PlayerInfo(user.User, user.UserInteraction);
                    FillPlayer(ref ply);
                    players.Add(ply);
                    ply.CardSelected += OnCardSelected;
                }

                Round();
            }
            catch (Exception e) { Console.Write(e); }
        }

        public bool AllAnswered()
        {
            foreach (var user in players)
            {
                if (user.User.Id == CurrentRound.Reader.Id) continue;
                for (int i = 0; i < AnswersCount; i++)
                {
                    if (user.SelectedCards[i] == -1)
                        return false;
                }
            }
            return true;
        }

        public void Round()
        {
            if (CurrentRound.Round >= MaxNoRounds)
            {
                GameEnd();
                return;
            }

            // Every round data clearing
            for (int x = 0; x < players.Count; x++)
            {
                var player = players[x];
                player.ClearRoundData();
                FillPlayer(ref player);
                players[x] = player;
            }

            // Checking black cards pool
            if (blackCardsPool.Count == 0)
                blackCardsPool.AddRange(blackCards);

            // Black card and reader choosing
            var index = random.Next(blackCardsPool.Count);
            var blackCard = blackCardsPool[index];
            blackCardsPool.RemoveAt(index);

            var reader = players[CurrentRound.Round % players.Count].User;

            CurrentRound = new RoundInfo(CurrentRound.Round + 1, reader, blackCard);

            // Next round nortification
            var embed = new EmbedBuilder()
                .WithTitle($"Round: {CurrentRound.Round}")
                .WithDescription($"White cards (choose {AnswersCount} card{(AnswersCount > 1 ? "s" : "")})")
                .WithThumbnailUrl("https://media.discordapp.net/attachments/1018240922896576612/1018241489467355226/CAH.png")
                .WithColor(CurrentRound.ColorTheme)
                .AddField("Game Master", CurrentRound.Reader.Username)
                .AddField("Black Card:", GetBlackCardText);

            EndRoundMessage = false;
            GameChannel.SendMessageAsync(embed: embed.Build());
        }

        public void GetPlayers(out List<PlayerInfo> joinedPlayers)
        {
            joinedPlayers = players;
        }

        public void GetPlayer(IUser user, out PlayerInfo player)
        {
            player = players.First(ply => ply.User == user);
        }

        /// <summary>
        /// Gets collection of players choosed cards 
        /// </summary>
        /// <returns></returns>
        public Dictionary<PlayerInfo, string> EndCardChoosing()
        {
            var result = new Dictionary<PlayerInfo, string>();

            foreach (var player in players)
            {
                if (CurrentRound.Reader.Id == player.User.Id) continue;

                string[] answers = new string[AnswersCount];
                for (int x = 0; x < answers.Length; x++)
                {
                    if (player.SelectedCards[x] == -1)
                        answers[x] = @"_\_\_\_";
                    else
                        answers[x] = player.WhiteCards[player.SelectedCards[x]];
                }

                var filledBlackCard = CurrentRound.BlackCard.FillCard(answers);
                Console.WriteLine($"Black  Card {filledBlackCard}");
                result.Add(player, filledBlackCard);
            }

            return result;
        }

        public bool SelectWinner(int option)
        {
            if (option < 0 || option >= players.Count) return false;

            var results = EndCardChoosing();

            var player = results.ElementAt(option);

            GetPlayer(player.Key.User, out var playerReference);
            playerReference.GainedPoints++;

            var embed = new EmbedBuilder()
               .WithTitle("Winner!")
               .WithAuthor(player.Key.User)
               .AddField($"User " + player.Key.User.Username, "Points: " + playerReference.GainedPoints)
               .AddField("Winning Answer:", player.Value);

            GameChannel.SendMessageAsync(embed: embed.Build());
            Task.Run(() => StartRoundIn(3000));
            return true;
        }

        void GameEnd()
        {
            EmbedBuilder results = new EmbedBuilder();

            var sortedPlayers = players.OrderByDescending(ply => ply.GainedPoints);

            var winner = sortedPlayers.ElementAt(0);

            results.WithTitle("Game end!")
                .WithThumbnailUrl("https://media.discordapp.net/attachments/1018240922896576612/1018241489467355226/CAH.png")
                .WithColor(CurrentRound.ColorTheme)
                .WithDescription($"{winner.User.Username} won game with {winner.GainedPoints} points!");

            int counter = 0;
            foreach (var ply in sortedPlayers)
            {
                results.AddField($"[{counter + 1}] {ply.User.Username}", $"{ply.GainedPoints} points");
                counter++;
            }

            GameChannel.SendMessageAsync(embed: results.Build());
            players.Clear();
            CardGameController.Instance.CurrentGame = null;
        }

        async Task StartRoundIn(int seconds)
        {
            await Task.Delay(seconds);
            Round();
        }

        void FillPlayer(ref PlayerInfo info)
        {
            for (int x = 0; x < info.WhiteCards.Length; x++)
            {
                if (info.WhiteCards[x] != null) continue;

                // Refilling white cards pool
                if (whiteCardsPool.Count == 0)
                    whiteCardsPool.AddRange(whiteCards);

                var index = random.Next(whiteCardsPool.Count);
                var card = whiteCardsPool[index];
                whiteCardsPool.RemoveAt(index);

                info.WhiteCards[x] = card;
            }
        }

        void OnCardSelected(PlayerInfo inPlayer)
        {
            if (!AllAnswered() || EndRoundMessage) return;

            var results = EndCardChoosing();
            var embed = new EmbedBuilder()
                .WithTitle("Users Choices")
                .WithColor(CurrentRound.ColorTheme)
                .WithThumbnailUrl("https://media.discordapp.net/attachments/1018240922896576612/1018241489467355226/CAH.png");

            int counter = 0;
            foreach (var str in results.Values)
            {
                embed.AddField($"[{counter}]", str);
                counter++;
            }

            EndRoundMessage = true;
            GameChannel.SendMessageAsync(embed: embed.Build());
        }
    }
}
