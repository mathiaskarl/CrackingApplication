using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PasswordCrackingClient.DataModel;
using PasswordCrackingClient.DataModel.EventArgs;

namespace PasswordCrackingClient.Network
{
    public class Client : ProgressStateUpdated
    {
        private TcpClient _tcpClient;
        private string _ipAddress;
        private int _port;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private bool IsConnected = false;

        public Client(string ipAddress, int port)
        {
            this._ipAddress = ipAddress;
            this._port = port;
            Initialize();
        }

        private void Initialize()
        {
            if (Connect())
            {
                this._streamReader = new StreamReader(_networkStream);
                this._streamWriter = new StreamWriter(_networkStream);
                Task.Factory.StartNew(ReadFromServer);
            }
        }

        private bool Connect()
        {
            try
            {
                this._tcpClient = new TcpClient(_ipAddress, _port);
                this._networkStream = _tcpClient.GetStream();
                Console.WriteLine("Connected to server");
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not establish connection - retrying in 5 seconds.");
                Thread.Sleep(5000);
                return Connect();
            }
        }

        private void ReadFromServer()
        {
            try
            {
                while (IsConnected)
                {
                    string message = _streamReader.ReadLine();
                    RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(new DataPacket(message)));
                }
            }
            catch (Exception ex)
            {
                this.IsConnected = false;
                RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(ProgressState.Disconnected));
                CloseConnection();
            }
        }

        public void WriteToServer(DataPacket dataPacket)
        {
            if (!IsConnected)
                throw new Exception("NOT_CONNECTED");

            this._streamWriter.WriteLine(dataPacket.RawData);
            this._streamWriter.Flush();
        }

        public void CloseConnection()
        {
            this._streamWriter.Close();
            this._streamReader.Close();
            this._networkStream.Close();
        }
    }
}
