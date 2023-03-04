using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MicroService.Service.Helpers
{
    public static class EnumHelper
    {
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute =
                    Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description) return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description) return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
        }

        public static IEnumerable<T> EnumToList<T>()
        {
            Type enumType = typeof(T);

            // Can't use generic type constraints on value types,
            // so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);
            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }

        public static bool IsEnumValid<T>(string value) where T : struct, Enum
        {
            return Enum.TryParse(value, out T result) && Enum.IsDefined(typeof(T), result);
        }

        public static List<PropertyInfo> GetPropertiesWithAttribute<TAttr>(Type type) where TAttr : Attribute
        {
            return type.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(TAttr), true).Length > 0)
                .ToList();
        }

        public static List<PropertyInfo> GetPropertiesWithoutExcludedAttribute<T, TAttr>()
            where TAttr : Attribute
        {
            return typeof(T).GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(TAttr), true).Length == 0)
                .ToList();
        }
    }
}
