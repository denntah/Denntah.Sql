using System;
using System.Collections.Concurrent;

namespace Denntah.Sql.Reflection
{
    public static class TypeHandler
    {
        private static readonly ConcurrentDictionary<Type, TypeDescriber> _types = new ConcurrentDictionary<Type, TypeDescriber>();

        public static TypeDescriber Get<T>()
        {
            if (!_types.TryGetValue(typeof(T), out TypeDescriber result))
                _types.TryAdd(typeof(T), result = new TypeDescriber(typeof(T)));

            return result;
        }

        public static TypeDescriber Get(object obj)
        {
            Type type = obj.GetType();
            
            if (!_types.TryGetValue(type, out TypeDescriber result))
                _types.TryAdd(type, result = new TypeDescriber(type));

            return result;
        }
    }
}
