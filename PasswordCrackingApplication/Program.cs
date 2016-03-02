using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PasswordCrackingApplication.DataModel;
using PasswordCrackingApplication.Model;
using PasswordCrackingApplication.Model.Controller;
using PasswordCrackingApplication.Model.Utilities;
using PasswordCrackingApplication.Network;

namespace PasswordCrackingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomTraceListener.AddCustomListener();
            string[] dictionaries = {"webster-dictionary.txt", "words-da"};
            var crackingHandler = new CrackingHandler("passwords.txt", dictionaries);
            var server = new Server(null, 6789);
            new ServerController(crackingHandler, server);
        }
    }
}
