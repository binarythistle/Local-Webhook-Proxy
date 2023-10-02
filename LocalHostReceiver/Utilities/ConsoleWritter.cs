namespace LocalHostReceiver;

public class ConsoleWriter
{
    public static void PrintColorMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void PrintColorChar(char character, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(character);
        Console.ResetColor();
    }
}