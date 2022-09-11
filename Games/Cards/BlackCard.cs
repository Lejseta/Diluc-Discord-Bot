namespace Bot.Games.Cards
{
    public class BlackCard
    {
        public int WhiteCardsSpaces { get; private set; }
        public string[] BlackCardText { get; private set; }

        public BlackCard(string text)
        {
            BlackCardText = text.Split('%');
            WhiteCardsSpaces = BlackCardText.Length - 1;
        }

        public string FillCard(string[] answers)
        {
            string allSentence = "";

            for (int i = 0; i < WhiteCardsSpaces; i++)
                allSentence += BlackCardText[i] + answers[i];
            allSentence += BlackCardText[BlackCardText.Length -1];
            return allSentence;
        }
    }
}
