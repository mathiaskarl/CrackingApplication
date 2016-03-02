using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingApplication.DataModel.Interfaces;
using PasswordCrackingApplication.Network;

namespace PasswordCrackingApplication.DataModel
{
    public class CrackingClient : IClient
    {
        public string ClientIdentifier {
            get { return Client.ClientIdentifier; }
            set { }
        }
        public ServerClient Client { get; set; }
        public List<DictionarySetGroup> DictionarySetGroups { get; set; } = new List<DictionarySetGroup>();
        public int SetSize { get; set; }

        public CrackingClient(IClient client, int setSize)
        {
            this.Client = (ServerClient)client;
            this.SetSize = setSize;
        }
    }
}
