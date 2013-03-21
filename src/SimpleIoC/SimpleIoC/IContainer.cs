using System;

namespace SimpleIoC
{
    public interface IContainer
    {
        object Resolve(Type type);
        void Register<TInterface, TImpl>() where TImpl : TInterface;
        void Register<TInterface>(Func<TInterface> resolver);
        T Resolve<T>();
    }
}