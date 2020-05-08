using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainName
{

    public class Block
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public string Data { get; set; }

        [JsonConstructor]
        private Block() { }

        public Block(DateTime timeStamp, string previousHash, string data)
        {
            Index = 0;
            TimeStamp = timeStamp;
            PreviousHash = previousHash;
            Data = data;
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] inputbytes = Encoding.UTF8.GetBytes($"{TimeStamp}-{PreviousHash ?? ""}-{Data}");
            byte[] outBytes = sha256.ComputeHash(inputbytes);
            return Convert.ToBase64String(outBytes);
        }
    }

}
