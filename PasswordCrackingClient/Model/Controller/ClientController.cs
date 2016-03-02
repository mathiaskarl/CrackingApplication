using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using PasswordCrackingClient.DataModel;
using PasswordCrackingClient.DataModel.EventArgs;
using PasswordCrackingClient.Model.Utilities;
using PasswordCrackingClient.Network;

namespace PasswordCrackingClient.Model.Controller
{
    public class ClientController
    {
        private CrackingHandler _crackingHandler;
        private Client _client;

        public ClientController(CrackingHandler crackingHandler, Client client)
        {
            this._crackingHandler = crackingHandler;
            this._client = client;
            Initialize();
        }

        private void Initialize()
        {
            _client.OnStateUpdatedEventHandler += StateUpdated;
            _crackingHandler.OnStateUpdatedEventHandler += StateUpdated;
        }

        private void StateUpdated(object sender, StateEventArgs eventArgs)
        {
            try
            {
                switch (eventArgs.DataPacket?.State ?? eventArgs.State)
                {
                    case ProgressState.Connected:
                        Console.WriteLine("- Connected to server.");
                        break;

                    case ProgressState.TestClientSpeed:
                        Console.WriteLine("- Testing client process speed.");
                        var dictionarySets = JsonConvert.DeserializeObject<List<DictionarySet>>(eventArgs.DataPacket.Data);
                        _client.WriteToServer(new DataPacket(ProgressState.TestClientSpeed, JsonConvert.SerializeObject(_crackingHandler.CrackingTest(dictionarySets))));
                        break;

                    case ProgressState.RequestUserAccount:
                        Console.WriteLine("- Received user account information.");
                        _crackingHandler.AddUserAccountSets(JsonConvert.DeserializeObject<List<UserAccountSet>>(eventArgs.DataPacket.Data));
                        break;

                    case ProgressState.RequestDictionarySet:
                        Console.WriteLine("- Received dictionary information.");
                        _crackingHandler.AddDictionarySetGroup(JsonConvert.DeserializeObject<DictionarySetGroup>(eventArgs.DataPacket.Data));
                        _crackingHandler.StartCracking();
                        break;

                    case ProgressState.CompletedDictionarySet:
                        Console.WriteLine("\n- Requesting new dictionary set");
                        _client.WriteToServer(new DataPacket(ProgressState.CompletedDictionarySet, eventArgs.DataPacket.Data));
                        _client.WriteToServer(new DataPacket(ProgressState.RequestDictionarySet));
                        break;

                    case ProgressState.PasswordFound:
                        var userAccountSet = JsonConvert.DeserializeObject<UserAccountSet>(eventArgs.DataPacket.Data);
                        Console.WriteLine("\n----- Possible password:\n------- " + userAccountSet.Username + ":" + userAccountSet.DecryptedPassword + "\n");
                        break;

                    case ProgressState.RequestClientProgress:
                        if(_crackingHandler.IsRunning)
                            _client.WriteToServer(new DataPacket(ProgressState.RequestClientProgress, JsonConvert.SerializeObject((_crackingHandler.CurrentCrackingIndex))));
                        break;

                    case ProgressState.ReceivedMessage:
                        if (eventArgs.DataPacket != null)
                            Console.WriteLine(eventArgs.DataPacket.Data);
                        break;

                    case ProgressState.ClientInactive:
                        Console.WriteLine("\n- Server aborted current cracking session");
                        _crackingHandler.AbortCracking();
                        break;

                    case ProgressState.Disconnected:
                        Console.WriteLine("\n- Disconnected from server.");
                        _crackingHandler.AbortCracking();
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
    }
}
