using System;

namespace LNUBookShareConsole
{
    public interface IConsoleIO
    {
        void WriteLine(string message);
        void Write(string message);
        string ReadLine();
        void SetColor(ConsoleColor color);
        void ResetColor();
    }

    // Реальна реалізація для запуску програми
    public class RealConsoleIO : IConsoleIO
    {
        public void WriteLine(string message) => Console.WriteLine(message);
        public void Write(string message) => Console.Write(message);
        public string ReadLine() => Console.ReadLine();
        public void SetColor(ConsoleColor color) => Console.ForegroundColor = color;
        public void ResetColor() => Console.ResetColor();
    }
}