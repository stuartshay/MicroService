using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services.Base
{
    public delegate IShapefileDataReaderService ShapefileDataReaderResolver(string key);

    public abstract class AbstractShapeService<T, TProfile> where T : class, new() where TProfile : Profile, new()
    {
        protected IShapefileDataReaderService ShapeFileDataReader { get; set; }

        protected IMapper Mapper { get; }

        protected readonly ILogger Logger;

        protected AbstractShapeService(ILogger logger, IMapper mapper)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public ShapefileHeader GetShapeProperties()
        {
            ShapefileHeader shpHeader = ShapeFileDataReader.ShapeHeader;
            return shpHeader;
        }

        public DbaseFileHeader GetShapeDatabaseProperties()
        {
            DbaseFileHeader header = ShapeFileDataReader.DbaseHeader;
            return header;
        }


        /// <summary>
        /// Validate/Map Shape Feature Properties
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, object>> ValidateFeatureKey(List<KeyValuePair<string, object>> attributes)
        {
            var shapeClass = Activator.CreateInstance<T>(); // create an instance of the class

            for (int i = 0; i < attributes.Count; i++)
            {
                var key = attributes[i].Key;
                var featureName = GetFeatureName(key);
                var propertyInfo = typeof(T).GetProperty(key);

                if (propertyInfo != null)
                {
                    var typeOfMyProperty = propertyInfo.PropertyType;
                    var value = attributes[i].Value;

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
                                convertedValue = Convert.ToString(value);
                            }
                            else
                            {
                                throw new NotSupportedException($"Converting type {value.GetType()} to {typeOfMyProperty} is not supported.");
                            }

                            if (convertedValue == null)
                            {
                                throw new FormatException($"Failed to convert value of type {value.GetType()} to {typeOfMyProperty}.");
                            }

                            propertyInfo.SetValue(shapeClass, convertedValue);
                            attributes[i] = new KeyValuePair<string, object>(featureName, convertedValue);
                        }
                        catch (FormatException ex)
                        {
                            throw new FormatException($"Failed to convert value of type {value.GetType()} to {typeOfMyProperty}.", ex);
                        }
                    }
                    else
                    {
                        attributes[i] = new KeyValuePair<string, object>(featureName, value);
                    }
                }
            }

            return attributes;
        }

        protected object MatchAttributeValue(object value, object expectedValue)
        {
            if (value is string s)
            {
                if (expectedValue is string es)
                {
                    return s == es ? s : default;
                }
            }
            else if (value is int i)
            {
                if (expectedValue is int ei)
                {
                    return i == ei ? i : default;
                }
                else if (expectedValue is double ed)
                {
                    return i == (int)ed ? i : default;
                }
            }
            else if (value is double d)
            {
                if (expectedValue is int ei)
                {
                    return (int)d == ei ? d : default;
                }

                if (expectedValue is double ed)
                {
                    return d == ed ? d : default;
                }
            }

            return default;
        }

        public string GetFeatureName(string propertyName)
        {
            T shapeClass = Activator.CreateInstance<T>();
            var featureName = ReflectionExtensions.GetAttributeFromProperty<FeatureNameAttribute>(shapeClass, propertyName);

            return featureName?.AttributeName;
        }

        public virtual IEnumerable<T> GetFeatureList()
        {
            var features = GetFeatures();
            Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);

            var results = Mapper.Map<IEnumerable<T>>(features);
            return results;
        }

        public virtual IEnumerable<T> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);

            var results = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => Mapper.Map<T>(f));

            return results;
        }

        public virtual T GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return Mapper.Map<T>(feature);
        }

        public IReadOnlyCollection<Feature> GetFeatures() =>
                ShapeFileDataReader.GetFeatures();
    }
}
