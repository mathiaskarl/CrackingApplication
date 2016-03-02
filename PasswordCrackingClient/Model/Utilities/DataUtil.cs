using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PasswordCrackingClient.Model.Utilities
{
    public class DataUtil
    {
        public static string SerializeList<T>(List<T> list)
        {
            return JsonConvert.SerializeObject(list);
        }

        public static List<T> DeserializeList<T>(string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public static string CombineDataPacket(string command, string data)
        {
            return String.Join("<>", command, data);
        }

        public static string Capitalize(string data)
        {
            if(String.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException();
            return data.Substring(0, 1).ToUpper() + data.Substring(1);
        }

        public static string Reverse(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException();

            string result = "";
            for (int i = 0; i < data.Length; i++)
                result += data.ElementAt(data.Length - 1 - i);
            return result;
        }
        private static readonly Converter<char, byte> Converter = CharToByte;

        public static Converter<char, byte> GetConverter()
        {
            return Converter;
        }

        /// <summary>
        /// Converting a char to a byte can be done in many ways.
        /// This is one way ...
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static byte CharToByte(char ch)
        {
            return Convert.ToByte(ch);
        }

        public static string CapitalizeLastLetter(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Trim().Length == 0)
            {
                return str;
            }
            string lastLetterUppercase = str.Substring(str.Length - 1, 1).ToUpper();
            string theRest = str.Substring(0, str.Length - 1);
            return theRest + lastLetterUppercase;
        }
    }
}
