using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PasswordCrackingApplication.DataModel;
using PasswordCrackingApplication.DataModel.EventArgs;
using PasswordCrackingApplication.Model.Utilities;
using PasswordCrackingApplication.Network;

namespace PasswordCrackingApplication.Model.Controller
{
    public class ServerController
    {
        private CrackingHandler _crackingHandler;
        private Server _server;

        public ServerController(CrackingHandler crackingHandler, Server server)
        {
            this._crackingHandler = crackingHandler;
            this._server = server;
            Initialize();

            while (true)
            {
                var readLine = Console.ReadLine();
                StateUpdated(null, new StateEventArgs(null, StateHandler.FetchClientState(readLine), true));
            }
        }

        private void Initialize()
        {
            _server.OnStateUpdatedEventHandler += StateUpdated;
            _crackingHandler.OnStateUpdatedEventHandler += StateUpdated;
            Task.Factory.StartNew(() => _crackingHandler.CheckClientsInactivity());
        }

        private void StateUpdated(object sender, StateEventArgs eventArgs)
        {
            try
            {
                switch (eventArgs.DataPacket?.State ?? eventArgs.State)
                {
                    case ProgressState.Connected:
                        Console.WriteLine("\nClient: " + eventArgs.Client.ClientIdentifier + "\n- Connected\n- Initializing and testing client");
                        InternalStateUpdate(eventArgs.Client, ProgressState.RequestUserAccount);
                        break;

                    case ProgressState.RequestUserAccount:
                        _server.SendToClient(eventArgs.Client, new DataPacket(ProgressState.RequestUserAccount, JsonConvert.SerializeObject(_crackingHandler.UserAccountSets)));
                        InternalStateUpdate(eventArgs.Client, ProgressState.TestClientSpeed);
                        break;

                    case ProgressState.TestClientSpeed:
                        if (eventArgs.InternalStateUpdate)
                        {
                            _server.SendToClient(eventArgs.Client, new DataPacket(ProgressState.TestClientSpeed, JsonConvert.SerializeObject(_crackingHandler.FetchTestDictionarySets())));
                            break;
                        }

                        _crackingHandler.InitializeCrackingClient(eventArgs.Client, Convert.ToInt32(eventArgs.DataPacket.Data));
                        InternalStateUpdate(eventArgs.Client, ProgressState.RequestDictionarySet);
                        break;

                    case ProgressState.RequestDictionarySet:
                        var dictionarySetGroup = _crackingHandler.FetchNextDictionarySetGroup(client: eventArgs.Client);
                        Console.WriteLine("- Cracking started\n- Client cracking based on " + dictionarySetGroup.DictionarySets.Count + " keywords.");
                        _server.SendToClient(eventArgs.Client, new DataPacket(ProgressState.RequestDictionarySet, JsonConvert.SerializeObject(dictionarySetGroup)));
                        break;

                    case ProgressState.CompletedDictionarySet:
                        Console.WriteLine("\nClient: "+ eventArgs.Client.ClientIdentifier + "\n- Completed dictionary set (Stored in textfile)");
                        _crackingHandler.SaveCrackingResults(eventArgs.Client, JsonConvert.DeserializeObject<DictionarySetGroup>(eventArgs.DataPacket.Data));
                        break;

                    case ProgressState.RequestClientProgress:
                        if (eventArgs.InternalStateUpdate)
                        {
                            foreach(var client in _server.ServerClients)
                                if(client.IsClientConnected)
                                    _server.SendToClient(client, new DataPacket(ProgressState.RequestClientProgress));
                            break;
                        }
                        var crackingClient = _crackingHandler.GetCrackingClient(eventArgs.Client.ClientIdentifier);
                        var result = _crackingHandler.CalculateClientProgress(eventArgs.Client, JsonConvert.DeserializeObject<int>(eventArgs.DataPacket.Data));

                        Console.WriteLine("\n Client: " + eventArgs.Client.ClientIdentifier + ":\n- Progress: " + result + "%\n- Total sets checked: " + crackingClient.DictionarySetGroups.Count(x => x.IsChecked == true));
                        break;

                    case ProgressState.OverallProgress:
                        int checkedSets = 0;
                        int uncheckedSets = 0;
                        foreach(var obj in _crackingHandler.CrackingClients)
                            foreach(var setGroup in obj.DictionarySetGroups)
                                if (setGroup.IsChecked)
                                    checkedSets += setGroup.DictionarySets.Count;
                                else
                                    uncheckedSets += setGroup.DictionarySets.Count(x => x.IsChecked == false);

                        Console.WriteLine("\nOverall progress:\n- Checked: " + checkedSets + " of " + _crackingHandler.DictionarySets.Count + " total sets.\n- Current checking: " + uncheckedSets + " sets.");
                        break;

                    case ProgressState.ReceivedMessage:
                        Console.WriteLine(eventArgs.DataPacket.Data);
                        break;

                    case ProgressState.ClientInactive:
                        Console.WriteLine("\nClient: " + eventArgs.Client.ClientIdentifier + "\n- Inactive for too long.\n- Client cracking reset.");
                        _server.SendToClient(eventArgs.Client, new DataPacket(ProgressState.ClientInactive));
                        InternalStateUpdate(eventArgs.Client, ProgressState.TestClientSpeed);
                        break;

                    case ProgressState.Disconnected:
                        Console.WriteLine("\nClient: " + eventArgs.Client.ClientIdentifier + "\n- Disconnected.\n- Clients cracking data has been reset.");
                        _crackingHandler.RemoveInactiveClientData(eventArgs.Client);
                        break;

                    case ProgressState.Invalid:
                        Console.WriteLine("INVALID_COMMAND");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InternalStateUpdate(ServerClient client, ProgressState progressState)
        {
            this.StateUpdated(null, new StateEventArgs(client, progressState, true));
        }
    }
}
