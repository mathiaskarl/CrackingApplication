using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingApplication.DataModel;
using PasswordCrackingApplication.DataModel.Interfaces;

namespace PasswordCrackingApplication.Model
{
    public class FileHandler
    {
        public static List<T> FetchSets<T>(string filepath) where T : class, IDataSet, new()
        {
            if (String.IsNullOrWhiteSpace(filepath))
                throw new ArgumentException();

            if (!File.Exists(filepath))
                throw new ArgumentException("INVALID_FILE_PATH");

            List<T> dataSet = new List<T>();

            FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                int index = 0;
                while (!streamReader.EndOfStream)
                {
                    string data = streamReader.ReadLine();
                    if (String.IsNullOrWhiteSpace(data))
                        continue;

                    var obj = new T();
                    obj.Initialize(data, index);
                    dataSet.Add(obj);
                    index++;
                }

                if (dataSet.Count < 1)
                    throw new Exception("NO_VALID_DICTIONARY_SETS");

                return dataSet;
            }
        }
    }
}
