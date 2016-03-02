using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingApplication.Network;

namespace PasswordCrackingApplication.DataModel
{
    [DataContract]
    public class DictionarySetGroup
    {
        [DataMember]
        public string Identifier { get; private set; }
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

            if (String.IsNullOrWhiteSpace(this.Identifier))
                this.Identifier = Guid.NewGuid().ToString();

            if (this.TimeCreated == default(DateTime))
                this.TimeCreated = DateTime.Now;
        }
    }
}
