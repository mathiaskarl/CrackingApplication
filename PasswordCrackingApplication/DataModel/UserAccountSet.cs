using System;
using System.Runtime.Serialization;
using PasswordCrackingApplication.DataModel.Interfaces;

namespace PasswordCrackingApplication.DataModel
{
    [DataContract]
    public class UserAccountSet : IDataSet
    {
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string DecryptedPassword { get; set; }
        [DataMember]
        public string EncryptedPassword { get; set; }
        [DataMember]
        public byte[] EncryptedPasswordArray { get; set; }
        
        public UserAccountSet(string username, string encryptedPassword)
        {
            Initialize(username, encryptedPassword);
        }

        public UserAccountSet() { }

        private void Initialize(string username, string encryptedPassword)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(encryptedPassword))
                throw new ArgumentNullException();

            this.Username = username;
            this.EncryptedPassword = encryptedPassword;
            this.EncryptedPasswordArray = Convert.FromBase64String(encryptedPassword);
        }

        public void Initialize(string data, int index)
        {
            string[] dataParts = data.Split(":".ToCharArray());
            Initialize(dataParts[0], dataParts[1]);
        }

        public override string ToString()
        {
            return Username + ":" + EncryptedPassword;
        }
    }
}
