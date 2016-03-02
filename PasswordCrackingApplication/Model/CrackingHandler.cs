using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PasswordCrackingApplication.DataModel;
using PasswordCrackingApplication.DataModel.EventArgs;
using PasswordCrackingApplication.DataModel.Interfaces;
using PasswordCrackingApplication.Model.Utilities;
using PasswordCrackingApplication.Network;

namespace PasswordCrackingApplication.Model
{
    public class CrackingHandler : ProgressStateUpdated
    {
        public string PasswordFile { get; private set; }
        public string DictionaryFile { get; private set; }
        public string DanishDictionaryFile { get; private set; }
        public List<UserAccountSet> UserAccountSets { get; private set; }                               = new List<UserAccountSet>(); 
        public List<UserAccountSet> UserAccountSetsResult { get; set; }                                 = new List<UserAccountSet>();
        public List<DictionarySet>  DictionarySets { get; private set; }                                = new List<DictionarySet>();
        public List<DictionarySet> DanishDictionarySets { get; private set; }                           = new List<DictionarySet>();
        public List<CrackingClient> CrackingClients { get; private set; }                               = new List<CrackingClient>();

        private const int BaseSetSize = 50;
        private const int AverageTimeToCompleteSet = 60;
        private string[] DictionaryFiles;

        public CrackingHandler(string passwordFile, string[] dictionaryFiles)
        {
            this.PasswordFile = passwordFile;
            this.DictionaryFiles = dictionaryFiles;

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                UserAccountSets = FileHandler.FetchSets<UserAccountSet>(this.PasswordFile);
                var tempDictionarySets = new List<DictionarySet>();
                foreach (var dictionary in DictionaryFiles)
                    tempDictionarySets.AddRange(
                        FileHandler.FetchSets<DictionarySet>(dictionary)
                            .Where(x => !String.IsNullOrWhiteSpace(x.Keyword))
                            .ToList());
                DictionarySets =
                    tempDictionarySets.GroupBy(x => x.Keyword).Select(x => x.First())
                    .OrderBy(x => x.Keyword)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void InitializeCrackingClient(IClient client, int timeToCompleTest)
        {
            int setSize = (AverageTimeToCompleteSet/timeToCompleTest)*BaseSetSize;
            CrackingClients.Add(new CrackingClient(client, setSize));
        }

        public void SaveCrackingResults(IClient client, DictionarySetGroup setGroup)
        {
            var crackingClient = GetCrackingClient(client.ClientIdentifier);

            if(!crackingClient.DictionarySetGroups.Exists(x => x.Identifier == setGroup.Identifier))
                throw new Exception("INVALID_DICTIONARY_SET_GROUP");

            var oldSetGroup = crackingClient.DictionarySetGroups.FirstOrDefault(x => x.Identifier == setGroup.Identifier);
            oldSetGroup.CrackingResults = setGroup.CrackingResults;
            oldSetGroup.IsChecked = true;

            if(oldSetGroup.CrackingResults.Count > 0)
                this.UserAccountSetsResult.AddRange(oldSetGroup.CrackingResults);

            foreach(var obj in oldSetGroup.CrackingResults)
                Trace.Write(obj.Username + ":" + obj.EncryptedPassword + ":" + obj.DecryptedPassword);
        }

        public List<DictionarySet> FetchTestDictionarySets()
        {
            return this.DictionarySets.Take(BaseSetSize).ToList();
        }

        public DictionarySetGroup FetchNextDictionarySetGroup(IClient client)
        {
            if(!this.DictionarySets.Exists(x => x.IsChecked == false))
                throw new Exception("ALL_DICTIONARY_SETS_CHECKED");

            if(IsClientCracking(client))
                throw new Exception("CLIENT_ALREADY_CRACKING");

            var crackingClient = GetCrackingClient(client.ClientIdentifier);
            var setGroup = new DictionarySetGroup(FetchNextDictionarySets(crackingClient.SetSize));
            crackingClient.DictionarySetGroups.Add(setGroup);
            return setGroup;
        }

        private List<DictionarySet> FetchNextDictionarySets(int setSize)
        {
            lock (this.DictionarySets)
            {
                var availableSets = DictionarySets.Where(x => x.IsChecked == false);
                var availableSetsTotal = availableSets.Count();

                setSize = setSize + (setSize / 5) >= availableSetsTotal ? availableSetsTotal : setSize;

                var setGroup = new List<DictionarySet>();
                int currentNumber = 0;
                for (int i = 0; i < DictionarySets.Count; i++)
                {
                    var obj = DictionarySets[i];
                    if (currentNumber >= setSize)
                        break;

                    if (obj.IsChecked == false)
                    {
                        setGroup.Add(new DictionarySet(obj.Keyword, i));
                        obj.IsChecked = true;
                        currentNumber++;
                    }
                }
                return setGroup;
            }
        }

        public int CalculateClientProgress(IClient client, int index)
        {
            var activeClientSet = GetActiveClientSet(client);
            double result = (index*100) / (double)activeClientSet.DictionarySets.Count;
            return Convert.ToInt32(result);
        }

        public void CheckClientsInactivity(int allowedInactivityInSeconds = (AverageTimeToCompleteSet*2))
        {
            while (this.CrackingClients.Count > 0)
            {
                foreach (var client in CrackingClients)
                {
                    if (IsClientCracking(client))
                        if ((DateTime.Now - GetActiveClientSet(client).TimeCreated).TotalSeconds > allowedInactivityInSeconds)
                        {
                            RemoveInactiveClientData(client);
                            RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(client.Client, new DataPacket(ProgressState.ClientInactive)));
                        }
                }
                Thread.Sleep(15000);
            }
            Thread.Sleep(5000);
            CheckClientsInactivity(allowedInactivityInSeconds);
        }

        private bool IsClientCracking(IClient client)
        {
            var crackingClient = GetCrackingClient(client.ClientIdentifier);
            return crackingClient.DictionarySetGroups.Exists(x => x.IsChecked == false);
        }

        public void RemoveInactiveClientData(IClient client)
        {
            if (!IsClientCracking(client))
                return;

            var crackingClient = GetCrackingClient(client.ClientIdentifier);
            var activeCrackingSetGroup = crackingClient.DictionarySetGroups.FirstOrDefault(x => x.IsChecked == false);
            MarkDictionarySets(activeCrackingSetGroup.DictionarySets);
            crackingClient.DictionarySetGroups.Remove(activeCrackingSetGroup);
        }

        private DictionarySetGroup GetActiveClientSet(IClient client)
        {
            return GetCrackingClient(client.ClientIdentifier).DictionarySetGroups.FirstOrDefault(x => x.IsChecked == false);
        }

        private void MarkDictionarySets(List<DictionarySet> dictionarySets, bool isChecked = false)
        {
            lock (this.DictionarySets)
            {
                var query = from firstItem in this.DictionarySets
                            join secondItem in dictionarySets
                            on firstItem.Index equals secondItem.Index
                            select firstItem;
                foreach (var obj in query)
                    obj.IsChecked = isChecked;
            }
        }
            
        public CrackingClient GetCrackingClient(string identifier)
        {
            return this.CrackingClients.FirstOrDefault(x => x.ClientIdentifier == identifier);
        }
    }
}
