using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using System;
using System.Collections.Generic;

namespace MicroService.Service.Mappings
{
    public class KeyValuePairToShapeClassConverter<T> : ITypeConverter<KeyValuePair<string, object>, T>
        where T : new()
    {
        public T Convert(KeyValuePair<string, object> source, T destination, ResolutionContext context)
        {
            var key = source.Key;
            var featureName = GetFeatureName(key);
            var propertyInfo = typeof(T).GetProperty(key);

            if (propertyInfo != null)
            {
                var typeOfMyProperty = propertyInfo.PropertyType;
                var value = source.Value;

                if (value != null && value.GetType() != typeOfMyProperty)
                {
                    try
                    {
                        object convertedValue = null;
                        if (typeOfMyProperty == typeof(int))
                        {
                            if (int.TryParse(value.ToString(), out int intValue))
                            {
                                convertedValue = intValue;
                            }
                        }
                        else if (typeOfMyProperty == typeof(double))
                        {
                            if (double.TryParse(value.ToString(), out double doubleValue))
                            {
                                convertedValue = doubleValue;
                            }
                        }
                        else if (typeOfMyProperty == typeof(string))
                        {
                            convertedValue = value.ToString();
                        }
                        else
                        {
                            throw new NotSupportedException($"Converting type {value.GetType()} to {typeOfMyProperty} is not supported.");
                        }

                        if (convertedValue == null)
                        {
                            throw new FormatException($"Failed to convert value of type {value.GetType()} to {typeOfMyProperty}.");
                        }

                        propertyInfo.SetValue(destination, convertedValue);
                    }
                    catch (FormatException ex)
                    {
                        throw new FormatException($"Failed to convert value of type {value.GetType()} to {typeOfMyProperty}.", ex);
                    }
                }
                else
                {
                    propertyInfo.SetValue(destination, value);
                }
            }

            return destination;
        }

        private string GetFeatureName(string propertyName)
        {
            T shapeClass = Activator.CreateInstance<T>();
            var featureName = ReflectionExtensions.GetAttributeFromProperty<FeatureNameAttribute>(shapeClass, propertyName);

            return featureName?.AttributeName;
        }
    }

}
