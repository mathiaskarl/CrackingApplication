using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PasswordCrackingApplication.DataModel;
using PasswordCrackingApplication.DataModel.EventArgs;
using PasswordCrackingApplication.Model.Utilities;

namespace PasswordCrackingApplication.Network
{
    public class Server : ProgressStateUpdated
    {
        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private readonly IPAddress _ipAdress;
        private readonly int _port;

        private readonly int _sleepTimer = 5000;
        public int ClientLimit;
        public List<ServerClient> ServerClients;

        public Server(string ipAdress, int port, int clientLimit = 10)
        {
            this._ipAdress = (ipAdress == null ? IPAddress.Any : IPAddress.Parse(ipAdress));
            this._port = port;
            this.ClientLimit = clientLimit;

            Initialize();
        }

        private void Initialize()
        {
            _tcpListener = new TcpListener(_ipAdress, _port);
            ServerClients = new List<ServerClient>();

            Task.Factory.StartNew(RunServer);
        }

        private void RunServer()
        {
            try
            {
                _tcpListener.Start();
                AcceptNewClients();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _tcpListener.Stop();
            }
        }

        private void AcceptNewClients()
        {
            while (ServerClients.Count < ClientLimit)
            {
                _tcpClient = _tcpListener.AcceptTcpClient();
                new ServerClient(this, _tcpClient);
            }
            Thread.Sleep(_sleepTimer);
            AcceptNewClients();
        }

        public void AddClient(ServerClient client)
        {
            if (ServerClients.Contains(client))
                return;

            ServerClients.Add(client);
            client.IsClientConnected = true;
            Task.Factory.StartNew(client.ClientListener);
            RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(client, ProgressState.Connected));
        }

        public void RemoveClient(ServerClient client)
        {
            if (!ServerClients.Contains(client))
                return;

            ServerClients.Remove(client);
            client.IsClientConnected = false;
            client.CloseConnection();
            RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(client, ProgressState.Disconnected));
        }

        public void SendToClient(ServerClient serverClient, DataPacket dataPacket)
        {
            var client = ServerClients.FirstOrDefault(x => x.ClientIdentifier == serverClient.ClientIdentifier);

            if(client != null && client.IsClientConnected)
                client.StreamWriter.WriteLine(dataPacket.RawData);
        }

        public void SendToClient(string identifier, DataPacket dataPacket)
        {
            var client = ServerClients.FirstOrDefault(x => x.ClientIdentifier == identifier);

            if (client != null && client.IsClientConnected)
                client.StreamWriter.WriteLine(dataPacket.RawData);
        }

        public void SendToAllClients(DataPacket dataPacket)
        {
            if (ServerClients.Count < 1)
                return;

            foreach(var client in ServerClients)
                if(client.IsClientConnected)
                    client.StreamWriter.WriteLine(dataPacket);
        }

        public void ReceiveMessage(ServerClient client, string message)
        {
            RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(client, new DataPacket(message)));
        }
    }
}
