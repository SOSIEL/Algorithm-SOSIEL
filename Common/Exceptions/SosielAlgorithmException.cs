using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class SosielAlgorithmException : Exception
    {
        public SosielAlgorithmException(string message)
            :base(message) { }
    }
}
