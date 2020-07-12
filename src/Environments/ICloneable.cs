/// Name: ICloneable.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik

namespace SOSIEL.Environments
{
    public interface ICloneable<T>
    {
        T Clone();
    }
}
