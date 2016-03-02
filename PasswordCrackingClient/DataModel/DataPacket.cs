using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackingClient.DataModel.EventArgs;
using PasswordCrackingClient.Model.Utilities;

namespace PasswordCrackingClient.DataModel
{
    public class DataPacket
    {
        public string RawData { get; }
        public string Data { get { return RawData.Split(new string[] { "<>" }, StringSplitOptions.None).Count() > 1 ? String.Join("", RawData.Split(new string[] { "<>" }, StringSplitOptions.None).Where((x, index) => index != 0).ToArray()) : null; } }

        public string RawState { get { return RawData.Split(new string[] { "<>" }, StringSplitOptions.None).Any() ? RawData.Split(new string[] { "<>" }, StringSplitOptions.None)[0] : RawData; } }
        public ProgressState State { get { return StateHandler.FetchClientState(this.RawState); } }

        public DataPacket(string data)
        {
            if(String.IsNullOrWhiteSpace(data))
                throw new Exception("EMPTY_DATA_STRING");

            this.RawData = data;
        }

        public DataPacket(ProgressState state, string data)
        {
            this.RawData = DataUtil.CombineDataPacket(StateHandler.FetchClientStateString(state), data);
        }

        public DataPacket(ProgressState state)
        {
            this.RawData = StateHandler.FetchClientStateString(state);
        }
    }
}
