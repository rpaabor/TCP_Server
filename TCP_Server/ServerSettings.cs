using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace TCP_Server
{
    internal struct ServerSettings
    {
        private List<ConectedClient> _conectedClients;
        private int _buffferSize;
        private int _port;
        private IPEndPoint _ipEndPoint;
        private byte[] _buffer;
        public int port => _port;
        public string ipadress => _ipEndPoint.ToString();
        public bool closing { get; set; }
        private Socket _serverSocet;
        public ServerSettings(int? buffferSize, int? port, IPAddress ipAddress = null)
        {
            _conectedClients = new List<ConectedClient>();
            _buffferSize = buffferSize == null ? 4096 : (int)buffferSize;
            _port = port == null ? 100 : (int)port;

            if (ipAddress == null)
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipnAddress = ipHostInfo.AddressList[1];
                _ipEndPoint = new IPEndPoint(ipnAddress, _port);
            }
            else
            {
                _ipEndPoint = new IPEndPoint(ipAddress, _port);
            }
            _serverSocet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocet.Bind(_ipEndPoint);
            _buffer = new byte[_buffferSize];
            closing = false;
            Console.WriteLine("server setup complete " + _ipEndPoint.Address + ":" + _ipEndPoint.Port);
        }

        public ConectedClient GetClient(Socket client)
        {
            return _conectedClients.First(x => x.clientsocet == client);
        }

        public void AddClient(ConectedClient socket)
        {
            _conectedClients.Add(socket);
        }

        public void LoginAtempt(Guid userGuid)
        {
            var user = _conectedClients.First(c => c.clientGuid == userGuid);
            user.ClientLogInAtempt();
            updateClient(user);
        }

        public ConectedClient LoginSucsess(Guid userGuid)
        {
            var user = _conectedClients.First(c => c.clientGuid == userGuid);
            user.LoginSucsess();
            updateClient(user);
            return user;
        }

        public void LoginFail(Guid userGuid)
        {
            var user = _conectedClients.First(c => c.clientGuid == userGuid);
            user.ClientFinishLoginAtempt();
            updateClient(user);
        }

        public void RemoveClient(ConectedClient client)
        {
            _conectedClients.Remove(client);
        }
        public byte[] resived(int lenght)
        {
            byte[] result = new byte[lenght];
            Array.Copy(_buffer, result, lenght);
            return result;
        }

        internal void updateClient(ConectedClient client)
        {
            var oldClient = _conectedClients.First(c => c.clientGuid == client.clientGuid);
            var newClient = new ConectedClient().CopyClient(oldClient);
            _conectedClients.Remove(oldClient);
            _conectedClients.Add(newClient);
        }
        public void OverideConnectedClients(List<ConectedClient> conectedClients)
        {
            _conectedClients = conectedClients;
        }

        public List<ConectedClient> ConnClients { get { return this._conectedClients; } }
        public int BufferSize { get { return this._buffferSize; } }
        public byte[] Buffer { get { return this._buffer; } }
        public IPEndPoint ipEndPoint { get { return this._ipEndPoint; } }
        public Socket ServerSocet { get { return this._serverSocet; } }

    }

    public class ConectedClient
    {
        public Socket clientsocet { get; private set; }
        public Guid clientGuid { get; private set; }
        public bool isValidated { get; set; }
        public bool brodcastNextMessage { get; set; }
        public bool LoginAtempt { get; set; }
        
        public ConectedClient CopyClient(ConectedClient oldCLient)
        {
            var newClient = new ConectedClient()
            {
                clientsocet = oldCLient.clientsocet,
                clientGuid = oldCLient.clientGuid,
                isValidated = oldCLient.isValidated,
                brodcastNextMessage = oldCLient.brodcastNextMessage,
                LoginAtempt = oldCLient.LoginAtempt
            };
            return newClient;
        }
        public ConectedClient() { }
        public ConectedClient(Socket socket)
        {
            clientsocet = socket;
            clientGuid = Guid.NewGuid();
            isValidated = false;
            brodcastNextMessage = false;
            LoginAtempt = false;
        }

        public void ClientLogInAtempt()
        {
            LoginAtempt = true;
        }

        public void LoginSucsess()
        {
            isValidated = true;
            LoginAtempt = false;
        }

        public void ClientFinishLoginAtempt()
        {
            LoginAtempt = false;
        }

    }



}

