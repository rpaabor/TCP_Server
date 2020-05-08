using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server
{
    public class Server
    {
        public event ServerHandlerEventArgs.NewMessage NewMessage;
        public event NewClientConnectedEventArgs.ClientConnect NewClientConnect;
        public event ClientLoginAttemtEventArgs.LoginAtempt LoginAtempt;
        private ServerSettings serverSettings;
        private Socket _socket;
        public int buffer => serverSettings.BufferSize;
        public string ipadress => serverSettings.ipadress;
        public int port => serverSettings.port;

        public string[] GetconnectedCLients()
        {
            UppdateClientList();
            return serverSettings.ConnClients.Select(x => x.clientGuid.ToString()).ToArray();
        }

        private void UppdateClientList()
        {
            serverSettings.OverideConnectedClients(serverSettings.ConnClients.Where(c => IsConnected(c.clientsocet)).ToList());
        }

        private bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public Server(int? buffferSize, int? port, IPAddress ipAddress = null)
        {
            serverSettings = new ServerSettings(buffferSize, port, ipAddress);
            _socket = serverSettings.ServerSocet;
        }
        public void Start()
        {            
            _socket.Listen(10);
            _socket.BeginAccept(AcceptCallback, null);
        }
        public void Stop()
        {
            UppdateClientList();
            foreach (var conectedClient in serverSettings.ConnClients)
            {
                conectedClient.clientsocet.Shutdown(SocketShutdown.Both);
                conectedClient.clientsocet.Close();
            }
            _socket.Close();
        }

        public void UserWhishToLogIn(Guid userGuid)
        {
            serverSettings.LoginAtempt(userGuid);
        }

        public void LoginSucsess(Guid userGuid)
        {
            var user = serverSettings.LoginSucsess(userGuid);
            SendData(user.clientsocet, Encoding.UTF8.GetBytes("You are loged in!"));
        }

        public void LogInFail(Guid userGuid)
        {
            serverSettings.LoginFail(userGuid);
        }


        private void AcceptCallback(IAsyncResult ar)
        {
            if (serverSettings.closing)
                return;
            var socket = _socket.EndAccept(ar);
            var client = new ConectedClient(socket);
            serverSettings.AddClient(client);
            //string welcome = string.Format("Welcome to server you are user {0}", client.clientGuid);
            OnClientConnect(new NewClientConnectedEventArgs(client.clientGuid));
            //SendData(socket, Encoding.UTF8.GetBytes(welcome));
            BeginRecive(socket);
            Console.WriteLine("Client connected, waiting for request...");
            _socket.BeginAccept(AcceptCallback, null);
        }

        private void BeginRecive(Socket socket)
        {
            socket.BeginReceive(serverSettings.Buffer, 0, serverSettings.BufferSize, SocketFlags.None, ReceiveCallback, socket);
        }

        public void SendMessageToSpecClient(Guid[] clientGuids, byte[] messge)
        {
            UppdateClientList();
            var clients = serverSettings.ConnClients.Where(c => clientGuids.Contains(c.clientGuid));
            foreach (var conectedClient in clients)
            {
                Task.Run((() => SendData(conectedClient.clientsocet, messge)));
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (serverSettings.closing)
                return;

            var current = (Socket)ar.AsyncState;
            var cur = serverSettings.GetClient(current);
            int received;

            try
            {
                received = current.EndReceive(ar);
                if (received == 0)
                    return;
                var bytes = serverSettings.resived(received);
                if (cur.LoginAtempt)
                {
                    string userName = "";
                    string password = "";
                    try
                    {
                        var namePassword = Encoding.UTF8.GetString(bytes).Split(',');
                        userName = namePassword[0];
                        password = namePassword[1];
                    }
                    catch (Exception e)
                    {
                        LogInFail(cur.clientGuid);
                        SendData(current, Encoding.UTF8.GetBytes("Login string wrong format, expecting, [username],[password]"));
                        BeginRecive(current);
                        return;
                    }
                    OnLoginatempt(new ClientLoginAttemtEventArgs(cur.clientGuid, userName ,password ));
                    BeginRecive(current);
                    return;
                }

                OnNewMessage(new ServerHandlerEventArgs(bytes, cur.clientGuid));
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                current.Close();
                serverSettings.RemoveClient(cur);
                return;
            }
            BeginRecive(current);
        }

        public void sendToSpesificClient(string clientGuid, string message)
        {
            UppdateClientList();
            var sock = serverSettings.ConnClients.First(c => c.clientGuid.ToString() == clientGuid);
            SendData(sock.clientsocet, Encoding.UTF8.GetBytes(message));
        }
        public void sendToAllExept(string clientToExept, string message)
        {
            UppdateClientList();
            var clients = serverSettings.ConnClients.Where(c => c.clientGuid.ToString() != clientToExept);
            foreach (var client in clients)
            {
                sendToSpesificClient(client.clientGuid.ToString(), message);
            }
        }

        private void SendData(Socket s, byte[] message)
        {
            s.BeginSend(message, 0, message.Length, SocketFlags.None, SendCallback, s);
            _socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            s.EndReceive(ar);
        }

        private void Broadcast(byte[] message)
        {
            UppdateClientList();
            foreach (var clientSocket in serverSettings.ConnClients)
            {
                SendData(clientSocket.clientsocet, message);
            }
        }
        public void BroadcastMessage(byte[] message)
        {
            Task.Run((() => Broadcast(message)));
        }

        private void Server_NewMessage(object sender, ServerHandlerEventArgs e)
        {
            IncommingMessage(e.message, e.ClientGuid);
        }

        internal void IncommingMessage(byte[] message, Guid clientGuid)
        {
            OnNewMessage(new ServerHandlerEventArgs(message, clientGuid));
        }

        internal void OnNewMessage(ServerHandlerEventArgs e)
        {
            if (NewMessage != null)
                NewMessage.Invoke(this, e);
        }

        private void Server_NewClientConnect(object sender, NewClientConnectedEventArgs e)
        {
            IncommingConnection(e.ClientGuid);
        }

        internal void IncommingConnection(Guid clientGuid)
        {
            OnClientConnect(new NewClientConnectedEventArgs(clientGuid));
        }
        internal void OnClientConnect(NewClientConnectedEventArgs e)
        {
            if(NewClientConnect != null)
                NewClientConnect.Invoke(this,e);
        }


        internal void OnLoginAtempt(Guid clientGuid, string user, string password)
        {
            OnLoginatempt(new ClientLoginAttemtEventArgs(clientGuid, user, password));
        }

        internal void OnLoginatempt(ClientLoginAttemtEventArgs e)
        {
            if(LoginAtempt != null)
                LoginAtempt.Invoke(this,e);
        }

    }
}

