using System.IO;
using Serilog;
using XSockets.Logger;

public class DefaultLogger : XLogger
{
    public DefaultLogger()
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
    }
}