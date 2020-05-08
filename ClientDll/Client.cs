using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ClientDll
{
    public class Client : IDisposable
    {
        public event CleintHandlerEventArgs.NewMessage NewMessage;
        private ClientSettings settings;
        private Socket _clientSocket;
        public bool connected = false;
        private int connectionAttempt = 1;
        public Client(int? buffferSize, int? port, IPAddress ipAddress = null)
        {
            settings = new ClientSettings(buffferSize, port, ipAddress);
            _clientSocket = settings.ClientSocet;

        }

        public void Disconnect()
        {
            _clientSocket.Disconnect(settings.Closing);
        }

        public void Send(string message)
        {
            Task.Run((() => SendData(message)));
        }

        public void StartClient()
        {
            var str = string.Format("Setting up client, Conecting to server {0}", settings.EndPoint.ToString());
            Console.WriteLine(str);
            TryToConnect();
        }

        public void TryToConnect()
        {
            {
                try
                {
                    if (connectionAttempt > 1)
                        Console.WriteLine("Connection attempt : {0}", connectionAttempt);
                    _clientSocket.BeginConnect(settings.EndPoint, new AsyncCallback(ConnectCallback), _clientSocket);

                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                Receive(client);
                Console.WriteLine("Client conection complete!");
                Console.Title = string.Format("Client => sever : {0}", settings.EndPoint);
                connected = true;
            }
            catch (SocketException socketException) when (socketException.SocketErrorCode == SocketError.ConnectionRefused)
            {
                connectionAttempt++;
                Console.WriteLine("No connection to remote server connection refused or server offline");
                Thread.Sleep(3000);
                Console.WriteLine("Reconnecting...");
                TryToConnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                _clientSocket.BeginReceive(settings.Buffer, 0, settings.BufferSize, SocketFlags.None, ReceiveCallback, _clientSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            s.EndReceive(ar);
        }
        private byte[] SendEncodeString(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (settings.Closing)
                return;

            var current = (Socket)ar.AsyncState;
            int received;
            try
            {
                received = current.EndReceive(ar);
                if (received == 0)
                    return;
                var bytes = settings.resived(received);
                OnNewMessage(new CleintHandlerEventArgs(bytes));
            }
            catch (SocketException)
            {
                Console.WriteLine("Server forcefully disconnected");
                current.Close();
                return;
            }
            Receive(current);
        }

        private void SendData(string message)
        {
            byte[] data = SendEncodeString(message);
            _clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, _clientSocket);
        }

        internal void IncommingMessage(byte[] message)
        {
            OnNewMessage(new CleintHandlerEventArgs(message));
        }

        internal void OnNewMessage(CleintHandlerEventArgs e)
        {
            if (NewMessage != null)
                NewMessage.Invoke(this, e);
        }
        public void Dispose()
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
