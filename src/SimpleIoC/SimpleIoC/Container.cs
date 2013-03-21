using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleIoC
{
    public class Container: IContainer
    {
        private readonly Dictionary<Type, Func<Type, object>> _resolvers = new Dictionary<Type, Func<Type, object>>(); 
        private readonly Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Tuple<ConstructorInfo,ParameterInfo[]>> _constructors = new Dictionary<Type, Tuple<ConstructorInfo,ParameterInfo[]>>(); 

        public void Register<TInterface, TImpl>() where TImpl: TInterface
        {
            _mappings.Add(typeof(TInterface),typeof(TImpl));
            _resolvers.Add(typeof(TInterface), ResolveFromMapping);
        }

        public void Register<TInterface>(Func<TInterface> resolver)
        {
            _resolvers.Add(typeof(TInterface), (x) => resolver());
        }

        public object Resolve(Type type)
        {
            if (_resolvers.ContainsKey(type))
            {
                return _resolvers[type](type);
            }
            return ResolveType(type,null);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof (T));
        }

        private object ResolveFromMapping(Type type)
        {
            return ResolveType(_mappings[type], type);
        }

        private object ResolveType(Type implType, Type baseType)
        {
            var constructors = implType.GetConstructors();
            foreach (var info in constructors.Select(x=> new Tuple<ConstructorInfo,ParameterInfo[]>(x, x.GetParameters())).OrderBy(x=> x.Item2.Count()))
            {
                try
                {
                    return InvokeConstuctor(info);
                }
                finally
                {
                    _constructors.Add(baseType ?? implType, info);
                    _resolvers[baseType ?? implType] = ResolveWithSavedConstructor;
                }
            }
            throw new InvalidOperationException("Can't resolve insance");
        }

        private object ResolveWithSavedConstructor(Type type)
        {
            return InvokeConstuctor(_constructors[type]);
        }

        private object InvokeConstuctor(Tuple<ConstructorInfo, ParameterInfo[]> constructorInfo)
        {
            var dependencies = new Dictionary<Type, object>();
            foreach (var parameterInfo in constructorInfo.Item2)
            {
                dependencies.Add(parameterInfo.ParameterType, Resolve(parameterInfo.ParameterType));
            }
            return constructorInfo.Item1.Invoke(dependencies.Values.ToArray());
        }
    }
}
