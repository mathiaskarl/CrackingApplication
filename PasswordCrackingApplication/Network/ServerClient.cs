using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingApplication.DataModel;
using PasswordCrackingApplication.DataModel.Interfaces;

namespace PasswordCrackingApplication.Network
{
    public class ServerClient : IClient
    {
        private Server _serverInstance;
        private TcpClient _clientInstance;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        public StreamWriter StreamWriter;

        public bool IsClientConnected = false;

        public string ClientIdentifier      { get; set; }
        public DateTime TimeConnected       { get; private set; }

        public ServerClient(Server server, TcpClient client)
        {
            InitalizeClient(server, client);
        }

        public void ClientListener()
        {
            try
            {
                while (IsClientConnected)
                    this._serverInstance.ReceiveMessage(this, _streamReader.ReadLine());
            }
            catch (Exception ex)
            {
                this.IsClientConnected = false;
            }
            finally
            {
                this._serverInstance.RemoveClient(this);
            }
        }

        private void InitalizeClient(Server server, TcpClient client)
        {
            this._serverInstance = server;
            this._clientInstance = client;
            this.ClientIdentifier = AssignClientIdentifier();
            this.TimeConnected = DateTime.Now;

            AssignStreamData();
            this._serverInstance.AddClient(this);
        }

        private string AssignClientIdentifier()
        {
            var identifier = Guid.NewGuid().ToString();
            if (_serverInstance.ServerClients.Any(x => x.ClientIdentifier == identifier))
                return AssignClientIdentifier();
            return identifier;
        }

        private void AssignStreamData()
        {
            this._networkStream = _clientInstance.GetStream();
            this.StreamWriter = new StreamWriter(_networkStream) { AutoFlush = true };
            this._streamReader = new StreamReader(_networkStream);
        }

        public void CloseConnection()
        {
            this.StreamWriter.Close();
            this._streamReader.Close();
            this._networkStream.Close();
            this._clientInstance.Close();
        }
    }
}
