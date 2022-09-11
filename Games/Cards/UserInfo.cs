using Discord;
using Discord.WebSocket;

namespace Bot.Games.Cards
{
    public struct UserInfo
    {
        public IUser User;
        public bool IsReady { get; set; }
        public SocketSlashCommand UserInteraction;

        public UserInfo(IUser user, bool isReady, SocketSlashCommand interaction)
        {
            User = user;
            IsReady = isReady;
            UserInteraction = interaction;
        }
    }
}
