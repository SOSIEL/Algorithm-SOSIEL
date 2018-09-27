using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Environments
{
    public interface ICloneable<T>
    {
        T Clone();
    }
}
