using System;
using System.Linq;
using XSockets.Core.Common.Socket;
using XSockets.Plugin.Framework;

namespace SelfHosted
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = Composable.GetExport<IXSocketServerContainer>())
            {
                container.Start();
                Console.ReadLine();
            }
        }
    }
}
