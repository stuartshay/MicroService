using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroService.Service.Helpers
{
    public static class ReflectionExtensions
    {
        public static bool ArePropertiesNotNull<T>(this T obj)
        {
            var result = PropertyCache<T>.PublicProperties.All(propertyInfo => propertyInfo.GetValue(obj) != null);
            return result;
        }

        public static class PropertyCache<T>
        {
            private static readonly Lazy<IReadOnlyCollection<PropertyInfo>> publicPropertiesLazy
                = new Lazy<IReadOnlyCollection<PropertyInfo>>(() => typeof(T).GetProperties());

            public static IReadOnlyCollection<PropertyInfo> PublicProperties => PropertyCache<T>.publicPropertiesLazy.Value;
        }

        public static TAttribute GetAttributeFromProperty<TAttribute>(object obj, string propertyName) where TAttribute : Attribute
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property!.GetCustomAttribute<TAttribute>();
        }

        public static PropertyInfo[] GetPropertiesWithCustomAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<T>() != null).ToArray();
        }
    }
}
