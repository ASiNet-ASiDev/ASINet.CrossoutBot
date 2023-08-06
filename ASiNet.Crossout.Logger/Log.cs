namespace ASiNet.Crossout.Logger;

public static class Log
{
    /// <summary>
    /// 0 - Disable Logs.
    /// 1 - Oly Error Logs.
    /// 2 - Error And Warning Logs.
    /// 3 - All Logs.
    /// </summary>
    public static int LogLevel = 3;

    private static readonly object _locker = new();

    public static void InfoLog(string text)
    {
        if(LogLevel < 3)
            return;
        lock (_locker)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("G")}][INFO] {text}");
            Console.ForegroundColor = oldColor;
        }
    }

    public static void WarningLog(string text)
    {
        if (LogLevel < 2)
            return;
        lock (_locker)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("G")}][WARNING] {text}");
            Console.ForegroundColor = oldColor;
        }
    }

    public static void ErrorLog(string text)
    {
        if (LogLevel < 1)
            return;
        lock (_locker)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("G")}][ERROR] {text}");
            Console.ForegroundColor = oldColor;
        }
    }
}
