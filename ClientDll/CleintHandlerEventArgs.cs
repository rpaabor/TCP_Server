using System;

namespace ClientDll
{
    public class CleintHandlerEventArgs : EventArgs
    {
        public byte[] message;
        public CleintHandlerEventArgs(byte[] message)
        {
            this.message = message;
        }
        public delegate void NewMessage(object sender, CleintHandlerEventArgs e);

    }
}
