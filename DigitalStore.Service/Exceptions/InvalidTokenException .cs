using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Service.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("Token is not correct X")
        {
        }

        public InvalidTokenException(string message) : base(message)
        {
        }
    }
}
