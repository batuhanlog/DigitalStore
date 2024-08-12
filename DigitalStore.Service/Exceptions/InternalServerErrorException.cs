using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Service.Exceptions
{
    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException() : base("We have encountered an unexpected situation. please check the information you have entered!")
        {
        }

        public InternalServerErrorException(string message) : base(message)
        {
        }

        public InternalServerErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    
    }
}
