using System;

namespace Blue3DPrinter
{
    /// <summary>
    /// Console message
    /// </summary>
    public static class LogMsg
    {
        public static void Error(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
        }
        public static void Warning(string message)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Success(string message, ConsoleColor font = ConsoleColor.White)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
        public static void Message(string message, ConsoleColor textcolor = ConsoleColor.White, bool newline =true)
        {
            Console.ForegroundColor = textcolor;
            if (newline) Console.WriteLine(message); else Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
