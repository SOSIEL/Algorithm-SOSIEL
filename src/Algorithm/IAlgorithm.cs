/// Name: IAlgorithm.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik


namespace SOSIEL.Algorithm
{
    public interface IAlgorithm<TData>
    {
        string Name { get; }

        void Initialize(TData data);

        TData Run(TData data);
    }
}
