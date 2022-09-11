using Discord;

namespace Bot.Games.Cards
{
    public struct RoundInfo
    {
        public int Round;
        public IUser Reader;
        public BlackCard BlackCard;
        public Discord.Color ColorTheme;

        public RoundInfo(int round)
        {
            Round = round;
            Reader = null;
            BlackCard = null;
            ColorTheme = (Discord.Color)Discord.Extended.Tools.GetRandomColor();
        }

        public RoundInfo(int round, IUser reader, BlackCard blackCard)
        {
            Round = round;
            Reader = reader;
            BlackCard = blackCard;
            ColorTheme = (Discord.Color)Discord.Extended.Tools.GetRandomColor();
        }
    }
}
