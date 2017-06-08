namespace UltravioletGame1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
