using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BlockChainName;
using ClientDll;
using Newtonsoft.Json;

namespace Client3
{
    class Program
    {
        private static BlockChain  blockChain;
        private static Client client;
        private static bool GotChainFromServer = false;

        static void Main(string[] args)
        {

            int port;
            IPAddress serveradress;

            Console.WriteLine("Set server port if nothing is selected default will be port:100 enter to continue.");
            Console.WriteLine("Set server port:");


            
            Int32.TryParse(Console.ReadLine(), out port);
            Console.WriteLine("Set server ip adress, default is local adress press enter to continue.");
            Console.WriteLine("Set ipadress:");

            IPAddress.TryParse(Console.ReadLine(), out serveradress);


            try
            {
                




                client = new Client(null,port == 0 ? 100 : port, serveradress);
                client.StartClient();
                client.NewMessage += client_newmessage;
                
                Console.WriteLine("Client started!");
                Console.Title = "Client";
                bool run = true;
                
                while (run)
                {
                    if (client.connected)
                    {
                        Console.WriteLine("Send message to server");
                        var text = Console.ReadLine();
                        if (text.ToLower() == "exit")
                        {
                            run = false;
                            break;
                        }
                        if(text.ToLower() == "check")
                        {
                            foreach (var block in blockChain.Chain)
                            {
                                Console.WriteLine(block.Data);
                            }
                        }
                        else
                        {
                            SendNewBlock(text);
                        }
                        
                    }
                    else
                    {
                        Console.ReadKey();
                    }
                }
                client.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.ReadKey();

        }

        public static void SendNewBlock(string content)
        {
            var block = new Block(DateTime.Now, null, content);
            blockChain.AddBlock(block);
            var obj = JsonConvert.SerializeObject(block);
            client.Send(obj);
        }

        private static void client_newmessage(object sender, CleintHandlerEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.message);
            var obj = JsonConvert.DeserializeObject<Block>(message);
            if (blockChain == null)
                blockChain = new BlockChain(obj);
            else
            {
                blockChain.AddBlock(obj);
            }
            Console.WriteLine($"Is Chain Valid: {blockChain.IsValid()}");
            
        }
    }
}
