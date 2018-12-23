using System;

namespace SOSIEL.Exceptions
{
    public class UnknownVariableException : Exception
    {
        readonly string variableName;

        public UnknownVariableException(string variableName)
        {
            this.variableName = variableName;
        }

        public override string ToString()
        {
            return string.Format("{0} wasn't defined for agent. Maybe you forgot to define it in config", variableName);
        }
    }
}
