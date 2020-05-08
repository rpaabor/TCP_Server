using System;
using System.Net;
using System.Net.Sockets;

namespace ClientDll
{
    public struct ClientSettings
    {
        private Socket _clientSocket;
        private int _buffferSize;
        private int _port;
        private byte[] _buffer;
        public bool Closing { get; set; }
        private IPEndPoint remoteEP;


        public ClientSettings(int? buffferSize, int? port, IPAddress ipAddress = null)
        {
            _buffferSize = buffferSize == null ? 2048 : (int)buffferSize;
            _port = port == null ? 100 : (int)port;
            _buffer = new byte[_buffferSize];
            Closing = false;
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipsAddress = ipAddress == null ? ipHostInfo.AddressList[1] : ipAddress;
            remoteEP = new IPEndPoint(ipsAddress, _port);
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public byte[] resived(int lenght)
        {
            byte[] result = new byte[lenght];
            Array.Copy(_buffer, result, lenght);
            return result;
        }
        public int BufferSize { get { return this._buffferSize; } }
        public byte[] Buffer { get { return this._buffer; } }
        public Socket ClientSocet { get { return this._clientSocket; } }
        public IPEndPoint EndPoint { get { return this.remoteEP; } }
    }
}
