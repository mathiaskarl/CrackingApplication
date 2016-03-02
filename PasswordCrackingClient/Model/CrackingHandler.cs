using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PasswordCrackingClient.DataModel;
using PasswordCrackingClient.DataModel.EventArgs;
using PasswordCrackingClient.Model.Utilities;

namespace PasswordCrackingClient.Model
{
    public class CrackingHandler : ProgressStateUpdated
    {
        private readonly List<string> _specialCharAndMoreList = new List<string>();
        public List<UserAccountSet> UserAccountSets { get; set; }                           = new List<UserAccountSet>();
        public DictionarySetGroup DictionarySetGroup { get; set; }                          = new DictionarySetGroup();

        public bool IsRunning = false;
        public int CurrentCrackingIndex = 0;

        private readonly HashAlgorithm _messageDigest = new SHA1CryptoServiceProvider();

        public CrackingHandler()
        {
            var list = new[] { "!", "#", "¤", "%", "&", "/", "(", ")", "=", "?", "´", "`", "|", "<", ">", "½", "@", "£", "$", "{", "[", "]", "}", "*", "-", "+", "_", ";", ":", ",", ".", "\\", " ", "ë", "é", "è", "ä", "ü", "á", "à", "ñ", "ö", "å", "æ", "ø", "ï", "í", "ì", "ÿ", "ý", "ó", "ò" };
            foreach (string s in list)
            {
                _specialCharAndMoreList.Add(s);
            }

            for (int i = 0; i < 100; i++)
            {
                _specialCharAndMoreList.Add(i.ToString());
            }

            for (int i = 1900; i < DateTime.Now.Year; i++)
            {
                _specialCharAndMoreList.Add(i.ToString());
            }
        }

        public void AddDictionarySetGroup(DictionarySetGroup setGroup)
        {
            if(setGroup.IsChecked)
                throw new Exception("DICTIONARY_SET_GROUP_ALREADY_CHECKED");

            if(setGroup.DictionarySets.Count < 1)
                throw new Exception("INVALID_DICTIONARY_SET_GROUP");

            this.DictionarySetGroup = setGroup;
        }

        public void AddUserAccountSets(List<UserAccountSet> userAccountSets)
        {
            if(userAccountSets.Count < 1)
                throw new Exception("INVALID_USERACCOUNT_SET");

            UserAccountSets = userAccountSets;
        }

        public int CrackingTest(List<DictionarySet> testSet)
        {
            if (testSet.Count < 1)
                throw new Exception("INVALID_TEST_SETS");

            Stopwatch stopWatch = Stopwatch.StartNew();
            foreach (var obj in testSet)
                RunCrackingIterations(obj.Keyword, true);

            stopWatch.Stop();
            return stopWatch.Elapsed.Seconds;
        }

