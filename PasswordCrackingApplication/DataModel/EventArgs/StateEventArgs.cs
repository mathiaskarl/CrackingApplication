using System;
using PasswordCrackingApplication.Network;

namespace PasswordCrackingApplication.DataModel.EventArgs
{
    public enum ProgressState
    {
        Invalid = 0, Connected, Disconnected, CompletedDictionarySet, RequestDictionarySet, RequestUserAccount, RequestClientProgress, ReceivedMessage, TestClientSpeed, ClientInactive, PasswordFound, OverallProgress
    }

    public class StateHandler
    {
        public static ProgressState FetchClientState(string state)
        {
            switch (state.ToUpper())
            {
                case "REQUEST_CLIENT_PROGRESS":
                    return ProgressState.RequestClientProgress;

                case "COMPLETED_DICTIONARY_SET":
                    return ProgressState.CompletedDictionarySet;

                case "REQUEST_DICTIONARY_SET":
                    return ProgressState.RequestDictionarySet;

                case "REQUEST_USER_ACCOUNT":
                    return ProgressState.RequestUserAccount;

                case "RECEIVED_MESSAGE":
                    return ProgressState.ReceivedMessage;

                case "TEST_CLIENT_SPEED":
                    return ProgressState.TestClientSpeed;

                case "CLIENT_INACTIVE":
                    return ProgressState.ClientInactive;

                case "PASSWORD_FOUND":
                    return ProgressState.PasswordFound;

                case "OVERALL_PROGRESS":
                    return ProgressState.OverallProgress;

                default:
                    return ProgressState.Invalid;
            }
        }

        public static string FetchClientStateString(ProgressState state)
        {
            switch (state)
            {
                case ProgressState.RequestClientProgress:
                    return "REQUEST_CLIENT_PROGRESS";

                case ProgressState.CompletedDictionarySet:
                    return "COMPLETED_DICTIONARY_SET";

                case ProgressState.RequestDictionarySet:
                    return "REQUEST_DICTIONARY_SET";

                case ProgressState.RequestUserAccount:
                    return "REQUEST_USER_ACCOUNT";

                case ProgressState.ReceivedMessage:
                    return "RECEIVED_MESSAGE";

                case ProgressState.TestClientSpeed:
                    return "TEST_CLIENT_SPEED";

                case ProgressState.ClientInactive:
                    return "CLIENT_INACTIVE";

                case ProgressState.PasswordFound:
                    return "PASSWORD_FOUND";

                case ProgressState.OverallProgress:
                    return "OVERALL_PROGRESS";

                default:
                    return "STATE_INVALID";
            }
        }
    }

    public class StateEventArgs : System.EventArgs
    {
        public bool InternalStateUpdate { get; set; } = false;
        public ServerClient Client { get; set; }
        public ProgressState State { get; set; }
        public DataPacket DataPacket { get; set; }

        public StateEventArgs(ServerClient client, ProgressState state)
        {
            this.Client = client;
            this.State = state;
        }
        public StateEventArgs(ServerClient client, ProgressState state, bool internalStateUpdate)
        {
            this.Client = client;
            this.State = state;
            this.InternalStateUpdate = internalStateUpdate;
        }

        public StateEventArgs(ServerClient client, DataPacket dataPacket)
        {
            this.Client = client;
            this.DataPacket = dataPacket;
        }
    }

    public class ProgressStateUpdated
    {
        public EventHandler<StateEventArgs> OnStateUpdatedEventHandler;
        public void RaiseEvent<T>(EventHandler<T> eventHandler, T eventArgs) where T : System.EventArgs
        {
            EventHandler<T> handler = eventHandler;
            if (handler != null)
                handler(this, eventArgs);
        }
    }
}
