using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PasswordCrackingClient.Model;
using PasswordCrackingClient.Model.Controller;
using PasswordCrackingClient.Model.Utilities;
using PasswordCrackingClient.Network;

namespace PasswordCrackingClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Address to the host: ");
            var ip = Console.ReadLine();
            var crackingHandler = new CrackingHandler();
            var client = new Client(ip, 6789);
            new ClientController(crackingHandler, client);
            Thread.Sleep(5000000);
        }
    }
}
