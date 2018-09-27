using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Algorithm
{
    public interface IAlgorithm
    {
        string Name { get; }

        string Run();
    }
}
