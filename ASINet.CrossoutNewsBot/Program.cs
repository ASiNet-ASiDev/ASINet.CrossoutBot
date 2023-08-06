using ASiNet.Crossout.Core;
using ASiNet.Crossout.Logger;

BotConfig? cnf = null;
do
{
    cnf = ConfigLoader.ReadConfig();
    if(cnf != null)
        break;
    Console.WriteLine("Config not found or damaged!\nCreate new empty config?\n[Y/N]");
    var resp = Console.ReadLine()?.ToLower().Trim().FirstOrDefault();

    if(resp is 'y')
    {
        if(ConfigLoader.CreateEmptyConfig())
            Console.WriteLine($"Config created, path: {ConfigLoader.CNF_PATH}\nPlease fill in the config with the data.");
        else
            Console.WriteLine($"Create config error: {ConfigLoader.CNF_PATH}");
    }
    else if(resp is 'n')
    {
        Console.WriteLine($"Create a new config, or add an existing one along the way: {ConfigLoader.CNF_PATH}");
        Console.WriteLine("Press [ENTER] after the operation is completed.");
        Console.ReadLine();
    }
    else
        Console.WriteLine("[invalid response]");
}
while (cnf is null);

Log.LogLevel = cnf.LogLevel;

using var core = new CrossautBotCore(cnf);

Console.ReadKey();