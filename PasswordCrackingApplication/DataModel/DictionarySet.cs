using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingApplication.DataModel.Interfaces;

namespace PasswordCrackingApplication.DataModel
{
    [DataContract]
    public class DictionarySet : IDataSet
    {
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public string Keyword { get; set; }
        [DataMember]
        public byte[] KeywordArray { get; set; }


        public DictionarySet(string keyword)
        {
            Initialize(keyword, 0);
        }

        public DictionarySet(string keyword, int index)
        {
            Initialize(keyword, index);
            this.Index = index;
        }

        public DictionarySet() { }

        public void Initialize(string keyword, int index)
        {
            if (String.IsNullOrWhiteSpace(keyword))
                throw new ArgumentNullException();

            this.Keyword = keyword;
            this.Index = index;
        }
    }
}
