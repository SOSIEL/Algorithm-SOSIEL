/// Name: UnknownVariableException.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;

namespace SOSIEL.Exceptions
{
    public class UnknownVariableException : Exception
    {
        readonly string _variableName;

        public UnknownVariableException(string variableName)
        {
            _variableName = variableName;
        }

        public override string ToString()
        {
            return string.Format("{0} wasn't defined for agent. Maybe you forgot to define it in config", _variableName);
        }
    }
}
