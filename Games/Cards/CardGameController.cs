using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Discord.WebSocket;

namespace Bot.Games.Cards
{
    public class CardGameController
    {
        public static CardGameController Instance { get; } = new CardGameController();

        public GameInfo CurrentGame;
        public bool GameStarted => CurrentGame != null;
        public int UsersCount => usersJoined.Count;

         List<UserInfo> usersJoined;

        public CardGameController()
        {
            usersJoined = new List<UserInfo>();
        }

        public bool AddUser(IUser user, SocketSlashCommand interaction)
        {
            if (ContainsUser(user)) return false;
            usersJoined.Add(new UserInfo(user, true, interaction));
            return true;
        }

        public bool RemoveUser(IUser user)
        {
            for (int x = 0; x < usersJoined.Count; x++)
            {
                if (usersJoined[x].User != user) continue;

                usersJoined.RemoveAt(x);
                return true;
            }
            return false;
        }

        public bool ContainsUser(IUser user)
        {
            return usersJoined.Any(info => info.User == user);
        }

        public bool StartGame(SocketSlashCommand interaction, int roundsCount)
        {
            try
            {
                if (usersJoined.Any(info => !info.IsReady)) return false;
                if (GameStarted) return false;

                List<string> whiteCards = new List<string>();
                List<BlackCard> blackCards = new List<BlackCard>();

                using (StreamReader reader = new StreamReader("./Cards/whiteCards.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string str = reader.ReadLine();
                        Console.WriteLine(str);
                        whiteCards.Add(str);
                    }
                }

                using (StreamReader reader = new StreamReader("./Cards/blackCards.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string str = reader.ReadLine();
                        Console.WriteLine(str);
                        blackCards.Add(new BlackCard(str));
                    }
                }

                CurrentGame = new GameInfo(usersJoined, whiteCards.ToArray(), blackCards.ToArray(), roundsCount, interaction);
                return true;
            }
            catch (Exception e) { Console.WriteLine(e); }
            return false;
        }
    }
}
