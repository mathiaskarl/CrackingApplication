using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingClient.Network;

namespace PasswordCrackingClient.DataModel
{
    [DataContract]
    public class DictionarySetGroup
    {
        [DataMember]
        public string Identifier { get; set; }
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public DateTime TimeCreated { get; set; }
        [DataMember]
        public List<DictionarySet> DictionarySets { get; set; }
        [DataMember]
        public List<UserAccountSet> CrackingResults { get; set; } = new List<UserAccountSet>();

        public DictionarySetGroup(List<DictionarySet> dictionarySets, bool isChecked = false)
        {
            this.IsChecked = isChecked;
            this.DictionarySets = dictionarySets;
        }

        public DictionarySetGroup() { }
    }
}
