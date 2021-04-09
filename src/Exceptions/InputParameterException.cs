/// Name: InputParameterException.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;

namespace SOSIEL.Exceptions
{
    public class InputParameterException: Exception
    {
        private string _parameter;
        private string _message;

        public InputParameterException(string parameter, string message)
        {
            _parameter = parameter;
            _message = message;
        }

        public override string ToString()
        {
            return string.Format("Value of {0} is wrong. {1}", _parameter, _message);
        }
    }
}
