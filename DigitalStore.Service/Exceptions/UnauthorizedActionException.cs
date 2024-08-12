using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Service.Exceptions
{
    public class UnauthorizedActionException : Exception
    {
        public UnauthorizedActionException() : base("You are not authorized to perform this operation X")
        {
        }

        public UnauthorizedActionException(string message) : base(message)
        {
        }
    }
}
