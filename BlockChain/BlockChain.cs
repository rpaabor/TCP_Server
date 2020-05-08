using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace BlockChainName
{
    public class BlockChain
    {
        public IList<Block> Chain { get; set; }

        [JsonConstructor]
        private BlockChain(string Bogus = "")
        {
            InitializeChain();
        }
        public BlockChain(Block GenesisBlock)
        {
            InitializeChain();
            Chain.Add(GenesisBlock);
        }

        public BlockChain()
        {
            InitializeChain();
            AddGenesisBlock();
        }

        public void InitializeChain()
        {
            Chain = new List<Block>();
        }

        public Block CreateGenesisBlock()
        {
            return new Block(DateTime.Now, null, "{}");
        }

        public void AddGenesisBlock()
        {
            Chain.Add(CreateGenesisBlock());
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public void AddBlock(Block block)
        {
            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.PreviousHash = latestBlock.Hash;
            block.Hash = block.CalculateHash();
            Chain.Add(block);
            if (!IsValid())
                throw new NotValidException("Chain is not correct");
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];
                if (currentBlock.Hash != currentBlock.CalculateHash())
                    return false;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
            }

            return true;
        }
    }
}
