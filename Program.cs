namespace Bot
{
    public class Program
    {
        
        static void Main(string[] args)
        {
            BotProgram bot = new BotProgram();
            bot.Bot().GetAwaiter().GetResult();
        }   
    }
}
