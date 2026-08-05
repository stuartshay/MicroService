using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Reflection;

namespace MicroService.Service.Services.Base
{
    public delegate IShapefileDataReaderService ShapefileDataReaderResolver(string key);

    public abstract class AbstractShapeService<TShape, TProfile>
        where TShape : class, new()
        where TProfile : Profile, new()
    {
        // Always assigned in each derived class's constructor via a ShapefileDataReaderResolver.
        protected IShapefileDataReaderService ShapeFileDataReader { get; set; } = null!;

        protected readonly IMapper Mapper;

        protected readonly ILogger Logger;

        protected AbstractShapeService(
            ILogger logger,
            IMapper mapper)
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
        public List<KeyValuePair<string, object>>? ValidateFeatureKey(List<KeyValuePair<string, object>>? attributes)
        {
            if (attributes == null)
            {
                return null;
            }

            var shapeClass = Activator.CreateInstance<TShape>(); // create an instance of the class

            for (int i = 0; i < attributes.Count; i++)
            {
                var key = attributes[i].Key;
                var propertyInfo = typeof(TShape).GetProperty(key);

                if (propertyInfo == null)
                {
                    continue;
                }

                var featureName = GetFeatureName(key) ?? key;
                var value = attributes[i].Value;

                attributes[i] = value != null && value.GetType() != propertyInfo.PropertyType
                    ? new KeyValuePair<string, object>(featureName, ConvertAttributeValue(shapeClass!, propertyInfo, value))
                    : new KeyValuePair<string, object>(featureName, value!);
            }

            return attributes;
        }

        private static object ConvertAttributeValue(TShape shapeClass, PropertyInfo propertyInfo, object value)
        {
            var typeOfMyProperty = propertyInfo.PropertyType;

            try
            {
                var convertedValue = ConvertToPropertyType(value, typeOfMyProperty)
                    ?? throw new FormatException($"Failed to convert value of type {value.GetType()} to {typeOfMyProperty}.");

                propertyInfo.SetValue(shapeClass, convertedValue);
                return convertedValue;
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Failed to convert value of type {value.GetType()} to {typeOfMyProperty}.", ex);
            }
        }

        private static object? ConvertToPropertyType(object value, Type typeOfMyProperty)
        {
            if (typeOfMyProperty == typeof(int))
            {
                return int.TryParse(value.ToString(), out int intValue) ? intValue : null;
            }

            if (typeOfMyProperty == typeof(double))
            {
                return double.TryParse(value.ToString(), out double doubleValue) ? doubleValue : null;
            }

            if (typeOfMyProperty == typeof(string))
            {
                return Convert.ToString(value);
            }

            throw new NotSupportedException($"Converting type {value.GetType()} to {typeOfMyProperty} is not supported.");
        }

        protected static object? MatchAttributeValue(object value, object expectedValue) => value switch
        {
            string s => MatchString(s, expectedValue),
            int i => MatchInt(i, expectedValue),
            double d => MatchDouble(d, expectedValue),
            _ => null
        };

        private static object? MatchString(string value, object expectedValue) =>
            expectedValue is string expected && value == expected ? value : null;

        private static object? MatchInt(int value, object expectedValue) => expectedValue switch
        {
            int expected => value == expected ? value : null,
            double expected => value == (int)expected ? value : null,
            _ => null
        };

        private static object? MatchDouble(double value, object expectedValue)
        {
            const double tolerance = 1e-9;

            return expectedValue switch
            {
                int expected => (int)value == expected ? value : null,
                double expected => Math.Abs(value - expected) <= tolerance ? value : null,
                _ => null
            };
        }

        public string? GetFeatureName(string propertyName)
        {
            TShape shapeClass = Activator.CreateInstance<TShape>();
            var featureName = ReflectionExtensions.GetAttributeFromProperty<FeatureNameAttribute>(shapeClass, propertyName);

            return featureName?.AttributeName;
        }

        public virtual IEnumerable<TShape> GetFeatureList()
        {
            var features = GetFeatures();
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);
            }

            var results = Mapper.Map<IEnumerable<TShape>>(features);
            return results;
        }

        public virtual IEnumerable<TShape> GetFeatureLookup(List<KeyValuePair<string, object>>? attributes)
        {
            if (attributes == null)
            {
                return Enumerable.Empty<TShape>();
            }

            attributes = ValidateFeatureKey(attributes);

            var results = GetFeatures()
                .Where(f => attributes!.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => Mapper.Map<TShape>(f));

            return results;
        }

        public virtual TShape? GetFeatureLookup(double x, double y, Datum datum)
        {
            ShapePropertiesAttribute shapePropertiesAttribute = typeof(TShape)
                .GetCustomAttributes(typeof(ShapePropertiesAttribute), false)
                .OfType<ShapePropertiesAttribute>()
                .SingleOrDefault()
                ?? throw new InvalidOperationException($"{typeof(TShape)} does not declare a {nameof(ShapePropertiesAttribute)}.");

            ShapeProperties shapeProperties = shapePropertiesAttribute.ShapeProperties;
            Datum sourceDatum = shapeProperties.GetAttribute<ShapeAttribute>().Datum;

            (x, y) = TransformCoordinates(x, y, datum, sourceDatum);

            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return Mapper.Map<TShape>(feature);
        }

        public IReadOnlyCollection<Feature> GetFeatures() =>
                ShapeFileDataReader.GetFeatures();

        private static (double, double) TransformCoordinates(double x, double y, Datum fromDatum, Datum toDatum)
        {
            // GeoTransformationHelper only returns null components when its own x/y
            // arguments are null; x and y here are non-nullable doubles, so the
            // conversion result is always populated.
            if (fromDatum != toDatum)
            {
                if (fromDatum == Datum.Nad83 && toDatum == Datum.Wgs84)
                {
                    var (x1, y1) = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
                    return (x1!.Value, y1!.Value);
                }
                else if (fromDatum == Datum.Wgs84 && toDatum == Datum.Nad83)
                {
                    var (x1, y1) = GeoTransformationHelper.ConvertWgs84ToNad83(x, y);
                    return (x1!.Value, y1!.Value);
                }
            }

            return (x, y);
        }
    }
}
