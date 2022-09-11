using Discord;
using Discord.WebSocket;
using System;

namespace Bot.Games.Cards
{
    public class PlayerInfo
    {
        public event Action<PlayerInfo> CardSelected;

        public IUser User;
        public int GainedPoints;
        public string[] WhiteCards;
        public int[] SelectedCards;
        public readonly SocketSlashCommand Interaction;

        readonly object objectLock = new object();

        public PlayerInfo(IUser user, SocketSlashCommand interaction)
        {
            User = user;
            WhiteCards = new string[15];
            SelectedCards = new int[15];
            GainedPoints = 0;
            Interaction = interaction;
            ClearRoundData();
        }
        /// <summary>
        /// CardIndex is card index in which <see cref="WhiteCards"/> position is location where it can be placed. Returns true when card set up is successful.
        /// </summary>
        /// <param name="cardIndex"></param>
        /// <param name="position"></param>
        public bool SelectCard(int cardIndex, int position)
        {
            lock (objectLock)
            {
                if (CardGameController.Instance.CurrentGame.EndRoundMessage)
                    return false;

                foreach (var card in SelectedCards)
                    if (card == cardIndex)
                        return false;

                SelectedCards[position] = cardIndex;

                //Event as backward communication with the game. When someone selects a card, GameInfo will be 
                // know about it (because while creating players, it subscribes to their events)
                CardSelected?.Invoke(this);
                return true;
            }
        }

        public void ClearRoundData()
        {
            for (int x = 0; x < SelectedCards.Length; x++)
            {
                if (SelectedCards[x] != -1)
                {
                    WhiteCards[SelectedCards[x]] = null;
                    SelectedCards[x] = -1;
                }
            }
        }
    }
}