        public void StartCracking()
        {
            this.IsRunning = true;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Console.WriteLine("\nStarted cracking\n- Keywords from: " + DictionarySetGroup.DictionarySets.First().Keyword.ToUpper()[0] + "-" + DictionarySetGroup.DictionarySets.Last().Keyword.ToUpper()[0]);
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    for (int i = 0; i < DictionarySetGroup.DictionarySets.Count; i++)
                    {
                        var obj = DictionarySetGroup.DictionarySets[i];
                        if (!IsRunning)
                            throw new Exception("ABORTED_CRACKING");

                        this.CurrentCrackingIndex = i;
                        decimal temp = DictionarySetGroup.DictionarySets.Count/10;
                        int roundedCount = (int)Math.Round(temp)*10;
                        if (i % (roundedCount / 10) == 0)
                            Console.WriteLine("Checked: " + i + " - " + (int)((i * 100) / (double)DictionarySetGroup.DictionarySets.Count) + "%");
                        RunCrackingIterations(obj.Keyword);
                    }
                    stopwatch.Stop();
                    Console.WriteLine("\nCompleted dictionary set\n- Passwords found: {1} out of {0} total.", UserAccountSets.Count, this.DictionarySetGroup.CrackingResults.Count);
                    Console.WriteLine("- Time elapsed: {0}", stopwatch.Elapsed);

                    RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(new DataPacket(ProgressState.CompletedDictionarySet, JsonConvert.SerializeObject(this.DictionarySetGroup))));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            });
        }

        private void RunCrackingIterations(string keyword, bool testMode = false)
        {
            InitiateCompare(keyword, testMode);
            InitiateCompare(keyword + keyword, testMode);

            InitiateCompare(keyword.ToUpper(), testMode);

            InitiateCompare(DataUtil.Capitalize(keyword), testMode);

            InitiateCompare(DataUtil.Reverse(keyword), testMode);

            CheckKeywordForEachCharUppercase(keyword, testMode);
            CheckKeywordForEachCharWithList(keyword, testMode);
            CheckNumberInFrontAndAfterKeyword(keyword, testMode);
            CheckKeywordCapitalizedFirstLastBoth(keyword, testMode);

        }

        private void CheckKeywordCapitalizedFirstLastBoth(string dictionaryEntry, bool testMode = false)
        {
            string possiblePasswordCapitalized = DataUtil.Capitalize(dictionaryEntry);
            InitiateCompare(possiblePasswordCapitalized, testMode);

            string possiblePasswordLastLetterCapitalized = DataUtil.CapitalizeLastLetter(dictionaryEntry);
            InitiateCompare(possiblePasswordLastLetterCapitalized, testMode);

            string possiblePasswordFirstAndLastCap = DataUtil.CapitalizeLastLetter(possiblePasswordCapitalized);
            InitiateCompare(possiblePasswordFirstAndLastCap, testMode);
        }

        private void CheckNumberInFrontAndAfterKeyword(string dictionaryEntry, bool testMode = false)
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    string possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    InitiateCompare(possiblePasswordStartEndDigit, testMode);
                }
            }

            for (int i = 1900; i < DateTime.Now.Year; i++)
            {
                for (int j = 1900; j < DateTime.Now.Year; j++)
                {
                    String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    InitiateCompare(possiblePasswordStartEndDigit, testMode);
                }
            }
        }

        private void CheckKeywordForEachCharWithList(string dictionaryEntry, bool testMode = false)
        {
            foreach (string s in _specialCharAndMoreList)
            {
                for (int i = 0; i < dictionaryEntry.Length + 1; i++)
                {
                    string a = dictionaryEntry.Substring(0, i);
                    string b = dictionaryEntry.Substring(i, dictionaryEntry.Length - a.Length);
                    string possiblePassword = a + s + b;
                    InitiateCompare(possiblePassword, testMode);
                }
            }
        }

        private void CheckKeywordForEachCharUppercase(string dictionaryEntry, bool testMode = false)
        {
            for (int i = 0; i < dictionaryEntry.Length; i++)
            {
                string a;
                string b;
                string c;

                if (i == 0)
                {
                    a = "";
                }
                else
                {
                    a = dictionaryEntry.Substring(0, i);
                }
                b = dictionaryEntry.Substring(i, 1).ToUpper();
                if ((i + 1 <= dictionaryEntry.Length))
                {
                    c = dictionaryEntry.Substring(i + 1, dictionaryEntry.Length - a.Length - b.Length);
                }
                else
                {
                    c = "";
                }
                InitiateCompare(a + b + c, testMode);
            }
        }

        private void InitiateCompare(string potentialPassword, bool testMode = false)
        {
            byte[] passwordAsBytes = Array.ConvertAll(potentialPassword.ToCharArray(), DataUtil.GetConverter());
            byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);

            foreach (UserAccountSet obj in UserAccountSets)
            {
                if (CompareBytes(obj.EncryptedPasswordArray, encryptedPassword))
                {
                    if (testMode)
                        break;

                    obj.DecryptedPassword = potentialPassword;
                    this.DictionarySetGroup.CrackingResults.Add(obj);
                    RaiseEvent(OnStateUpdatedEventHandler, new StateEventArgs(new DataPacket(ProgressState.PasswordFound, JsonConvert.SerializeObject(obj))));
                }
            }
        }

        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            if (firstArray.Count != secondArray.Count)
                return false;

            for (int i = 0; i < firstArray.Count; i++)
                if (firstArray[i] != secondArray[i])
                    return false;
            return true;
        }

        public void AbortCracking()
        {
            if (this.IsRunning)
                this.IsRunning = false;
        }
    }
}
