using System;

namespace TCP_Server
{
    public class ServerHandlerEventArgs : EventArgs
    {
        public byte[] message;
        public Guid ClientGuid;

        public ServerHandlerEventArgs(byte[] message, Guid clientGuid)
        {
            this.message = message;
            this.ClientGuid = clientGuid;
        }

        public delegate void NewMessage(object sender, ServerHandlerEventArgs e);
    }

    public class NewClientConnectedEventArgs : EventArgs
    {
        public Guid ClientGuid;

        public NewClientConnectedEventArgs(Guid ClientGuid)
        {
            this.ClientGuid = ClientGuid;
        }

        public delegate void ClientConnect(object sender, NewClientConnectedEventArgs e);
    }

    public class ClientLoginAttemtEventArgs : EventArgs
    {
        public Guid ClientGuid;
        public string UserName;
        public string Password;

        public ClientLoginAttemtEventArgs(Guid clientGuid, string UserName, string Password)
        {
            this.ClientGuid = clientGuid;
            this.UserName = UserName;
            this.Password = Password;
        }

        public delegate void LoginAtempt(object sender, ClientLoginAttemtEventArgs e);

    }
}