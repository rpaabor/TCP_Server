using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainName
{
    public class NotValidException : Exception
    {
        public NotValidException()
        {

        }

        public NotValidException(string message) : base(message)
        {

        }

        public NotValidException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
