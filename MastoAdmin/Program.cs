using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using MastoAdmin.RemoteApi;

namespace MastoAdmin
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("MastoAdmin");
            Console.WriteLine("Automate crap that doesn't have a proper API");
            Console.WriteLine();
            Console.WriteLine("Usage: MastoAdmin <admin domain> <admin username> <admin password> <action> <parameters>");
            Console.WriteLine();
            Console.WriteLine("Available actions:");
            Console.WriteLine("  blockdomain <domain> [public comment] [private comment]");
            Environment.Exit(1);
        }

        private static Scraping _scraping;
        
        static async Task Main(string[] args)
        {
            if (args.Length < 4)
                PrintUsage();

            var domain = args[0];
            var username = args[1];
            var password = args[2];
            var action = args[3];
            var actionParameters = args.Skip(4).ToArray();
            
            _scraping = new Scraping(new Uri(domain));
            var login = await _scraping.Login(username, password);
            if (!login)
                throw new Exception("Failed to login");

            switch (action)
            {
                case "blockdomain":
                    await BlockDomain(actionParameters);
                    break;
                default:
                    Console.WriteLine($"did not recognize action '{action}'");
                    break;
            }
        }

        static async Task BlockDomain(string[] args)
        {
            var block = args[0];
            var publicComment = args.Length > 1 ? args[1] : "";
            var privateComment = args.Length > 2 ? args[2] : "";

            await _scraping.BlockDomain(block, publicComment, privateComment);
            Console.WriteLine($"domain '{block}' blocked");
        }
    }
}