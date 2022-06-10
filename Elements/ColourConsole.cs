namespace Elements
{
    public static class ColourConsole
    {

        public static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = foregroundColor != null ? foregroundColor : ConsoleColor.White;
            Console.BackgroundColor = backgroundColor != null ? backgroundColor : ConsoleColor.Black;
            Console.WriteLine(timeStamp + " - " + text);
            Console.ResetColor();
        }

        public static void WriteLine(string text, ConsoleColor foregroundColor)
        {
            WriteLine(text, foregroundColor, ConsoleColor.Black);
        }

        public static void WriteNormal(string text)
        {
            WriteLine(text, ConsoleColor.White, ConsoleColor.Black);
        }

        public static void WriteInfo(string text)
        {
            WriteLine(text, ConsoleColor.Blue, ConsoleColor.Black);
        }

        public static void WriteSuccess(string text)
        {
            WriteLine(text, ConsoleColor.Green, ConsoleColor.Black);
        }

        public static void WriteWarning(string text)
        {
            WriteLine(text, ConsoleColor.DarkYellow, ConsoleColor.Black);
        }

        public static void WriteDanger(string text)
        {
            WriteLine(text, ConsoleColor.Red, ConsoleColor.Black);
        }

        public static void WriteError(string text)
        {
            WriteLine(text, ConsoleColor.White, ConsoleColor.Red);
        }

    }
}
