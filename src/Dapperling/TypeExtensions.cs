using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper;

internal static class TypeExtensions
{
    public static string GetDefaultTableName(this Type type)
    {
        return type.IsInterface
            ? $"{type.Name.Substring(1)}s"
            : $"{type.Name}s";
    }

    public static Type? GetCollectionElementType(this Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (typeof(IEnumerable<>).IsAssignableFrom(type))
        {
            var genericType = GetInterfaces(type)
                .Where(x => x.IsGenericType)
                .FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return genericType?.GetGenericArguments().FirstOrDefault();
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments().FirstOrDefault();
        }

        return null;
    }

    private static IEnumerable<Type> GetInterfaces(Type type)
    {
        yield return type;

        foreach (var interfaceType in type.GetInterfaces())
        {
            yield return interfaceType;
        }
    }
}
