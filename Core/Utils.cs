using System;
using System.ComponentModel;
using System.Reflection;

namespace VMGuide
{
    public static class Utils
    {
        public static string GetTypeDescription(this object o) {
            var type = o.GetType();
            var attr = type.GetCustomAttribute<DescriptionAttribute>();
            return (attr?.Description) ?? type.Name;
        }

        public static bool IsGenericTypeOf(this Type type, Type generic, out Type elementType) {
            try {
                elementType = type.GenericTypeArguments[0];
                var baseType = generic.MakeGenericType(elementType);
                if (baseType.IsAssignableFrom(type))
                    return true;
                else
                    throw new Exception($"{type.Name} is not a generic type of {generic.Name}");
            } catch {
                elementType = default(Type);
                return false;
            }
        }   
    }
}