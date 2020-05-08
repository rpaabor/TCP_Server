using BlockChainName;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP_Server
{
    class Program
    {
        public static BlockChain blockChain;
        public static Server server;

        static void Main(string[] args)
        {
            
            int port;
            int buffer;

            Console.WriteLine("Set server port if nothing is selected default will be port:100 enter to continue");
            Console.WriteLine("Set server port:");
            
            Int32.TryParse(Console.ReadLine(), out port);
            Console.WriteLine("Set server buffer size default is 4096 enter to continue");
            Console.WriteLine("Set buffer:");
            
            Int32.TryParse(Console.ReadLine(), out buffer);

            
            


            server = new Server(buffer == 0 ? 4096 : buffer,port == 0 ? 100 : port);
            server.Start();

            Console.WriteLine($"Server started use port:{server.port} and a buffer of {server.buffer}");

            blockChain = new BlockChain();
            //blockChain.InitializeChain();
            Console.WriteLine("Genesis block created!");
            Console.WriteLine("Chain is ready to be filld");
            server.NewMessage += Server_NewMessage;
            server.NewClientConnect += Server_NewClient;
            server.LoginAtempt += Server_LoginAtempt;



            Console.Title = string.Format("Server:{0}", server.ipadress);

            bool run = true;
            ////Vänta här!
            while (run)
            {
                Console.WriteLine("Send message to clients");
                var text = Console.ReadLine();
                if (text.ToLower() == "exit")
                {
                    run = false;
                    break;
                }
                server.BroadcastMessage(EencodeMessage(text));
            }
        }

        public static byte[] EencodeMessage(string content)
        {
            var block = new Block(DateTime.Now, null, content);
            blockChain.AddBlock(block);
            var obj = JsonConvert.SerializeObject(block);
            return Encoding.UTF8.GetBytes(obj);
        }


        private static void Server_LoginAtempt(object sender, ClientLoginAttemtEventArgs e)
        {
            Console.WriteLine("User login atempt");
            Console.WriteLine(e.ClientGuid);
            Console.WriteLine(e.UserName);
            Console.WriteLine(e.Password);
            server.LoginSucsess(e.ClientGuid);
        }

        private static void Server_NewClient(object sender, NewClientConnectedEventArgs e)
        {
            var clientGuid = e.ClientGuid;
            foreach (var block in blockChain.Chain)
            {
                var obj = JsonConvert.SerializeObject(block);
                server.sendToSpesificClient(clientGuid.ToString(), obj);
                Thread.Sleep(100);
            }
            Console.WriteLine("Block chain sent to client");
        }

        private static void Server_NewMessage(object sender, ServerHandlerEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.message);
            var obj = JsonConvert.DeserializeObject<Block>(message);
            server.sendToAllExept(e.ClientGuid.ToString(), message);
            blockChain.AddBlock(obj);
            //robbCoin.AddBlock(new Block(DateTime.Now, null, "{sender:Robban,reciver:Linus,amount:100"));
        }
    }
}
